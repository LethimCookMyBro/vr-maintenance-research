# Data dictionary

`RESEARCH_DATA_DICTIONARY.md` remains the canonical field-level schema for exported CSV files. This companion records the verified invariants relevant to the spatial polish.

| Field/invariant | Meaning | Verified status |
|---|---|---|
| Stable research ID | Protocol identity of a functional root; visual children are not replacements. | Computer 13, Fan 15, Training 3 preserved. |
| `coordinate_space_id` | Stable task-local coordinate convention rooted at `TaskCoordinateRoot`; metres. | Existing implementation retained; no schema change. |
| First meaningful action | Deliberate source selection, grab, installation, inspection, or device test; not passive hover. | Direct foundation test retained. |
| Raw hover events | Controller-specific enter/exit events, retained as raw events. | Existing logging preserved. |
| Information events | Source opened/closed, page changed, video play/pause/stop/seek/completed. | Source switching and page change exercised in 2026-08-03 Play Mode. |
| Task summary | One row per completed task/attempt using existing CSV escape and invariant-number rules. | Direct foundation test retained. |

No CSV column, event name, coordinate convention, or stable research ID was changed by commit `ac8837e`.
