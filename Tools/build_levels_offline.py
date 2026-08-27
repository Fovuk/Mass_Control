#!/usr/bin/env python3
"""Generate tilemap data and patch Level 4-6 scenes without Unity batch mode."""

from __future__ import annotations

import re
import shutil
from dataclasses import dataclass, field
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCENES = ROOT / "Assets" / "Scenes"
TEMPLATE = SCENES / "Level_03.unity"

GROUND_CELL_Y = 3
TILE_ENTRY = """  - first: {{x: {x}, y: {y}, z: 0}}
    second:
      serializedVersion: 2
      m_TileIndex: {idx}
      m_TileSpriteIndex: {idx}
      m_TileMatrixIndex: 0
      m_TileColorIndex: 0
      m_TileObjectToInstantiateIndex: 65535
      dummyAlignment: 0
      m_AllTileFlags: 1073741825"""


@dataclass
class MovingSpike:
    x: float
    y: float
    ax: float
    ay: float
    bx: float
    by: float
    speed: float


@dataclass
class LevelDef:
    name: str
    map: list[str]
    player: tuple[float, float]
    finish: tuple[float, float]
    stars: list[tuple[float, float]]
    pushable: tuple[float, float] | None
    static_spikes: list[tuple[float, float]] = field(default_factory=list)
    moving_spikes: list[MovingSpike] = field(default_factory=list)


def pad(width: int, *rows: str) -> list[str]:
    out = []
    for row in rows:
        out.append(row[:width].ljust(width))
    return out


def level4() -> LevelDef:
    w = 130
    return LevelDef(
        name="Spike Alley",
        map=pad(
            w,
            "                                                                                                                                  ",
            "                                                              *                                                                   ",
            "                                                        ##################                                                        ",
            "                                                  ##########################                                                      ",
            "                                            ##########################                                                            ",
            "                                      ##########################                                                                  ",
            "                                ##########################                                                                        ",
            "                          ##########################                                                                              ",
            "                    ##########################        ##########################                                                  ",
            "              ##########################                      ##########################                                        ",
            "        ##########################                                      ##########################                              ",
            "  ##########################                                                  ##########################                        ",
            "##################    ##########    ##########    ##########    ##########    ##########    ##########    ##########              ",
            "##########    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##          ",
            "##########          ##          ##          ##          ##          ##          ##          ##          ##          *            ",
            "##########    ########    ########    ########    ########    ########    ########    ########    ########    ########            ",
            "#######          ##########          ##########          ##########          ##########          ##########          ##########  ",
            "##################################################################################################################################",
        ),
        player=(4, 28),
        finish=(122, 28),
        stars=[(22, 36), (58, 44), (108, 40)],
        pushable=(44, 36),
        static_spikes=[(18, 28), (72, 28), (98, 28)],
        moving_spikes=[
            MovingSpike(32, 28, 30, 28, 38, 28, 10),
            MovingSpike(52, 28, 48, 32, 56, 32, 11),
            MovingSpike(82, 28, 78, 28, 86, 28, 12),
            MovingSpike(105, 28, 102, 28, 110, 28, 11),
        ],
    )


def level5() -> LevelDef:
    w = 165
    return LevelDef(
        name="Heavy Lifting",
        map=pad(
            w,
            "                                                                                                                                                                     ",
            "                                                                                *                                                                                    ",
            "                                                                          ##################                                                                       ",
            "                                                                    ##########################                                                                     ",
            "                                                              ##########################                                                                           ",
            "                                                        ##########################                                                                                 ",
            "                                                  ##########################                                                                                       ",
            "                                            ##########################                                                                                             ",
            "                                      ##########################                                                                                                   ",
            "                                ##########################                                                                                                         ",
            "                          ##########################        ##########################                                                                               ",
            "                    ##########################                      ##########################                                                                     ",
            "              ##########################                                    ##########################                                                               ",
            "        ##########################                                                ##########################                                                     ",
            "  ##########################                                                            ##########################                                               ",
            "##################              ##################              ##################              ##################              ##################                ",
            "################    ########    ########    ########    ########    ########    ########    ########    ########    ########    ################                ",
            "##############          ##              ##              ##              ##              ##              ##              ##              *                       ",
            "##############    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########                         ",
            "##########    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##                ",
            "########          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##                    ",
            "######          ##########          ##########          ##########          ##########          ##########          ##########          ##########              ",
            "#################################################################################################################################################################",
        ),
        player=(4, 28),
        finish=(156, 28),
        stars=[(28, 38), (72, 50), (130, 42)],
        pushable=(58, 40),
        static_spikes=[(24, 28), (48, 28), (88, 28), (115, 28)],
        moving_spikes=[
            MovingSpike(38, 28, 35, 28, 43, 28, 11),
            MovingSpike(62, 28, 58, 32, 66, 32, 12),
            MovingSpike(95, 28, 92, 28, 100, 28, 12),
            MovingSpike(118, 28, 114, 28, 122, 28, 13),
            MovingSpike(140, 32, 136, 32, 144, 32, 12),
        ],
    )


