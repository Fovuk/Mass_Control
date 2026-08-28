#!/usr/bin/env python3
"""Generate consistent directional and warning sign sprites for the platformer."""

from __future__ import annotations

import math
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "Assets" / "Sprites" / "Signs"

SIZE = 192

# Shared palette
BOARD_DARK = (0x35, 0x4A, 0x28, 255)
BOARD_BORDER = (0x2A, 0x3A, 0x20, 255)
BOARD_FACE = (0x5F, 0x82, 0x46, 255)
BOARD_FACE_LIGHT = (0x72, 0x98, 0x56, 255)
BOARD_FACE_SHADOW = (0x4A, 0x66, 0x36, 255)
POST = (0x6B, 0x4A, 0x34, 255)
POST_LIGHT = (0x8A, 0x62, 0x46, 255)
POST_DARK = (0x3F, 0x2A, 0x1E, 255)
RIVET = (0xB8, 0xA8, 0x88, 255)
RIVET_DARK = (0x6A, 0x5A, 0x44, 255)
SYMBOL = (0xFF, 0xFA, 0xE8, 255)
SYMBOL_EDGE = (0x2A, 0x3A, 0x20, 255)
ACCENT = (0xFF, 0xC9, 0x2A, 255)
ACCENT_DARK = (0xC9, 0x8A, 0x12, 255)
DANGER_RED = (0xE5, 0x3A, 0x3A, 255)


def new_canvas() -> tuple[Image.Image, ImageDraw.ImageDraw]:
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img)


def board_layout(size: int) -> tuple[float, float, float, float, float, float]:
    cx = size / 2
    board_w = size * 0.76
    board_h = size * 0.52
    left = cx - board_w / 2
    top = size * 0.12
    right = cx + board_w / 2
    bottom = top + board_h
    inner_pad = size * 0.045
    return (
        left,
        top,
        right,
        bottom,
        left + inner_pad,
        top + inner_pad,
        right - inner_pad,
        bottom - inner_pad,
    )


def draw_board(draw: ImageDraw.ImageDraw, size: int) -> tuple[float, float, float, float]:
    left, top, right, bottom, il, it, ir, ib = board_layout(size)
    cx = size / 2
    radius = int(size * 0.045)

    # Ground shadow
    shadow_w = size * 0.22
    draw.ellipse(
        (cx - shadow_w, bottom + size * 0.14, cx + shadow_w, bottom + size * 0.2),
        fill=(0, 0, 0, 45),
    )

    # Post
    post_w = size * 0.1
    post_h = size * 0.17
    post_left = cx - post_w / 2
    post_top = bottom - size * 0.035
    draw.rounded_rectangle(
        (post_left - 2, post_top, post_left + post_w + 2, post_top + post_h + 2),
        radius=int(post_w * 0.2),
        fill=POST_DARK,
    )
    draw.rounded_rectangle(
        (post_left, post_top, post_left + post_w, post_top + post_h),
        radius=int(post_w * 0.2),
        fill=POST,
    )
    draw.line(
        (post_left + post_w * 0.35, post_top + 2, post_left + post_w * 0.35, post_top + post_h - 2),
        fill=POST_LIGHT,
        width=2,
    )

    # Outer frame
    draw.rounded_rectangle(
        (left - 4, top - 4, right + 4, bottom + 4),
        radius=radius + 2,
        fill=BOARD_BORDER,
    )
    draw.rounded_rectangle(
        (left - 1, top - 1, right + 1, bottom + 1),
        radius=radius + 1,
        fill=BOARD_DARK,
    )

    # Face with simple vertical bands for depth
    draw.rounded_rectangle((left, top, right, bottom), radius=radius, fill=BOARD_FACE)
    band_w = (right - left) / 5
    for i in range(5):
        bx0 = left + i * band_w
        bx1 = bx0 + band_w
        tone = BOARD_FACE_LIGHT if i % 2 == 0 else BOARD_FACE_SHADOW
        draw.rectangle((bx0 + 1, top + 3, bx1 - 1, bottom - 3), fill=tone)

    # Inner inset panel
    draw.rounded_rectangle((il, it, ir, ib), radius=max(2, radius - 3), fill=BOARD_FACE)
    draw.rounded_rectangle(
        (il, it, ir, ib),
        radius=max(2, radius - 3),
        outline=BOARD_DARK,
        width=2,
    )

    # Top highlight
    draw.line((il + 4, it + 3, ir - 4, it + 3), fill=BOARD_FACE_LIGHT, width=2)

    # Corner rivets
    rivet_r = size * 0.013
    for rx, ry in ((il + 6, it + 6), (ir - 6, it + 6), (il + 6, ib - 6), (ir - 6, ib - 6)):
        draw.ellipse((rx - rivet_r - 1, ry - rivet_r - 1, rx + rivet_r + 1, ry + rivet_r + 1), fill=RIVET_DARK)
        draw.ellipse((rx - rivet_r, ry - rivet_r, rx + rivet_r, ry + rivet_r), fill=RIVET)

    return il, it, ir, ib


