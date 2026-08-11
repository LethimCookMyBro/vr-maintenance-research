using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Single source of truth for the lab's PBR surface values.
    /// The greybox look came from every surface sharing near-identical
    /// roughness, so metal, plastic, PCB and rubber never separated.
    /// Re-runnable: it only writes shader properties on existing materials.
    /// </summary>
    public static class ResearchMaterialPalette
    {
        const string k_MaterialFolder = "Assets/VRMaintenanceResearch/Materials/Lab";

        struct Surface
        {
            public string Color;
            public float Metallic;
            public float Smoothness;
            public string Emission;   // null = off
            public float EmissionBoost;
            public float Alpha;       // 0 = opaque (the default); anything else goes transparent
        }

        // ponytail: literal table beats a ScriptableObject nobody edits twice.
        static readonly Dictionary<string, Surface> k_Surfaces = new Dictionary<string, Surface>
        {
            // --- room shell: matte, so the props read as the shiny things ---
            ["Lab_Wall"] = new Surface { Color = "#E8E9E6", Metallic = 0f, Smoothness = 0.10f },
            ["Lab_Ceiling"] = new Surface { Color = "#F2F3F1", Metallic = 0f, Smoothness = 0.06f },
            ["Lab_Floor"] = new Surface { Color = "#9DA2A7", Metallic = 0f, Smoothness = 0.22f },
            ["Lab_LightPanel"] = new Surface { Color = "#F4F6F8", Metallic = 0f, Smoothness = 0.30f, Emission = "#FFF6E4", EmissionBoost = 1.15f },

            // --- structure ---
            ["Lab_Navy"] = new Surface { Color = "#1C2838", Metallic = 0.25f, Smoothness = 0.48f },
            ["Lab_Trim"] = new Surface { Color = "#38455C", Metallic = 0.20f, Smoothness = 0.44f },
            // The board the notice panels hang on. It was mid grey, which was fine
            // behind pale cards and became a bright slab the moment the cards went to
            // dark glass. Dark slate reads as the mounting plate it is.
            ["Lab_StationBoard"] = new Surface { Color = "#333C48", Metallic = 0.05f, Smoothness = 0.26f },
            ["Lab_PanelSurface"] = new Surface { Color = "#F4F6F8", Metallic = 0f, Smoothness = 0.24f },

            // --- worked surfaces: these carry the specular story ---
            // Lab_Metal is the workbench top and other large painted-steel panels.
            // Full metallic reads as black without a strong reflection source, so
            // these stay semi-metallic; bare steel lives in Lab_ToolSteel.
            ["Lab_Metal"] = new Surface { Color = "#98A0A8", Metallic = 0.45f, Smoothness = 0.55f },
            ["Lab_MetalDark"] = new Surface { Color = "#79808A", Metallic = 0.55f, Smoothness = 0.48f },
            // Smoothness was 0.52, which is a semi-gloss dielectric: under the ceiling
            // panels every upward-facing surface returned a specular highlight strong
            // enough to read as light grey. The graphics card's backplate is the whole
            // top of the largest part in the case, so the card rendered as a bright
            // shelf spanning the machine, and the case bezel washed out the same way.
            // Matte black plastic is also just what these parts are.
            ["Lab_PlasticDark"] = new Surface { Color = "#23272C", Metallic = 0f, Smoothness = 0.26f },
            ["Lab_PlasticLight"] = new Surface { Color = "#DDE1E5", Metallic = 0f, Smoothness = 0.56f },
            // Board green was so dark it merged with the case interior; lifted until
            // a populated board reads as a board rather than a black rectangle.
            ["Lab_Pcb"] = new Surface { Color = "#1E7A50", Metallic = 0.05f, Smoothness = 0.42f },
            ["Lab_Rubber"] = new Surface { Color = "#17191C", Metallic = 0f, Smoothness = 0.14f },

            // --- signal colours ---
            ["Lab_Accent"] = new Surface { Color = "#2E7BE6", Metallic = 0f, Smoothness = 0.62f, Emission = "#2E7BE6", EmissionBoost = 0.30f },
            ["Lab_Warning"] = new Surface { Color = "#F2A22C", Metallic = 0f, Smoothness = 0.55f, Emission = "#F2A22C", EmissionBoost = 0.35f },
            ["Lab_Indicator"] = new Surface { Color = "#9AA3AC", Metallic = 0.30f, Smoothness = 0.72f },
        };

        /// <summary>Extra materials the redesign needs; created on demand.</summary>
        static readonly Dictionary<string, Surface> k_NewSurfaces = new Dictionary<string, Surface>
        {
            // The case was lifted to mid grey back when the interior was handmade dark
            // parts that needed something to silhouette against. With the licensed
            // board, cooler and supply in it that reversed: a 0.85-metallic mid grey
            // under the lab's key light rendered as near-white, so the tower read as a
            // pale carton and the machine inside it read as debris in a box. These are
            // now painted steel at the value real chassis are.
            ["Lab_CaseSteel"] = new Surface { Color = "#3C424A", Metallic = 0.55f, Smoothness = 0.46f },
            ["Lab_CasePanel"] = new Surface { Color = "#2D323A", Metallic = 0.45f, Smoothness = 0.56f },
            // Matte graphite, the way a powder-coated interior actually is. The silver
            // supply, the aluminium cooler and the pale connectors now carry the light
            // in the cavity instead of the walls doing it.
            ["Lab_CaseInterior"] = new Surface { Color = "#31363D", Metallic = 0.20f, Smoothness = 0.30f },
            // Front intake: darker still and nearly matte, so the mesh field reads as
            // an opening rather than another panel.
            ["Lab_CaseMesh"] = new Surface { Color = "#191C21", Metallic = 0.10f, Smoothness = 0.22f },
            ["Lab_HeatsinkAlu"] = new Surface { Color = "#BFC6CD", Metallic = 0.88f, Smoothness = 0.62f },
            ["Lab_PcbDark"] = new Surface { Color = "#15563A", Metallic = 0.05f, Smoothness = 0.38f },
            ["Lab_Copper"] = new Surface { Color = "#B87333", Metallic = 0.95f, Smoothness = 0.72f },
            ["Lab_Gold"] = new Surface { Color = "#C9A227", Metallic = 0.95f, Smoothness = 0.78f },
            ["Lab_Silicon"] = new Surface { Color = "#2E3238", Metallic = 0.55f, Smoothness = 0.66f },
            ["Lab_CableBlack"] = new Surface { Color = "#141619", Metallic = 0f, Smoothness = 0.44f },
            ["Lab_CableYellow"] = new Surface { Color = "#D8B22A", Metallic = 0f, Smoothness = 0.44f },
            ["Lab_CableRed"] = new Surface { Color = "#B33A32", Metallic = 0f, Smoothness = 0.44f },
            // The fourth ATX rail colour. A 24-pin loom is black, red, yellow and
            // orange in roughly that quantity, and three of the four already existed;
            // without orange the bundle reads as "some wires" rather than as the one
            // loom every PC carries.
            ["Lab_CableOrange"] = new Surface { Color = "#C06A24", Metallic = 0f, Smoothness = 0.44f },
            ["Lab_ConnectorWhite"] = new Surface { Color = "#D5D8DB", Metallic = 0f, Smoothness = 0.50f },
            ["Lab_ToolHandle"] = new Surface { Color = "#B8422E", Metallic = 0f, Smoothness = 0.58f },
            ["Lab_ToolSteel"] = new Surface { Color = "#C8CDD3", Metallic = 0.95f, Smoothness = 0.80f },
            ["Lab_AntiStatic"] = new Surface { Color = "#3E5166", Metallic = 0.05f, Smoothness = 0.30f },
            ["Lab_LabelPlate"] = new Surface { Color = "#EDEFF2", Metallic = 0f, Smoothness = 0.35f },
            ["Lab_Line"] = new Surface { Color = "#BFC6CE", Metallic = 0f, Smoothness = 0.20f },
            ["Lab_GlassPanel"] = new Surface { Color = "#20242B", Metallic = 0.40f, Smoothness = 0.92f },

            // Actually transparent, because the fuse element has to be visible
            // through it. Opaque "glass" made both fuses black cylinders: identical
            // from across the bench, which was the intent, but also identical with a
            // magnifier, which removed the diagnosis entirely.
            ["Lab_FuseGlass"] = new Surface { Color = "#C8D2DA", Metallic = 0.10f, Smoothness = 0.95f, Alpha = 0.30f },
            ["Lab_StatusGreen"] = new Surface { Color = "#35C46A", Metallic = 0f, Smoothness = 0.70f, Emission = "#35C46A", EmissionBoost = 1.1f },
            ["Lab_StatusRed"] = new Surface { Color = "#E0524A", Metallic = 0f, Smoothness = 0.70f, Emission = "#E0524A", EmissionBoost = 1.1f },

            // --- industrial dressing -------------------------------------------
            // One yellow for every safety marking in the room: the floor aisle lines,
            // the work zone box and the wall stripe. Deliberately not emissive and
            // deliberately not Lab_Warning, which is the amber the UI reserves for a
            // warning state — a painted line that glows reads as a signal.
            ["Lab_SafetyYellow"] = new Surface { Color = "#D9A81C", Metallic = 0f, Smoothness = 0.32f },
            // Stacking crates. A saturated moulded blue, well away from Lab_Accent so
            // the room's dressing cannot be mistaken for the interface's accent.
            ["Lab_CrateBlue"] = new Surface { Color = "#1E4E9B", Metallic = 0f, Smoothness = 0.42f },
        };

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Apply Material Palette")]
        public static void Apply()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("[Palette] URP Lit shader missing");
                return;
            }

            var touched = 0;
            foreach (var pair in k_Surfaces)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>($"{k_MaterialFolder}/{pair.Key}.mat");
                if (material == null)
                {
                    Debug.LogWarning($"[Palette] missing {pair.Key}");
                    continue;
                }
                Write(material, pair.Value);
                touched++;
            }

            foreach (var pair in k_NewSurfaces)
            {
                var path = $"{k_MaterialFolder}/{pair.Key}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, path);
                }
                Write(material, pair.Value);
                touched++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Palette] wrote {touched} materials");
        }

        /// <summary>
        /// Writes a surface, and writes nothing it does not have to.
        ///
        /// Re-running this used to dirty all forty-odd materials whether or not a value
        /// had changed, because URP's legacy `_Color` alias round-trips through a
        /// colour-space conversion: setting 0.10980392 stores 0.10980389. Every pass
        /// therefore produced a commit-sized diff of 4e-8 drifts across the palette, and
        /// a real change was invisible in it. Values are compared before they are
        /// written, so a run that changes nothing leaves every file byte-identical.
        /// </summary>
        static void Write(Material material, Surface surface)
        {
            var color = Hex(surface.Color);
            var changed = false;
            if (surface.Alpha > 0f)
            {
                color.a = surface.Alpha;
                changed |= SetTransparent(material);
            }

            changed |= SetColor(material, "_BaseColor", color);
            changed |= SetColor(material, "_Color", color);
            changed |= SetFloat(material, "_Metallic", surface.Metallic);
            changed |= SetFloat(material, "_Smoothness", surface.Smoothness);
            changed |= SetFloat(material, "_Glossiness", surface.Smoothness);

            // URP needs the keyword toggled, not just the colour, or emission is ignored.
            var emissive = surface.Emission != null;
            if (material.IsKeywordEnabled("_EMISSION") != emissive)
            {
                if (emissive) material.EnableKeyword("_EMISSION");
                else material.DisableKeyword("_EMISSION");
                changed = true;
            }

            var flags = emissive ? MaterialGlobalIlluminationFlags.RealtimeEmissive : MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            if (material.globalIlluminationFlags != flags)
            {
                material.globalIlluminationFlags = flags;
                changed = true;
            }

            changed |= SetColor(material, "_EmissionColor", emissive ? Hex(surface.Emission) * surface.EmissionBoost : Color.black);

            if (changed)
                EditorUtility.SetDirty(material);
        }

        // 1e-4 is two orders below the smallest step an 8-bit colour channel can
        // express, so it swallows the round-trip noise and nothing else.
        const float k_Epsilon = 1e-4f;

        static bool SetColor(Material material, string property, Color value)
        {
            if (!material.HasProperty(property))
                return false;
            var current = material.GetColor(property);
            if (Mathf.Abs(current.r - value.r) < k_Epsilon && Mathf.Abs(current.g - value.g) < k_Epsilon &&
                Mathf.Abs(current.b - value.b) < k_Epsilon && Mathf.Abs(current.a - value.a) < k_Epsilon)
                return false;
            material.SetColor(property, value);
            return true;
        }

        static bool SetFloat(Material material, string property, float value)
        {
            if (!material.HasProperty(property) || Mathf.Abs(material.GetFloat(property) - value) < k_Epsilon)
                return false;
            material.SetFloat(property, value);
            return true;
        }

        /// <summary>
        /// URP Lit needs all of this to go transparent — setting the colour's alpha
        /// alone leaves the surface fully opaque.
        /// </summary>
        static bool SetTransparent(Material material)
        {
            var changed = SetFloat(material, "_Surface", 1f);
            changed |= SetFloat(material, "_Blend", 0f);
            changed |= SetFloat(material, "_ZWrite", 0f);
            changed |= SetFloat(material, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            changed |= SetFloat(material, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetOverrideTag("RenderType", "Transparent");
            if (!material.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"))
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                changed = true;
            }
            material.DisableKeyword("_ALPHATEST_ON");
            if (material.renderQueue != (int)UnityEngine.Rendering.RenderQueue.Transparent)
            {
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                changed = true;
            }
            return changed;
        }

        public static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out var color);
            return color;
        }

        /// <summary>Loads a palette material by name, e.g. "Lab_CaseSteel".</summary>
        public static Material Load(string name)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>($"{k_MaterialFolder}/{name}.mat");
            if (material == null)
                Debug.LogWarning($"[Palette] Load missing {name}");
            return material;
        }
    }
}
