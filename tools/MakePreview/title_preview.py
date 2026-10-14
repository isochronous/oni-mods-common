#!/usr/bin/env python3
"""Make a 512x512 Workshop preview: a background image with the mod's title over it and a
frame round the edge, in the style of the Salty Surf, Detect Any Element and Natural
Backwalls previews.

    python title_preview.py <background> <out.png> --title "Detect Any Element"
                            [--style serif|sans] [--font lora|liberation|graystroke|file]
                            [--position top|center|bottom|split|0.0-1.0] [--lines "Detect Any|Element"]
                            [--no-border] [--tone r,g,b] [--fill r,g,b] [--fill2 r,g,b]

The background is cropped to a square from its centre (--focus x,y in 0-1 moves the crop)
and scaled to 512. Colours that are not given come from the background: its average,
darkened, is the frame and the text's outline, so they sit in the picture's own palette.

Styles (the look; --font changes the font and keeps the look):
  serif  Lora Bold in capitals, cream with a dark outline and a soft shadow (Salty Surf,
         Detect Any Element).
  sans   Liberation Sans Bold (the game's own UI font) as typed, white with a black outline,
         a halo in a lighter shade of the picture and a soft shadow (Natural Backwalls).

Fonts: lora and liberation are in fonts/ under open licences (their licence files are
beside them). graystroke is the game's heading and menu-toggle font, free on DaFont
(https://www.dafont.com/graystroke.font) but with no licence text to ship it under, so it is
not in the repository: put GRAYSTROKE REGULAR.otf in fonts/ (git ignores it) or install it in
Windows. Any other .ttf or .otf can be given by path.

The title wraps to two lines at the space nearest its middle unless --lines splits it
("|" between lines). It is sized to fill --width of the image (0.86 by default), smaller
when that would make it taller than the reference previews' lettering. --position split puts
the first line at the top and the second at the bottom. Everything is drawn at twice the size
and scaled down.
Requires Pillow.
"""
import argparse
import os

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont, ImageStat

HERE = os.path.dirname(os.path.abspath(__file__))
FONTS = os.path.join(HERE, "fonts")
SIZE = 512
SCALE = 2  # drawn at 1024 and scaled down, for smooth edges

STYLES = {
    "serif": {"font": "Lora.ttf", "weight": "Bold", "upper": True, "fill": (241, 238, 218), "look": "outline"},
    "sans": {"font": "LiberationSans-Bold.ttf", "upper": False, "fill": (253, 253, 253), "look": "halo"},
}

# --font names: the file in fonts/ (or installed in Windows) and the weight of a variable font.
FONT_NAMES = {
    "lora": ("Lora.ttf", "Bold"),
    "liberation": ("LiberationSans-Bold.ttf", None),
    "graystroke": ("GRAYSTROKE REGULAR.otf", None),
}


def rgb(text):
    return tuple(int(v) for v in text.split(","))


def scaled(color, factor):
    return tuple(max(0, min(255, int(c * factor))) for c in color)


def square(image, focus):
    """The largest square of the image, centred on focus (fractions of the width and height)."""
    w, h = image.size
    side = min(w, h)
    x = int(min(max(focus[0] * w - side / 2, 0), w - side))
    y = int(min(max(focus[1] * h - side / 2, 0), h - side))
    return image.crop((x, y, x + side, y + side))


def font_path(name):
    """The font in fonts/, else installed in Windows (for one that is not in the repository)."""
    if os.path.isabs(name):
        return name
    places = [FONTS, os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts"),
              os.path.join(os.environ.get("LOCALAPPDATA", ""), "Microsoft", "Windows", "Fonts")]
    for place in places:
        if os.path.exists(os.path.join(place, name)):
            return os.path.join(place, name)
    raise SystemExit("font %s not found in %s; see the script's notes on where to get it" % (name, " or ".join(places)))


def load_font(style, size):
    font = ImageFont.truetype(font_path(style["font"]), size)
    if style.get("weight"):
        font.set_variation_by_name(style["weight"])
    return font


