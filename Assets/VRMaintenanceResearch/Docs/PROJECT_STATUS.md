# VR Maintenance Research - Project Status

## Current phase

Visual redesign is complete on branch `visual-polish-claude`, built from the validated
baseline `f117cc8`. The graybox prototype is now a clean academic VR maintenance
laboratory. Unity MCP is responsive, the Editor is idle outside Play Mode, the Console
has zero errors, and a Windows Development + Allow Debugging Mono fallback build
succeeds. The serialized Standalone backend remains IL2CPP; the IL2CPP path is still
pending the Visual Studio C++ toolchain and Windows SDK.

## Recovered foundation (unchanged by the redesign)

- All custom content remains under `Assets/VRMaintenanceResearch`; original XRI example scenes/assets and package versions remain untouched.
- Unity import is complete and scripts compile. Research scenes remain out of Play Mode after validation.
- The invalid unreferenced v1 source assets still have missing script references and are still untracked, unstaged and unreferenced. Valid task definitions reference only the eight `_v2` assets.
- Computer and Fan video sources use real, 60-second silent self-authored baseline MP4s, Unity `VideoPlayer`, RenderTextures, and shared Play/Pause/Stop/Seek/Restart controls.
- Natural video completion records `VideoCompleted`, clearing playback state before later close/scene teardown.
- Task log timestamps are task-relative with ISO 8601 absolute timestamps retained; multi-task summaries append one row per completed task.
- All 31 stable research IDs (13 Computer, 15 Fan, 3 Training) are byte-identical to the baseline.

## Visual redesign, 2026-08-02

- A shared `LabEnvironment` prefab (`Prefabs/Environment/LabEnvironment.prefab`, ~884 triangles) provides the same 9 x 8 x 3 m room shell, two-light rig, ceiling panels, 0.92 m workbench, parts/tool trays, control pedestal, storage unit and information station to Training, Computer, Fan and ResearcherSetup.
- 17 shared URP materials under `Materials/Lab` replace the single built-in `Lit` instance that every object previously used. Two Poly Haven CC0 textures and 16 Kenney CC0 icons are the only external assets; see `ThirdParty/THIRD_PARTY_ASSETS.md`.
- Two objects that rendered magenta (`Default-Material` on the built-in Standard shader, which does not exist in URP) are fixed.
- The participant no longer spawns inside the device; the start pose is `(0, 0, -1.6)` in all three participant scenes.
- The four information sources are now identifiable while closed, with identical tile size, icon size, accent weight, caption type size, slot label and viewing geometry. Their transforms are deliberately unchanged.
- `ResearcherSetup` is a centred two-column TextMeshPro desktop interface over a laboratory backdrop, replacing the IMGUI box. The pseudonymity warning is retained and now visually prominent.
- Training has a readable world-space board with live progress indicators and a Continue button that unlocks only when the three training requirements are met.
- Researcher controls keep every function (Pause, Resume, Retry, Reset Task, Abort Task, Safety Stop, Continue to Next Task) but are collapsed behind a handle (or F9) and Safety Stop is separated and given the reserved warning treatment.
- A read-only task status board mirrors task state and attempt number above the information station.

## Latest executed evidence

- Two full Play Mode sessions on the redesigned scenes: `ResearcherSetup -> VRTraining -> Computer -> Fan` and `ResearcherSetup -> Fan -> Computer`, both ending `Completed` with correct manifest, event, movement and summary CSVs, including a failed device test, a retry and a development reset.
  - `...\VRMaintenanceResearchData\Development\20260802T083541Z_44a43476`
  - `...\VRMaintenanceResearchData\Development\20260802T083741Z_66a5f85e`
- Zero Console errors across the whole redesign.
- Windows 64-bit Development + Allow Debugging build: **Succeeded**, 0 errors, 486 package/shader warnings, 325 MB, 5 min 59 s.
- Scene integrity verified: exactly one XR Origin, XR Interaction Manager, EventSystem, camera, `Lab Environment` and two lights per scene.
- Before/after participant-viewpoint screenshots in `Docs/Screenshots`.

See `TEST_REPORT.md` for exact evidence paths and `CLAUDE_VISUAL_REDESIGN_PLAN.md` for
the audit, design decisions and the changes that were deliberately not made.

## Next action

Physical Meta Quest 3 validation, approved translations and participant QA remain
pending. Relaunch the standalone Windows artifact to confirm the redesigned scenes
outside the Editor, and review the spatial changes recorded in `PROTOCOL_CHANGE_LOG.md`
with the advisor before data collection.

## Final spatial verification update - 2026-08-03

- Commit `ac8837e` moves the four equal source cards and the single reader to a fixed left-side station in both task scenes. The source selector is 0.58 m x 0.28 m per card; the reader is 0.90 m x 0.53 m; the task card is 0.95 m x 0.34 m.
- Runtime verification in `ComputerRepairTask` confirmed one EventSystem, one XRUIInputModule, no legacy input module, one tracked-device graphic raycaster, grounded desktop locomotion, a hidden simulator HUD, and a compact fixed task card.
- Runtime source-switch verification opened Manual, advanced to page 1, switched to Text Troubleshooting Guide, and reported `active_reader_count=1`. No research-script runtime error was logged.
- A new Windows Mono Development build completed at `Builds/Windows/VRMaintenanceResearch-Final/VRMaintenanceResearch.exe`: `Succeeded; errors=0; warnings=0`. It launched outside Unity and wrote `Standalone-Validation-Player.log`; OpenXR reported no available form factor on this desktop.
- Physical Quest 3 validation and an interactive standalone full-flow remain pending.

## Localization and reader correction - 2026-08-03

- The reader now registers local dynamic TMP fallback assets for Thai and Japanese. Actual Play Mode captures show both scripts without missing-glyph boxes: `Docs/Screenshots/Final/FanThaiManual_Runtime.png` and `Docs/Screenshots/Final/FanJapaneseManual_Runtime.png`.
- The video reader now shows the existing RenderTexture in-panel, includes Close and Restart controls, and displays elapsed/total time. `ComputerVideoReader_Runtime.png` and `FanVideoReader_Runtime.png` are the corresponding runtime captures.
- The rendered visual children retain the original functional roots, colliders, `ResearchInteractable` components and stable IDs; only their visible presentation changed.
- Fresh artifact: `Builds/Windows/VRMaintenanceResearch-Final-Localized-Clean/VRMaintenanceResearch.exe` built `Succeeded; errors=0; warnings=485; size=356181522`. Its executable SHA-256 is `F5AF8F2582C77647AA735CDD9F4D9CAE9FF79AAB0910F9524CF71018F28D8B50`.
