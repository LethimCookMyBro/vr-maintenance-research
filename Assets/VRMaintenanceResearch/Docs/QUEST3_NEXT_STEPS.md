# Meta Quest 3 Next Steps

## Current status - 2026-08-02

Physical Quest 3 validation remains pending. The recovered Windows development build verified the desktop simulator and logging path; it does not establish headset tracking, controller behavior, or OpenXR hardware readiness.

1. Create a non-development pseudonymous pilot configuration and confirm the `Sessions` output path is writable on the intended PC.
2. Verify OpenXR runtime, Quest Link/Air Link connection, controller tracking, locomotion, grab, socket, ray selection, UI readability, haptics, and audio.
3. Confirm headset/left/right pose rows have valid coordinates throughout a timed pilot, not only at startup.
4. Run both task orders with approved content and record any comfort, safety-stop, accessibility, or simulator-to-hardware differences.
5. Review generated manifest, raw events, movement rows, and summaries before pilot data are retained.
6. Never describe Quest 3 validation as complete until these physical checks are observed and recorded.