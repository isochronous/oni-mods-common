"""Build a single-sprite Klei kanim (build + anim + texture) from one PNG.

    python make_kanim.py <sprite.png> <out-dir> --name <kanim_name> [--size 220] [--center 0,-100]
                         [--texture-size 512] [--anims off,on_pre,on,on_pst,place,ui] [--rate 30]

Writes <out-dir>/<name>_0.png, <name>_build.bytes and <name>_anim.bytes. Put the folder at
<mod>/anim/assets/<name>/ and reference it in the BuildingDef as "<name>_kanim".

Every animation is one static frame showing the sprite, so a building whose code plays
on/off style anims keeps working. Units are Klei anim pixels: a grid cell is 200 x 200 and
the origin is the bottom centre of the building's cell, y growing downwards, so the cell
centre is (0, -100). "place" is the construction ghost, "ui" the build-menu icon.

Format notes (verified against the vanilla critter_sensor files, BILD v10 / ANIM v5):
  BILD: magic, version, symbolCount, frameCount, name; per symbol: hash, pathHash, colour,
        flags, frameCount; per frame: sourceFrame, duration, imageIndex, pivotX, pivotY,
        pivotW, pivotH, u1, v1, u2, v2; then hash table (count; hash, string).
  ANIM: magic, version, totalElements, totalFrames, animCount; per anim: name, rootHash,
        rate, frameCount; per frame: bboxX, bboxY, bboxW, bboxH, elementCount; per element:
        symbolHash, frame, folderHash, flags, a, b, g, r, m00, m01, m10, m11, tx, ty, order;
        then maxVisibleSymbolFrames and the hash table.
  Hashes are Klei's SDBM-lower. Pivot (0,0) is the sprite centre.
"""
import argparse, os, struct
from PIL import Image

def sdbm(s):
    h = 0
    for c in s.lower():
        h = (ord(c) + (h << 6) + (h << 16) - h) & 0xffffffff
    return h - (1 << 32) if h >= 1 << 31 else h

def kstr(s):
    b = s.encode("utf-8")
    return struct.pack("<i", len(b)) + b

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("sprite"); ap.add_argument("out")
    ap.add_argument("--name", required=True)
    ap.add_argument("--size", type=float, default=220, help="display size of the longer side, anim px (cell = 200)")
    ap.add_argument("--center", default="0,-100")
    ap.add_argument("--texture-size", type=int, default=512)
    ap.add_argument("--anims", default="off,on_pre,on,on_pst,place,ui")
    ap.add_argument("--rate", type=float, default=30.0)
    a = ap.parse_args()
    os.makedirs(a.out, exist_ok=True)

    im = Image.open(a.sprite).convert("RGBA")
    im = im.crop(im.getbbox())
    ts = a.texture_size
    scale = min(ts / im.width, ts / im.height)
    tex_sprite = im.resize((round(im.width * scale), round(im.height * scale)), Image.LANCZOS)
    tex = Image.new("RGBA", (ts, ts), (0, 0, 0, 0))
    tex.paste(tex_sprite, (0, 0))
    tex.save(os.path.join(a.out, f"{a.name}_0.png"))
    u2, v2 = tex_sprite.width / ts, tex_sprite.height / ts

    disp = min(a.size / im.width, a.size / im.height)
    w, h = im.width * disp, im.height * disp
    cx, cy = (float(v) for v in a.center.split(","))
    anims = [s for s in a.anims.split(",") if s]

    # Symbols: one per anim name that needs its own (place, ui) plus the body used by the rest.
    symbols = ["body"] + [n for n in ("place", "ui") if n in anims]
    build = b"BILD" + struct.pack("<iii", 10, len(symbols), len(symbols)) + kstr(a.name)
    for sym in symbols:
        build += struct.pack("<iiiii", sdbm(sym), sdbm(sym), 0, 0, 1)
        build += struct.pack("<iiiffffffff", 0, 1, 0, 0.0, 0.0, w, h, 0.0, 0.0, u2, v2)
    build += struct.pack("<i", len(symbols))
    for sym in symbols:
        build += struct.pack("<i", sdbm(sym)) + kstr(sym)
    open(os.path.join(a.out, f"{a.name}_build.bytes"), "wb").write(build)

    root = sdbm("building")
    anim = b"ANIM" + struct.pack("<iiii", 5, len(anims), len(anims), len(anims))
    names = {"building"}
    for n in anims:
        sym = n if n in ("place", "ui") else "body"
        names.add(sym); names.add(n)
        anim += kstr(n) + struct.pack("<ifi", root, a.rate, 1)
        anim += struct.pack("<ffffi", cx, cy, w, h, 1)
        anim += struct.pack("<iiii", sdbm(sym), 0, sdbm(sym), 0)
        anim += struct.pack("<ffff", 1.0, 1.0, 1.0, 1.0)
        anim += struct.pack("<ffffff", 1.0, 0.0, 0.0, 1.0, cx, cy)
        anim += struct.pack("<f", 0.0)
    anim += struct.pack("<i", 1)
    anim += struct.pack("<i", len(names))
    for n in sorted(names):
        anim += struct.pack("<i", sdbm(n)) + kstr(n)
    open(os.path.join(a.out, f"{a.name}_anim.bytes"), "wb").write(anim)
    print(f"wrote {a.name}: texture {ts}x{ts} (sprite {tex_sprite.size}), display {w:.0f}x{h:.0f} at ({cx},{cy}), anims {anims}")

if __name__ == "__main__":
    main()
