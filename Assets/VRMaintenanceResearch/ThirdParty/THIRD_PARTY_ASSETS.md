# Third-Party Assets

All third-party content used by the VR Maintenance Research project lives under
`Assets/VRMaintenanceResearch/ThirdParty/<Source>/<AssetName>/`. Every entry below
was license-verified before download. Assets whose license could not be verified
were not downloaded.

Original XRI example assets shipped with this repository are **not** third-party
imports of this work and are never modified.

Access date for every entry below: 2026-08-02.

---

## Poly Haven — CC0 1.0 Universal

License page: <https://polyhaven.com/license> (verified: CC0, attribution not required).
Local copy of the licence statement: `ThirdParty/PolyHaven/LICENSE.txt`.

### Concrete Floor Worn 001

| Field | Value |
|---|---|
| Asset name | Concrete Floor Worn 001 (`concrete_floor_worn_001`) |
| Creator | Dimitrios Savva (photography), Rico Cilliers (processing) |
| Source | Poly Haven |
| Source page | <https://polyhaven.com/a/concrete_floor_worn_001> |
| License | CC0 1.0 Universal |
| Attribution required | No (credited voluntarily) |
| Downloaded files | `concrete_floor_worn_001_diff_1k.jpg` (116 KB), `concrete_floor_worn_001_nor_gl_1k.jpg` (108 KB) |
| Modifications | None to the source files. Imported as Unity textures; smoothness is a constant on the shared URP Lit material, so no roughness map was retained. |
| Intended use | Laboratory floor material (`Lab_Floor.mat`) in Training, Computer and Fan scenes |
| Polygon count | n/a (texture only) |
| Material count | Feeds 1 shared URP Lit material |
| Texture sizes | 1024 x 1024, JPG |

### Beige Wall 001

| Field | Value |
|---|---|
| Asset name | Beige Wall 001 (`beige_wall_001`) |
| Creator | Dimitrios Savva (photography), Rico Cilliers (processing) |
| Source | Poly Haven |
| Source page | <https://polyhaven.com/a/beige_wall_001> |
| License | CC0 1.0 Universal |
| Attribution required | No (credited voluntarily) |
| Downloaded files | `beige_wall_001_diff_1k.jpg` (32 KB), `beige_wall_001_nor_gl_1k.jpg` (150 KB) |
| Modifications | None to the source files. Imported as Unity textures and tinted to off-white by the shared wall material. |
| Intended use | Laboratory wall material (`Lab_Wall.mat`) in Training, Computer and Fan scenes |
| Polygon count | n/a (texture only) |
| Material count | Feeds 1 shared URP Lit material |
| Texture sizes | 1024 x 1024, JPG |

---

## Kenney — CC0 1.0 Universal

License page: <https://kenney.nl/assets/game-icons> (verified: "Creative Commons CC0").
Local copy of the licence statement: `ThirdParty/Kenney/GameIcons/LICENSE.txt`.

| Field | Value |
|---|---|
| Asset name | Game Icons |
| Creator | Kenney Vleugels (kenney.nl) |
| Source | Kenney |
| Source page | <https://kenney.nl/assets/game-icons> |
| Direct download | `https://kenney.nl/media/pages/assets/game-icons/1ebf9c14af-1677661579/kenney_game-icons.zip` (1.05 MB) |
| License | CC0 1.0 Universal — "Credit would be nice but is not mandatory" |
| Attribution required | No (credited voluntarily) |
| Downloaded filename | `kenney_game-icons.zip`; 16 PNG files extracted into the project |
| Modifications | None to the source files. Imported as Unity sprites (single sprite, point-free bilinear, no compression change). |
| Intended use | World-space information-source tiles, researcher controls, training UI |
| Polygon count | n/a (2D sprites) |
| Material count | Rendered by the default UI sprite material |
| Texture sizes | 100 x 100 PNG, white glyph on transparent |

Icons retained and their mapped roles:

| File | Role |
|---|---|
| `open.png` | Product Manual source tile |
| `menuList.png` | Text Troubleshooting Guide source tile |
| `movie.png` | Instructional Video source tile |
| `menuGrid.png` | Visual Step-by-Step Guide source tile |
| `zoom.png` | Inspect / device test control |
| `return.png` | Reset control |
| `pause.png` | Pause control |
| `stop.png` | Safety Stop control |
| `warning.png` | Safety Stop separation / warning accents |
| `fastForward.png` | Video seek control |
| `previous.png`, `next.png` | Information page controls |
| `power.png` | Device power / speed control |
| `information.png` | Information station signage |
| `checkmark.png` | Training progress indicator (satisfied) |
| `exclamation.png` | Training progress indicator (outstanding) |

All 105 icons in the pack are CC0; only the 16 above were copied into the project
to keep the imported asset set small.

### UI Pack 2.0

Imported 2026-08-03 for the interface restyle. This is the pack that supplies the
project's UI visual language; the Game Icons above are now supporting glyphs only.

| Field | Value |
|---|---|
| Asset name | UI Pack (2.0) |
| Creator | Kenney Vleugels (kenney.nl) |
| Source page | <https://kenney.nl/assets/ui-pack> |
| Direct download | `https://kenney.nl/media/pages/assets/ui-pack/f651646eab-1718203990/kenney_ui-pack.zip` (1.17 MB) |
| License | CC0 1.0 Universal — verified in the pack's own `License.txt`, creation date 12-06-2024 |
| Attribution required | No (credited voluntarily) |
| Local copy of licence | `ThirdParty/Kenney/UIPack/LICENSE.txt` |
| Modifications | None to the source files. Imported as Unity sprites and one TMP font asset. |
| Style check | Flat/soft-shadow modern UI with rounded rectangles and a raised "depth" edge. Not fantasy or stone — matches the navy/slate XR-lab theme. |

