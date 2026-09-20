#!/usr/bin/env python3
"""Render one frame of a kanim (build + anim + texture) to a transparent PNG.

Multi-part buildings only have their parts in the texture atlas, so a preview needs the
frame assembled the way the game does it. Extract the three files with UnityPy from
sharedassets0.assets (Texture2D <name>_0, TextAssets <name>_build and <name>_anim).

    python render_kanim.py fridge_0.png fridge_build.bytes fridge_anim.bytes out.png --anim off

--list prints the anim names. --scale is output pixels per anim unit (0.5 = texture resolution).
--skip drops symbols by name (e.g. status lights that are tinted at runtime).
"""
import argparse
import struct

import numpy as np
from PIL import Image


class Reader:
    def __init__(self, data):
        self.d, self.o = data, 0

    def read(self, fmt):
        v = struct.unpack_from("<" + fmt, self.d, self.o)
        self.o += struct.calcsize("<" + fmt)
        return v if len(v) > 1 else v[0]

    def string(self):
        n = self.read("i")
        s = self.d[self.o:self.o + n].decode("utf-8")
        self.o += n
        return s


def read_build(data):
    r = Reader(data)
    assert r.d[:4] == b"BILD"
    r.o = 4
    version, symbol_count, _ = r.read("iii")
    r.string()
    symbols = {}
    for _ in range(symbol_count):
        h = r.read("i")
        if version > 9:
            r.read("i")
        r.read("ii")
        frames = {}
        for _ in range(r.read("i")):
            num, _dur, _img, px, py, pw, ph, u1, v1, u2, v2 = r.read("iiiffffffff")
            frames[num] = (px, py, pw, ph, u1, v1, u2, v2)
        symbols[h] = frames
    names = {}
    for _ in range(r.read("i")):
        h = r.read("i")
        names[h] = r.string()
    return symbols, names


def read_anims(data):
    r = Reader(data)
    assert r.d[:4] == b"ANIM"
    r.o = 4
    _version, _elements, _frames, anim_count = r.read("iiii")
    anims = {}
    for _ in range(anim_count):
        name = r.string()
        r.read("if")
        frames = []
        for _ in range(r.read("i")):
            r.read("ffff")
            elements = []
            for _ in range(r.read("i")):
                image, index, _layer, _flags = r.read("iiii")
                rgba = r.read("ffff")
                a, b, c, d, tx, ty = r.read("ffffff")
                r.read("f")
                elements.append((image, index, rgba, (a, b, c, d, tx, ty)))
            frames.append(elements)
        anims[name] = frames
    return anims


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("texture")
    p.add_argument("build")
    p.add_argument("anim")
    p.add_argument("output", nargs="?")
    p.add_argument("--anim", dest="anim_name", default="off")
    p.add_argument("--frame", type=int, default=0)
    p.add_argument("--scale", type=float, default=0.5)
    p.add_argument("--skip", default="", help="comma-separated symbol names to leave out")
    p.add_argument("--list", action="store_true")
    a = p.parse_args()

    symbols, names = read_build(open(a.build, "rb").read())
    anims = read_anims(open(a.anim, "rb").read())
    if a.list or not a.output:
        for n, f in anims.items():
            print(n, len(f), "frame(s)")
        print("symbols:", ", ".join(sorted(names.values())))
        return

    tex = Image.open(a.texture).convert("RGBA")
    tw, th = tex.size
    skip = {s for s in a.skip.split(",") if s}
    layers, corners = [], []
    for image, index, rgba, (ma, mb, mc, md, tx, ty) in anims[a.anim_name][a.frame]:
        frames = symbols.get(image)
        if not frames or names.get(image) in skip:
            continue
        # The game falls back to the closest lower frame number when the exact one is missing.
        usable = [n for n in frames if n <= index]
        px, py, pw, ph, u1, v1, u2, v2 = frames[max(usable) if usable else min(frames)]
        box = (round(u1 * tw), round(v1 * th), round(u2 * tw), round(v2 * th))
        cw, ch = box[2] - box[0], box[3] - box[1]
        if cw <= 0 or ch <= 0:
            continue
        # crop pixel -> symbol-local anim units -> animated position
        local = np.array([[pw / cw, 0, px - pw / 2], [0, ph / ch, py - ph / 2], [0, 0, 1]])
        world = np.array([[ma, mc, tx], [mb, md, ty], [0, 0, 1]]) @ local
        layers.append((tex.crop(box), world, rgba))
        for x, y in ((0, 0), (cw, 0), (0, ch), (cw, ch)):
            corners.append((world @ np.array([x, y, 1]))[:2])

    corners = np.array(corners)
    lo, hi = corners.min(axis=0), corners.max(axis=0)
    size = tuple(int(np.ceil(v * a.scale)) + 2 for v in (hi - lo))
    to_canvas = np.array([[a.scale, 0, -lo[0] * a.scale + 1], [0, a.scale, -lo[1] * a.scale + 1], [0, 0, 1]])
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    # Elements are stored front to back.
    for crop, world, rgba in reversed(layers):
        inv = np.linalg.inv(to_canvas @ world)
        layer = crop.transform(size, Image.AFFINE, tuple(inv[:2].flatten()), resample=Image.BICUBIC)
        if rgba != (1.0, 1.0, 1.0, 1.0):
            r, g, b, al = layer.split()
            layer = Image.merge("RGBA", (r, g, b, al.point(lambda v, m=rgba[0]: int(v * m))))
        canvas.alpha_composite(layer)
    canvas.save(a.output)
    print("wrote", a.output, canvas.size)


if __name__ == "__main__":
    main()
