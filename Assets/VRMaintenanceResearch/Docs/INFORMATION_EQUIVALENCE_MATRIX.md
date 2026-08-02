# Information Equivalence Matrix

## Current development baseline

All study-facing information sources are independent, concurrent, closed by default, and have no recommended or highlighted source. The primary source objects are at four fixed slots for both task scenes: A `(-3.30, 1.00, 3.00)`, B `(-1.10, 1.00, 3.00)`, C `(1.10, 1.00, 3.00)`, and D `(3.30, 1.00, 3.00)`. When opened, every content panel uses the same yaw-only comfort-follow behavior: 1.35-1.45 m ahead, 0.10-0.12 m below eye level, recentring after a 25-degree horizontal view change and pausing during pointer/drag input. This keeps display depth and height comparable across Computer and Fan.

| Task | Slot | Type | Valid asset | Essential facts | English words | Navigation / media | Visual detail |
|---|---:|---|---|---|---:|---|---|
| Computer | A | Product manual | `ComputerProductManual_v2` | Disconnect; trace PSU/main connector; reconnect motherboard power; neutral power-button test | 35 | Previous / Next controls; `Information/Manuals/Computer_Maintenance_Manual.pdf` | Localized text panel |
| Computer | B | Text troubleshooting guide | `ComputerTextGuide_v2` | Same four canonical facts | 35 | Previous / Next controls | Localized text panel |
| Computer | C | Instructional video | `ComputerVideo_v2` | Same four canonical facts | 35 | Play, Pause, Stop, Seek +10 | `Video/Final/ComputerInstructional_60s.mp4`, 60 s, 1280x720 H.264, burned captions |
| Computer | D | Visual step-by-step guide | `ComputerVisualGuide_v2` | Same four canonical facts | 35 | Previous / Next controls | `Information/VisualGuides/Computer_Visual_Guide.png` |
| Fan | A | Product manual | `FanProductManual_v2` | Disconnect; trace cord/fuse holder; install working fuse; neutral speed-control test | 36 | Previous / Next controls; `Information/Manuals/Fan_Maintenance_Manual.pdf` | Localized text panel |
| Fan | B | Text troubleshooting guide | `FanTextGuide_v2` | Same four canonical facts | 36 | Previous / Next controls | Localized text panel |
| Fan | C | Instructional video | `FanVideo_v2` | Same four canonical facts | 36 | Play, Pause, Stop, Seek +10 | `Video/Final/FanInstructional_60s.mp4`, 60 s, 1280x720 H.264, burned captions |
| Fan | D | Visual step-by-step guide | `FanVisualGuide_v2` | Same four canonical facts | 36 | Previous / Next controls | `Information/VisualGuides/Fan_Visual_Guide.png` |

## Verification and approval boundary

The active scene references are the 60-second files under `Video/Final`; each is derived from the self-authored baseline and has burned English instructional captions. There is no audio track. `VideoPlayed`, `VideoSeeked`, `VideoPaused`, `VideoStopped`, `VideoCompleted`, and source close events are logged. The older MP4s remain unreferenced rather than being deleted outside a Unity-managed deletion action. `InformationSourceController` selects the Thai, Japanese, or English title/body from the session language at open time; all eight `_v2` assets now carry non-empty localized fields.

Before study deployment, advisors must approve final task facts, Thai/Japanese translations, source word-count comparability, visual density, media accessibility, and source-layout assignment. The current material is a complete development content set, not an advisor sign-off record.
