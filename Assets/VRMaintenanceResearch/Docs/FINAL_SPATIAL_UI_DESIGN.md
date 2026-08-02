# Final spatial UI design

## Evidence and references

- Unity's `TrackedDeviceGraphicRaycaster` raycasts a Canvas for tracked devices: <https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.0/api/UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster.html>
- XRI ray interactors explicitly enable UI interaction: <https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.0/manual/xr-ray-interactor.html>
- Extended reading surfaces stay world-locked; relocating them is a deliberate action, not head tracking: <https://learn.microsoft.com/en-us/windows/mixed-reality/design/hand-menu>

## Selected pattern

- A **single fixed information station** uses four equal source cards in a 2 x 2 grid. The station is left of the active device, while tools and parts remain right.
- Opening a source closes the previously open source before displaying the selected content at the fixed reading station. The selector stays in its world location.
- The task card is a compact right-side status surface. Training uses a compact left-side instruction surface.
- Content uses layered slate/navy surfaces, off-white text, blue selection/interaction accent, amber warning, and red only for stop controls.

## Rejected patterns

- Headset-following or yaw-chasing panels.
- A wall-wide source banner or task board.
- Four unequal source signs that privilege one format.
- Continuous motion, parallax, or panel tilt while the participant reads.

## Physical dimensions and layout

| Surface | Size | Fixed role |
|---|---:|---|
| Source card | 0.58 m x 0.28 m | Equal 2 x 2 selector cards |
| Reading panel | 0.90 m x 0.53 m | One fixed opened-content location |
| Task status | 0.95 m x 0.34 m | Right of device at eye height |
| Training board | 1.05 m x 0.52 m | Left of training workbench |
| Controls | minimum 62 px in their world-space canvas | Deliberate ray selection |

The participant starts about 1.2 m from the bench. The reader is fixed left of the task-device envelope at approximately 2.3 m from the initial desktop camera; the status card is on the right and does not overlap either surface.

## Interaction states and timing

| State | Visual treatment | Duration |
|---|---|---:|
| Idle | Slate/navy surface, neutral border | — |
| Hover | Blue accent rule plus 2% scale increase | 0.12 s ease |
| Press | XRI select activation | Input frame |
| Selected | Persistent blue accent rule plus 3.5% scale increase | 0.16 s ease |
| Disabled | Muted label and low-contrast surface | — |
| Open/close | Single fixed content panel, no duplicate | Immediate |

Motion is intentionally restricted to user-triggered state changes. No participant-facing panel automatically moves after opening.

## Implementation constraints

- All participant canvases are world-space and use the main participant camera plus a `TrackedDeviceGraphicRaycaster`.
- The task status, training board, selector, and reader have no `ComfortFollowPanel` component.
- Exactly one active XR origin, interaction manager, input action manager, event system, and XR UI input module remain a scene-level invariant.
- The four source definitions and all research event names are preserved; selector styling does not encode correctness.
- Video readers render their existing RenderTexture in-panel and include Play, Pause, Stop, +10 s, Restart, Close and an elapsed/total status label.
- Thai and Japanese reader strings use local TMP fallback assets so the same fixed reader can render all configured scripts.

## Verification still required

Runtime captures prove readable EN/TH/JA strings and the visible controller rays; future participant QA must still exercise both ray paths and fixed-world behavior after head movement. Quest 3 validation remains pending.
