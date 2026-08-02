# Test Report - 2026-08-02

## Executed Edit Mode tests

The compiled menu command `VR Maintenance Research/Run Foundation Edit Mode Tests` ran successfully in the Unity Editor (not via the blocked TestRunner API). Console evidence: `[ResearchFoundationDirectTestRunner] PASS 6 tests`.

1. `SessionConfigRejectsRecordingWithoutConsent`
2. `CsvUsesInvariantNumbersAndEscapesInternationalText`
3. `DevelopmentLoggerCreatesSeparateSessionFiles`
4. `LoggerCanStartAnotherSessionAfterClosing`
5. `TaskEventTimestampIsRelativeToTaskStart`
6. `SessionSummaryContainsOneRowPerCompletedTask`

## Executed Play Mode validations

- Real baseline video playback: prepared, advancing frames, visible on the participant-facing source panel, and controlled through the shared source controller.
- Real video completion lifecycle: source open, play, 19-second seek, `VideoCompleted`, close; the post-run Console had zero errors.
  - Evidence: `C:\Users\User\AppData\LocalLow\Unity Technologies\XRI Examples\VRMaintenanceResearchData\Development\20260730T144236Z_dfa131f7`
- Current-code training-enabled Computer -> Fan flow: Training, Computer, and Fan each completed; Computer and Fan both recorded failed test, incorrect repair, information return, retry, successful test, and completion. The manifest says `Completed` and `task_summary.csv` has one header plus three rows.
  - Evidence: `C:\Users\User\AppData\LocalLow\Unity Technologies\XRI Examples\VRMaintenanceResearchData\Development\20260730T144437Z_39650a2d`
- Earlier verified Fan -> Computer full flow after the logging-summary fix:
  - Evidence: `C:\Users\User\AppData\LocalLow\Unity Technologies\XRI Examples\VRMaintenanceResearchData\Development\20260730T141644Z_eb059e35`
- Low-activity suppression while a source is open:
  - Evidence: `C:\Users\User\AppData\LocalLow\Unity Technologies\XRI Examples\VRMaintenanceResearchData\Development\20260730T141007Z_03faef21`
- Prior task-relative timestamp and source-control event validation:
  - Evidence: `C:\Users\User\AppData\LocalLow\Unity Technologies\XRI Examples\VRMaintenanceResearchData\Development\20260730T140349Z_8fad3f92`
  - Evidence: `C:\Users\User\AppData\LocalLow\Unity Technologies\XRI Examples\VRMaintenanceResearchData\Development\20260730T141400Z_6ee7cd03`

## Console classification

- Research scripts: zero Console errors. The recovered direct runner logged `PASS 6 tests` again after the build and runtime checks.
- Pre-existing or external warnings: Unity AI account/subscription and Codex executable-signature messages, Performance Test JSON rewrite, 486 package shader/build warnings, Adaptive Performance provider initialization, OpenXR form-factor diagnostics without a headset, and simulator haptics/audio messages. The non-destructive scene validator also emitted one editor-only last-scene unload warning.
- Input System/XInput device-layout: the current Unity Console filters returned zero matching entries. No package or version change was made.
- The runtime log records OpenXR `XR_ERROR_FORM_FACTOR_UNAVAILABLE` on this desktop without a headset; it is separate from the research scripts and keeps physical Quest 3 validation pending.

## Windows build preparation and recovery - 2026-08-02

- Unity MCP reconnected to the open project. The Editor was responsive, idle, out of Play Mode, and active on `ResearcherSetup`; all five required build scenes remained enabled at indices 0-4.
- The resumed IL2CPP build fully finished and stopped at the missing Visual Studio C++ toolchain and Windows SDK. No IL2CPP executable was produced; Unity and package versions were unchanged.
- A temporary in-memory `Mono2x` fallback produced the Windows Development + Allow Debugging build, then restored the serialized Standalone backend to `IL2CPP`. Build result: `Succeeded`, 0 errors, 486 warnings, build report size `337087358` bytes, duration about 95 seconds.
- Verified artifact: `D:\TMU_VR\XR-Interaction-Toolkit-Examples\Builds\Windows\VRMaintenanceResearch\VRMaintenanceResearch.exe` (667136 bytes) and `VRMaintenanceResearch_Data` (185 files). The two build-created XRI renderer edits were restored and both working hashes match `HEAD`.
- Direct launch proof: the visible build loaded ResearcherSetup, a mouse click started VRTraining, foreground keyboard `I` moved the XRI simulator view, and mouse development controls advanced Computer then Fan. The build returned to ResearcherSetup after the Fan step and was closed cleanly.
- Runtime evidence was written under `C:\Users\User\AppData\LocalLow\Unity Technologies\XRI Examples\VRMaintenanceResearchData\Development\20260802T065545Z_e3534f03`: `session_manifest.csv` is `Completed`, `task_summary.csv` contains Training `Completed` plus Computer and Fan `Aborted` rows, and Training/Computer/Fan event CSVs are present.
- Screenshots and the player log are under `D:\TMU_VR\XR-Interaction-Toolkit-Examples\Builds\Windows\VRMaintenanceResearch` (`recovery-*.png`, `keyboard-before2.png`, `keyboard-after2.png`, and `VRMaintenanceResearch-runtime.log`).
- The eight invalid untracked v1 information-source assets remain untouched and unreferenced. Task definitions still reference only the eight valid `_v2` sources.

## Not validated

Physical Meta Quest 3 operation, participant usability, approved translations, ethics/research approval, and an IL2CPP release build remain pending.

---

# Visual redesign validation - 2026-08-02 (branch `visual-polish-claude`)

Everything in this section was actually executed. Nothing is inferred.

## Console

