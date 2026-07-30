# Task Configuration Guide

## Configurable ScriptableObjects
- `ScriptableObjects/Tasks/TrainingDevelopment.asset`
- `ScriptableObjects/Tasks/ComputerMaintenanceDevelopment.asset`
- `ScriptableObjects/Tasks/FanMaintenanceDevelopment.asset`
- `ScriptableObjects/InformationSources/*_v2.asset`

Configure task context, layout ID, time limit, movement rate, fault IDs, source assignment, content version, and source layout ID through the Unity Inspector/MCP asset workflow. Keep stable IDs unchanged once data collection begins.

## Important serialization rule
`ResearchTaskId` is serialized by integer. Do not reorder its enum members. If a new member is added, re-open and explicitly verify all existing task assets; the recovery pass already reset Training, Computer, and Fan to their intended values.

## Fault development defaults
- Computer: install `computer.main-power-connector`, then use `computer.power-button`.
- Fan: install `fan.working-fuse`, then use `fan.speed-selector`.

The incorrect Computer/Fan components are retained for observable error-correction and retry events. Adjust task difficulty only after protocol approval and record the new content version.