#!/usr/bin/env python3
"""Read, write, render and extract Klei kanims (texture atlas + build + anim).

    python kanim.py list <kanim>                     anims, and symbols with their frames
    python kanim.py dump <kanim> [--anim A] [--frame N]
                                                     each element of a frame, front to back
    python kanim.py render <kanim> [out.png] [--anim off] [--frame 0] [--scale 0.5] [--hide a,b]
    python kanim.py extract <name>... [--out DIR]    copy vanilla kanims out of the game
    python kanim.py make <sprite.png> <out-dir> --name <kanim_name> [--size 220] [--center 0,-100]
                         [--texture-size 512] [--anims off,on_pre,on,on_pst,place,ui] [--rate 30]

<kanim> is a folder holding <name>_0.png, <name>_build.bytes and <name>_anim.bytes (a mod's
src/<Mod>/anim/assets/<name>), or the name of a vanilla kanim such as logic_ribbon_reader.
Vanilla kanims come out of the installed game's sharedassets0.assets (UnityPy) and are cached
in the temp folder; the game folder is --game, else ONI_GAME_FOLDER, else the Steam folders
the mod builds look in. They are Klei's art: keep them out of the repo. render without an
output path writes to the temp folder and opens the image.

`make` builds a one-sprite kanim from a PNG: every anim is one static frame of the sprite, so a
building whose code plays on/off style anims keeps working, "ui" is the build-menu icon and
"place" the construction ghost, drawn as the sprite's white outline (see place_outline).

From a mod's art script:

    sys.path.insert(0, "<repo>/common/tools/Kanim")
    from kanim import Sprite, write_kanim

    write_kanim(out_dir, "my_building",
        symbols={"body": [Sprite(red_img, 224, 214), Sprite(green_img, 224, 214)], "cap": [Sprite(cap_img, 198, 57)]},
        anims={"off": [[("body", 0, 0, -100), ("cap", 0, 0, -230)]], "on": [[("body", 1, 0, -100), ("cap", 0, 0, -230)]]})

A Sprite is an image plus its display size in anim units (a grid cell is 200 x 200; the
texture resolution is independent of the display size). An anim is a list of frames, a frame
a list of elements (symbol, frame index, x, y) ordered FRONT TO BACK, where x, y place the
sprite's centre relative to the bottom centre of the building's origin cell, y growing
downwards. An element may carry a fifth item, a dict with any of alpha (0..1), scale and
rotation (degrees, clockwise on screen, about the sprite's centre); that is how the vanilla
art fades and pulses things. "matrix": (a, b, c, d) instead sets the element's whole 2x2
transform, for the mirrored and stretched parts vanilla art is full of. The same PIL image
used for several sprites is stored once. Symbols named in bloom= glow, like vanilla's
*_bloom lights.

read_kanim() turns an existing kanim into the same symbols and anims, so a script can change a
few parts and write it back under a new name. vanilla() loads a vanilla kanim, render() draws
one frame, drift() compares a fresh build of a kanim with the committed one. read_build_file()
and read_anim_file() give the files' raw contents, which write_build_file() and
write_anim_file() write back byte for byte, for scripts that rework an artist's kanim in place.

Every building kanim needs a "place" anim: the game plays it on the construction site and
the placement cursor, and tints it (green/red/white) itself. Vanilla place art is the
sprite's black ink outlines turned white with everything else transparent, which is what
place_outline() makes from a cartoon-style sprite; feeding the full-colour sprite instead
makes an unbuilt building look finished.

Format (verified against vanilla files, BILD v10 / ANIM v5; strings are int length + UTF-8):
  BILD: magic, version, symbolCount, frameCount, name; per symbol: hash, pathHash, colour,
        flags (1 = bloom, 8 = foreground), frameCount; per frame: frameNumber, duration,
        imageIndex, pivotX, pivotY, pivotW, pivotH, u1, v1, u2, v2; then the hash table
        (count; hash, string).
  ANIM: magic, version, totalElements, totalFrames, animCount; per anim: name, rootHash,
        rate, frameCount; per frame: bboxX, bboxY, bboxW, bboxH, elementCount; per element:
        symbolHash, frame, layerHash, flags, a, b, g, r, m00, m01, m10, m11, tx, ty, order;
        then maxVisibleSymbolFrames and the hash table. The game ignores the two totals.
  Hashes are Klei's SDBM of the lower-cased name. Pivot (0, 0) is the sprite centre.
"""
import argparse
import math
import os
import struct
import sys
import tempfile
from collections import namedtuple

