using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Applies the Kenney UI Pack 2.0 (CC0) surface language to every world-space
    /// interface in the project.
    ///
    /// Before: panels and buttons were flat untextured quads using URP Lit colours
    /// and TextMeshPro's stock LiberationSans, which is literally Unity's default UI
    /// look. Now every panel body, control button and header rule is a Kenney sprite
    /// on an unlit material, and all UI type is Kenney Future Narrow.
    /// </summary>
    public static class ResearchUiStyleBuilder
    {
        const string k_PackFolder = "Assets/VRMaintenanceResearch/ThirdParty/Kenney/UIPack";
        const string k_MaterialFolder = "Assets/VRMaintenanceResearch/Materials/UI";
        const string k_FontAssetPath = "Assets/VRMaintenanceResearch/Resources/Fonts/TMP_KenneyFutureNarrow.asset";

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Apply UI Style Pack", priority = 2)]
        public static void ApplyAll()
        {
            ImportSprites();
            var font = BuildFontAsset();
            BuildMaterials();

            foreach (var scenePath in ResearchSceneSet.AllScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                StyleScene(font);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log("[UiStyle] Kenney UI Pack applied to all scenes");
        }

        /// <summary>Sprites must import as sprites, not default textures, to tint cleanly.</summary>
        static void ImportSprites()
        {
            foreach (var path in Directory.GetFiles(k_PackFolder, "*.png"))
            {
                var assetPath = path.Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                    continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// Generates the TMP asset for Kenney Future Narrow. Swapping this is the
        /// single clearest signal that the UI is no longer stock Unity.
        /// </summary>
        static TMP_FontAsset BuildFontAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(k_FontAssetPath);
            if (existing != null)
                return existing;

            var ttf = AssetDatabase.LoadAssetAtPath<Font>($"{k_PackFolder}/KenneyFutureNarrow.ttf");
            if (ttf == null)
            {
                Debug.LogError("[UiStyle] Kenney font missing");
                return null;
            }

            var asset = TMP_FontAsset.CreateFontAsset(ttf, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
            if (asset == null)
            {
                Debug.LogError("[UiStyle] font asset generation failed");
                return null;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(k_FontAssetPath));
            AssetDatabase.CreateAsset(asset, k_FontAssetPath);

            // The atlas texture and material are sub-assets of the font asset.
            asset.atlasTextures[0].name = "Kenney Future Narrow Atlas";
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            asset.material.name = "Kenney Future Narrow Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);

            AssetDatabase.SaveAssets();
            return asset;
        }

        static void BuildMaterials()
        {
            Directory.CreateDirectory(k_MaterialFolder);
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Debug.LogError("[UiStyle] URP Unlit shader missing");
                return;
            }

            foreach (var (sprite, material, tint) in Surfaces())
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{k_PackFolder}/{sprite}.png");
                if (texture == null)
                {
                    Debug.LogWarning($"[UiStyle] missing sprite {sprite}");
                    continue;
                }

                var path = $"{k_MaterialFolder}/{material}.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(shader);
                    AssetDatabase.CreateAsset(mat, path);
                }

                mat.shader = shader;
                mat.SetTexture("_BaseMap", texture);
                mat.SetColor("_BaseColor", ResearchMaterialPalette.Hex(tint));
                mat.SetFloat("_Surface", 1f);          // transparent
                mat.SetFloat("_Blend", 0f);            // alpha
                mat.SetFloat("_AlphaClip", 0f);
                mat.SetFloat("_ZWrite", 0f);
                mat.renderQueue = 3000;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                EditorUtility.SetDirty(mat);
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// The panel surfaces, as dark translucent glass.
        ///
        /// The tints carry their own alpha (8-digit hex), which is all the change
        /// the reference look needed: these materials were already built transparent
        /// — <c>_Surface 1</c>, alpha blend, no depth write, queue 3000 — and were
        /// simply being given opaque colours. The Kenney sprites are what supply the
        /// rounded corner on the world-space panels.
        ///
        /// Nothing is taken below about 88 % opacity. A panel a participant has to
        /// read against a lit lab wall stops being readable well before it stops
        /// looking like glass, and reading time is a measure in this study.
        /// Buttons stay fully opaque for the same reason.
        /// </summary>
        static IEnumerable<(string sprite, string material, string tint)> Surfaces()
        {
            yield return ("panel_surface", "UI_PanelSurface", "#0D141FEB");
            yield return ("panel_flat", "UI_CardFace", "#1B2739F2");
            yield return ("panel_border", "UI_PanelBorder", "#42536DE0");
            yield return ("button_accent", "UI_ButtonPrimary", "#2E7BE6");
            yield return ("button_neutral", "UI_ButtonSecondary", "#4E5A6B");
            yield return ("divider", "UI_Divider", "#2E7BE6");
        }

        /// <summary>Short, upper-case, non-paragraph labels — the only place a display face belongs.</summary>
        static bool IsDisplayText(TMP_Text label)
        {
            var name = label.name;
            if (name == "GEN Body" || name == "Body" || name == "Footer" || name == "GEN Label")
                return false;

            return name == "GEN Title"
                || name == "Header"
                || name == "Bay Title"
                || name == "Heading"
                || name == "Socket Label"
                || name == "Reset Caption"
                || name.StartsWith("Label ")     // bench zone signage
                || name.StartsWith("Step ");     // training step markers
        }

        static Material Load(string name) =>
            AssetDatabase.LoadAssetAtPath<Material>($"{k_MaterialFolder}/{name}.mat");

        static string TopLevel(Transform t)
        {
            while (t.parent != null)
                t = t.parent;
            return t.name;
        }

        /// <summary>
        /// The information reader's copy was authored dark, for a near-white panel.
        /// The panel is now dark glass, so the copy has to invert with it or the
        /// reader goes black on black. The two values are the same pair the status
        /// board uses on <see cref="ResearchUiKit.Glass"/>: about 14:1 for the title
        /// and 11:1 for the body against the composited surface, both well past the
        /// 4.5:1 floor the notice board was already held to.
        ///
        /// Only the GEN labels, which arrived with the original panel prefabs and
        /// have no builder that owns them. Every builder-made label carries its own
        /// colour and is corrected at its own source.
        /// </summary>
        static void RecolourCopyForGlass()
        {
            var title = ResearchMaterialPalette.Hex("#F2F5F8");
            var body = ResearchMaterialPalette.Hex("#DCE4EE");

            foreach (var label in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (label.name == "GEN Title")
                    label.color = title;
                else if (label.name == "GEN Body")
                    label.color = body;
                else
                    continue;
                EditorUtility.SetDirty(label);
            }
        }

        /// <summary>Retextures the world-space UI in the open scene.</summary>
        static void StyleScene(TMP_FontAsset font)
        {
            var panelSurface = Load("UI_PanelSurface");
            var panelBorder = Load("UI_PanelBorder");
            var cardFace = Load("UI_CardFace");
            var buttonPrimary = Load("UI_ButtonPrimary");
            var buttonSecondary = Load("UI_ButtonSecondary");
            var divider = Load("UI_Divider");

            foreach (var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var go = renderer.gameObject;
                var name = go.name;
                var group = TopLevel(go.transform);

                // --- work order board ---
                // Built as scene geometry, so it is the one panel that has to be
                // restyled by name rather than through ResearchUiKit. Same three
                // surfaces as every other panel: glass body, raised header, accent rule.
                if (group == "Task Brief")
                {
                    if (name == "Backing" && panelBorder != null)
                        renderer.sharedMaterial = panelBorder;
                    else if (name == "Face" && panelSurface != null)
                        renderer.sharedMaterial = panelSurface;
                    else if (name == "Header Band" && cardFace != null)
                        renderer.sharedMaterial = cardFace;
                    else if (name == "Header Rule" && divider != null)
                        renderer.sharedMaterial = divider;
                    continue;
                }

                // --- information source dock housing ---
                if (group == "Information Dock")
                {
                    if (name == "Housing" && cardFace != null)
                        renderer.sharedMaterial = cardFace;
                    else if (name == "Housing Edge" && divider != null)
                        renderer.sharedMaterial = divider;
                    else if (name.StartsWith("Slot ") && panelSurface != null)
                        renderer.sharedMaterial = panelSurface;
                    continue;
                }

                // --- information reader panels ---
                if (name == "GEN Frame" && panelSurface != null)
                    renderer.sharedMaterial = panelSurface;
                else if (name == "GEN Header" && cardFace != null)
                    renderer.sharedMaterial = cardFace;
                else if ((name == "GEN Header Rule" || name == "GEN Accent Bar") && divider != null)
                    renderer.sharedMaterial = divider;

                // --- controls: primary actions accent, everything else neutral ---
                else if (name.Contains(".control."))
                {
                    var primary = name.EndsWith("Next") || name.EndsWith("Play") || name.EndsWith("Restart");
                    var target = primary ? buttonPrimary : buttonSecondary;
                    if (target != null)
                        renderer.sharedMaterial = target;
                }

                // --- selector card bodies and notice cards ---
                else if (name == "GEN Slot" && cardFace != null)
                    renderer.sharedMaterial = cardFace;
                else if (go.transform.parent != null && go.transform.parent.name.StartsWith("Notice ")
                         && name == "Face" && panelSurface != null)
                    renderer.sharedMaterial = panelSurface;
                else if (go.transform.parent != null && go.transform.parent.name.StartsWith("Notice ")
                         && name == "Accent" && divider != null)
                    renderer.sharedMaterial = divider;
            }

            RecolourCopyForGlass();

            if (font == null)
                return;

            // Display face for headings and signage only. Kenney Future Narrow is an
            // all-caps display font: it renders paragraphs as shouty blocks and its
            // X reads as an H, so body copy and button labels keep the text face.
            var textFace = TMP_Settings.defaultFontAsset;
            foreach (var label in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                // Assigned both ways so re-running the pass corrects labels that an
                // earlier run had already switched to the display face.
                var target = IsDisplayText(label) ? font : textFace;
                if (target == null || label.font == target)
                    continue;
                label.font = target;
                EditorUtility.SetDirty(label);
            }
        }
    }
}
