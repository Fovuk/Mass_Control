#!/usr/bin/env python3
"""Fix menu icon meta files and scene sprite references."""

from __future__ import annotations

import math
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ICONS_DIR = ROOT / "Assets" / "UI" / "Icons"
SCENE = ROOT / "Assets" / "Scenes" / "MainMenu.unity"

SETTINGS_GUID = "a1b2c3d4e5f6478990111223344556677"
QUIT_GUID = "b2c3d4e5f6a7488990111223344556678"
SETTINGS_SPRITE_ID = "8877665544332211001"
QUIT_SPRITE_ID = "8877665544332211002"


def draw_gear(draw: ImageDraw.ImageDraw, cx: float, cy: float, outer_r: float, inner_r: float, teeth: int, fill):
    points = []
    for i in range(teeth * 2):
        angle = math.pi * 2 * i / (teeth * 2) - math.pi / 2
        radius = outer_r if i % 2 == 0 else outer_r * 0.76
        points.append((cx + math.cos(angle) * radius, cy + math.sin(angle) * radius))
    draw.polygon(points, fill=fill)
    draw.ellipse((cx - inner_r, cy - inner_r, cx + inner_r, cy + inner_r), fill=(0, 0, 0, 0))
    hole_r = inner_r * 0.45
    draw.ellipse((cx - hole_r, cy - hole_r, cx + hole_r, cy + hole_r), fill=fill)


def draw_power(draw: ImageDraw.ImageDraw, cx: float, cy: float, size: float, fill, width: int):
    top = cy - size * 0.42
    bottom = cy + size * 0.34
    left = cx - size * 0.34
    right = cx + size * 0.34
    draw.arc((left, top, right, bottom), start=135, end=405, fill=fill, width=width)
    draw.line((cx, cy - size * 0.08, cx, cy - size * 0.46), fill=fill, width=width)


def create_icons():
    size = 128

    gear = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    gear_draw = ImageDraw.Draw(gear)
    draw_gear(gear_draw, size / 2, size / 2, size * 0.38, size * 0.2, 8, (55, 189, 247, 255))

    power = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    power_draw = ImageDraw.Draw(power)
    draw_power(power_draw, size / 2, size / 2 + size * 0.03, size * 0.78, (255, 255, 255, 255), max(12, size // 9))

    ICONS_DIR.mkdir(parents=True, exist_ok=True)
    gear.save(ICONS_DIR / "icon_settings_gear.png")
    power.save(ICONS_DIR / "icon_quit_power.png")


def write_sprite_meta(path: Path, guid: str, sprite_name: str, sprite_id: str, width: int, height: int):
    content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable:
  - first:
      213: {sprite_id}
    second: {sprite_name}
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
  spriteMode: 2
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
  - serializedVersion: 4
    buildTarget: Standalone
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
    sprites:
    - serializedVersion: 2
      name: {sprite_name}
      rect:
        serializedVersion: 2
        x: 0
        y: 0
        width: {width}
        height: {height}
      alignment: 0
      pivot: {{x: 0.5, y: 0.5}}
      border: {{x: 0, y: 0, z: 0, w: 0}}
      customData: 
      outline: []
      physicsShape: []
      tessellationDetail: -1
      bones: []
      spriteID: 5e97eb03825dee720800000000000000
      internalID: {sprite_id}
      vertices: []
      indices: 
      edges: []
      weights: []
    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable:
      {sprite_name}: {sprite_id}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    path.write_text(content, encoding="utf-8")


def patch_scene():
    content = SCENE.read_text(encoding="utf-8")
    content = content.replace(
        f"{{fileID: 21300000, guid: {SETTINGS_GUID}, type: 3}}",
        f"{{fileID: {SETTINGS_SPRITE_ID}, guid: {SETTINGS_GUID}, type: 3}}",
    )
    content = content.replace(
        f"{{fileID: 21300000, guid: {QUIT_GUID}, type: 3}}",
        f"{{fileID: {QUIT_SPRITE_ID}, guid: {QUIT_GUID}, type: 3}}",
    )
    SCENE.write_text(content, encoding="utf-8")


def main():
    create_icons()
    write_sprite_meta(
        ICONS_DIR / "icon_settings_gear.png.meta",
        SETTINGS_GUID,
        "icon_settings_gear_0",
        SETTINGS_SPRITE_ID,
        128,
        128,
    )
    write_sprite_meta(
        ICONS_DIR / "icon_quit_power.png.meta",
        QUIT_GUID,
        "icon_quit_power_0",
        QUIT_SPRITE_ID,
        128,
        128,
    )
    patch_scene()
    print("Fixed icon sprites and scene references.")


if __name__ == "__main__":
    main()
