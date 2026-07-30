# Changelog

## 2026-07-30 - recovered research foundation
- Repaired class/file layout for information-source definitions and regenerated valid `_v2` ScriptableObjects.
- Reassigned serialized task IDs after the Training enum insertion.
- Added and verified Researcher Setup, neutral Training, Computer, and Fan scenes using official XRI setup/simulator references.
- Added session/event/movement logging, summaries, development controls, and compact Edit Mode regression coverage.
- Fixed closed-session restart state and simulator controller pose binding.
- Ran Computer, Fan, Training, full ComputerThenFan, and FanThenComputer-selection Play Mode validations.
- Added operational research and test documentation.
## 2026-07-30 recovery and QA continuation
- Corrected task-relative event/movement timestamps and multi-task `task_summary.csv` append behavior.
- Added source page/video controls, actual self-authored development video playback, `VideoCompleted` lifecycle logging, and participant-facing source-panel geometry.
- Added a direct compiled Edit Mode runner for six existing NUnit foundation tests; all six passed.
- Verified a current training-enabled Computer -> Fan session and documented exact local evidence paths.