from PIL import Image, ImageChops

Sprite = namedtuple("Sprite", "image width height")

BLOOM, FOREGROUND = 1, 8
STEAM_FOLDERS = (r"F:\SteamLibrary\steamapps\common\OxygenNotIncluded",
                 r"C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded")


def sdbm(s):
    h = 0
    for c in s.lower():
        h = (ord(c) + (h << 6) + (h << 16) - h) & 0xffffffff
    return h - (1 << 32) if h >= 1 << 31 else h


def kstr(s):
    b = s.encode("utf-8")
    return struct.pack("<i", len(b)) + b


# ---------------------------------------------------------------- raw files

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

    def hashes(self):
        return [(self.read("i"), self.string()) for _ in range(self.read("i"))]


BuildFile = namedtuple("BuildFile", "version name symbols hashes")
AnimFile = namedtuple("AnimFile", "version anims max_visible hashes")


def read_build_file(data):
    """A build's raw contents: symbols are [hash, path, colour, flags, frames], each frame
    [number, duration, image, px, py, pw, ph, u1, v1, u2, v2]; hashes is [(hash, name)] in file
    order. Lists, so a script can change them in place and write the file back."""
    r = Reader(data)
    assert data[:4] == b"BILD", "not a build file"
    r.o = 4
    version, symbol_count, _ = r.read("iii")
    name = r.string()
    symbols = []
    for _ in range(symbol_count):
        h = r.read("i")
        path = r.read("i") if version > 9 else h
        colour, flags = r.read("ii")
        frames = [list(r.read("iiiffffffff")) for _ in range(r.read("i"))]
        symbols.append([h, path, colour, flags, frames])
    return BuildFile(version, name, symbols, r.hashes())


def write_build_file(build):
    out = [b"BILD", struct.pack("<iii", build.version, len(build.symbols), sum(len(s[4]) for s in build.symbols)),
           kstr(build.name)]
    for h, path, colour, flags, frames in build.symbols:
        out.append(struct.pack("<i", h) + (struct.pack("<i", path) if build.version > 9 else b"")
                   + struct.pack("<iii", colour, flags, len(frames)))
        out += [struct.pack("<iiiffffffff", *f) for f in frames]
    out.append(struct.pack("<i", len(build.hashes)))
    out += [struct.pack("<i", h) + kstr(n) for h, n in build.hashes]
    return b"".join(out)


def read_anim_file(data):
    """An anim's raw contents: anims are [name, hash, rate, frames], each frame
    [x, y, w, h, elements] and each element [symbol, frame, layer, flags, a, b, g, r,
    m00, m01, m10, m11, tx, ty, order], front to back; hashes is [(hash, name)] in file order."""
    r = Reader(data)
    assert data[:4] == b"ANIM", "not an anim file"
    r.o = 4
    version, _elements, _frames, anim_count = r.read("iiii")
    anims = []
    for _ in range(anim_count):
        name = r.string()
        h, rate = r.read("if")
        frames = []
        for _ in range(r.read("i")):
            x, y, w, hh, count = r.read("ffffi")
            frames.append([x, y, w, hh, [list(r.read("iiiifffffffffff")) for _ in range(count)]])
        anims.append([name, h, rate, frames])
    max_visible = r.read("i")
    hashes = r.hashes()
    assert r.o == len(data), "trailing bytes in the anim file"
    return AnimFile(version, anims, max_visible, hashes)


def write_anim_file(anim):
    frames = [f for a in anim.anims for f in a[3]]
    out = [b"ANIM", struct.pack("<iiii", anim.version, sum(len(f[4]) for f in frames), len(frames), len(anim.anims))]
    for name, h, rate, anim_frames in anim.anims:
        out.append(kstr(name) + struct.pack("<ifi", h, rate, len(anim_frames)))
        for x, y, w, hh, elements in anim_frames:
            out.append(struct.pack("<ffffi", x, y, w, hh, len(elements)))
            out += [struct.pack("<iiiifffffffffff", *e) for e in elements]
    out.append(struct.pack("<ii", anim.max_visible, len(anim.hashes)))
    out += [struct.pack("<i", h) + kstr(n) for h, n in anim.hashes]
    return b"".join(out)


