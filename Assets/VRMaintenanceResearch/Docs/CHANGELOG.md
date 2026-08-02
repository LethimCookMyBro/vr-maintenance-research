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

## 2026-08-02 - Visual redesign (branch `visual-polish-claude`)

- Added a shared `LabEnvironment` prefab (room shell, lighting rig, workbench, trays, control pedestal, storage unit, information station) used by all four build scenes.
- Added 17 shared URP materials and 16 unlit icon materials; every object previously shared one built-in `Lit` instance.
- Imported the project's first external assets: 2 Poly Haven CC0 textures and 16 Kenney CC0 icons, recorded in `ThirdParty/THIRD_PARTY_ASSETS.md`.
- Rebuilt `ResearcherSetupController` as a two-column TextMeshPro desktop interface; behaviour, validation and the pseudonymity warning are unchanged.
- Rebuilt `TrainingInstructions` as a fixed world-space board with live progress indicators and a Continue button gated on the three training requirements.
- Added `TaskStatusBoard`, a read-only world-space mirror of task state and attempt number.
- Restyled `ResearcherTaskControls`: collapsed by default behind a handle or F9, Safety Stop separated and given the reserved warning treatment; every control retained.
- Added `Editor/ResearchSceneCapture.cs` for reproducible participant-viewpoint screenshots.
- Fixed: participant spawning inside the device, two magenta Standard-shader objects, the Fan body clipping the floor, the zero-scale training canvas, the training source having no content panel, reversed information-control reading order, and unequal auto-sized source captions.
- Re-authored device and component transforms onto a 0.92 m workbench. All stable research IDs, scripts, interactable and collider components, task references, logging and completion logic are unchanged; every spatial change is tabulated in `PROTOCOL_CHANGE_LOG.md`.