def board_center(inner: tuple[float, float, float, float]) -> tuple[float, float]:
    il, it, ir, ib = inner
    return (il + ir) / 2, (it + ib) / 2


def rotate_point(x: float, y: float, cx: float, cy: float, angle_deg: float) -> tuple[float, float]:
    rad = math.radians(angle_deg)
    dx, dy = x - cx, y - cy
    cos_a, sin_a = math.cos(rad), math.sin(rad)
    return cx + dx * cos_a - dy * sin_a, cy + dx * sin_a + dy * cos_a


def transform_points(points: list[tuple[float, float]], cx: float, cy: float, angle_deg: float) -> list[tuple[float, float]]:
    return [rotate_point(x, y, cx, cy, angle_deg) for x, y in points]


def draw_arrow_in_board(draw: ImageDraw.ImageDraw, inner: tuple[float, float, float, float], angle_deg: float) -> None:
    il, it, ir, ib = inner
    cx, cy = board_center(inner)
    span = min(ir - il, ib - it)

    # Symmetric arrow pointing up, centered on board panel.
    h = span * 0.34
    w_head = span * 0.26
    w_shaft = span * 0.11

    tip = (cx, cy - h)
    head_l = (cx - w_head, cy + h * 0.05)
    head_r = (cx + w_head, cy + h * 0.05)
    shaft_tl = (cx - w_shaft, cy + h * 0.05)
    shaft_tr = (cx + w_shaft, cy + h * 0.05)
    shaft_bl = (cx - w_shaft, cy + h)
    shaft_br = (cx + w_shaft, cy + h)

    head = transform_points([tip, head_l, head_r], cx, cy, angle_deg)
    shaft = transform_points([shaft_tl, shaft_tr, shaft_br, shaft_bl], cx, cy, angle_deg)

    # Outline pass
    for poly in (shaft, head):
        expanded = [(x + 1.2, y + 1.4) for x, y in poly]
        draw.polygon(expanded, fill=SYMBOL_EDGE)
    draw.polygon(shaft, fill=SYMBOL)
    draw.polygon(head, fill=SYMBOL)


def make_arrow(name: str, angle: float) -> None:
    img, draw = new_canvas()
    inner = draw_board(draw, SIZE)
    draw_arrow_in_board(draw, inner, angle)
    img.save(OUT_DIR / f"{name}.png")


def load_font(size: int) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    candidates = [
        "C:/Windows/Fonts/segoeuib.ttf",
        "C:/Windows/Fonts/arialbd.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
        "/System/Library/Fonts/Supplemental/Arial Bold.ttf",
    ]
    for path in candidates:
        if Path(path).exists():
            return ImageFont.truetype(path, size)
    return ImageFont.load_default()


def fit_font(draw: ImageDraw.ImageDraw, text: str, max_w: float, start_size: int) -> ImageFont.ImageFont:
    size = start_size
    while size > 8:
        font = load_font(size)
        bbox = draw.textbbox((0, 0), text, font=font)
        if bbox[2] - bbox[0] <= max_w:
            return font
        size -= 1
    return load_font(8)


def draw_centered_text(
    draw: ImageDraw.ImageDraw,
    text: str,
    inner: tuple[float, float, float, float],
    font: ImageFont.ImageFont,
    fill,
) -> None:
    il, it, ir, ib = inner
    cx, cy = board_center(inner)
    bbox = draw.textbbox((0, 0), text, font=font)
    tw = bbox[2] - bbox[0]
    th = bbox[3] - bbox[1]
    x = cx - tw / 2 - bbox[0]
    y = cy - th / 2 - bbox[1]
    draw.text((x + 1.2, y + 1.4), text, font=font, fill=SYMBOL_EDGE)
    draw.text((x, y), text, font=font, fill=fill)


def make_text_sign(filename: str, text: str) -> None:
    img, draw = new_canvas()
    inner = draw_board(draw, SIZE)
    il, it, ir, ib = inner
    max_w = (ir - il) * 0.82
    font = fit_font(draw, text, max_w, start_size=16)
    draw_centered_text(draw, text, inner, font, SYMBOL)
    img.save(OUT_DIR / f"{filename}.png")