def read_build(data, with_flags=False):
    """symbols {hash: {frame number: (px, py, pw, ph, u1, v1, u2, v2)}}, names {hash: name}, and
    with_flags also {hash: flags} (BLOOM marks a glowing symbol)."""
    build = read_build_file(data)
    symbols = {s[0]: {f[0]: tuple(f[3:]) for f in s[4]} for s in build.symbols}
    names = dict(build.hashes)
    if with_flags:
        return symbols, names, {s[0]: s[3] for s in build.symbols}
    return symbols, names


def read_anims(data):
    """{anim name: [frame: [(image hash, frame index, (a, b, g, r), (m1..m6), layer hash)]]},
    elements front to back."""
    return {name: [[(e[0], e[1], tuple(e[4:8]), tuple(e[8:14]), e[2]) for e in f[4]] for f in frames]
            for name, _, _, frames in read_anim_file(data).anims}


# ---------------------------------------------------------------- loading

def game_folder(game=None):
    """--game when given, else ONI_GAME_FOLDER, else the first Steam folder that exists."""
    for given, source in ((game, "--game"), (os.environ.get("ONI_GAME_FOLDER"), "ONI_GAME_FOLDER")):
        if given:
            if not os.path.isdir(given):
                sys.exit("no game folder at %s (from %s)" % (given, source))
            return given
    for folder in STEAM_FOLDERS:
        if os.path.isdir(folder):
            return folder
    sys.exit("Oxygen Not Included not found in %s. Pass --game <folder> or set ONI_GAME_FOLDER."
             % " or ".join(STEAM_FOLDERS))


def vanilla_cache(game=None):
    """The cache folder for the installed game's kanims; a game update gets a new one."""
    assets = os.path.join(game_folder(game), "OxygenNotIncluded_Data", "sharedassets0.assets")
    if not os.path.exists(assets):
        sys.exit("%s is missing; is %s the game folder?" % (assets, game_folder(game)))
    return os.path.join(tempfile.gettempdir(), "oni-vanilla-kanims", str(int(os.path.getmtime(assets)))), assets


def extract(names, game=None):
    """Makes sure the vanilla kanims are in the cache, reading the game's assets only when one
    is missing; returns the cache folder."""
    cache, assets = vanilla_cache(game)
    files = {f"{n}{suffix}": ext for n in names for suffix, ext in (("_0", ".png"), ("_build", ".bytes"), ("_anim", ".bytes"))}
    missing = {f for f, ext in files.items() if not os.path.exists(os.path.join(cache, f + ext))}
    if not missing:
        return cache
    try:
        import UnityPy
    except ImportError:
        sys.exit("Extracting vanilla kanims needs UnityPy: pip install -r common/tools/requirements.txt")
    os.makedirs(cache, exist_ok=True)
    print("reading", assets, file=sys.stderr)
    for obj in UnityPy.load(assets).objects:
        if obj.type.name not in ("Texture2D", "TextAsset"):
            continue
        data = obj.read()
        if data.m_Name not in missing:
            continue
        if obj.type.name == "Texture2D":
            data.image.save(os.path.join(cache, data.m_Name + ".png"))
        else:
            script = data.m_Script
            if isinstance(script, str):
                script = script.encode("utf-8", "surrogateescape")
            with open(os.path.join(cache, data.m_Name + ".bytes"), "wb") as f:
                f.write(script)
        missing.discard(data.m_Name)
    if missing:
        sys.exit("not in the game's assets: " + ", ".join(sorted(missing)))
    return cache


def load_files(folder, name):
    with open(os.path.join(folder, name + "_build.bytes"), "rb") as f:
        build = f.read()
    with open(os.path.join(folder, name + "_anim.bytes"), "rb") as f:
        anim = f.read()
    return Image.open(os.path.join(folder, name + "_0.png")), build, anim