def split_title(title, lines):
    if lines:
        return lines.split("|")
    words = title.split()
    if len(words) < 2 or len(title) <= 12:
        return [title]
    # The break nearest the middle, by characters.
    best = min(range(1, len(words)), key=lambda i: abs(len(" ".join(words[:i])) - len(" ".join(words[i:]))))
    return [" ".join(words[:best]), " ".join(words[best:])]


def line_masks(lines, style, width, max_height):
    """Each line of the title as a mask, all at one font size: the size that fills the width
    with the widest line, or less when the lines stacked would be taller than max_height."""
    def extent(font):
        """The lines' common top and bottom from where they are drawn: the font's ascent and
        descent, widened to any ink beyond them (Graystroke's capitals rise above its ascent)."""
        ascent, descent = font.getmetrics()
        boxes = [font.getbbox(line) for line in lines]
        return min(0, min(b[1] for b in boxes)), max(ascent + descent, max(b[3] for b in boxes))

    size = 400
    for _ in range(2):
        font = load_font(style, size)
        boxes = [font.getbbox(line) for line in lines]
        widest = max(b[2] - b[0] for b in boxes)
        top, _ = extent(font)
        line_h = font.getmetrics()[0] - top  # ascent (and ink above it): capitals and ascenders
        total_h = line_h * len(lines) + int(line_h * 0.18) * (len(lines) - 1)
        size = int(size * min(width / widest, max_height / total_h))
    font = load_font(style, size)
    ascent, descent = font.getmetrics()
    top, bottom = extent(font)
    masks = []
    for line in lines:
        box = font.getbbox(line)
        mask = Image.new("L", (box[2] - box[0] + size, bottom - top + size), 0)
        ImageDraw.Draw(mask).text((size // 2 - box[0], size // 2 - top), line, font=font, fill=255)
        # Cropped to the line's own box across, and to the lines' common top and bottom up and
        # down, so lines with and without descenders stack evenly.
        left, _, right, _ = mask.getbbox()
        masks.append(mask.crop((left, size // 2, right, size // 2 + bottom - top)))
    return masks, size, int(ascent * 0.18)


def grow(mask, radius):
    """The mask spread outward by radius pixels, with a round edge."""
    if radius <= 0:
        return mask
    return mask.filter(ImageFilter.GaussianBlur(radius / 2)).point(lambda v: 255 if v > 8 else int(v * 255 / 8))


def compose_text(canvas, mask, at, style, fill, tone, light, font_size):
    """Draws the title's layers onto the canvas, its mask's top-left at `at`."""
    s = mask.height
    pad = s // 2
    big = Image.new("L", (mask.width + 2 * pad, mask.height + 2 * pad), 0)
    big.paste(mask, (pad, pad))
    origin = (at[0] - pad, at[1] - pad)

    def layer(alpha, color, offset=(0, 0)):
        tile = Image.new("RGB", big.size, color)
        canvas.paste(tile, (origin[0] + offset[0], origin[1] + offset[1]), alpha)

    # Outline and shadow widths in preview pixels for a 70 px font (the reference previews'
    # size), scaled with the font: u is one such pixel on the canvas.
    u = font_size / 70  # the font is drawn at SCALE, so a 70 px preview font is 70 * SCALE here
    if style["look"] == "outline":
        outline = grow(big, round(3 * u))
        shadow = grow(big, round(4 * u)).filter(ImageFilter.GaussianBlur(3 * u)).point(lambda v: int(v * 0.85))
        layer(shadow, scaled(tone, 0.55), (round(2 * u), round(3 * u)))
        layer(outline, tone)
    else:
        black = grow(big, round(2 * u))
        halo = grow(big, round(4.5 * u))
        shadow = grow(big, round(6 * u)).filter(ImageFilter.GaussianBlur(3 * u)).point(lambda v: int(v * 0.8))
        layer(shadow, scaled(tone, 0.45), (round(u), round(2 * u)))
        layer(halo, light)
        layer(black, scaled(tone, 0.12))
    layer(big, fill)


def frame(canvas, tone):
    """A band in the dark tone round the edge, then a darker line and a faint lighter one."""
    draw = ImageDraw.Draw(canvas)
    w, h = canvas.size
    rings = [(9 * SCALE, tone), (SCALE, scaled(tone, 0.7)), (SCALE, scaled(tone, 1.45))]
    d = 0
    for width, color in rings:
        for _ in range(width):
            draw.rectangle((d, d, w - 1 - d, h - 1 - d), outline=color)
            d += 1


def main():
    ap = argparse.ArgumentParser(description=__doc__.split("\n\n")[0])
    ap.add_argument("background")
    ap.add_argument("out")
    ap.add_argument("--title", required=True)
    ap.add_argument("--style", choices=sorted(STYLES), default="serif")
    ap.add_argument("--lines", help='the title split into lines with "|", instead of the automatic split')
    ap.add_argument("--position", default="center", help="top, center, bottom (a tenth of the image from that edge), split (first line at the top, second at the bottom), or the text's centre as a fraction of the height")
    ap.add_argument("--width", type=float, default=0.86, help="the text's width as a fraction of the image (default 0.86)")
    ap.add_argument("--focus", default="0.5,0.5", help="the crop's centre in the background, as fractions (default 0.5,0.5)")
    ap.add_argument("--no-border", action="store_true", help="no frame round the edge")
    ap.add_argument("--tone", help="the dark tone r,g,b for the frame and outline (default: from the background)")
    ap.add_argument("--fill", help="the text colour r,g,b (default: the style's)")
    ap.add_argument("--fill2", help="the second line's colour r,g,b (default: as the first)")
    ap.add_argument("--font", help="lora, liberation, graystroke, or a font file, instead of the style's font (the look and case stay)")
    a = ap.parse_args()

    style = STYLES[a.style]
    if a.font in FONT_NAMES:
        style = dict(style, font=FONT_NAMES[a.font][0], weight=FONT_NAMES[a.font][1])
    elif a.font:
        style = dict(style, font=os.path.abspath(a.font), weight=None)
    full = SIZE * SCALE
    canvas = square(Image.open(a.background).convert("RGB"), [float(v) for v in a.focus.split(",")])
    canvas = canvas.resize((full, full), Image.LANCZOS)
    mean = tuple(int(v) for v in ImageStat.Stat(canvas).mean)
    tone = rgb(a.tone) if a.tone else scaled(mean, 0.5)
    light = scaled(mean, 1.6)  # the halo: a lighter shade of the picture
    fill = rgb(a.fill) if a.fill else style["fill"]
    fill2 = rgb(a.fill2) if a.fill2 else fill

    title = a.title.upper() if style["upper"] else a.title
    lines = split_title(title, a.lines.upper() if a.lines and style["upper"] else a.lines)
    split = a.position == "split" and len(lines) > 1
    masks, font_size, gap = line_masks(lines, style, int(full * a.width),
                                       int(full * (0.17 * len(lines) if split else 0.34 if len(lines) > 1 else 0.19)))
    # top and bottom keep the text a tenth of the image from that edge (split: a twenty-fifth,
    # inside the frame); a number places its centre.
    if split:
        margin = int(full * 0.04)
        tops = [margin, full - margin - masks[-1].height]
    else:
        block = sum(m.height for m in masks) + gap * (len(masks) - 1)
        margin = int(full * 0.1)
        if a.position in ("top", "split"):
            y = margin
        elif a.position == "bottom":
            y = full - margin - block
        else:
            y = int(full * (0.5 if a.position == "center" else float(a.position)) - block / 2)
        tops = []
        for m in masks:
            tops.append(y)
            y += m.height + gap
    for i, (m, top) in enumerate(zip(masks, tops)):
        compose_text(canvas, m, ((full - m.width) // 2, top), style, fill2 if i > 0 else fill, tone, light, font_size)
    if not a.no_border:
        frame(canvas, tone)
    canvas.resize((SIZE, SIZE), Image.LANCZOS).save(a.out, optimize=True)
    print("wrote %s (%s, tone %s)" % (a.out, a.style, ",".join(map(str, tone))))


if __name__ == "__main__":
    main()
