#!/usr/bin/env python3
"""Inject ParallaxBackground hierarchy into Level_01.unity."""

from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCENE = ROOT / "Assets/Scenes/Level_01.unity"

BACKGROUND_SORTING_LAYER_ID = 3251153685
ROOT_TRANSFORM_ID = 880001002
MATERIAL = "{fileID: 2100000, guid: a97c105638bdf8b4a8650670310a4cd3, type: 2}"
PARALLAX_BG_SCRIPT = "{fileID: 11500000, guid: f3a8b2c1d4e54f6a9b0c1d2e3f4a5b6c, type: 3}"
PARALLAX_LAYER_SCRIPT = "{fileID: 11500000, guid: e8f4a2b1c3d54e6f9a0b1c2d3e4f5a6b, type: 3}"

LAYERS = [
    {
        "name": "Layer_1",
        "go": 880001011,
        "transform": 880001012,
        "renderer": 880001013,
        "parallax": None,
        "sprite": "{fileID: -855298285791758126, guid: 47ab356e04029a4468e4d4c73be47530, type: 3}",
        "size": "{x: 5.76, y: 3.24}",
        "order": -10,
    },
    {
        "name": "Layer_2",
        "go": 880001021,
        "transform": 880001022,
        "renderer": 880001023,
        "parallax": 880001024,
        "sprite": "{fileID: 6937817954938267909, guid: 92eb90b3c61ec5a43a21cfdf098b4402, type: 3}",
        "size": "{x: 5.76, y: 2.3}",
        "order": -9,
        "factor": 0.08,
        "vertical": 0.04,
    },
    {
        "name": "Layer_3",
        "go": 880001031,
        "transform": 880001032,
        "renderer": 880001033,
        "parallax": 880001034,
        "sprite": "{fileID: -6470460504253458178, guid: 6e074b2e13ff6e846a2d498d547cf484, type: 3}",
        "size": "{x: 3.22, y: 0.24}",
        "order": -8,
        "factor": 0.18,
        "vertical": 0.06,
    },
    {
        "name": "Layer_4",
        "go": 880001041,
        "transform": 880001042,
        "renderer": 880001043,
        "parallax": 880001044,
        "sprite": "{fileID: -4178046375966092550, guid: 11331f9c40b7d2b469d17ddb03674123, type: 3}",
        "size": "{x: 5.76, y: 2.92}",
        "order": -7,
        "factor": 0.35,
        "vertical": 0.08,
    },
]


def sprite_renderer_block(game_object_id: int, renderer_id: int, sprite_ref: str, size: str, order: int) -> str:
    return f"""--- !u!212 &{renderer_id}
SpriteRenderer:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {game_object_id}}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
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
  - {MATERIAL}
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
  m_SortingLayerID: {BACKGROUND_SORTING_LAYER_ID}
  m_SortingLayer: 1
  m_SortingOrder: {order}
  m_MaskInteraction: 0
  m_Sprite: {sprite_ref}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {size}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_SpriteSortPoint: 0
"""


def parallax_layer_block(game_object_id: int, component_id: int, factor: float, vertical: float) -> str:
    return f"""--- !u!114 &{component_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {game_object_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {PARALLAX_LAYER_SCRIPT}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::ParallaxLayer
  parallaxFactor: {factor}
  verticalParallaxFactor: {vertical}
"""


def build_parallax_yaml() -> str:
    child_transforms = "\n  - ".join(f"{{fileID: {layer['transform']}}}" for layer in LAYERS)
    chunks = [f"""--- !u!1 &880001001
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ROOT_TRANSFORM_ID}}}
  - component: {{fileID: 880001009}}
  m_Layer: 0
  m_Name: ParallaxBackground
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{ROOT_TRANSFORM_ID}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 880001001}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {child_transforms}
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &880001009
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 880001001}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {PARALLAX_BG_SCRIPT}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::ParallaxBackground
  viewportPadding: 1.6
  horizontalTileCopies: 2
"""]

    for layer in LAYERS:
        components = [f"  - component: {{fileID: {layer['transform']}}}", f"  - component: {{fileID: {layer['renderer']}}}"]
        if layer["parallax"] is not None:
            components.append(f"  - component: {{fileID: {layer['parallax']}}}")

        chunks.append(
            f"""--- !u!1 &{layer['go']}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
{chr(10).join(components)}
  m_Layer: 0
  m_Name: {layer['name']}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{layer['transform']}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {layer['go']}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ROOT_TRANSFORM_ID}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
"""
        )
        chunks.append(
            sprite_renderer_block(layer["go"], layer["renderer"], layer["sprite"], layer["size"], layer["order"])
        )
        if layer["parallax"] is not None:
            chunks.append(
                parallax_layer_block(layer["go"], layer["parallax"], layer["factor"], layer["vertical"])
            )

    return "\n".join(chunks)


def patch_scene() -> None:
    text = SCENE.read_text(encoding="utf-8")

    if "m_Name: ParallaxBackground" in text:
        print("ParallaxBackground already exists, skipping.")
        return

    text = re.sub(
        r"m_BackGroundColor: \{r: 0, g: 0, b: 0, a: 0\}",
        "m_BackGroundColor: {r: 0.45, g: 0.72, b: 0.98, a: 1}",
        text,
        count=1,
    )

    roots_pattern = (
        r"(SceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n)"
        rf"(  - \{{fileID: {ROOT_TRANSFORM_ID}\}}\n)?"
    )
    if re.search(roots_pattern, text):
        text = re.sub(roots_pattern, rf"\1  - {{fileID: {ROOT_TRANSFORM_ID}}}\n", text, count=1)
    else:
        text = text.rstrip() + f"\n  - {{fileID: {ROOT_TRANSFORM_ID}}}\n"

    insertion = build_parallax_yaml() + "\n"
    marker = "\nSceneRoots:"
    if marker not in text:
        raise RuntimeError("SceneRoots block not found.")

    text = text.replace(marker, "\n" + insertion + "SceneRoots:", 1)
    SCENE.write_text(text, encoding="utf-8")
    print("Level_01 parallax injected.")


if __name__ == "__main__":
    patch_scene()