def vanilla(name, game=None):
    """A vanilla kanim's (texture, build bytes, anim bytes)."""
    return load_files(extract([name], game), name)


def load(kanim, game=None):
    """(name, texture, build bytes, anim bytes) of a kanim folder or a vanilla kanim's name."""
    if os.path.isdir(kanim):
        builds = [f[:-len("_build.bytes")] for f in os.listdir(kanim) if f.endswith("_build.bytes")]
        if len(builds) != 1:
            sys.exit("%s holds %d kanims; expected one <name>_build.bytes" % (kanim, len(builds)))
        return (builds[0],) + load_files(kanim, builds[0])
    if os.sep in kanim or "/" in kanim:
        sys.exit("no such kanim folder: " + kanim)
    return (kanim,) + vanilla(kanim, game)


def show(path):
    """Opens an image in the system's viewer."""
    if sys.platform == "win32":
        os.startfile(path)
    else:
        import subprocess
        subprocess.Popen(["open" if sys.platform == "darwin" else "xdg-open", path])


def drift(generated, committed):
    """The files that differ between two kanim folders (a fresh build and the committed one)."""
    names = set(os.listdir(generated)) | (set(os.listdir(committed)) if os.path.isdir(committed) else set())
    changed = []
    for n in sorted(names):
        a, b = os.path.join(generated, n), os.path.join(committed, n)
        if not (os.path.exists(a) and os.path.exists(b)) or open(a, "rb").read() != open(b, "rb").read():
            changed.append(n)
    return changed


# ---------------------------------------------------------------- writing

def place_outline(image, ink=40, fade=30):
    """White line-art for the "place" symbol: pixels whose brightest channel is below `ink`
    keep their alpha and turn white, brightness up to ink + fade tails off, the rest is
    transparent. Works on the black-outlined cartoon style the game's art uses."""
    r, g, b, a = image.convert("RGBA").split()
    brightest = ImageChops.lighter(ImageChops.lighter(r, g), b)
    coverage = brightest.point(lambda v: 255 if v < ink else 0 if v >= ink + fade else int(255 * (ink + fade - v) / fade))
    out = Image.new("RGBA", image.size, (255, 255, 255, 0))
    out.putalpha(ImageChops.multiply(a, coverage))
    return out


def pack_atlas(images, size, pad):
    """Shelf-packs the images into a size x size atlas; returns (atlas, [box per image])."""
    atlas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    boxes, x, y, shelf = [], pad, pad, 0
    for im in images:
        if x + im.width + pad > size:
            x, y, shelf = pad, y + shelf + pad, 0
        if y + im.height + pad > size:
            raise ValueError("sprites do not fit a %dx%d texture" % (size, size))
        atlas.paste(im, (x, y))
        boxes.append((x, y, x + im.width, y + im.height))
        x += im.width + pad
        shelf = max(shelf, im.height)
    return atlas, boxes


def element_matrix(opts):
    """An element's 2x2 transform (a, b, c, d) from its options."""
    if "matrix" in opts:
        return tuple(opts["matrix"])
    scale, angle = opts.get("scale", 1.0), math.radians(opts.get("rotation", 0.0))
    cos, sin = math.cos(angle) * scale, math.sin(angle) * scale
    return cos, sin, -sin, cos


