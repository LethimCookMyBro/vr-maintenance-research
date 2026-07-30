# XRI Simulator Reuse Audit

**Scope:** read-only inspection of the current Unity project, its imported XRI 3.4.0 samples, and the active Editor scene. No original XRI asset, scene, prefab, package, or project setting was changed.  
**Inspected:** 2026-07-30; Unity `6000.3.20f1`; `com.unity.xr.interaction.toolkit` `3.4.0`.

## Confirmed current state

- The only enabled build scene and the active loaded Editor scene are `Assets/XRI_Examples/Scenes/XRI_Examples_Main.unity`.
- The active scene has one `Unity.XR.CoreUtils.XROrigin` and one `UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager`, both on `Complete XR Origin Set Up Variant`.
- The active scene has no serialized `XRInteractionManager`, `EventSystem`, `XRUIInputModule`, or `XRInteractionSimulator` component. This is an Editor-scene observation, not a Play Mode claim.
- `Assets/XRI_Examples/Global/Prefabs/Complete XR Origin Set Up Variant.prefab` contains one `XROrigin`, one `InputActionManager`, and neither an interaction manager nor an EventSystem. Its input manager enables only `Assets/Samples/XR Interaction Toolkit/3.4.0/Starter Assets/XRI Default Input Actions.inputactions`.
- `Assets/Samples/XR Interaction Toolkit/3.4.0/Starter Assets/Prefabs/XR Origin (XR Rig).prefab` has the same manager counts and enables the same default XRI action asset.
- `Assets/Samples/XR Interaction Toolkit/3.4.0/XR Interaction Simulator/XR Interaction Simulator.prefab` contains `UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator` plus one `InputActionManager`; it has no XR Origin, interaction manager, or EventSystem. Its manager enables three distinct simulator assets listed below.

## Reuse inventory

| Reuse decision | Exact source path | Confirmed capability | Research use |
|---|---|---|---|
| Direct, one instance only | `Assets/Samples/XR Interaction Toolkit/3.4.0/Starter Assets/Prefabs/XR Origin (XR Rig).prefab` | One XR Origin; one input-action manager; default XRI actions | Base rig in each standalone research scene. Do not also add the example-origin variant. |
| Direct, one instance only | `Assets/Samples/XR Interaction Toolkit/3.4.0/XR Interaction Simulator/XR Interaction Simulator.prefab` | Official `XRInteractionSimulator`; separate simulator input manager | Desktop simulation companion to the one research XR Origin. |
| Direct data reference | `Assets/Samples/XR Interaction Toolkit/3.4.0/Starter Assets/XRI Default Input Actions.inputactions` | Default XRI head, controller, locomotion, and UI action maps | Keep enabled by the XR Origin input manager; do not copy or edit it. |
| Direct data reference | `Assets/Samples/XR Interaction Toolkit/3.4.0/XR Interaction Simulator/XR Interaction Simulator Controls.inputactions` | Simulator navigation, device selection, reset, quick-action, and menu bindings | Enabled by the simulator input manager. |
| Direct data reference | `Assets/Samples/XR Interaction Toolkit/3.4.0/XR Interaction Simulator/XR Interaction Controller Controls.inputactions` | Simulated controller axis, grip, trigger, and button bindings | Enabled by the simulator input manager. |
| Direct data reference | `Assets/Samples/XR Interaction Toolkit/3.4.0/XR Interaction Simulator/XR Interaction Hand Controls.inputactions` | Simulated hand gesture bindings | Enabled by the simulator input manager. |
| Direct for a disposable training station only | `Assets/XRI_Examples/GrabInteractables/Prefabs/InstantCube.prefab` | `XRGrabInteractable` with stock affordance feedback | One training grab object; keep task-specific parts custom. |
| Direct for a disposable training station only | `Assets/XRI_Examples/SocketInteractors/Prefabs/SimpleSocket.prefab` | `XRSocketInteractor` with `AutoSocketAttach` | One training socket; do not use as a research task-completion rule without custom logging. |
| Reference only | `Assets/Samples/XR Interaction Toolkit/3.4.0/Starter Assets/DemoAssets/Prefabs/UI/Interactive Controls.prefab` | World UI and tracked-device raycaster, plus sample-specific poke/toggle scripts | Reference the wiring; make a minimal research UI instead of importing this demo surface. |
| Reference only | `Assets/XRI_Examples/UI_2D/Prefabs/MainMenu/SpatialPanelNoNav.prefab` | Multiple tracked-device raycasters and example UI hierarchy | Reference only; importing it would add more UI/raycaster surface than the research flow needs. |
| Reference only | `Assets/XRI_Examples/Global/Prefabs/Complete XR Origin Set Up Variant.prefab` | The origin currently used by the official example scene | Preserve as-is. Prefer the Starter Assets origin for clean research scenes. |

