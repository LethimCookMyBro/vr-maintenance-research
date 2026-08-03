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
            ["Lab_StationBoard"] = new Surface { Color = "#7E8A99", Metallic = 0.05f, Smoothness = 0.26f },
            ["Lab_PanelSurface"] = new Surface { Color = "#F4F6F8", Metallic = 0f, Smoothness = 0.24f },

            // --- worked surfaces: these carry the specular story ---
            // Lab_Metal is the workbench top and other large painted-steel panels.
            // Full metallic reads as black without a strong reflection source, so
            // these stay semi-metallic; bare steel lives in Lab_ToolSteel.
            ["Lab_Metal"] = new Surface { Color = "#98A0A8", Metallic = 0.45f, Smoothness = 0.55f },
            ["Lab_MetalDark"] = new Surface { Color = "#79808A", Metallic = 0.55f, Smoothness = 0.48f },
            ["Lab_PlasticDark"] = new Surface { Color = "#23272C", Metallic = 0f, Smoothness = 0.52f },
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
            // The case was near-black inside and out, so the interior rendered as a
            // cave and none of the components silhouetted. Shell and tray are lifted
            // to mid grey; the bezel keeps Lab_PlasticDark so the front still reads
            // as the dark face of a tower.
            ["Lab_CaseSteel"] = new Surface { Color = "#59616B", Metallic = 0.85f, Smoothness = 0.55f },
            ["Lab_CasePanel"] = new Surface { Color = "#434B55", Metallic = 0.70f, Smoothness = 0.62f },
            // Mid grey, not light grey: the tray, liners and drive cage are backdrop.
            // Lifting them as far as the components washed the whole cavity out, and
            // a bright heatsink against a bright tray reads as one grey mass.
            ["Lab_CaseInterior"] = new Surface { Color = "#585F68", Metallic = 0.55f, Smoothness = 0.44f },
            ["Lab_HeatsinkAlu"] = new Surface { Color = "#BFC6CD", Metallic = 0.88f, Smoothness = 0.62f },
            ["Lab_PcbDark"] = new Surface { Color = "#15563A", Metallic = 0.05f, Smoothness = 0.38f },
            ["Lab_Copper"] = new Surface { Color = "#B87333", Metallic = 0.95f, Smoothness = 0.72f },
            ["Lab_Gold"] = new Surface { Color = "#C9A227", Metallic = 0.95f, Smoothness = 0.78f },
            ["Lab_Silicon"] = new Surface { Color = "#2E3238", Metallic = 0.55f, Smoothness = 0.66f },
            ["Lab_CableBlack"] = new Surface { Color = "#141619", Metallic = 0f, Smoothness = 0.44f },
            ["Lab_CableYellow"] = new Surface { Color = "#D8B22A", Metallic = 0f, Smoothness = 0.44f },
            ["Lab_CableRed"] = new Surface { Color = "#B33A32", Metallic = 0f, Smoothness = 0.44f },
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

        static void Write(Material material, Surface surface)
        {
            var color = Hex(surface.Color);
            if (surface.Alpha > 0f)
            {
                color.a = surface.Alpha;
                SetTransparent(material);
            }

            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", surface.Metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", surface.Smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", surface.Smoothness);

            // URP needs the keyword toggled, not just the colour, or emission is ignored.
            if (surface.Emission != null)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                if (material.HasProperty("_EmissionColor"))
                    material.SetColor("_EmissionColor", Hex(surface.Emission) * surface.EmissionBoost);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                if (material.HasProperty("_EmissionColor"))
                    material.SetColor("_EmissionColor", Color.black);
            }

            EditorUtility.SetDirty(material);
        }

        /// <summary>
        /// URP Lit needs all of this to go transparent — setting the colour's alpha
        /// alone leaves the surface fully opaque.
        /// </summary>
        static void SetTransparent(Material material)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
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