def read_kanim(texture, build, anim):
    """(symbols, anims, bloom) from a kanim's texture (PIL image) and its build and anim bytes,
    in write_kanim's terms. Each symbol's frames are listed in frame-number order and the
    elements' frame indices point into that list (the game falls back to the closest lower
    frame number, and so does this). Every element carries its full matrix. bloom is the set
    of glowing symbols."""
    built, names, flags = read_build(build, with_flags=True)
    tex = texture.convert("RGBA")
    tw, th = tex.size
    symbols, numbers = {}, {}
    for h, frames in built.items():
        sprites = []
        for num in sorted(frames):
            px, py, pw, ph, u1, v1, u2, v2 = frames[num]
            box = (round(u1 * tw), round(v1 * th), round(u2 * tw), round(v2 * th))
            sprites.append(Sprite(tex.crop(box), pw, ph))
        symbols[names[h]] = sprites
        numbers[h] = sorted(frames)

    anims = {}
    for anim_name, frames in read_anims(anim).items():
        out = []
        for frame in frames:
            elements = []
            for image, index, rgba, (a, b, c, d, tx, ty), _layer in frame:
                if image not in built:
                    continue
                nums = numbers[image]
                lower = [n for n in nums if n <= index]
                num = max(lower) if lower else min(nums)
                px, py = built[image][num][:2]
                # The sprite's centre sits at (px, py) in symbol space; write_kanim places centres.
                x, y = a * px + c * py + tx, b * px + d * py + ty
                elements.append((names[image], nums.index(num), x, y, {"matrix": (a, b, c, d), "alpha": rgba[0]}))
            out.append(elements)
        anims[anim_name] = out
    bloom = {names[h] for h, f in flags.items() if f & BLOOM}
    return symbols, anims, bloom


def write_kanim(out_dir, name, symbols, anims, texture_size=512, rate=30.0, pad=2, bloom=()):
    os.makedirs(out_dir, exist_ok=True)
    flat = [(sym, i, sp) for sym, frames in symbols.items() for i, sp in enumerate(frames)]
    unique = []
    for _, _, sp in flat:
        if not any(sp.image is im for im in unique):
            unique.append(sp.image)
    atlas, boxes = pack_atlas(unique, texture_size, pad)
    atlas.save(os.path.join(out_dir, name + "_0.png"))
    box_of = {id(im): box for im, box in zip(unique, boxes)}
    uv = {(sym, i): tuple(v / texture_size for v in box_of[id(sp.image)]) for sym, i, sp in flat}

    build = BuildFile(10, name, [
        [sdbm(sym), sdbm(sym), 0, BLOOM if sym in bloom else 0,
         [[i, 1, 0, 0.0, 0.0, sp.width, sp.height, *uv[(sym, i)]] for i, sp in enumerate(frames)]]
        for sym, frames in symbols.items()], [(sdbm(sym), sym) for sym in symbols])
    with open(os.path.join(out_dir, name + "_build.bytes"), "wb") as f:
        f.write(write_build_file(build))

    root = sdbm("building")
    out = []
    for anim_name, frames in anims.items():
        out_frames = []
        for frame in frames:
            frame = [tuple(e) + ({},) if len(e) == 4 else tuple(e) for e in frame]
            matrices = [element_matrix(opts) for _, _, _, _, opts in frame]
            # Bounds of each element's transformed sprite (half extents of a transformed box).
            ext = [(abs(a) * symbols[s][i].width / 2 + abs(c) * symbols[s][i].height / 2,
                    abs(b) * symbols[s][i].width / 2 + abs(d) * symbols[s][i].height / 2)
                   for (s, i, _, _, _), (a, b, c, d) in zip(frame, matrices)]
            x1 = min(x - ex for (_, _, x, _, _), (ex, _) in zip(frame, ext))
            x2 = max(x + ex for (_, _, x, _, _), (ex, _) in zip(frame, ext))
            y1 = min(y - ey for (_, _, _, y, _), (_, ey) in zip(frame, ext))
            y2 = max(y + ey for (_, _, _, y, _), (_, ey) in zip(frame, ext))
            elements = [[sdbm(s), i, sdbm(s), 0, opts.get("alpha", 1.0), 1.0, 1.0, 1.0, a, b, c, d, x, y, 0.0]
                        for (s, i, x, y, opts), (a, b, c, d) in zip(frame, matrices)]
            out_frames.append([(x1 + x2) / 2, (y1 + y2) / 2, x2 - x1, y2 - y1, elements])
        out.append([anim_name, root, rate, out_frames])
    names = {"building"} | set(symbols) | set(anims)
    anim = AnimFile(5, out, max(len(frame) for frames in anims.values() for frame in frames),
                    [(sdbm(n), n) for n in sorted(names)])
    with open(os.path.join(out_dir, name + "_anim.bytes"), "wb") as f:
        f.write(write_anim_file(anim))


# ---------------------------------------------------------------- rendering

