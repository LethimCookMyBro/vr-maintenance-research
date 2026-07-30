# VR Maintenance Research Tool

## Purpose
A configurable Unity development prototype for two VR maintenance tasks. It records observable interactions and simulator poses into local CSV files; it does not infer ability, personality, nationality, or clinical traits.

## Scene flow
1. `ResearcherSetup` - researcher enters a pseudonymous participant code and configuration.
2. `VRTraining` - neutral simulator familiarization: grab gray objects, socket an object, open one neutral source, reset if needed.
3. `ComputerRepairTask` and `FanRepairTask` - task order follows `TaskOrder`.

## Development run
Open `Assets/VRMaintenanceResearch/Scenes/ResearcherSetup.unity`, enter only a pseudonymous identifier, then select the desired order and press **Start Session**. Development output is written to:

`Application.persistentDataPath/VRMaintenanceResearchData/Development/<session_id>/`

Production mode uses the adjacent `Sessions` folder. Keep these output folders out of source control and do not enter names, emails, phone numbers, student IDs, or other direct identifiers.

## Scope boundaries
- Graybox primitives are deliberate development assets.
- Official XRI origin and simulator prefabs are instantiated in the research scenes; original sample content is not modified.
- The information source content is English development placeholder material. Translation and equivalence review are required before study data collection.