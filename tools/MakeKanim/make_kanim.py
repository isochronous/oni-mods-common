"""Build a single-sprite Klei kanim (build + anim + texture) from one PNG.

    python make_kanim.py <sprite.png> <out-dir> --name <kanim_name> [--size 220] [--center 0,-100]
                         [--texture-size 512] [--anims off,on_pre,on,on_pst,place,ui] [--rate 30]

Writes <out-dir>/<name>_0.png, <name>_build.bytes and <name>_anim.bytes. Put the folder at
<mod>/anim/assets/<name>/ and reference it in the BuildingDef as "<name>_kanim".

Every animation is one static frame showing the sprite, so a building whose code plays
on/off style anims keeps working. Units are Klei anim pixels: a grid cell is 200 x 200 and
the origin is the bottom centre of the building's cell, y growing downwards, so the cell
centre is (0, -100). "ui" is the build-menu icon; "place" is the construction ghost and is
drawn as the sprite's white outline (see kanim_writer.place_outline), not the sprite itself.

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
import argparse
import os

from PIL import Image

from kanim_writer import Sprite, place_outline, write_kanim


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


if __name__ == "__main__":
    main()
