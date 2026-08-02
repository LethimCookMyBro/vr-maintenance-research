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
