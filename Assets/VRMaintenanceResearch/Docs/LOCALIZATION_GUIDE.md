# Localization Guide

`ParticipantGroup` and `ResearchLanguage` are stored in the session manifest. Development task/source content and the world-space training instructions are English placeholders.

Before deployment, maintain an approved Thai/Japanese/English table for every visible instruction, source title/body, button, researcher prompt, and error message. Review semantic equivalence rather than literal word counts. Version the approved content in `taskContentVersion` and retain the source-layout ID in each manifest.

Do not alter a participant's source order or add a recommended source during translation. Any translated instructional video requires script, captions, audio, duration, and licensing review.
## Localization fields

`ResearchTaskDefinition` exposes Thai/Japanese title and participant-instruction fields. `InformationSourceDefinition` exposes Thai/Japanese title and body-content fields. Empty values indicate that advisor-approved translation is pending; the runtime does not substitute a translation automatically.
