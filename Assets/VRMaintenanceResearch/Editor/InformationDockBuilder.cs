using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Moves every information source off the workbench and onto one arm-mounted
    /// helper dock at the participant's left.
    ///
    /// Before: the four selector cards sat in a 2x2 grid on the bench's back edge at
    /// head height, directly over the spare-parts tray, and the opened reader covered
    /// the left third of the bench.
    ///
    /// Compact state is a single row of selector cards. Opening a source expands the
    /// reader over that row — the dock never grows, so it cannot creep back over the bench.
    /// InformationSourceController.Open() already closes any other open source, so a
    /// single shared reader position is safe.
    /// </summary>
    public static class InformationDockBuilder
    {
        /// <summary>Dock origin, placed to clear every task object's sight line from the participant pose.</summary>
        public static readonly Vector3 Dock = new Vector3(-1.78f, 0f, 0.58f);
        public const float DockYaw = -38f;
        public const float PanelTilt = 8f;

        const float k_GridY = 1.30f;        // centre of both the selector row and the reader
        const float k_CardW = 0.27f;        // rendered card width
        const float k_CardH = 0.140f;
        const float k_GapX = 0.024f;

        // Authored base scales: card (0.58, 0.28, 0.07), reader (0.90, 0.53, 0.05).
        // The reader's scale is measured at build time by FitReaderWidth.
        static readonly Vector3 k_CardScale = new Vector3(0.270f, 0.140f, 0.032f);

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Rebuild Information Docks")]
        public static void BuildAll()
        {
            foreach (var scenePath in new[] { ResearchSceneSet.Computer, ResearchSceneSet.Fan, ResearchSceneSet.Training })
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Build();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            Debug.Log("[InformationDock] rebuilt in all task scenes");
        }

        public static void Build()
        {
            var sources = Object.FindObjectsByType<InformationSourceController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(SourceOrder)
                .ToList();

            if (sources.Count == 0)
            {
                Debug.Log("[InformationDock] no information sources in this scene");
                return;
            }

            var rotation = Quaternion.Euler(0f, DockYaw, 0f);
            var fixture = BuildFixture(rotation, sources.Count);

            // The cards' order is authored here and baked into the scene, so the one
            // thing that can change it at run time rides on the dock it rearranges.
            // It is inert unless ResearchSessionConfig.counterbalanceInformationSourceOrder
            // is set, which it is not.
            if (fixture != null && fixture.GetComponent<InformationSourceLayoutApplier>() == null)
                fixture.AddComponent<InformationSourceLayoutApplier>();

            // One scale for every reader. Measuring each panel separately gave the
            // visual guide a different size (its sprite art changes the measured
            // width), so that one reader failed to cover the selector row.
            var readerScale = Vector3.zero;

            for (var i = 0; i < sources.Count; i++)
            {
                var slot = GridSlot(i, sources.Count);
                var card = sources[i].transform;
                // Absolute scale, not a measured fit: the cards share one authored
                // base size, and measuring their world AABB picks up child rotations.
                card.localScale = k_CardScale;
                card.SetPositionAndRotation(
                    Dock + rotation * new Vector3(slot.x, slot.y, -0.012f),
                    Quaternion.Euler(PanelTilt, DockYaw, 0f));
                TidyCard(sources[i]);

                var panel = FindContentPanel(sources[i]);
                if (panel == null)
                    continue;

                // The reader's readable face is +Z (frame at local -z, text at +z),
                // the opposite of the selector cards. Without the 180 deg flip it
                // presented its blank backing plate to the participant.
                panel.transform.SetPositionAndRotation(
                    Dock + rotation * new Vector3(0f, k_GridY, -0.075f),
                    Quaternion.Euler(-PanelTilt, DockYaw + 180f, 0f));
                if (readerScale == Vector3.zero)
                    readerScale = MeasureReaderScale(panel, RowWidth(sources.Count) + 0.04f);
                panel.transform.localScale = readerScale;
                TidyReaderHeader(panel);
                FitGuideArt(panel);

                PrefillReaderText(sources[i], panel);
                panel.SetActive(false);   // compact by default; Open() reveals it
            }
        }

        /// <summary>
        /// Single-row slot position in dock-local space. The row is wide and short
        /// to match the reader's authored 3:1 aspect, so the open state can cover
        /// the selectors without the reader's text being squashed to fit.
        /// </summary>
        static Vector2 GridSlot(int index, int count)
        {
            var totalW = count * k_CardW + (count - 1) * k_GapX;
            return new Vector2(-totalW * 0.5f + k_CardW * 0.5f + index * (k_CardW + k_GapX), k_GridY);
        }

        /// <summary>Bench-clamped arm so the dock reads as lab equipment, not a floating decal.</summary>
        static GameObject BuildFixture(Quaternion rotation, int cardCount)
        {
            var root = Root("Information Dock", Dock, new Vector3(0f, DockYaw, 0f));
            var t = root.transform;

            var panelW = cardCount * k_CardW + (cardCount - 1) * k_GapX + 0.06f;
            var panelH = k_CardH + 0.06f;

            // Clamp onto the left end of the bench, then rise and reach in.
            var clampLocal = Quaternion.Inverse(rotation) * (new Vector3(LabLayoutBuilder.DockClampX, 0f, 0.95f) - Dock);
            var clamp = Group("Clamp", t, new Vector3(clampLocal.x, 0.90f, clampLocal.z));
            Box("Clamp Jaw", clamp, Vector3.zero, new Vector3(0.10f, 0.070f, 0.16f), "Lab_Navy");
            Box("Clamp Pad", clamp, new Vector3(0f, -0.042f, 0f), new Vector3(0.09f, 0.014f, 0.15f), "Lab_Rubber");
            Cyl("Clamp Screw", clamp, new Vector3(0f, -0.060f, 0f), new Vector3(0.020f, 0.026f, 0.020f), "Lab_ToolSteel");

            var postTop = k_GridY + panelH * 0.5f - 0.04f;
            Cyl("Post", t, new Vector3(clampLocal.x, (0.94f + postTop) * 0.5f, clampLocal.z),
                new Vector3(0.030f, (postTop - 0.94f) * 0.5f, 0.030f), "Lab_MetalDark");
            Cyl("Post Collar", t, new Vector3(clampLocal.x, postTop - 0.02f, clampLocal.z), new Vector3(0.042f, 0.016f, 0.042f), "Lab_Navy");

            Box("Arm", t, new Vector3(clampLocal.x * 0.5f, postTop, clampLocal.z * 0.5f),
                new Vector3(Mathf.Abs(clampLocal.x) + 0.04f, 0.024f, 0.024f), "Lab_MetalDark",
                new Vector3(0f, Mathf.Atan2(-clampLocal.z, -clampLocal.x) * Mathf.Rad2Deg, 0f));
            Cyl("Arm Elbow", t, new Vector3(0f, postTop, 0f), new Vector3(0.038f, 0.016f, 0.038f), "Lab_Navy");

            // Housing sized to the grid only — the old backing was a 0.8 m blank slab.
            Box("Housing", t, new Vector3(0f, k_GridY, 0.028f), new Vector3(panelW, panelH, 0.024f), "Lab_Navy", new Vector3(PanelTilt, 0f, 0f));
            Box("Housing Edge", t, new Vector3(0f, k_GridY + panelH * 0.5f - 0.007f, 0.022f), new Vector3(panelW, 0.012f, 0.026f), "Lab_Accent", new Vector3(PanelTilt, 0f, 0f));

            // Recessed slot behind each card: navy cards on a navy housing vanished.
            for (var i = 0; i < cardCount; i++)
            {
                var slot = GridSlot(i, cardCount);
                Box($"Slot {i + 1}", t, new Vector3(slot.x, slot.y, 0.016f),
                    new Vector3(k_CardW + 0.016f, k_CardH + 0.016f, 0.008f), "Lab_Trim", new Vector3(PanelTilt, 0f, 0f));
            }

            var header = Label("Header", t, new Vector3(0f, k_GridY + panelH * 0.5f + 0.055f, 0.012f),
                "INFORMATION SOURCES", 0.26f, "#E8EDF3", new Vector3(PanelTilt, 0f, 0f), panelW);
            header.fontStyle = TMPro.FontStyles.Bold;
            header.characterSpacing = 10f;

            return root;
        }

        static void TidyCard(InformationSourceController source)
        {
            foreach (var label in source.GetComponentsInChildren<TMPro.TMP_Text>(true))
            {
                if (label.name == "GEN Caption")
                {
                    var definition = source.Definition;
                    if (definition != null)
                        label.text = InformationSourceController.CompactSourceLabel(definition.sourceType, ResearchLanguage.English);
                    label.fontSize = 0.68f;
                    EditorUtility.SetDirty(label);
                }
                else if (label.name == "GEN Slot")
                {
                    label.gameObject.SetActive(false);
                    EditorUtility.SetDirty(label.gameObject);
                }
            }
        }

        static float RowWidth(int count) => count * k_CardW + (count - 1) * k_GapX;

        /// <summary>
        /// The visual guide's sprite rendered 1.4x wider than its own panel, so it
        /// overhung the reader on both sides and reached back over the bench.
        /// </summary>
        static void FitGuideArt(GameObject panel)
        {
            var art = panel.transform.Find("Visual Guide Art");
            if (art == null)
                return;

            var artRenderer = art.GetComponent<Renderer>();
            var frame = panel.transform.Find("GEN Frame");
            var frameRenderer = frame != null ? frame.GetComponent<Renderer>() : null;
            if (artRenderer == null || frameRenderer == null)
                return;

            var artWidth = artRenderer.bounds.size.x;
            var target = frameRenderer.bounds.size.x * 0.62f;
            if (artWidth <= 0.0001f || artWidth <= target)
                return;

            art.localScale *= target / artWidth;
            art.localPosition = new Vector3(art.localPosition.x, -0.06f, art.localPosition.z);
        }

        /// <summary>
        /// Returns the scale that makes the reader cover the selector row while
        /// preserving its authored aspect. Measured with rotation temporarily
        /// cleared: a rotated object's world AABB is inflated by the rotation and
        /// over-reports width, which left the outer cards poking out either side.
        /// </summary>
        static Vector3 MeasureReaderScale(GameObject panel, float target)
        {
            var t = panel.transform;
            var rotation = t.rotation;
            var scale = t.localScale;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;

            var renderers = panel.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                t.rotation = rotation;
                t.localScale = scale;
                return scale;
            }

            var bounds = renderers[0].bounds;
            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);
            var unitWidth = bounds.size.x;   // width at scale 1

            t.rotation = rotation;
            if (unitWidth <= 0.0001f)
            {
                t.localScale = scale;
                return scale;
            }

            // Authored panel proportions are 0.90 : 0.53 : 0.05.
            var sx = target / unitWidth;
            var fitted = new Vector3(sx, sx * (0.53f / 0.90f), sx * (0.05f / 0.90f));
            t.localScale = fitted;
            return fitted;
        }

        /// <summary>
        /// The close control shipped labelled "Next" and overlapping the reader
        /// title. Panel-local -X is the viewer's right, so it is pinned to the far
        /// right of the header and the title is inset to leave room for it.
        /// </summary>
        static void TidyReaderHeader(GameObject panel)
        {
            foreach (var t in panel.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.EndsWith(".control.Close"))
                {
                    t.localPosition = new Vector3(-0.408f, 0.400f, 0.800f);
                    t.localScale = new Vector3(0.170f, 0.090f, 0.028f);
                    foreach (var label in t.GetComponentsInChildren<TMPro.TMP_Text>(true))
                    {
                        label.text = "Close";
                        EditorUtility.SetDirty(label);
                    }
                    EditorUtility.SetDirty(t);
                }
                else if (t.name == "GEN Title")
                {
                    var title = t.GetComponent<TMPro.TMP_Text>();
                    if (title == null)
                        continue;
                    title.alignment = TMPro.TextAlignmentOptions.Left;
                    title.enableAutoSizing = true;
                    title.fontSizeMin = 0.10f;
                    title.rectTransform.sizeDelta = new Vector2(0.72f, 0.14f);
                    t.localPosition = new Vector3(0.115f, 0.400f, 0.800f);
                    EditorUtility.SetDirty(title);
                }
            }
        }

        /// <summary>
        /// Writes the definition's English copy into the reader's labels at build
        /// time. InformationSourceController only fills these in Awake(), so before
        /// this the reader was a blank navy slab in the editor and in every
        /// screenshot — the "unloaded guide content" the audit flagged.
        /// </summary>
        static void PrefillReaderText(InformationSourceController source, GameObject panel)
        {
            var definition = source.Definition;
            if (definition == null)
                return;

            var title = definition.englishTitle.Replace(" (Development)", string.Empty);
            var isVideo = definition.sourceType == InformationSourceType.InstructionalVideo;

            var modernLabels = panel.GetComponentsInChildren<TMPro.TMP_Text>(true);
            foreach (var label in modernLabels)
            {
                if (label.name == "GEN Title")
                    label.text = title;
                else if (label.name == "GEN Body")
                {
                    label.enabled = !isVideo;
                    if (label.enabled)
                        label.text = definition.englishContent;
                }
                EditorUtility.SetDirty(label);
            }

            foreach (var label in panel.GetComponentsInChildren<TextMesh>(true))
            {
                if (!label.name.EndsWith(" Text"))
                    continue;
                label.text = title + "\n\n" + definition.englishContent;
                // TMP is the visible reader copy. Some legacy fan panels still had
                // their old TextMesh enabled, producing mirrored ghost text behind
                // the dock when the reader opened.
                if (modernLabels.Length > 0)
                    label.gameObject.SetActive(false);
                EditorUtility.SetDirty(label);
            }
        }

        static GameObject FindContentPanel(InformationSourceController source)
        {
            var so = new SerializedObject(source);
            var prop = so.FindProperty("contentPanel");
            return prop != null ? prop.objectReferenceValue as GameObject : null;
        }

        static int SourceOrder(InformationSourceController source)
        {
            var definition = source.Definition;
            return definition == null ? 99 : (int)definition.sourceType;
        }
    }
}
