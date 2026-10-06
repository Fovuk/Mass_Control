# MASS CONTROL

A Unity 2D platform / puzzle game. Change your character’s mass and size to clear obstacles, push objects, collect stars, and reach the portal.

![Level select](Docs/screenshots/level-select.jpeg)

## Gameplay

- **Small form:** Fast movement, double jump, fit through narrow passages.
- **Big form:** Slower but heavier; push movable objects (boulders).
- Collect **3 stars** per level and reach the **finish portal**.
- Touching traps (spikes, etc.) restarts the level.

![Gameplay — Level 1](Docs/screenshots/gameplay.jpeg)

## Controls

| Input | Action |
|-------|--------|
| Move (joystick / WASD / arrows) | Horizontal movement |
| **Jump** | Jump (double jump while small) |
| **Change Size** | Toggle small ↔ big form |

## Features

- Level select screen (Levels 1–6) with star progress
- Morph (size / mass) mechanic
- Pushable objects
- Tilemap-based levels
- Moving platform support
- Looping menu / in-game music and SFX
- Mobile-friendly touch UI (joystick + buttons)

## Requirements

- [Unity](https://unity.com/) **6000.x** (URP 2D)
- Open this repo and start from `Assets/Scenes/MainMenu.unity`

## Getting started

1. Unity Hub → **Add** → select this folder
2. Open the project
3. Load `Assets/Scenes/MainMenu.unity`
4. Press Play

## Scenes

| Scene | Description |
|-------|-------------|
| `MainMenu` | Main menu / level select |
| `Level_01` … `Level_06` | Playable levels |

## Technical notes

- Render: **Universal Render Pipeline (2D)**
- Input: **Unity Input System**
- Camera: **Cinemachine**
- Maps: **2D Tilemap**
- Save: local progress (`SaveManager`)

## License / assets

Game code lives in this repo. Art may come from third-party packs (e.g. Free Platform Game Assets); check those licenses for commercial use.