def render(texture, build, anim, anim_name, frame=0, scale=0.5, skip=()):
    """One frame as a transparent image just big enough for it, plus the canvas pixel where
    the anim origin (the bottom centre of the building's origin cell) falls. texture is a PIL
    image, build and anim the files' bytes, skip a set of symbol names to leave out."""
    import numpy as np

    symbols, names = read_build(build)
    anims = read_anims(anim)
    if anim_name not in anims:
        raise KeyError("no anim %r; the kanim has %s" % (anim_name, ", ".join(anims)))
    tex = texture.convert("RGBA")
    tw, th = tex.size
    skip = set(skip)
    layers, corners = [], []
    for image, index, rgba, (ma, mb, mc, md, tx, ty), _layer in anims[anim_name][frame]:
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
    size = tuple(int(np.ceil(v * scale)) + 2 for v in (hi - lo))
    to_canvas = np.array([[scale, 0, -lo[0] * scale + 1], [0, scale, -lo[1] * scale + 1], [0, 0, 1]])
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    # Elements are stored front to back.
    for crop, world, rgba in reversed(layers):
        inv = np.linalg.inv(to_canvas @ world)
        layer = crop.transform(size, Image.AFFINE, tuple(inv[:2].flatten()), resample=Image.BICUBIC)
        if rgba != (1.0, 1.0, 1.0, 1.0):
            r, g, b, al = layer.split()
            layer = Image.merge("RGBA", (r, g, b, al.point(lambda v, m=rgba[0]: int(v * m))))
        canvas.alpha_composite(layer)
    return canvas, (to_canvas[0, 2], to_canvas[1, 2])


# ---------------------------------------------------------------- command line

def cmd_list(a):
    name, _, build, anim = load(a.kanim, a.game)
    symbols, names, flags = read_build(build, with_flags=True)
    print(name)
    print("anims:")
    for n, frames in read_anims(anim).items():
        print("  %-24s %d frame(s)" % (n, len(frames)))
    print("symbols:")
    for h in sorted(symbols, key=lambda h: names.get(h, "")):
        tags = [t for t, bit in (("bloom", BLOOM), ("foreground", FOREGROUND)) if flags[h] & bit]
        sizes = sorted({(round(f[2]), round(f[3])) for f in symbols[h].values()})
        print("  %-24s %2d frame(s)  %s%s" % (names.get(h, h), len(symbols[h]),
                                             ", ".join("%dx%d" % s for s in sizes), "  " + " ".join(tags) if tags else ""))


def cmd_dump(a):
    name, _, build, anim = load(a.kanim, a.game)
    raw = read_anim_file(anim)
    names = dict(read_build_file(build).hashes)
    names.update(raw.hashes)
    for anim_name, _, rate, frames in raw.anims:
        if a.anim and anim_name != a.anim:
            continue
        if a.frame >= len(frames):
            print("%s: %d frame(s), no frame %d" % (anim_name, len(frames), a.frame))
            continue
        x, y, w, h, elements = frames[a.frame]
        print("%s frame %d of %d (%g fps), bounds %.1f, %.1f  %.1f x %.1f" % (anim_name, a.frame, len(frames), rate, x, y, w, h))
        for i, e in enumerate(elements):
            symbol, index, layer, _flags, alpha = e[:5]
            m = e[8:14]
            print("  %2d %-22s #%-3d layer %-18s alpha %.2f  matrix %6.3f %6.3f %6.3f %6.3f  at %7.1f %7.1f"
                  % (i, names.get(symbol, symbol), index, names.get(layer, layer), alpha, *m))
    if a.anim and a.anim not in {x[0] for x in raw.anims}:
        sys.exit("%s has no anim %r" % (name, a.anim))


def cmd_render(a):
    name, texture, build, anim = load(a.kanim, a.game)
    hidden = {s for s in (a.hide or "").split(",") if s}
    try:
        canvas, _ = render(texture, build, anim, a.anim, a.frame, a.scale, hidden)
    except KeyError as e:
        sys.exit(e.args[0])
    out = a.output or os.path.join(tempfile.gettempdir(), "%s-%s-%d.png" % (name, a.anim, a.frame))
    canvas.save(out)
    print("wrote", out, canvas.size)
    if not a.output:
        show(out)


