# VR Maintenance Research - Project Status

## Part recognition - 2026-08-08

The parts a participant has to identify in order to diagnose are now modelled at the
real parts' dimensions. Before this pass the answer to Task A was a 70 mm white block
with a row of gold pips on its top face, and the fuse the fan task turns on was 84 mm
long and 26 mm across - a torch battery. Neither could be named by looking at it, which
matters because naming a part by looking at it is the step the study measures.

- The 24-pin ATX connector exists three times in `ComputerRepairTask` - the header on
  the board, the plug hanging off the supply's loom, and the replacement lead in the
  spares tray. They were three separate hand builds at three different sizes. They are
  now one geometry description at Molex Mini-Fit Jr. dimensions: 4.2 mm pitch, twelve
  circuits by two rows, 54 x 13 x 15 mm body, 20+4 split seam, latch, and a loom in the
  four rail colours a real 24-pin carries.
- Every fuse in `FanRepairTask` is a 6 x 30 mm glass cartridge built by one function -
  the spare in the tray, the stowed spare, and the one fitted in the holder. The blown
  one differs in the element and nothing else: the opaque dark blob that used to sit
  inside its glass was a distance-visible cue and is gone. The holder came down to a
  44 mm carrier with sprung clips and two mounting screws, and the service bay it sits
  in came down with it.
- The three controls a participant presses or turns now use the XRI example kit's own
  meshes - `PushButton.fbx` for `computer.power-button`, `Dial.fbx` for
  `fan.speed-selector`, `Slider.fbx` for `fan.power-switch` - repainted into the lab
  palette. Mesh only: `XRPushButton`, `XRKnob` and `XRSlider` keep their own state and
  would compete with `ResearchInteractable`.
- `fan.body` and `fan.front-cover` were **not** rebuilt. Both already read as a desk fan
  and a wire guard; captures are in `Docs/Screenshots/Recognition/`.

Telemetry is untouched: all 31 stable ids are byte-identical to the baseline, no
collider was resized, moved or re-owned, no task definition or CSV column changed, and
no object was added to or removed from either bench. Triangle cost: `FanRepairTask`
36,120 → 37,898, `ComputerRepairTask` 118,967 → 120,385. Full numbers, the real-part
dimensions each piece is sized from, and what was deliberately left alone are in
`Docs/Verification/PART_RECOGNITION_RECORD.md`.

## Current phase - 2026-08-08

A session can now be run end to end in the configuration a real participant run uses.
Before this pass it could not: `ResearcherTaskControls` is the only caller of
`CompleteCurrentTaskAndAdvance`, `SafetyStop`, `AbortTask` and `Retry`, and it is an
IMGUI panel behind F9 that returned early unless `developmentMode` was set. With
development mode off there was no route from the first task to the second and no
safety stop for anyone, participant or researcher.

The division of control is deliberate and follows the protocol: the participant does
everything inside the headset that belongs to the task - training, reading, tools,
components, inspect, retry - and the researcher owns the transitions between tasks,
because the headset comes off between them for NASA-TLX.

Editor-side work is complete against the current design. What remains is physical
Meta Quest 3 validation, approved translations, and advisor review of the spatial
and framing changes. No hardware claim is made anywhere in this document.

### Sections below are the record up to 2026-08-03

The three dated sections that follow describe the visual redesign and the spatial
verification. The ITEM_3D model rebuild (`298a538`, `e7cd4d4`, `c02bb64`), the
diagnostic framing pass (`de7d5fd`) and the collider-shadowing fix (`743b1c3`) are
recorded in `Docs/Verification/ITEM_3D_REBUILD_RECORD.md` and
`Docs/Verification/DIAGNOSTIC_FRAMING_RECORD.md`.

## Participant flow, telemetry and regression cover - 2026-08-08

