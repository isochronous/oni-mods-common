"""Write a multi-symbol Klei kanim (texture atlas + build + anim) from PIL images.

make_kanim.py covers the one-sprite case from the command line; this module is for buildings
made of several parts or states (a cap that moves, a panel that changes colour). Import it
from a mod's own art script:

    sys.path.insert(0, "<repo>/common/tools/MakeKanim")
    from kanim_writer import Sprite, write_kanim

    write_kanim(out_dir, "my_building",
        symbols={"body": [Sprite(red_img, 224, 214), Sprite(green_img, 224, 214)], "cap": [Sprite(cap_img, 198, 57)]},
        anims={"off": [[("body", 0, 0, -100), ("cap", 0, 0, -230)]], "on": [[("body", 1, 0, -100), ("cap", 0, 0, -230)]]})

A Sprite is an image plus its display size in anim units (a grid cell is 200 x 200; the
texture resolution is independent of the display size). An anim is a list of frames, a frame
a list of elements (symbol, frame index, x, y) ordered FRONT TO BACK, where x, y place the
sprite's centre relative to the bottom centre of the building's origin cell, y growing
downwards. See make_kanim.py for the binary format notes.
"""
import os
import struct
from collections import namedtuple

from PIL import Image

Sprite = namedtuple("Sprite", "image width height")


def sdbm(s):
    h = 0
    for c in s.lower():
        h = (ord(c) + (h << 6) + (h << 16) - h) & 0xffffffff
    return h - (1 << 32) if h >= 1 << 31 else h


def kstr(s):
    b = s.encode("utf-8")
    return struct.pack("<i", len(b)) + b


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


def write_kanim(out_dir, name, symbols, anims, texture_size=512, rate=30.0, pad=2):
    os.makedirs(out_dir, exist_ok=True)
    flat = [(sym, i, sp) for sym, frames in symbols.items() for i, sp in enumerate(frames)]
    atlas, boxes = pack_atlas([sp.image for _, _, sp in flat], texture_size, pad)
    atlas.save(os.path.join(out_dir, name + "_0.png"))
    uv = {(sym, i): tuple(v / texture_size for v in box) for (sym, i, _), box in zip(flat, boxes)}

    build = b"BILD" + struct.pack("<iii", 10, len(symbols), len(flat)) + kstr(name)
    for sym, frames in symbols.items():
        build += struct.pack("<iiiii", sdbm(sym), sdbm(sym), 0, 0, len(frames))
        for i, sp in enumerate(frames):
            build += struct.pack("<iiiffffffff", i, 1, 0, 0.0, 0.0, sp.width, sp.height, *uv[(sym, i)])
    build += struct.pack("<i", len(symbols))
    for sym in symbols:
        build += struct.pack("<i", sdbm(sym)) + kstr(sym)
    open(os.path.join(out_dir, name + "_build.bytes"), "wb").write(build)

    total_frames = sum(len(frames) for frames in anims.values())
    total_elements = sum(len(frame) for frames in anims.values() for frame in frames)
    names = {"building"} | set(symbols) | set(anims)
    root = sdbm("building")
    data = b"ANIM" + struct.pack("<iiii", 5, total_elements, total_frames, len(anims))
    for anim_name, frames in anims.items():
        data += kstr(anim_name) + struct.pack("<ifi", root, rate, len(frames))
        for frame in frames:
            x1 = min(x - symbols[s][i].width / 2 for s, i, x, y in frame)
            x2 = max(x + symbols[s][i].width / 2 for s, i, x, y in frame)
            y1 = min(y - symbols[s][i].height / 2 for s, i, x, y in frame)
            y2 = max(y + symbols[s][i].height / 2 for s, i, x, y in frame)
            data += struct.pack("<ffffi", (x1 + x2) / 2, (y1 + y2) / 2, x2 - x1, y2 - y1, len(frame))
            for s, i, x, y in frame:
                data += struct.pack("<iiii", sdbm(s), i, sdbm(s), 0)
                data += struct.pack("<ffff", 1.0, 1.0, 1.0, 1.0)
                data += struct.pack("<ffffff", 1.0, 0.0, 0.0, 1.0, x, y)
                data += struct.pack("<f", 0.0)
    data += struct.pack("<i", max(len(frame) for frames in anims.values() for frame in frames))
    data += struct.pack("<i", len(names))
    for n in sorted(names):
        data += struct.pack("<i", sdbm(n)) + kstr(n)
    open(os.path.join(out_dir, name + "_anim.bytes"), "wb").write(data)