def make_warning_exclamation() -> None:
    img, draw = new_canvas()
    inner = draw_board(draw, SIZE)
    cx, cy = board_center(inner)
    span = min(inner[2] - inner[0], inner[3] - inner[1])

    tri_h = span * 0.34
    tri_w = span * 0.36
    tri = [(cx, cy - tri_h * 0.55), (cx - tri_w, cy + tri_h * 0.45), (cx + tri_w, cy + tri_h * 0.45)]

    draw.polygon([(x + 1, y + 1.5) for x, y in tri], fill=SYMBOL_EDGE)
    draw.polygon(tri, fill=ACCENT, outline=ACCENT_DARK)

    bar_w = span * 0.055
    bar_top = cy - tri_h * 0.18
    bar_bottom = cy + tri_h * 0.05
    draw.rounded_rectangle(
        (cx - bar_w, bar_top, cx + bar_w, bar_bottom),
        radius=int(bar_w * 0.4),
        fill=SYMBOL_EDGE,
    )
    dot_r = span * 0.045
    dot_cy = cy + tri_h * 0.22
    draw.ellipse((cx - dot_r - 1, dot_cy - dot_r, cx + dot_r + 1, dot_cy + dot_r + 1), fill=SYMBOL_EDGE)
    draw.ellipse((cx - dot_r, dot_cy - dot_r, cx + dot_r, dot_cy + dot_r), fill=SYMBOL_EDGE)
    img.save(OUT_DIR / "sign_warning.png")


def make_no_entry() -> None:
    img, draw = new_canvas()
    inner = draw_board(draw, SIZE)
    cx, cy = board_center(inner)
    span = min(inner[2] - inner[0], inner[3] - inner[1])
    r = span * 0.3

    draw.ellipse((cx - r - 2, cy - r - 1, cx + r + 2, cy + r + 3), fill=SYMBOL_EDGE)
    draw.ellipse((cx - r, cy - r, cx + r, cy + r), fill=DANGER_RED, outline=SYMBOL)
    draw.line((cx - r * 0.62, cy + r * 0.62, cx + r * 0.62, cy - r * 0.62), fill=SYMBOL, width=max(4, int(span * 0.07)))
    img.save(OUT_DIR / "sign_no_entry.png")


def write_meta(path: Path) -> None:
    import uuid

    guid = uuid.uuid4().hex
    meta = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 256
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 256
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    path.write_text(meta, encoding="utf-8")


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)

    arrows = {
        "sign_arrow_right": 90,
        "sign_arrow_left": -90,
        "sign_arrow_up": 0,
        "sign_arrow_down": 180,
        "sign_arrow_up_right": 45,
        "sign_arrow_up_left": -45,
        "sign_arrow_down_right": 135,
        "sign_arrow_down_left": -135,
    }
    for name, angle in arrows.items():
        make_arrow(name, angle)

    make_warning_exclamation()
    make_text_sign("sign_caution", "CAUTION")
    make_text_sign("sign_danger", "DANGER")
    make_text_sign("sign_slow", "SLOW")
    make_text_sign("sign_stop", "STOP")
    make_text_sign("sign_info", "INFO")
    make_no_entry()

    legacy = OUT_DIR / "sign_dikkat.png"
    if legacy.exists():
        legacy.unlink()
        legacy_meta = OUT_DIR / "sign_dikkat.png.meta"
        if legacy_meta.exists():
            legacy_meta.unlink()

    caution_meta = OUT_DIR / "sign_caution.png.meta"
    if not caution_meta.exists():
        write_meta(caution_meta)

    print(f"Regenerated {len(list(OUT_DIR.glob('*.png')))} sign sprites in {OUT_DIR.relative_to(ROOT)}")
    sync_sign_tile_colliders()


def sync_sign_tile_colliders() -> None:
    """Ensure Unity tile assets for signs never generate Tilemap colliders."""
    tile_dir = (
        ROOT
        / "Assets"
        / "Free Platform Game Assets"
        / "Platform Game Assets"
        / "Environment"
        / "png"
        / "2048x2048"
    )
    if not tile_dir.exists():
        return

    updated = 0
    for asset in sorted(tile_dir.glob("sign_*.asset")):
        text = asset.read_text(encoding="utf-8")
        if "m_ColliderType: 1" in text:
            asset.write_text(text.replace("m_ColliderType: 1", "m_ColliderType: 0", 1), encoding="utf-8")
            updated += 1
    if updated:
        print(f"Set Collider Type=None on {updated} sign tile assets")


if __name__ == "__main__":
    main()
