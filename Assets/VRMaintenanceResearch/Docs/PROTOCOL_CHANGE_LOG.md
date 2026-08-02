# Protocol Change Log

| Area | Previous material may describe | Current development direction | Status / advisor follow-up |
|---|---|---|---|
| Task A | Component replacement only | Computer maintenance with inspection, information seeking, diagnosis, repair attempts, error correction, and retries | Confirm final fault and completion rule before data collection |
| Task B | Fault diagnosis only | Electric-fan maintenance with the same free interaction cycle | Confirm final fault and completion rule before data collection |
| Information | One internal manual / forced usage | Four independent, concurrent source types; no recommended source or forced order | Review Thai/Japanese/English equivalence before study use |
| Validation | Inspect action as main validation | Observable interaction and device-test outcomes, with raw events retained | Confirm final stop and scoring rules |
| Timing | Earlier fixed timings | Per-task configurable limits; development values are provisional | Approve research limits and timer visibility |
| Analytics | Broad learner labels risk | Record observable behavior only; derive transparent counts/timestamps from raw events | No personality or nationality conclusions are automated |

## Documents requiring corresponding updates before data collection
- Participant instructions and researcher procedure.
- Thai/Japanese translations and approved information content.
- Ethics/consent materials, especially first-person recording language.
- Counterbalancing schedule and source-layout assignment schedule.

## Visual redesign, 2026-08-02 (branch `visual-polish-claude`)

The visual redesign changed no research behaviour, but it did change the physical
layout of the two task scenes. Everything below is a **spatial** change; the
logging schema, stable research IDs, completion conditions, retry behaviour and
the four information-source conditions are unchanged.

| Item | Before | After | Reason |
|---|---|---|---|
| Participant start pose | `(0, 0, 0)` — inside the device volume | `(0, 0, -1.6)` in Training, Computer and Fan | The XR Origin previously spawned inside the Desktop Case / Fan Body. The new pose is identical in all three scenes, so cross-task comparability is preserved. |
| Device and component transforms | Free-floating graybox primitives at mixed scales | Human-scale equipment resting on a shared 0.92 m workbench | The old scene had a 2.4 m PC case and a 3 m fan clipping through the floor. Every stable ID, script, interactable and collider component is preserved; only position/rotation/scale changed. |
| Information-source tiles and panels | `(x, 1.0, 3.0)` tiles, `(x, 1.65, 1.5)` panels | **Unchanged** | Deliberately not moved: the transforms are recorded against `information_source_layout_id`, and moving them would alter relative salience. |
| Information-source control buttons | Next left of Prev; Seek left of Play | Mirrored about the panel centre so controls read left-to-right | Reading-order defect; positions are symmetric and identical across all four sources and both tasks. |
| Fan front guard | Mounted in front of the blade | Removed and laid on the bench beside the fan | With the guard mounted, its collider blocked controller rays to `fan.blade`. The "partly disassembled" arrangement keeps every component ray-reachable and reads as a maintenance scene. |
| Computer internals | Scattered in mid-air in front of a 2.4 m case | Laid out on the bench beside a desktop tower | Same reason: the internals must stay individually reachable, and a solid case shell would occlude them. |
| Status/task board | None | Read-only world-space board above the information station | Required by the brief. It mirrors `MaintenanceTaskController.State` and the attempt number only; it writes no events and never names the faulty component. |

### Advisor follow-up

- Confirm the new participant start pose (2.05 m from the bench edge) before data collection.
- Confirm that laying the fan guard and the computer internals on the bench does not change the intended diagnostic difficulty.
- Movement CSV coordinates from sessions recorded before 2026-08-02 are not spatially comparable with later ones because the origin offset changed. The schema is unchanged.
