# Known Limitations

## Research scope

- This is a research-development prototype, not a validated learning intervention.
- No Meta Quest 3 hardware run has occurred.
- Development source content and silent media are English placeholders; Thai/Japanese content is configurable but awaits approved translation and equivalence review.
- The self-authored MP4s are functionally verified in Windows Editor but Windows Media Foundation emits a color-primaries fallback warning; no research-script error followed the normal completion lifecycle.
- Low activity is interaction inactivity, not a clinical or cognitive measure.
- Current Play Mode runs prove schema, flow, simulator integration, and source media behavior; they do not prove participant behavior, hardware tracking quality, or research outcomes.
- The eight invalid v1 information assets remain locally visible until a permitted Unity Editor deletion action is available; only `_v2` assets are used by task definitions. They are untracked and unstaged.

## Build and platform

- The Windows artifact is a `Mono2x` Development build. The serialized Standalone backend remains `IL2CPP`, but the IL2CPP build path awaits the Visual Studio C++ toolchain and Windows SDK.
- The desktop runtime reports OpenXR form-factor unavailable without a connected headset; the Windows simulator run does not establish Quest 3 hardware behavior.
- The Windows build emits 486 package/build warnings, mainly shader and Performance Test preprocessor messages; no research-script Console error accompanies the build or runtime flow.

## Visual redesign, 2026-08-02

- **The redesigned scenes have not been run in the standalone player.** The Windows build succeeds, but every visual and interaction check in this pass was done in the Unity Editor.
- **No Quest 3 performance claim is made.** The scenes are *prepared* for Quest — 17 shared URP materials, ~20 k triangles per task scene, 2 real-time directional lights, no post-processing change, no transparency beyond the existing UI, 1024 px maximum texture, static batching flags on all environment geometry — but none of this has been measured on hardware.
- **The participant start pose changed** from `(0, 0, 0)` to `(0, 0, -1.6)` in all three participant scenes. Movement CSV coordinates recorded before 2026-08-02 are therefore not spatially comparable with later ones. The logging schema is unchanged.
- **Device and component transforms changed.** Stable IDs, scripts, interactable components, collider components, task references and completion logic are all preserved, but positions, rotations and scales were re-authored so the equipment is human-scale and rests on a workbench. `PROTOCOL_CHANGE_LOG.md` records every change; an advisor should confirm the new arrangement does not alter intended task difficulty.
- **The fan front guard is now a removed part lying on the bench** rather than a mounted guard. Mounted, its collider blocked controller rays to `fan.blade`. The disassembled arrangement is deliberate and keeps every component reachable.
- **Interaction was validated through the component API, not by hand.** Grab, socket placement, controller-ray hover and poke were confirmed present and wired, and the XR simulator rig, rays and controller visuals render, but a human did not drive a controller through the full task in this pass.
- **The `ResearcherSetup` interface is built at runtime in code**, not as an editable prefab. This removes prefab-wiring drift but means the layout cannot be adjusted in the Inspector; edit `ResearcherSetupController.cs`.
- **The task status board and training board are runtime-built world-space canvases.** They do not appear in Editor scene view screenshots; the Play Mode captures in `Docs/Screenshots` show them.
- **World-space text is English-only and not yet localised.** The information-source captions, panel titles and body text render `englishTitle`/`englishContent` from the source definitions; Thai and Japanese fields exist but are not yet displayed.
- **The information-source layout was deliberately not changed** even though the brief suggested moving the station to the participant's left. The four tiles are recorded against `information_source_layout_id`, and moving them would alter relative salience without changing the logged layout ID. Only appearance was equalised.
- **Two Poly Haven textures were downloaded but only partially used.** `beige_wall_001` contributes its normal map only; its diffuse was dropped because the warm beige conflicted with the neutral off-white target. The unused roughness maps were deleted rather than left as dead assets.