## Keyboard and mouse bindings: confirmed asset evidence

The bindings below are read directly from the three simulator `.inputactions` assets. They are confirmed bindings, not a Play Mode usability result.

### Simulator navigation and selection

Source: `Assets/Samples/XR Interaction Toolkit/3.4.0/XR Interaction Simulator/XR Interaction Simulator Controls.inputactions`.

| Action | Binding |
|---|---|
| X translate | `A` negative, `D` positive |
| Z translate | `S` negative, `W` positive |
| Y translate | `Q` negative, `E` positive |
| Keyboard rotation delta | Arrow keys |
| Mouse scroll | Mouse wheel |
| Toggle X/Y/Z constraint | `V` / `C` / `Z` |
| Reset | `R` |
| Cycle devices | `Tab` |
| Manipulate right / left / head | `]` / `[` / `H` |
| Cycle quick action / perform it | Backquote / `Space` |
| Left-device actions | `Shift` |
| Toggle mouse rotation / rotate | Right mouse button / mouse delta |
| Primary / secondary 2D-axis target | `9` / `0` |
| Toggle action menu / input-selection menu | `X` / `Y` |

### Simulated controller and hand actions

Source: `Assets/Samples/XR Interaction Toolkit/3.4.0/XR Interaction Simulator/XR Interaction Controller Controls.inputactions`.

| Action | Binding |
|---|---|
| Controller 2D axis | `I` up, `K` down, `J` left, `L` right |
| Resting-hand 2D axis | `I/K/J/L`; mouse forward/back buttons and `Q/E` provide additional mapped directions |
| Grip / trigger | `G` / `T` |
| Primary / secondary button | `1` / `2` |
| Menu | `M` |
| Primary / secondary 2D-axis click | `3` / `4` |
| Primary / secondary 2D-axis touch | `5` / `6` |
| Primary / secondary touch | `7` / `8` |

Source: `Assets/Samples/XR Interaction Toolkit/3.4.0/XR Interaction Simulator/XR Interaction Hand Controls.inputactions`.

| Hand action | Binding |
|---|---|
| Grab / poke / pinch | `K` / `N` / `M` |
| Thumb / fist / open | `L` / `P` / `O` |

## Minimum research-scene composition

1. Start each new research scene empty; do **not** duplicate or load `Assets/XRI_Examples/Scenes/XRI_Examples_Main.unity` additively.
2. Add exactly one instance of `XR Origin (XR Rig)` and exactly one instance of `XR Interaction Simulator`.
3. The official pair intentionally contains two `InputActionManager` components: the Origin owns the one default XRI action asset; the Simulator owns its three simulator action assets. These sets are distinct. Do not add a third manager and do not enable either asset set a second time.
4. Add exactly one explicit `XRInteractionManager` for the research scene, because neither inspected reusable prefab contains one. Do not add one through every task prefab.
5. For research world-space UI, use one scene-level `EventSystem` with the XRI UI input module and one `TrackedDeviceGraphicRaycaster` per intentionally interactive canvas. Keep the EventSystem out of reusable task and information-source prefabs.
6. Reuse the grab and socket prefabs only in `VRTraining`; build Computer and Fan task objects as custom research content with stable IDs and logging hooks.

## Unknowns requiring verification during implementation

- The current Editor scene may create infrastructure at runtime; Play Mode was not entered, so automatic manager creation and runtime manager counts are unknown.
- The listed bindings have not been exercised in this project, so grabbing, ray UI, sockets, reset, and keyboard/mouse ergonomics remain unverified.
- The strict "one InputActionManager component" interpretation conflicts with the official Origin-plus-Simulator pair, which contains two managers for four non-overlapping action assets. If that strict rule is required, the lead must create a custom research input variant and validate every simulator binding before replacing the official pair.
- No physical Quest 3/OpenXR interaction test was performed.

## Evidence inspected

- `Packages/manifest.json`
- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/EditorBuildSettings.asset`
- `Assets/XRI_Examples/Scenes/XRI_Examples_Main.unity`
- All paths listed in the reuse inventory
- Unity MCP read-only component and serialized input-action-manager inspections
