# XR UI and locomotion design

## Fixed spatial surfaces

- Source selector: four equal 0.58 m x 0.28 m cards in a left-side 2 x 2 station.
- Reader: one fixed 0.90 m x 0.53 m panel. Switching source replaces content there; it never follows the HMD.
- Task status: fixed right-side 0.95 m x 0.34 m world-space card.
- Training instructions: fixed left-side 1.05 m x 0.52 m world-space board.

`ComfortFollowPanel` is not added to any of these surfaces. The F9 researcher panel is an overlay only in development mode and is hidden by default. The XRI simulator HUD is suppressed at startup and only F10 can reveal it during a development session.

## XR path

Runtime diagnostic verified one EventSystem, one XRUIInputModule, no competing legacy module, and one tracked-device graphic raycaster. Source cards remain `XRSimpleInteractable` targets with equal colliders and equal visual emphasis; hover changes only a neutral blue accent and small scale amount.

## Locomotion

The existing grounded desktop walker retains horizontal WASD movement and gravity while disabling simulator vertical movement. The physical tracking path remains unverified pending Quest 3 hardware.

## Runtime evidence

The 2026-08-03 Computer Play Mode run showed the compact status card, hidden simulator HUD, fixed source reader, and one active reader after a Manual -> Text Guide switch. See `TEST_REPORT.md`.
