# Meta Quest 3 Next Steps

## Current status - 2026-08-08

Physical Quest 3 validation is still pending and nothing below is a hardware claim.
What changed is that the software side no longer blocks it.

Verified editor-side:

- A participant can complete setup, training, both tasks and the end of session
  without leaving the headset, with `developmentMode` off. Both task orders pass:
  `Docs/Verification/Full_Flow_Walkthrough_*.txt`.
- Every button on that path sits on a world-space canvas with a
  `TrackedDeviceGraphicRaycaster` and a graphic that accepts raycasts. **Whether a
  controller ray actually lands on them is a hardware check and is not established.**
- OpenXR configuration for Android is correct for this headset: Meta Quest Support
  enabled with Quest 3 (`eureka`) targeted, Oculus Touch and Meta Quest Touch Plus
  controller profiles both enabled, IL2CPP, ARM64, Vulkan, Linear colour, URP, single
  pass instanced. The Standalone profile set for Quest Link is enabled too, at
  multi-pass.
- **An Android player was actually produced**: `Succeeded`, 179 MB APK, 46 minutes.
  `Docs/Verification/Quest3_Build.txt`. It was not installed and not run — no headset
  was connected. The three errors in that build are one Unity OpenXR package bug
  (`MetaQuestFeature.cs:554`), not a project setting; the build's validation pass was
  abandoned as a result, so validation was re-run separately and is clean for both
  Android and Standalone: `Docs/Verification/OpenXR_Validation.txt`.
- The active build target was returned to Windows afterwards, and the platform state
  the Android build wrote into version-controlled files was reverted.

Still open, and only a headset can close them:

- Headset and controller tracking, ray aiming, grab, socket placement, poke, haptics
  and audio.
- Text legibility at the real display resolution, including Thai and Japanese.
- Comfort, locomotion and reachability for participants of different heights.
- Frame timing. No performance claim has ever been measured on hardware.

## Earlier status - 2026-08-02

Physical Quest 3 validation remains pending. The recovered Windows development build verified the desktop simulator and logging path; it does not establish headset tracking, controller behavior, or OpenXR hardware readiness.

1. Create a non-development pseudonymous pilot configuration and confirm the `Sessions` output path is writable on the intended PC.
2. Verify OpenXR runtime, Quest Link/Air Link connection, controller tracking, locomotion, grab, socket, ray selection, UI readability, haptics, and audio.
3. Confirm headset/left/right pose rows have valid coordinates throughout a timed pilot, not only at startup.
4. Run both task orders with approved content and record any comfort, safety-stop, accessibility, or simulator-to-hardware differences.
5. Review generated manifest, raw events, movement rows, and summaries before pilot data are retained.
6. Never describe Quest 3 validation as complete until these physical checks are observed and recorded.