Files copied into the project (13 of 1343 in the pack):

| File | Source in pack | Role |
|---|---|---|
| `panel_surface.png` | `PNG/Grey/Default/button_rectangle_depth_flat.png` | Reader panel body, notice-board cards |
| `panel_flat.png` | `PNG/Grey/Default/button_rectangle_flat.png` | Selector card face |
| `panel_border.png` | `PNG/Grey/Default/button_rectangle_border.png` | Panel outline / recessed slots |
| `button_accent.png` | `PNG/Blue/Default/button_rectangle_depth_flat.png` | Primary controls (Next, Play) |
| `button_accent_flat.png` | `PNG/Blue/Default/button_rectangle_flat.png` | Pressed / flat accent state |
| `button_neutral.png` | `PNG/Grey/Default/button_rectangle_depth_border.png` | Secondary controls (Prev, Close) |
| `divider.png` | `PNG/Extra/Default/divider.png` | Header rules |
| `icon_play.png`, `icon_repeat.png`, `icon_arrow_up.png`, `icon_arrow_down.png` | `PNG/Extra/Default/` | Video and paging controls |
| `KenneyFutureNarrow.ttf` | `Font/Kenney Future Narrow.ttf` | UI typeface, replacing the default LiberationSans |

---

## Noto Sans Thai and Noto Sans JP — SIL Open Font License 1.1

| Field | Noto Sans Thai | Noto Sans JP |
|---|---|---|
| Source | <https://github.com/google/fonts/tree/main/ofl/notosansthai> | <https://github.com/google/fonts/tree/main/ofl/notosansjp> |
| License | SIL Open Font License 1.1 | SIL Open Font License 1.1 |
| Local font | `Fonts/NotoSansThai.ttf` (218,652 bytes) | `Fonts/NotoSansJP.ttf` (9,589,900 bytes) |
| Local license | `Fonts/NotoSansThai-OFL.txt` | `Fonts/NotoSansJP-OFL.txt` |
| Modification | Unity-generated dynamic TMP fallback asset only | Unity-generated dynamic TMP fallback asset only |
| Intended use | Thai titles, captions and reader text | Japanese titles, captions and reader text |

Imported on 2026-08-03. The generated fallback assets are
`Resources/Fonts/TMP_NotoSansThai_v2.asset` and
`Resources/Fonts/TMP_NotoSansJP_v2.asset`; they are loaded only by the
information-reader localization path. English continues to use the existing TMP font.

---

## Assets deliberately NOT imported

| Candidate | Reason |
|---|---|
| Poly Haven metal textures (`metal_plate`, `rusty_metal_*`, `corrugated_iron_*`) | Every metal texture in the Poly Haven library is heavily rusted or industrial-weathered. None matched the clean academic-laboratory target style. Replaced with an untextured shared URP Lit metal material (metallic 0.75 / smoothness 0.55), which is also cheaper on Quest. |
| Quaternius / Sketchfab furniture and appliance models | Not required: the workbench, shelving, tool tray and room shell are authored from Unity primitives with shared materials, which keeps the triangle budget and material count far below any imported set and avoids scale/pivot mismatches against the validated interaction colliders. See `Docs/CLAUDE_VISUAL_REDESIGN_PLAN.md`. |
| GLTF / model-importer packages | The goal forbids adding packages purely to import assets. No package versions were changed. |

## Manual-download shortlist (optional future work)

Not required for the current build; listed only if higher-fidelity props are wanted later.
Priority order:

1. Quaternius — modular workshop / props packs (CC0) — <https://quaternius.com>
2. Kenney — "Furniture Kit" (CC0) — <https://kenney.nl/assets/furniture-kit>
3. Poly Haven — HDRI `studio_small_08` (CC0) — <https://polyhaven.com/a/studio_small_08> for a neutral indoor reflection probe

Each would need pivot/scale alignment against the existing interaction colliders
before use, and must remain a visual child of the functional root.

---

## Poly Haven model imports

The following CC0 model imports are visual children of the existing interaction roots; the stable colliders and research IDs remain on the original scene objects. The 1k FBX and diffuse map files were downloaded from the official Poly Haven API on 2026-08-02. Exact MD5 and byte checks are recorded in `PolyHaven/DOWNLOAD_VERIFICATION.csv`.

| Model | Source page | Scene use | Local files |
|---|---|---|---|
| Screwdriver | <https://polyhaven.com/a/screwdriver> | `computer.tool.screwdriver`, `fan.tool.screwdriver` | `PolyHaven/Screwdriver/` |
| Power Box 01 | <https://polyhaven.com/a/power_box_01> | `computer.case` visual | `PolyHaven/PowerBox/` |
| Ceiling Fan | <https://polyhaven.com/a/ceiling_fan> | `fan.body` visual reconstruction | `PolyHaven/CeilingFan/` |

Local URP Lit materials in `PolyHaven/Materials/` bind the downloaded diffuse maps. The assets are intentionally kept at 1k for the current desktop/Quest candidate budget; lighting and physical interaction remain owned by the research scene.
