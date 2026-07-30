# Implementation Plan

1. **Foundation** — retain the stock XRI project unchanged; isolate all additions under `Assets/VRMaintenanceResearch`; reuse the official XR Origin and simulator prefabs by reference only.
2. **Session and data** — validate researcher-entered configuration; persist one session manager; write manifest, append-only raw task events, movement samples, technical log, and reproducible summaries under `Application.persistentDataPath/VRMaintenanceResearchData`.
3. **Task content** — use ScriptableObject definitions for Computer and Fan faults, layouts, time limits, information sources, localization-ready placeholders, and stable IDs.
4. **Scenes** — create Researcher Setup, neutral training, Computer, and Fan scenes via Unity Editor APIs; each task scene has one official XR Origin/simulator setup, not duplicated managers.
5. **Research interactions** — present all four neutral information sources concurrently; log source, component, device-test, retry, timeout, abort, and low-activity events without inferring traits.
6. **Validation** — compile in Unity, run focused EditMode/PlayMode tests, exercise development sessions in both task orders, inspect generated CSV output, and record limitations.

## Deliberate development defaults
- English information content is placeholder material and is marked as such.
- Computer fault: disconnected motherboard power connector. Fan fault: replaceable fuse.
- Time limits and source-layout assignments remain configurable; no participant assignment schedule is encoded.
- Grayboxes are retained until visual improvements are shown not to change task difficulty.