- Unity Console errors before the work started: **0**.
- Unity Console errors after every batch and at the end of the work: **0**.
- One compiler warning (`TMP_Text.enableWordWrapping` obsolete) was introduced and then fixed.

## Play Mode smoke run A - Computer then Fan, training required

Participant code `FINAL_CF`, development mode, XR simulator.
Evidence: `C:\Users\User\AppData\LocalLow\Unity Technologies\XRI Examples\VRMaintenanceResearchData\Development\20260802T083541Z_44a43476`

Executed and observed:

- `ResearcherSetup` built the redesigned TextMeshPro canvas (45 TMP labels, 1 EventSystem, 1 camera) and started the session.
- `VRTraining` loaded; the world-space training board was built; the Continue button reported `interactable = False` before the training requirements were met (gating works).
- The neutral training information source opened its new content panel.
- `ComputerRepairTask`: all four information sources were opened, paged and closed; the video source was played, seeked and paused.
- Wrong part (`computer.ram`) then device test -> task stayed `Active` and logged `IncorrectComponentInteraction` + `DeviceTestFailed`.
- Pause, Resume, Retry.
- Correct part (`computer.main-power-connector`) then device test -> `Completed`.
- `FanRepairTask`: all four sources exercised; wrong fuse -> `Active`; working fuse -> `Completed`.
- Session ended and returned to `ResearcherSetup`.

CSV output verified on disk:

| File | Result |
|---|---|
| `session_manifest.csv` | 1 row, `session_completion_status = Completed`, `logging_status = active` |
| `task_summary.csv` | 3 rows: Training / Computer / Fan, all `Completed` |
| `Training/events.csv` | 5 rows, `Training/movement.csv` 1593 rows |
| `Computer/events.csv` | 30 rows, `Computer/movement.csv` 420 rows |
| `Fan/events.csv` | 27 rows, `Fan/movement.csv` 321 rows |

## Play Mode smoke run B - Fan then Computer, training skipped

Participant code `FINAL_FC`.
Evidence: `C:\Users\User\AppData\LocalLow\Unity Technologies\XRI Examples\VRMaintenanceResearchData\Development\20260802T083741Z_66a5f85e`

- `FanRepairTask` loaded first (task-order selection honoured), completed on the working fuse.
- `ComputerRepairTask` loaded second; `Reset Task` was exercised (attempt 1 recorded as `Reset`, attempt 2 started `Active`), then completed.
- `task_summary.csv` recorded Fan `Completed`, Computer attempt 1 `Reset`, Computer attempt 2 `Completed` — one row per attempt, as before.
- `session_manifest.csv`: `Completed` / `active`.

## Stable research IDs after the redesign

Enumerated at runtime from `ResearchInteractable` components; identical to the baseline set.

- Computer (13): `computer.case`, `computer.cooling-fan`, `computer.external-power-cable`, `computer.internal-cable`, `computer.main-power-connector`, `computer.motherboard`, `computer.non-target-module`, `computer.power-button`, `computer.psu`, `computer.psu-switch`, `computer.ram`, `computer.side-panel`, `computer.tool.screwdriver`
- Fan (15): `fan.blade`, `fan.body`, `fan.fastener`, `fan.faulty-fuse`, `fan.front-cover`, `fan.fuse-holder`, `fan.internal-wire`, `fan.motor-module`, `fan.non-target-module`, `fan.power-cord`, `fan.power-plug`, `fan.power-switch`, `fan.speed-selector`, `fan.tool.screwdriver`, `fan.working-fuse`
- Training (3): `training.training-cube-a`, `training.training-cube-b`, `training.training-cylinder`

## Scene integrity check (all four build scenes)

| Scene | XR Origin | Interaction Manager | EventSystem | Cameras | Lights | Lab Environment | ResearchInteractables | Renderers | Triangles | Unique materials |
|---|---|---|---|---|---|---|---|---|---|---|
| ResearcherSetup | 0 | 0 | 0 (built at runtime) | 1 | 2 | 1 | 0 | 52 | 1,084 | 12 |
| VRTraining | 1 | 1 | 1 | 1 | 2 | 1 | 3 | 91 | 17,446 | 18 |
| ComputerRepairTask | 1 | 1 | 1 | 1 | 2 | 1 | 13 | 156 | 19,272 | 26 |
| FanRepairTask | 1 | 1 | 1 | 1 | 2 | 1 | 15 | 159 | 19,988 | 25 |

A duplicate `Lab Environment` instance was found during validation (the prefab root was renamed on save, so the de-duplication check missed it) and removed; the counts above are after that fix.

## Windows build

`BuildPipeline.BuildPlayer`, Windows 64-bit, Development + Allow Debugging, `Mono2x` fallback (the IL2CPP toolchain is still unavailable on this machine; the serialized project backend was restored to `IL2CPP` immediately after the build).

- Result: **Succeeded**
- Errors: 0
- Warnings: 486 (package/shader warnings, same class and count as the recovered baseline)
- Size: 325 MB
- Duration: 00:05:59
- Output: `D:\TMU_VR\XR-Interaction-Toolkit-Examples\Builds\Windows\VRMaintenanceResearch\VRMaintenanceResearch.exe`

The XR `preloadedAssets` entries that the build post-processor strips from
`ProjectSettings.asset` were restored so the working tree matches `HEAD` for that field.

## Not executed

- The built Windows player was **not** relaunched after this redesign; the build itself succeeded but the redesigned scenes were exercised in the Editor, not in the standalone artifact.
- Grab, socket-placement and controller-ray interaction were exercised through the interactable API and confirmed present, but were **not** driven by hand through the XR simulator in this pass.
- No Meta Quest 3 hardware run occurred. No Quest performance claim is made.