def level6() -> LevelDef:
    w = 205
    return LevelDef(
        name="Grand Finale",
        map=pad(
            w,
            "                                                                                                                                                                                                             ",
            "                                                      *                                                     *                                                     *                                      ",
            "                                                ##################                                   ##################                                   ##################                            ",
            "                                          ##########################                             ##########################                             ##########################                      ",
            "                                    ##########################                           ##########################                           ##########################                                ",
            "                              ##########################                         ##########################                         ##########################                                          ",
            "                        ##########################                       ##########################                       ##########################                                                    ",
            "                  ##########################                     ##########################                     ##########################                                                              ",
            "            ##########################                   ##########################                   ##########################                                                                    ",
            "      ##########################                 ##########################                 ##########################                                                                                  ",
            "##########################             ##########################             ##########################             ##########################                                                    ",
            "################    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########              ",
            "##############          ##              ##              ##              ##              ##              ##              ##              ##              ##              ##              ##            ",
            "##############    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########                  ",
            "##########    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##    ##            ",
            "########          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##          ##        ",
            "######    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########    ########            ",
            "####          ##########          ##########          ##########          ##########          ##########          ##########          ##########          ##########          ##########          ",
            "#####################################################################################################################################################################################################",
        ),
        player=(4, 28),
        finish=(196, 28),
        stars=[(32, 44), (98, 52), (168, 46)],
        pushable=(62, 40),
        static_spikes=[(20, 28), (45, 28), (78, 28), (112, 28), (148, 28), (178, 28)],
        moving_spikes=[
            MovingSpike(30, 28, 27, 28, 35, 28, 12),
            MovingSpike(55, 28, 52, 32, 60, 32, 13),
            MovingSpike(85, 28, 82, 28, 90, 28, 13),
            MovingSpike(105, 28, 102, 28, 110, 28, 14),
            MovingSpike(130, 32, 126, 32, 136, 32, 13),
            MovingSpike(160, 28, 156, 28, 166, 28, 14),
        ],
    )


def map_to_cells(level: LevelDef) -> list[tuple[int, int, int]]:
    cells: list[tuple[int, int, int]] = []
    rows = level.map
    for row_i, row in enumerate(rows):
        cell_y = GROUND_CELL_Y + (len(rows) - 1 - row_i)
        for col, ch in enumerate(row):
            if ch == "#":
                cells.append((col, cell_y, 0))
            elif ch == "=":
                cells.append((col, cell_y, 1))
    return cells


def tile_footer(count: int, min_x: int, min_y: int, width: int, height: int) -> str:
    return f"""  m_AnimatedTiles: {{}}
  m_TileAssetArray:
  - serializedVersion: 2
    m_RefCount: {count}
    m_Data: {{fileID: 11400000, guid: 88e8f32211282bb4fa11e9f8a9b2af9f, type: 2}}
  m_TileSpriteArray:
  - serializedVersion: 2
    m_RefCount: {count}
    m_Data: {{fileID: -1347532211411968617, guid: 7699c54989f3f2944925baaa284cce24, type: 3}}
  m_TileMatrixArray:
  - serializedVersion: 2
    m_RefCount: {count}
    m_Data:
      e00: 1
      e01: 0
      e02: 0
      e03: 0
      e10: 0
      e11: 1
      e12: 0
      e13: 0
      e20: 0
      e21: 0
      e22: 1
      e23: 0
      e30: 0
      e31: 0
      e32: 0
      e33: 1
  m_TileColorArray:
  - serializedVersion: 2
    m_RefCount: {count}
    m_Data: {{r: 1, g: 1, b: 1, a: 1}}
  m_TileObjectToInstantiateArray: []
  m_AnimationFrameRate: 1
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_Origin: {{x: {min_x}, y: {min_y}, z: 0}}
  m_Size: {{x: {width}, y: {height}, z: 1}}
  m_TileAnchor: {{x: 0.5, y: 0.5, z: 0}}
  m_TileOrientation: 0
  m_TileOrientationMatrix:
    e00: 1
    e01: 0
    e02: 0
    e03: 0
    e10: 0
    e11: 1
    e12: 0
    e13: 0
    e20: 0
    e21: 0
    e22: 1
    e23: 0
    e30: 0
    e31: 0
    e32: 0
    e33: 1"""


