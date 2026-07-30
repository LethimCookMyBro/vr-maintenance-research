# Research Data Dictionary

## Files
- `session_manifest.csv`: configuration and environment snapshot for one session.
- `session_events.csv`: session lifecycle events.
- `<Task>/events.csv`: append-only observable interaction events.
- `<Task>/movement.csv`: headset, left-controller, and right-controller pose samples.
- `task_summary.csv`: transparent derivations from task events.
- `technical_log.txt`: logging failures only; it is not a participant narrative field.

## Event fields
`event_sequence_number` is monotonic within a session. `timestamp_from_task_start_seconds` is measured from the session clock for comparability with task records. `object_id`, `information_source_id`, `source_slot`, `layout_id`, and `task_content_version` are stable configuration keys. Position and rotation values use the documented `task-local` coordinate-space label.

## Movement fields
`tracking_valid` is true only when the source transform exists. `simulator_mode` records whether poses originated from the XRI simulator configuration. `sampling_frequency_hz` is the configured target rate, not a guarantee that an editor automation frame sequence achieved that rate.

## Derived summary fields
First action/source timestamps, source switching, repeat access, source-open durations, unsuccessful actions, retries, tests, low-activity periods, completion time, timeout, and abort are derived from raw task events. Preserve raw event files; regenerate summaries if derivation rules change.

## Privacy
Use pseudonymous participant codes only. First-person recording remains disabled unless recorded consent is set. The runtime performs basic technical-note checks for names, email-like markers, and phone-like numbers; researchers remain responsible for reviewing entries.