- `ResearcherTaskControls` no longer gates itself on `developmentMode`, which is what
  restores every operator control - Pause, Resume, Retry, Abort, Safety Stop and
  Continue to Next Task - in a real session. Reset Task and Skip Training still hide
  outside development mode and both underlying methods self-gate. The panel is the
  operator surface on the PC running Quest Link.
- `TaskStatusBoard` shows a finished notice in the participant's language once the
  task ends, and carries **no control**. The participant removes the headset between
  the two tasks for NASA-TLX, so a Continue button in VR would let them load the
  second task before the questionnaire was administered. Advancing is the
  researcher's.
- The session now **ends in the finished task scene** rather than loading
  `ResearcherSetup`. That scene has no XR Origin and its `Setup Camera` carries no
  `TrackedPoseDriver`, so loading it at the end put a participant who was still
  wearing the headset in front of a view that does not follow their head, and showed
  them the configuration screen with the participant code on it.
  `ResearchSessionManager.ReturnToSetup()` is the researcher's route back, on a
  desktop button that appears once the session is complete.
- The `OnMouseEnter`/`OnMouseDown` path on interactables and information sources is
  gated on `developmentMode`. The game view sits on the researcher's monitor during a
  Quest Link session, and a stray click over it wrote hovers and grabs into the
  participant's event stream under `interactor=mouse`.
- `TaskReset` was being dropped. `ResetDevelopmentTask` logged it before `EndTask`, and
  a task that had already ended closed its writer when it ended, so resetting a
  finished task wrote `Dropped event before task writer: TaskReset` to the technical
  log and nothing to the event stream. It is now logged after `StartAttempt`, in the
  attempt it creates, carrying the previous attempt id.
- `task_summary.csv` gained `incorrect_tool_selected_count`,
  `incorrect_component_interaction_count`, `device_test_failed_count` and
  `unsuccessful_action_event_count`. `derivation_version` is `1.1`; the raw event
  schema is unchanged at `1.0` and the columns are appended at the end.
- Runtime errors and exceptions now reach `technical_log.txt`, and the first fifty
  exceptions per session also become `TechnicalError` rows.
- `VRTraining` still had the collider-shadowing bug that `743b1c3` fixed in the two
  task scenes: only the workstation builders called `BindOwnColliders`, so
  `training.training-cylinder` kept an empty collider list.
  `TrainingSceneBuilder` now binds too. All three training stable ids are unchanged.
- `TrainingDevelopment.availableComponents` declared `training.cube-a/-b/cylinder`
  while the scene logs `training.training-cube-a/-b/-cylinder`. The manifest is read
  only by the visual validator and the scene ids are the ones that reach the logs, so
  the manifest was corrected. The validator now reports ALL SCENES PASS with no
  warnings at all, for the first time.

## Executed evidence - 2026-08-08

Editor-side only. No Quest hardware run and no human pilot data.

- `Docs/Verification/Scene_Integrity_Tests.txt` - 7/7 PASS. Stable id uniqueness,
  collider ownership, the required repair object, both task orders resolving to
  enabled build scenes, information-source localisation, work-order localisation
  wiring, and English task fallbacks.
- `Docs/Verification/Full_Flow_Walkthrough_ComputerThenFan.txt` and
  `Full_Flow_Walkthrough_FanThenComputer.txt` - both WALKTHROUGH PASSED. Setup,
  training, first task, second task and end of session, driven with
  `developmentMode` off, across the scene loads that separate them.
- `Docs/Verification/Runtime_Checks_ComputerRepairTask.txt` and
  `Runtime_Checks_FanRepairTask.txt` - fail, repair, pass, reset, plus the Continue
  button appearing after completion and disappearing after reset.
- `VR Maintenance Research/Run Foundation Edit Mode Tests` - 6/6 PASS.
- Three consecutive walkthrough sessions wrote three separate `Sessions` folders with
  no overwriting. Every technical log is empty: no dropped events, no exceptions.
- Zero console errors throughout. The remaining warnings are XRI simulator haptics
  and a desktop audio driver fallback, both from running without a headset attached.

## Earlier phases

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
