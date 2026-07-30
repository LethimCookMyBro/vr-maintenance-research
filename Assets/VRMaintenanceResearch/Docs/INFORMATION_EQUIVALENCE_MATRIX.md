# Information Equivalence Matrix

## Current development baseline

All study-facing information sources are independent, concurrent, closed by default, and have no recommended or highlighted source. The primary source objects are at four fixed slots for both task scenes: A `(-3.30, 1.00, 3.00)`, B `(-1.10, 1.00, 3.00)`, C `(1.10, 1.00, 3.00)`, and D `(3.30, 1.00, 3.00)`. When opened, every content panel is displayed at the matching slot x-coordinate with `(y=1.65, z=1.50)` and faces the participant. This preserves equal display depth and height across Computer and Fan.

| Task | Slot | Type | Valid asset | Essential facts | English words | Navigation / media | Visual detail |
|---|---:|---|---|---|---:|---|---|
| Computer | A | Product manual | `ComputerProductManual_v2` | Inspect power path; identify/reconnect motherboard power connector; test with power button | 24 | Previous / Next controls | Textual development panel |
| Computer | B | Text troubleshooting guide | `ComputerTextGuide_v2` | Same three approved development facts | 24 | Previous / Next controls | Textual development panel |
| Computer | C | Instructional video | `ComputerVideo_v2` | Same three approved development facts | 24 | 20.000 s silent self-authored MP4; Play, Pause, Stop, Seek +10 | RenderTexture video panel |
| Computer | D | Visual step-by-step guide | `ComputerVisualGuide_v2` | Same three approved development facts | 24 | Previous / Next controls | Development visual-panel placeholder |
| Fan | A | Product manual | `FanProductManual_v2` | Inspect power path; identify/install working replaceable fuse; test with speed control | 25 | Previous / Next controls | Textual development panel |
| Fan | B | Text troubleshooting guide | `FanTextGuide_v2` | Same three approved development facts | 25 | Previous / Next controls | Textual development panel |
| Fan | C | Instructional video | `FanVideo_v2` | Same three approved development facts | 25 | 20.000 s silent self-authored MP4; Play, Pause, Stop, Seek +10 | RenderTexture video panel |
| Fan | D | Visual step-by-step guide | `FanVisualGuide_v2` | Same three approved development facts | 25 | Previous / Next controls | Development visual-panel placeholder |

## Verification and approval boundary

The active scene references are `ComputerMaintenanceDevelopmentBaseline.mp4` and `FanMaintenanceDevelopmentBaseline.mp4`; each is self-authored, contains no third-party media, and was rendered and controlled in Play Mode. `VideoPlayed`, `VideoSeeked`, `VideoPaused`, `VideoStopped`, `VideoCompleted`, and source close events are logged. The older non-baseline development MP4s remain unreferenced rather than being deleted outside a Unity-managed deletion action.

Before study deployment, advisors must approve final task facts, Thai/Japanese translations, source word-count comparability, visual density, media accessibility, and source-layout assignment. Empty Thai/Japanese title/content fields are intentionally configurable placeholders, not translated study material.
