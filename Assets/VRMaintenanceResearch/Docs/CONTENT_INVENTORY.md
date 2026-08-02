# Content inventory

## Active information-source definitions

The task scenes reference eight valid source definitions: four per task.

| Task | Product Manual | Text Troubleshooting Guide | Instructional Video | Visual Step-by-Step Guide |
|---|---|---|---|---|
| Computer | `computer.source.manual` | `computer.source.text` | `computer.source.video` | `computer.source.visual` |
| Fan | `fan.source.manual` | `fan.source.text` | `fan.source.video` | `fan.source.visual` |

Each definition carries English, Thai, and Japanese title/content fields. `InformationSourceController` presents the configured session language when a translation field is populated, keeps all formats on the same fixed reader, and preserves the existing source-open, source-close, page, and video event names.

## Presentation inventory

- Four equal closed selector cards per task, with a source icon and label.
- One reader panel per task scene; switching source closes the prior active reader before the next source opens.
- Product Manual and Text Guide use page controls; Video retains Play/Pause/Stop/Seek; Visual Guide uses page navigation.
- The existing self-authored video assets and unreferenced invalid v1 assets were not changed by the spatial polish commit.

## Verification boundary

The 2026-08-03 runtime check proves Manual open, page advance, and switch to Text Guide with exactly one active reader. It does not establish approved Thai/Japanese copy or hardware glyph rendering; those remain listed in `KNOWN_LIMITATIONS.md`.
