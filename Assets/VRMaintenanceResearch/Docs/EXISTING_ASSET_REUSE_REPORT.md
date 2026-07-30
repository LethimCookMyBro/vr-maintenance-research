# Existing XRI Asset Reuse Report

| Classification | Source | Research use | Decision |
|---|---|---|---|
| Reuse directly | `Assets/Samples/XR Interaction Toolkit/3.4.0/Starter Assets/Prefabs/XR Origin (XR Rig).prefab` | One XR Origin per standalone research scene | Direct reference; no copied manager stack |
| Reuse directly | `Assets/Samples/XR Interaction Toolkit/3.4.0/XR Interaction Simulator/XR Interaction Simulator.prefab` | Keyboard-and-mouse development simulation | Direct reference; controls documented from installed assets |
| Reuse directly | Starter Assets interactors and UI samples | Grab, ray, socket and world-space UI capability | Direct reference where scene setup requires it |
| Custom research content | Primitives created under `Assets/VRMaintenanceResearch` | Graybox training/computer/fan task objects | New content with stable research IDs |
| Reference only | `Assets/XRI_Examples/Scenes/XRI_Examples_Main.unity` | Existing example patterns | Never modified by this project |
| Not needed | Full example environment/station ring | Research tasks need focused workspaces | Do not copy it |

No external assets have been imported. The prototype remains graybox so visual changes cannot alter experimental difficulty.