def build_tilemap_yaml(cells: list[tuple[int, int, int]]) -> tuple[str, int, int, int, int, int]:
    if not cells:
        raise ValueError("No tiles in map")

    xs = [c[0] for c in cells]
    ys = [c[1] for c in cells]
    min_x, max_x = min(xs), max(xs)
    min_y, max_y = min(ys), max(ys)
    width = max_x - min_x + 1
    height = max_y - min_y + 1

    entries = [TILE_ENTRY.format(x=x, y=y, idx=idx) for x, y, idx in cells]
    footer = tile_footer(len(cells), min_x, min_y, width, height)
    body = "  m_Tiles:\n" + "\n".join(entries) + "\n" + footer
    return body, len(cells), min_x, min_y, width, height


STRIP_OBJECT_NAMES = {
    "Grid",
    "Grid (1)",
    "Spikes",
    "Stars",
    "Moveable_Object",
    "FinishPortal",
    "Ground",
    "Spike",
    "Spike (1)",
    "Star",
    "Star (1)",
    "Star (2)",
    "Star (3)",
    "Tilemap",
}


def should_strip(name: str) -> bool:
    if name in STRIP_OBJECT_NAMES:
        return True
    return name.startswith(("Moving_Spike", "Triangle", "PointA", "PointB"))


def extract_block_id(block: str) -> int | None:
    m = re.match(r"--- !u!\d+ &(\d+)", block)
    return int(m.group(1)) if m else None


def extract_name_from_block(block: str) -> str:
    for line in block.splitlines():
        if line.strip().startswith("m_Name:"):
            return line.split("m_Name:")[1].strip()
    return ""


def extract_fileid_refs(block: str) -> set[int]:
    return {int(x) for x in re.findall(r"\{fileID: (\d+)\}", block)}


def strip_gameplay_blocks(text: str) -> str:
    lines = text.splitlines(keepends=True)
    blocks: list[str] = []
    i = 0
    while i < len(lines):
        if lines[i].startswith("--- !u!"):
            start = i
            i += 1
            while i < len(lines) and not lines[i].startswith("--- !u!"):
                i += 1
            blocks.append("".join(lines[start:i]))
        else:
            i += 1

    stripped_go_ids: set[int] = set()
    stripped_block_ids: set[int] = set()

    for block in blocks:
        if not block.startswith("--- !u!1 &"):
            continue
        name = extract_name_from_block(block)
        if not should_strip(name):
            continue
        go_id = extract_block_id(block)
        if go_id is None:
            continue
        stripped_go_ids.add(go_id)
        stripped_block_ids.add(go_id)
        stripped_block_ids.update(extract_fileid_refs(block))

    for block in blocks:
        block_id = extract_block_id(block)
        if block_id is None or block_id in stripped_block_ids:
            continue
        go_ref = re.search(r"m_GameObject: \{fileID: (\d+)\}", block)
        if go_ref and int(go_ref.group(1)) in stripped_go_ids:
            stripped_block_ids.add(block_id)
            stripped_block_ids.update(extract_fileid_refs(block))

    kept = []
    for block in blocks:
        block_id = extract_block_id(block)
        if block_id is not None and block_id in stripped_block_ids:
            continue
        kept.append(block)
    return "".join(kept)


