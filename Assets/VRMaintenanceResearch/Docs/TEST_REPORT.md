# Test Report - 2026-07-30

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

No research-script Console error was present after the final natural video completion check or the current Computer -> Fan flow. The external Input System/XInput device-layout discovery error has a `com.unity.inputsystem` package stack frame, not a research script. Other warnings are Adaptive Performance initialization, XR audio/haptics simulator behavior, Unity AI account/relay connectivity, and a Windows Media Foundation color-primaries fallback for the self-authored development MP4. These are recorded separately from research logic.

## Windows build attempt (not a validation pass)

`BuildPipeline.BuildPlayer` was invoked for the five enabled scenes with `StandaloneWindows64`, `Development`, and `AllowDebugging`. The Unity MCP request did not return within 300 seconds; `Builds/Windows` contained no output files, and `Editor.log` recorded only the Performance Test build preprocessor writing `Assets/Resources/PerformanceTestRunInfo.json` and `PerformanceTestRunSettings.json`. It contains no matching build-success or build-failure result. The generated JSON files remain untracked and untouched. Two incidental XRI renderer-data edits caused during the attempt were restored to their tracked baseline. No Windows executable or build-run result is claimed.

## Not validated

Physical Meta Quest 3 operation, participant usability, approved translations, ethics/research approval, and a Windows executable have not been validated in this report.
