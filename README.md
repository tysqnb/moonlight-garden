# Moonlight Garden — Unity project (PROG2006 Assessment 1)

Zihao Wang · 25259382 · PROG2006 Designing the User Experience

A short interactive storybook for phones, built in **Unity 2022.3.62f2** (2D, built-in
render pipeline) and published as a WebGL build on itch.io.

## Opening the project

1. Install Unity **2022.3.62f2** with the **WebGL Build Support** module.
2. In Unity Hub choose *Add project from disk* and pick this folder.
3. Open `Assets/Scenes/Home.unity` and press Play.

The `Library` folder is not included, as the course submission guide asks; Unity
rebuilds it on first open.

## Scenes

| # | Scene | Page |
|---|---|---|
| 1 | `Assets/Scenes/Home.unity` | Home |
| 2 | `Assets/Scenes/Scene1_Firefly.unity` | Scene 1 — tap the firefly |
| 3 | `Assets/Scenes/Scene2_Snail.unity` | Scene 2 — drag the dew drop |
| 4 | `Assets/Scenes/Scene3_Seed.unity` | Scene 3 — swipe across the reeds |
| 5 | `Assets/Scenes/Scene4_Moonflower.unity` | Scene 4 — press and hold the bud |
| 6 | `Assets/Scenes/Credits.unity` | Credits |

## Where things live

- `Assets/Art/` — the 25 separate element sprites (characters, props, icons, lanterns).
- `Assets/Scripts/` — page navigation, lantern progress and the four gestures.
- `Assets/Editor/SceneBuilder.cs` — rebuilds all six screens and the WebGL build.
  Menu: **Moonlight Garden → 1 Build all scenes**, then **2 Build WebGL**.
- `Assets/WebGLTemplates/MoonlightGarden/` — the full-window WebGL template used for
  the itch.io build.

## Design rules the build follows

- Four colours only: ink indigo `#16213E`, paper cream `#FFF9EC`, moon amber `#F4D03F`,
  leaf sage `#77C9A3`.
- Portrait stage, 1080 × 1920 design size; the canvas scales to the browser window.
- Six lanterns across the bottom: one lights per finished page, with a sage ring on
  the current page.
- One gesture per story scene: tap, drag, swipe, press and hold.