def split_blocks(text: str) -> list[str]:
    lines = text.splitlines(keepends=True)
    blocks: list[str] = []
    i = 0
    while i < len(lines):
        if lines[i].startswith("--- !u!"):
            start = i
            i += 1
            while i < len(lines) and not lines[i].startswith("--- !u!"):
                i += 1
            blocks.append("".join(lines[start:i]))
        else:
            i += 1
    return blocks


def rebuild_scene_roots(text: str) -> str:
    root_ids: list[int] = []
    for block in split_blocks(text):
        if not (block.startswith("--- !u!4 &") or block.startswith("--- !u!224 &")):
            continue
        if re.search(r"m_Father: \{fileID: 0\}", block):
            block_id = extract_block_id(block)
            if block_id is not None:
                root_ids.append(block_id)

    roots_body = "\n".join(f"  - {{fileID: {rid}}}" for rid in root_ids)
    replacement = (
        "--- !u!1660057539 &9223372036854775807\n"
        "SceneRoots:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_Roots:\n"
        f"{roots_body}\n"
    )
    return re.sub(
        r"--- !u!1660057539 &9223372036854775807\nSceneRoots:.*",
        replacement.rstrip(),
        text,
        flags=re.DOTALL,
    )


def replace_tilemap(text: str, tile_body: str) -> str:
    pattern = re.compile(
        r"(--- !u!1839735485 &\d+\nTilemap:.*?  m_Tiles:\n)(.*?)(  m_AnimatedTiles: \{\})",
        re.DOTALL,
    )
    match = pattern.search(text)
    if not match:
        raise RuntimeError("Tilemap block not found")
    # tile_body already includes m_Tiles header content through footer ending before next --- 
    new_block = match.group(1) + tile_body.split("  m_Tiles:\n", 1)[1]
    # rebuild properly
    full = match.group(1) + tile_body.split("  m_Tiles:\n", 1)[1]
    return text[: match.start()] + full + text[match.end() :]


def set_player_position(text: str, x: float, y: float) -> str:
    marker = "m_Name: Player\n  m_TagString: Player"
    idx = text.find(marker)
    if idx == -1:
        raise RuntimeError("Player object not found")

    z0 = text.find("m_LocalPosition:", idx)
    if z0 == -1:
        raise RuntimeError("Player transform not found")
    text = text[:z0] + f"m_LocalPosition: {{x: {x}, y: {y}, z: 0}}" + text[text.find("}", z0) + 1 :]

    z1 = text.find("m_LocalPosition:", z0 + 1)
    if z1 != -1 and text.find("m_TagString: Player", z0, z1) == -1:
        # Cinemachine follow target uses z:-10 on duplicate transform in template
        pass

    # Update all Player-tagged transform positions (player body + camera target)
    parts = text.split(marker, 1)
    tail = parts[1]
    tail = re.sub(
        r"m_LocalPosition: \{x: [^,]+, y: [^,]+, z: (0|-10)\}",
        lambda m: f"m_LocalPosition: {{x: {x}, y: {y}, z: {m.group(1)}}}",
        tail,
        count=2,
    )
    return parts[0] + marker + tail


def set_camera_position(text: str, x: float, y: float) -> str:
    marker = "m_Name: CinemachineCamera"
    idx = text.find(marker)
    if idx == -1:
        return text
    pos_idx = text.find("m_LocalPosition:", idx)
    if pos_idx == -1:
        return text
    end = text.find("}", pos_idx) + 1
    return text[:pos_idx] + f"m_LocalPosition: {{x: {x}, y: {y}, z: -10}}" + text[end:]


def patch_level(level_num: int, level: LevelDef) -> None:
    target = SCENES / f"Level_{level_num:02d}.unity"
    shutil.copy2(TEMPLATE, target)
    text = target.read_text(encoding="utf-8")
    text = strip_gameplay_blocks(text)

    cells = map_to_cells(level)
    tile_body, tile_count, _, _, _, _ = build_tilemap_yaml(cells)
    grid_yaml = generate_grid_yaml(tile_body)
    gameplay = generate_gameplay_yaml(level)

    text = text.replace(
        "--- !u!1660057539 &9223372036854775807\nSceneRoots:",
        grid_yaml + "\n" + gameplay + "\n--- !u!1660057539 &9223372036854775807\nSceneRoots:",
    )

    text = set_player_position(text, *level.player)
    text = set_camera_position(text, *level.player)
    text = rebuild_scene_roots(text)

    target.write_text(text, encoding="utf-8")
    print(f"Level {level_num:02d} ({level.name}): {tile_count} tiles, finish x={level.finish[0]}")


