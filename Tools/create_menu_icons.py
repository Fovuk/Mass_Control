#!/usr/bin/env python3
"""Generate settings/quit icon textures and patch MainMenu button icons."""

from __future__ import annotations

import math
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ICONS_DIR = ROOT / "Assets" / "UI" / "Icons"
SCENE = ROOT / "Assets" / "Scenes" / "MainMenu.unity"

SETTINGS_GUID = "a1b2c3d4e5f6478990111223344556677"
QUIT_GUID = "b2c3d4e5f6a7488990111223344556678"
GUID_IMAGE = "fe87c0e1cc204ed48ad3b37840f39efc"


def draw_gear(draw: ImageDraw.ImageDraw, cx: float, cy: float, outer_r: float, inner_r: float, teeth: int, fill):
    points = []
    for i in range(teeth * 2):
        angle = math.pi * 2 * i / (teeth * 2) - math.pi / 2
        radius = outer_r if i % 2 == 0 else outer_r * 0.78
        points.append((cx + math.cos(angle) * radius, cy + math.sin(angle) * radius))
    draw.polygon(points, fill=fill)
    draw.ellipse(
        (cx - inner_r, cy - inner_r, cx + inner_r, cy + inner_r),
        fill=(0, 0, 0, 0),
    )
    hole_r = inner_r * 0.42
    draw.ellipse((cx - hole_r, cy - hole_r, cx + hole_r, cy + hole_r), fill=fill)


def draw_power(draw: ImageDraw.ImageDraw, cx: float, cy: float, size: float, fill, width: int):
    top = cy - size * 0.42
    bottom = cy + size * 0.34
    left = cx - size * 0.34
    right = cx + size * 0.34
    draw.arc((left, top, right, bottom), start=135, end=405, fill=fill, width=width)
    draw.line((cx, cy - size * 0.08, cx, cy - size * 0.46), fill=fill, width=width)


def create_icon(path: Path, draw_fn, color: tuple[int, int, int, int], size: int = 128):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw_fn(draw, size, color)
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path)


def draw_settings(draw, size, color):
    cx = cy = size / 2
    draw_gear(draw, cx, cy, outer_r=size * 0.34, inner_r=size * 0.18, teeth=8, fill=color)


