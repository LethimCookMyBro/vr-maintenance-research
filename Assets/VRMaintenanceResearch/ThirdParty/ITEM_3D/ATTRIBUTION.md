# ITEM_3D Third-Party Attribution

Selected model data came from `D:\ITEM_3D` on 2026-08-03. Referenced originals are copied here; models that use `Optimized/` derivatives retain source provenance and hashes below without duplicating their unreferenced raw copies in the project. No source mesh was edited. Scale, rotation, hierarchy, and collider corrections live in project-owned scene wrappers.

## Imported models

| Source record / relative path | Title / creator | License | Source | SHA-256 |
|---|---|---|---|---|
| `Motherboard/source/anakart.glb` | MSI B450 TOMAHAWK MAX — shig0 | CC BY 4.0 | https://sketchfab.com/3d-models/msi-b450-tomahawk-max-8ea715471e344599a07bbdbbc77dfbdd | `9BA3491D5AB2CC02179BB9B1AFA72F5F268FA89E574FDBA88094A9DB1555E7BE` |
| `CPU/ryzen_5_5600.glb` | Ryzen 5 5600 — McMiwok | CC BY 4.0 | https://sketchfab.com/3d-models/ryzen-5-5600-358bdc563a264a58a610abd001d36d89 | `FB85937AA9C0B9C652198B872F9AA0F1E05E444FA7C7B2CFCFD84CDD0CDF7D58` |
| `RAM/random_access_memory_ram_ddr4.glb` | Random access memory (RAM) DDR4 — suuugar_mommy | CC BY-NC 4.0 | https://sketchfab.com/3d-models/random-access-memory-ram-ddr4-5b7cecfa59294a90a8f7482eeb809d2d | `1F51001C34AC1D8031FA23E64A717CA304C8761960E452BFB13A74C4790E0033` |
| `GPU/source/gpu.glb` | MSI GAMING X RTX 4060Ti — shig0 | CC BY 4.0 | https://sketchfab.com/3d-models/msi-gaming-x-rtx-4060ti-88bcc40d2ecc450d9bf10f1d6c6f079c | `1F33FEABE9F648535E9B6767A345BEF6CADC6FEEF73588EE76D991D925C5E8C4` |
| `Cooler/source/amdwraithstealthnocable.glb` | (Free) AMD Wraith Stealth CPU Cooler — PolyDavid | CC BY 4.0 | https://sketchfab.com/3d-models/free-amd-wraith-stealth-cpu-cooler-ff1e128c191c4f808e60e7a7a523c9cc | `DCB3150F84F15188806E4C29964306B2DE7204FCC3BE377DD131244A71B7AC6C` |
| `PSU/psu_power_supply_unit.glb` | PSU Power Supply Unit — dhafintaufiqi21 | CC BY 4.0 | https://sketchfab.com/3d-models/psu-power-supply-unit-69ccd1be3a77497cb2acc9e39e7c52b3 | `53EB3AC4BB5DB2FCC9807A0E1DFAD3AD4DEE33D9B034C3BA307FCFC8EEB11730` |
| `Storage/source/ssd-kit.glb` | SSD Kit - Samsung — PolyDavid | CC BY 4.0 | https://sketchfab.com/3d-models/ssd-kit-samsung-46516350ecc64ce4a1051690890e5f4d | `855D6E7DDCA994ED4EAA65CE1AEB6893DDD064416F577719127F2B0E5ABBAC19` |
| `Fans/120mm_computer_fans.glb` | 120mm Computer Fans — kusuma844 | CC BY 4.0 | https://sketchfab.com/3d-models/120mm-computer-fans-6c17bfc4a2a5438eb9996fb3c73e1a91 | `E3304F5EB2039275EE933C7C759D95C34E184C1E23AC8C6748C61B02E615C314` |
| `Tools/cc0_-_screwdriver.glb` | CC0 - Screwdriver — plaggy | CC BY 4.0 | https://sketchfab.com/3d-models/cc0-screwdriver-d68e0f4f80c74c92a15e3353f2c5b873 | `DD7821C0919A00AAEE60746ED10752AE7ADE82E870EA50447083DB63227EDF94` |

The screwdriver filename contains `cc0`, but its embedded metadata identifies CC BY 4.0; the embedded license controls this use.

## Quest texture derivatives

The four source GLBs that embedded textures above 2048 px were copied and repacked without mesh changes. Each raster image was downsampled with Lanczos filtering to a maximum dimension of 2048 px; source files and hashes above remain unchanged.

| Derived project path | Maximum embedded texture | SHA-256 |
|---|---:|---|
| `Optimized/Motherboard/anakart_quest.glb` | 2048 px | `15288E0AF4F461D8927A5373541320DB3368B83FACFD070EAF4386717B7B2997` |
| `Optimized/RAM/random_access_memory_ram_ddr4_quest.glb` | 2048 px | `9451C573054EB2D9A42ED282159B590E77B6B7DA06073E892E31FAF3FE0EF5DA` |
| `Optimized/GPU/gpu_quest.glb` | 2048 px | `9FF3E15CCD9306B0DAD59CE8B59DAB90C85FAD82F5C24865E3F500EC3C1B001C` |
| `Optimized/Tools/cc0_-_screwdriver_quest.glb` | 2048 px | `B889270EA6FD3820231ECB48D3A944C98ACA1E567F35188C61FC8E4B9CE91E83` |

Reproducible optimizer: `C:\Users\User\.codex\visualizations\2026\08\03\019fc7b2-e25f-7482-b38f-a4fb4366fd70\item-3d-preimport\optimize_glb_textures.py`.

## License links

- CC BY 4.0: https://creativecommons.org/licenses/by/4.0/
- CC BY-NC 4.0: https://creativecommons.org/licenses/by-nc/4.0/

Attribution must remain with redistributed research project copies and derivatives. The RAM model is restricted to non-commercial use.

## Excluded files

- `01_Case/fractal_design._meshify_c__-__pc_case.glb`: excluded because 223,920 triangles and 67 meshes are excessive for the current Quest-first budget without a reproducible reduction pipeline.
- `05_GPU/Extracted/source/graphicscard.glb`: valid CC BY provenance was found, but the 113,728-triangle candidate was rejected in favor of the 42,677-triangle RTX 4060Ti.
- `11_Cables/low_poly_pc_cable.glb`: valid CC BY provenance, but the visible model is an external wall-power cable, not a 24-pin ATX loom.
- `99_Archive/9800x3d-cpu-low-poly.zip`: excluded because the selected Ryzen 5 model already fills the CPU role and has embedded attribution.

The downloaded 120 mm fan is selected only for the Computer scene. It must not replace the serviceable desk-fan task.