def generate_grid_yaml(tile_body: str) -> str:
    return f"""--- !u!1 &900001000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 900001001}}
  - component: {{fileID: 900001002}}
  m_Layer: 3
  m_Name: Grid
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &900001001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 900001000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 9.6, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: 900001011}}
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!156049354 &900001002
Grid:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 900001000}}
  m_Enabled: 1
  m_CellSize: {{x: 1, y: 1, z: 0}}
  m_CellGap: {{x: 0, y: 0, z: 0}}
  m_CellLayout: 0
  m_CellSwizzle: 0
--- !u!1 &900001010
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 900001011}}
  - component: {{fileID: 900001014}}
  - component: {{fileID: 900001013}}
  - component: {{fileID: 900001012}}
  m_Layer: 3
  m_Name: Tilemap
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &900001011
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 900001010}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 14.5, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 900001001}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!19719996 &900001012
TilemapCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 900001010}}
  m_Enabled: 1
  serializedVersion: 3
  m_Density: 1
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_ForceSendLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ForceReceiveLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ContactCaptureLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_CallbackLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_IsTrigger: 0
  m_UsedByEffector: 0
  m_CompositeOperation: 0
  m_CompositeOrder: 0
  m_Offset: {{x: 0, y: 0}}
  m_MaximumTileChangeCount: 1000
  m_ExtrusionFactor: 0
  m_UseDelaunayMesh: 0
--- !u!483693784 &900001013
TilemapRenderer:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 900001010}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 0
  m_ReflectionProbeUsage: 0
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 1
  m_SmallMeshCulling: 1
  m_ForceMeshLod: -1
  m_MeshLodSelectionBias: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 2100000, guid: a97c105638bdf8b4a8650670310a4cd3, type: 2}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_GlobalIlluminationMeshLod: 0
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 0
  m_MaskInteraction: 0
  m_ChunkSize: {{x: 32, y: 32, z: 32}}
  m_ChunkCullingBounds: {{x: 0, y: 0, z: 0}}
  m_MaxChunkCount: 16
  m_MaxFrameAge: 16
  m_SortOrder: 0
  m_Mode: 0
  m_DetectChunkCullingBounds: 0
--- !u!1839735485 &900001014
Tilemap:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 900001010}}
  m_Enabled: 1
{tile_body}
"""


def generate_gameplay_yaml(level: LevelDef) -> str:
    # Minimal gameplay objects - stars, spikes, finish, pushable
    chunks = []

    # Stars parent
    star_children = []
    star_ids = [(900010010, 900010011, "Star"), (900010020, 900010021, "Star (1)"), (900010030, 900010031, "Star (2)")]
    for i, ((go_id, tr_id, name), (sx, sy)) in enumerate(zip(star_ids, level.stars)):
        star_children.append(f"  - {{fileID: {tr_id}}}")
        chunks.append(star_yaml(go_id, tr_id, name, sx, sy, 900002001))

    chunks.insert(
        0,
        f"""--- !u!1 &900002000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 900002001}}
  m_Layer: 0
  m_Name: Stars
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &900002001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 900002000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
{chr(10).join(star_children)}
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}""",
    )

    # Spikes parent + children
    spike_child_refs = []
    sid = 900020000
    for i, (sx, sy) in enumerate(level.static_spikes):
        go_id, tr_id = sid + i * 10, sid + i * 10 + 1
        spike_child_refs.append(f"  - {{fileID: {tr_id}}}")
        chunks.append(static_spike_yaml(go_id, tr_id, f"Spike ({i+1})" if i else "Spike", sx, sy, 900003001))

    for i, ms in enumerate(level.moving_spikes):
        base = 900030000 + i * 100
        spike_child_refs.append(f"  - {{fileID: {base + 1}}}")
        chunks.append(moving_spike_yaml(base, ms, 900003001))

    chunks.append(
        f"""--- !u!1 &900003000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 900003001}}
  m_Layer: 0
  m_Name: Spikes
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &900003001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 900003000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
{chr(10).join(spike_child_refs) if spike_child_refs else "  []"}
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}"""
    )

    chunks.append(finish_yaml(level.finish))
    if level.pushable:
        chunks.append(pushable_yaml(level.pushable))

    return "\n".join(chunks)