def draw_quit(draw, size, color):
    cx = cy = size / 2
    draw_power(draw, cx, cy + size * 0.03, size * 0.72, color, width=max(10, size // 11))


def write_meta(path: Path, guid: str):
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
    filterMode: 1
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


def sprite_ref(guid: str) -> str:
    return f"{{fileID: 21300000, guid: {guid}, type: 3}}"


def image_block(comp_id: str, go_id: str, sprite_guid: str, color: str, raycast: int = 0) -> str:
    return f"""--- !u!114 &{comp_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_IMAGE}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image
  m_Material: {{fileID: 0}}
  m_Color: {color}
  m_RaycastTarget: {raycast}
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {sprite_ref(sprite_guid)}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
"""


def patch_scene():
    content = SCENE.read_text(encoding="utf-8")

    # Square icon buttons, settings left / quit right
    content = content.replace(
        "  m_AnchoredPosition: {x: -290, y: -36}\n  m_SizeDelta: {x: 240, y: 80}",
        "  m_AnchoredPosition: {x: -130, y: -36}\n  m_SizeDelta: {x: 80, y: 80}",
        1,
    )
    content = content.replace(
        "  m_AnchoredPosition: {x: -36, y: -36}\n  m_SizeDelta: {x: 240, y: 80}",
        "  m_AnchoredPosition: {x: -36, y: -36}\n  m_SizeDelta: {x: 80, y: 80}",
        1,
    )

    settings_icon = image_block(
        "3100000113",
        "3100000110",
        SETTINGS_GUID,
        "{r: 0.21568629, g: 0.7411765, b: 0.9686275, a: 1}",
    )
    quit_icon = image_block(
        "3200000113",
        "3200000110",
        QUIT_GUID,
        "{r: 1, g: 1, b: 1, a: 1}",
    )

    # Settings text child -> icon
    content = content.replace("  m_Name: Text (TMP)\n  m_TagString: Untagged\n  m_Icon: {fileID: 0}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n--- !u!224 &3100000111", "  m_Name: Icon\n  m_TagString: Untagged\n  m_Icon: {fileID: 0}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n--- !u!224 &3100000111", 1)
    content = content.replace(
        "  - component: {fileID: 3100000113}\n  - component: {fileID: 9200000008}",
        "  - component: {fileID: 3100000113}",
        1,
    )
    content = content.replace(
        "  m_AnchorMin: {x: 0, y: 0}\n  m_AnchorMax: {x: 1, y: 1}\n  m_AnchoredPosition: {x: 0, y: 4}\n  m_SizeDelta: {x: 0, y: -8}\n  m_Pivot: {x: 0.5, y: 0.5}\n--- !u!222 &3100000112",
        "  m_AnchorMin: {x: 0.5, y: 0.5}\n  m_AnchorMax: {x: 0.5, y: 0.5}\n  m_AnchoredPosition: {x: 0, y: 0}\n  m_SizeDelta: {x: 52, y: 52}\n  m_Pivot: {x: 0.5, y: 0.5}\n--- !u!222 &3100000112",
        1,
    )

    import re

    content = re.sub(
        r"--- !u!114 &3100000113\nMonoBehaviour:[\s\S]*?m_maskOffset: \{x: 0, y: 0, z: 0, w: 0\}\n",
        settings_icon,
        content,
        count=1,
    )
    content = re.sub(
        r"--- !u!114 &9200000008\nMonoBehaviour:[\s\S]*?localizationKey: settings\n",
        "",
        content,
        count=1,
    )

    # Quit text child -> icon
    content = content.replace("--- !u!1 &3200000110\nGameObject:[\s\S]*?m_Name: Text (TMP)", "--- !u!1 &3200000110\nGameObject:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n  m_Component:\n  - component: {fileID: 3200000111}\n  - component: {fileID: 3200000112}\n  - component: {fileID: 3200000113}\n  m_Layer: 5\n  m_Name: Icon", 1)

    # simpler quit replacements
    content = content.replace(
        "  - component: {fileID: 3200000113}\n  - component: {fileID: 3200000114}",
        "  - component: {fileID: 3200000113}",
        1,
    )
    idx = content.find("--- !u!1 &3200000110")
    if idx != -1:
        content = content[:idx] + content[idx:].replace("  m_Name: Text (TMP)", "  m_Name: Icon", 1)

    content = content.replace(
        "  m_Father: {fileID: 3200000101}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: 0, y: 0}\n  m_AnchorMax: {x: 1, y: 1}\n  m_AnchoredPosition: {x: 0, y: 4}\n  m_SizeDelta: {x: 0, y: -8}",
        "  m_Father: {fileID: 3200000101}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: 0.5, y: 0.5}\n  m_AnchorMax: {x: 0.5, y: 0.5}\n  m_AnchoredPosition: {x: 0, y: 0}\n  m_SizeDelta: {x: 52, y: 52}",
        1,
    )

    content = re.sub(
        r"--- !u!114 &3200000113\nMonoBehaviour:[\s\S]*?m_maskOffset: \{x: 0, y: 0, z: 0, w: 0\}\n",
        quit_icon,
        content,
        count=1,
    )
    content = re.sub(
        r"--- !u!114 &3200000114\nMonoBehaviour:[\s\S]*?localizationKey: quit_game\n",
        "",
        content,
        count=1,
    )

    SCENE.write_text(content, encoding="utf-8")


def main():
    settings_png = ICONS_DIR / "icon_settings_gear.png"
    quit_png = ICONS_DIR / "icon_quit_power.png"

    create_icon(settings_png, draw_settings, (55, 189, 247, 255))
    create_icon(quit_png, draw_quit, (255, 255, 255, 255))
    write_meta(settings_png.with_suffix(".png.meta"), SETTINGS_GUID)
    write_meta(quit_png.with_suffix(".png.meta"), QUIT_GUID)
    patch_scene()
    print("Created icons and patched MainMenu buttons.")


if __name__ == "__main__":
    main()
