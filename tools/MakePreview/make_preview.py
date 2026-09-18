"""Compose a 256x256 Workshop preview from a game sprite plus a caption band.

    python make_preview.py <sprite-or-atlas.png> <out.png> [--crop x,y,w,h] [--auto]
                           [--flip] [--rotate DEG] [--no-shadow] [--tint r,g,b] [--text "..."] [--font-size N] [--pad N]

--auto picks the largest opaque connected component in the atlas (typically the
building's main sprite) instead of an explicit crop. The caption is white bold text on
a translucent black band, the same closed-caption style as the other isochronous mods.
Requires Pillow. Atlases are extracted with UnityPy from sharedassets0.assets (Texture2D
named <kanim>_0) and are Klei art: keep them untracked, commit only the composed preview.
"""
import argparse, sys
from PIL import Image, ImageDraw, ImageFont

CANVAS = 256

def auto_crop(im):
    a = im.getchannel("A")
    w, h = a.size
    px = a.load()
    seen = bytearray(w * h)
    best = None
    for y0 in range(h):
        for x0 in range(w):
            if px[x0, y0] <= 8 or seen[y0 * w + x0]:
                continue
            stack = [(x0, y0)]; seen[y0 * w + x0] = 1
            minx = maxx = x0; miny = maxy = y0; n = 0
            while stack:
                x, y = stack.pop(); n += 1
                minx = min(minx, x); maxx = max(maxx, x); miny = min(miny, y); maxy = max(maxy, y)
                for nx, ny in ((x-1,y),(x+1,y),(x,y-1),(x,y+1),(x-1,y-1),(x+1,y+1),(x-1,y+1),(x+1,y-1)):
                    if 0 <= nx < w and 0 <= ny < h and not seen[ny * w + nx] and px[nx, ny] > 8:
                        seen[ny * w + nx] = 1; stack.append((nx, ny))
            if best is None or n > best[0]:
                best = (n, minx, miny, maxx - minx + 1, maxy - miny + 1)
    return best[1:]

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("src"); ap.add_argument("out")
    ap.add_argument("--crop"); ap.add_argument("--auto", action="store_true")
    ap.add_argument("--flip", action="store_true"); ap.add_argument("--rotate", type=float, default=0)
    ap.add_argument("--text", default=""); ap.add_argument("--font-size", type=int, default=96)
    ap.add_argument("--pad", type=int, default=10)
    ap.add_argument("--font", default="C:/Windows/Fonts/arialbd.ttf")
    ap.add_argument("--no-shadow", action="store_true", help="erase the translucent black drop shadow (bottom rows)")
    ap.add_argument("--tint", help="multiply the sprite by r,g,b (0-255), e.g. the in-game placeholder tint")
    a = ap.parse_args()
    im = Image.open(a.src).convert("RGBA")
    if a.crop:
        x, y, w, h = map(int, a.crop.split(","))
    elif a.auto:
        x, y, w, h = auto_crop(im)
    else:
        x, y, w, h = 0, 0, im.width, im.height
    print(f"crop {x},{y},{w},{h}")
    sprite = im.crop((x, y, x + w, y + h))
    if a.no_shadow:
        px = sprite.load()
        for yy in range(max(0, sprite.height - 24), sprite.height):
            for xx in range(sprite.width):
                r, g, b, al = px[xx, yy]
                if al < 120 and r < 40 and g < 40 and b < 40:
                    px[xx, yy] = (0, 0, 0, 0)
    if a.tint:
        tr, tg, tb = (int(v) / 255 for v in a.tint.split(","))
        r_, g_, b_, a_ = sprite.split()
        sprite = Image.merge("RGBA", (r_.point(lambda v: round(v * tr)), g_.point(lambda v: round(v * tg)), b_.point(lambda v: round(v * tb)), a_))
    if a.flip:
        sprite = sprite.transpose(Image.FLIP_TOP_BOTTOM)
    if a.rotate:
        sprite = sprite.rotate(a.rotate, expand=True, resample=Image.BICUBIC)
    scale = min((CANVAS - 2 * a.pad) / sprite.width, (CANVAS - 2 * a.pad) / sprite.height)
    sprite = sprite.resize((round(sprite.width * scale), round(sprite.height * scale)), Image.LANCZOS)
    canvas = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
    canvas.alpha_composite(sprite, ((CANVAS - sprite.width) // 2, (CANVAS - sprite.height) // 2))
    if a.text:
        font = ImageFont.truetype(a.font, a.font_size)
        overlay = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
        d = ImageDraw.Draw(overlay)
        l, t, r, b = d.textbbox((0, 0), a.text, font=font)
        tw, th = r - l, b - t
        cx, cy = CANVAS / 2, CANVAS / 2
        band = (16, cy - th / 2 - 9, CANVAS - 16, cy + th / 2 + 9)
        d.rounded_rectangle(band, radius=9, fill=(0, 0, 0, 145))
        d.text((cx - tw / 2 - l, cy - th / 2 - t), a.text, font=font, fill=(255, 255, 255, 255))
        canvas.alpha_composite(overlay)
    canvas.save(a.out)
    print(f"wrote {a.out} {canvas.size}")

if __name__ == "__main__":
    main()