def star_yaml(go_id: int, tr_id: int, name: str, x: float, y: float, parent: int) -> str:
    col_id, sr_id = tr_id + 1, tr_id + 2
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr_id}}}
  - component: {{fileID: {col_id}}}
  - component: {{fileID: {sr_id}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Star
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{tr_id}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {x}, y: {y}, z: 0.2}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {parent}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!61 &{col_id}
BoxCollider2D:
  m_ObjectHideFlags: 0
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  serializedVersion: 3
  m_IsTrigger: 1
  m_Size: {{x: 0.88, y: 1}}
--- !u!212 &{sr_id}
SpriteRenderer:
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_Sprite: {{fileID: 657193517335828509, guid: cf83d3e6f5530114c8a312261f2f748d, type: 3}}
  m_Color: {{r: 1, g: 0.98024374, b: 0.27358478, a: 1}}"""


def static_spike_yaml(go_id: int, tr_id: int, name: str, x: float, y: float, parent: int) -> str:
    col_id, sr_id = tr_id + 1, tr_id + 2
    return f"""--- !u!1 &{go_id}
GameObject:
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr_id}}}
  - component: {{fileID: {col_id}}}
  - component: {{fileID: {sr_id}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Trap
--- !u!4 &{tr_id}
Transform:
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalPosition: {{x: {x}, y: {y}, z: 0}}
  m_Father: {{fileID: {parent}}}
--- !u!61 &{col_id}
BoxCollider2D:
  m_GameObject: {{fileID: {go_id}}}
  m_IsTrigger: 1
  m_Size: {{x: 1.5, y: 1}}
--- !u!212 &{sr_id}
SpriteRenderer:
  m_GameObject: {{fileID: {go_id}}}
  m_Sprite: {{fileID: 3921425457498842617, guid: 15562fffe2efc534bb9cb4cdb86aa490, type: 3}}
  m_Color: {{r: 1, g: 0.48113197, b: 0.48113197, a: 1}}"""


def moving_spike_yaml(base: int, ms: MovingSpike, parent: int) -> str:
    go_id = base
    tr_id = base + 1
    scr_id = base + 2
    rb_id = base + 3
    col_id = base + 4
    sr_id = base + 5
    pa_id = base + 6
    pb_id = base + 7
    pa_go = base + 8
    pb_go = base + 9
    lax, lay = ms.ax - ms.x, ms.ay - ms.y
    lbx, lby = ms.bx - ms.x, ms.by - ms.y
    return f"""--- !u!1 &{go_id}
GameObject:
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr_id}}}
  - component: {{fileID: {scr_id}}}
  - component: {{fileID: {rb_id}}}
  - component: {{fileID: {col_id}}}
  - component: {{fileID: {sr_id}}}
  m_Layer: 0
  m_Name: Moving_Spike
  m_TagString: Trap
--- !u!4 &{tr_id}
Transform:
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalPosition: {{x: {ms.x}, y: {ms.y}, z: 0}}
  m_LocalScale: {{x: 1.32, y: 1.09, z: 1}}
  m_Children:
  - {{fileID: {pa_id}}}
  - {{fileID: {pb_id}}}
  m_Father: {{fileID: {parent}}}
--- !u!114 &{scr_id}
MonoBehaviour:
  m_GameObject: {{fileID: {go_id}}}
  m_Script: {{fileID: 11500000, guid: 4f837501ca9f3a74dae1de9b8faffdd3, type: 3}}
  pointA: {{fileID: {pa_id}}}
  pointB: {{fileID: {pb_id}}}
  speed: {ms.speed}
  startAtPointA: 1
--- !u!50 &{rb_id}
Rigidbody2D:
  m_GameObject: {{fileID: {go_id}}}
  m_BodyType: 1
--- !u!61 &{col_id}
BoxCollider2D:
  m_GameObject: {{fileID: {go_id}}}
  m_IsTrigger: 1
  m_Offset: {{x: 0, y: 0.1}}
  m_Size: {{x: 2.34, y: 2.27}}