def cmd_extract(a):
    cache = extract(a.names, a.game)
    if a.out:
        os.makedirs(a.out, exist_ok=True)
        import shutil
        for n in a.names:
            for f in (n + "_0.png", n + "_build.bytes", n + "_anim.bytes"):
                shutil.copy(os.path.join(cache, f), a.out)
    print("extracted to", a.out or cache)


def cmd_make(a):
    im = Image.open(a.sprite).convert("RGBA")
    im = im.crop(im.getbbox())
    # Two copies (sprite + outline) have to share the atlas; scale the sprite so both fit.
    scale = min((a.texture_size - 6) / im.width, (a.texture_size - 6) / (2 * im.height), 1.0)
    if scale < 1.0:
        im = im.resize((round(im.width * scale), round(im.height * scale)), Image.LANCZOS)

    disp = min(a.size / im.width, a.size / im.height)
    w, h = im.width * disp, im.height * disp
    cx, cy = (float(v) for v in a.center.split(","))
    anims = [s for s in a.anims.split(",") if s]

    body = Sprite(im, w, h)
    symbols = {"body": [body]}
    if "place" in anims:
        symbols["place"] = [Sprite(place_outline(im), w, h)]
    if "ui" in anims:
        symbols["ui"] = [body]
    frames = {n: [[(n if n in symbols else "body", 0, cx, cy)]] for n in anims}
    write_kanim(a.out, a.name, symbols, frames, texture_size=a.texture_size, rate=a.rate)
    print(f"wrote {a.name}: sprite {im.size}, display {w:.0f}x{h:.0f} at ({cx},{cy}), anims {anims}")


def main():
    p = argparse.ArgumentParser(description=__doc__.split("\n\n")[0],
                                epilog="Run '%(prog)s <command> -h' for a command's options.")
    game = argparse.ArgumentParser(add_help=False)
    game.add_argument("--game", help="the game folder (default: ONI_GAME_FOLDER, then the usual Steam folders)")
    sub = p.add_subparsers(dest="command", required=True)

    s = sub.add_parser("list", parents=[game], help="anims and symbols")
    s.add_argument("kanim", help="a kanim folder or a vanilla kanim's name")
    s.set_defaults(run=cmd_list)

    s = sub.add_parser("dump", parents=[game], help="each element of a frame, front to back")
    s.add_argument("kanim", help="a kanim folder or a vanilla kanim's name")
    s.add_argument("--anim", help="only this anim (default: every anim)")
    s.add_argument("--frame", type=int, default=0)
    s.set_defaults(run=cmd_dump)

    s = sub.add_parser("render", parents=[game], help="one frame to a transparent PNG")
    s.add_argument("kanim", help="a kanim folder or a vanilla kanim's name")
    s.add_argument("output", nargs="?", help="default: <name>-<anim>-<frame>.png in the temp folder, opened when written")
    s.add_argument("--anim", default="off")
    s.add_argument("--frame", type=int, default=0)
    s.add_argument("--scale", type=float, default=0.5, help="output pixels per anim unit (0.5 = texture resolution)")
    s.add_argument("--hide", "--skip", help="comma-separated symbols to leave out (e.g. lights tinted at runtime)")
    s.set_defaults(run=cmd_render)

    s = sub.add_parser("extract", parents=[game], help="copy vanilla kanims out of the game")
    s.add_argument("names", nargs="+")
    s.add_argument("--out", help="folder to copy them to (default: just fill the cache)")
    s.set_defaults(run=cmd_extract)

    s = sub.add_parser("make", help="a one-sprite kanim from a PNG")
    s.add_argument("sprite")
    s.add_argument("out")
    s.add_argument("--name", required=True)
    s.add_argument("--size", type=float, default=220, help="display size of the longer side, anim px (cell = 200)")
    s.add_argument("--center", default="0,-100")
    s.add_argument("--texture-size", type=int, default=512)
    s.add_argument("--anims", default="off,on_pre,on,on_pst,place,ui")
    s.add_argument("--rate", type=float, default=30.0)
    s.set_defaults(run=cmd_make)

    a = p.parse_args()
    a.run(a)


if __name__ == "__main__":
    main()