--- !u!212 &{sr_id}
SpriteRenderer:
  m_GameObject: {{fileID: {go_id}}}
  m_Sprite: {{fileID: 3921425457498842617, guid: 15562fffe2efc534bb9cb4cdb86aa490, type: 3}}
  m_Color: {{r: 1, g: 0.48113197, b: 0.48113197, a: 1}}
--- !u!1 &{pa_go}
GameObject:
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {pa_id}}}
  m_Name: PointA
--- !u!4 &{pa_id}
Transform:
  m_GameObject: {{fileID: {pa_go}}}
  m_LocalPosition: {{x: {lax}, y: {lay}, z: 0}}
  m_Father: {{fileID: {tr_id}}}
--- !u!1 &{pb_go}
GameObject:
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {pb_id}}}
  m_Name: PointB
--- !u!4 &{pb_id}
Transform:
  m_GameObject: {{fileID: {pb_go}}}
  m_LocalPosition: {{x: {lbx}, y: {lby}, z: 0}}
  m_Father: {{fileID: {tr_id}}}"""


def finish_yaml(pos: tuple[float, float]) -> str:
    x, y = pos
    return f"""--- !u!1 &900004000
GameObject:
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 900004001}}
  - component: {{fileID: 900004002}}
  - component: {{fileID: 900004003}}
  - component: {{fileID: 900004004}}
  m_Name: FinishPortal
  m_TagString: Finish
--- !u!4 &900004001
Transform:
  m_GameObject: {{fileID: 900004000}}
  m_LocalPosition: {{x: {x}, y: {y}, z: 0}}
  m_Father: {{fileID: 0}}
--- !u!114 &900004002
MonoBehaviour:
  m_GameObject: {{fileID: 900004000}}
  m_Script: {{fileID: 11500000, guid: 7c4006288eed051479c473988472ffdf, type: 3}}
--- !u!61 &900004003
BoxCollider2D:
  m_GameObject: {{fileID: 900004000}}
  m_IsTrigger: 1
--- !u!212 &900004004
SpriteRenderer:
  m_GameObject: {{fileID: 900004000}}
  m_Sprite: {{fileID: 7482667652216324306, guid: 311925a002f4447b3a28927169b83ea6, type: 3}}
  m_Color: {{r: 0.003144443, g: 0.8618967, b: 1, a: 1}}"""


def pushable_yaml(pos: tuple[float, float]) -> str:
    x, y = pos
    return f"""--- !u!1 &900005000
GameObject:
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 900005001}}
  - component: {{fileID: 900005002}}
  - component: {{fileID: 900005003}}
  - component: {{fileID: 900005004}}
  - component: {{fileID: 900005005}}
  m_Name: Moveable_Object
  m_TagString: Pushable
--- !u!4 &900005001
Transform:
  m_GameObject: {{fileID: 900005000}}
  m_LocalPosition: {{x: {x}, y: {y}, z: 0}}
  m_LocalScale: {{x: 9.64, y: 9.43, z: 1}}
  m_Father: {{fileID: 0}}
--- !u!114 &900005002
MonoBehaviour:
  m_GameObject: {{fileID: 900005000}}
  m_Script: {{fileID: 11500000, guid: b8e4f12a6c3d4e5f9081726354aab9cd, type: 3}}
--- !u!50 &900005003
Rigidbody2D:
  m_GameObject: {{fileID: 900005000}}
  m_Mass: 12
--- !u!58 &900005004
CircleCollider2D:
  m_GameObject: {{fileID: 900005000}}
  m_Radius: 0.5
--- !u!212 &900005005
SpriteRenderer:
  m_GameObject: {{fileID: 900005000}}
  m_Sprite: {{fileID: -2413806693520163455, guid: fc117e5f8e9c60a47a7854b751617643, type: 3}}"""


def main() -> None:
    levels = {4: level4(), 5: level5(), 6: level6()}
    for num, defn in levels.items():
        patch_level(num, defn)
    marker = ROOT / "Library" / ".level_builder_v2_done"
    marker.write_text("done", encoding="utf-8")
    print("Done.")


if __name__ == "__main__":
    main()
