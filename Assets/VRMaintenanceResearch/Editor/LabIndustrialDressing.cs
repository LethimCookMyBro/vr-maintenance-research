using UnityEditor;
using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Turns the greybox shell into the industrial service bay the reference asks
    /// for: two-tone walls, yellow floor marking and guardrails, steel racking with
    /// stacking crates, a wall control cabinet with a live-looking display and
    /// status lamps, shape-only safety signage, and green ESD matting on the bench.
    ///
    /// It writes into the <c>LabEnvironment</c> prefab rather than into four scenes,
    /// so Training, Computer, Fan and ResearcherSetup get exactly the same room and
    /// cannot drift apart. Re-runnable: the whole group is rebuilt each time.
    ///
    /// Everything here is background, and three rules keep it that way.
    /// * No <see cref="ResearchInteractable"/>, so nothing can be a task object.
    /// * No collider — <see cref="ResearchBuildKit"/> strips the primitive's own on
    ///   creation and <see cref="AssertInert"/> fails the build if one survives — so
    ///   nothing here can answer a controller ray aimed at a part.
    /// * Nothing stands between the participant pose and the workstation. The two
    ///   free bands of far wall are the ones the work order and the notice board do
    ///   not already occupy, and the racking, rails and cabinet sit in those; the
    ///   rails run down the side aisles outside the bench.
    ///
    /// Marked batching-static, because sixty-odd small props at one draw call each
    /// is the thing that would actually cost a Quest frame.
    /// </summary>
    public static class LabIndustrialDressing
    {
        const string k_PrefabPath = "Assets/VRMaintenanceResearch/Prefabs/Environment/LabEnvironment.prefab";
        const string k_RootName = "Industrial Dressing";

        // Inner faces of the shell: walls are 0.10 thick, centred at +-4.55 / -3.55 / 4.55.
        const float k_WallNorth = 4.50f;
        const float k_WallSouth = -3.50f;
        const float k_WallWest = -4.50f;
        const float k_WallEast = 4.50f;
        const float k_DadoTop = 1.30f;

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Build Industrial Dressing", priority = 6)]
        public static void Build()
        {
            var prefab = PrefabUtility.LoadPrefabContents(k_PrefabPath);
            try
            {
                var existing = prefab.transform.Find(k_RootName);
                if (existing != null)
                    Object.DestroyImmediate(existing.gameObject);

                var root = Group(k_RootName, prefab.transform);
                TwoToneWalls(root);
                FloorMarking(root);
                Guardrail(root, "Guardrail West", -3.88f);
                Guardrail(root, "Guardrail East", 3.88f);
                // Both bays are in the far right corner, meeting at a right angle. The
                // first attempt put one at x = 0.55 — the middle of the free wall band —
                // and it stood directly behind the device on both benches. The band
                // behind the workstation stays empty on purpose.
                Racking(root, "Racking East", new Vector3(4.20f, 0f, 3.00f), 90f);
                Racking(root, "Racking North", new Vector3(4.05f, 0f, 4.20f), 0f);
                ControlCabinet(root);
                SafetySigns(root);
                BenchMatting(root);

                RepaintWorkZoneLines(prefab.transform);
                MarkStatic(root);
                AssertInert(root);

                PrefabUtility.SaveAsPrefabAsset(prefab, k_PrefabPath);
                Debug.Log($"[IndustrialDressing] rebuilt: {root.GetComponentsInChildren<Renderer>(true).Length} renderers");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        /// <summary>
        /// Navy to waist height, the shell's off-white above it, with a safety-yellow
        /// rail cap on the joint. The dado stands a few millimetres proud of the wall
        /// rather than replacing it, so nothing about the room's dimensions changes.
        /// </summary>
        static void TwoToneWalls(Transform root)
        {
            var t = Group("Two Tone Walls", root);
            const float y = k_DadoTop * 0.5f;

            Box("Dado North", t, new Vector3(0f, y, k_WallNorth - 0.016f), new Vector3(9.00f, k_DadoTop, 0.030f), "Lab_Navy");
            Box("Stripe North", t, new Vector3(0f, k_DadoTop + 0.025f, k_WallNorth - 0.019f), new Vector3(9.00f, 0.050f, 0.036f), "Lab_SafetyYellow");

            Box("Dado South", t, new Vector3(0f, y, k_WallSouth + 0.016f), new Vector3(9.00f, k_DadoTop, 0.030f), "Lab_Navy");
            Box("Stripe South", t, new Vector3(0f, k_DadoTop + 0.025f, k_WallSouth + 0.019f), new Vector3(9.00f, 0.050f, 0.036f), "Lab_SafetyYellow");

            Box("Dado West", t, new Vector3(k_WallWest + 0.016f, y, 0.50f), new Vector3(0.030f, k_DadoTop, 8.00f), "Lab_Navy");
            Box("Stripe West", t, new Vector3(k_WallWest + 0.019f, k_DadoTop + 0.025f, 0.50f), new Vector3(0.036f, 0.050f, 8.00f), "Lab_SafetyYellow");

            Box("Dado East", t, new Vector3(k_WallEast - 0.016f, y, 0.50f), new Vector3(0.030f, k_DadoTop, 8.00f), "Lab_Navy");
            Box("Stripe East", t, new Vector3(k_WallEast - 0.019f, k_DadoTop + 0.025f, 0.50f), new Vector3(0.036f, 0.050f, 8.00f), "Lab_SafetyYellow");
        }

        /// <summary>Two aisle lines just inboard of the guardrails; the work-zone box is the existing marking, repainted.</summary>
        static void FloorMarking(Transform root)
        {
            var t = Group("Floor Marking", root);
            Box("Aisle Line West", t, new Vector3(-3.55f, 0.005f, 0.50f), new Vector3(0.070f, 0.010f, 7.80f), "Lab_SafetyYellow");
            Box("Aisle Line East", t, new Vector3(3.55f, 0.005f, 0.50f), new Vector3(0.070f, 0.010f, 7.80f), "Lab_SafetyYellow");
        }

        /// <summary>
        /// The work zone was outlined in Lab_Accent, which is the interface's blue.
        /// Floor marking is yellow in every real bay, and reserving the accent for the
        /// interface keeps a painted line from reading as a signal.
        /// </summary>
        static void RepaintWorkZoneLines(Transform prefabRoot)
        {
            var yellow = ResearchMaterialPalette.Load("Lab_SafetyYellow");
            if (yellow == null)
                return;

            foreach (var child in prefabRoot.GetComponentsInChildren<Transform>(true))
            {
                if (!child.name.StartsWith("Work Zone Line"))
                    continue;
                var renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.sharedMaterial = yellow;
            }
        }

        /// <summary>A 3 m run of yellow barrier down one side aisle: posts, two rails, kick plate.</summary>
        static void Guardrail(Transform root, string name, float x)
        {
            var t = Group(name, root);
            const float zCentre = 2.10f;
            const float length = 3.00f;

            for (var i = 0; i < 5; i++)
            {
                var z = zCentre - length * 0.5f + i * (length / 4f);
                Cyl($"Post {i + 1}", t, new Vector3(x, 0.525f, z), new Vector3(0.055f, 0.525f, 0.055f), "Lab_SafetyYellow");
            }

            Box("Rail Top", t, new Vector3(x, 1.020f, zCentre), new Vector3(0.055f, 0.055f, length + 0.055f), "Lab_SafetyYellow");
            Box("Rail Mid", t, new Vector3(x, 0.600f, zCentre), new Vector3(0.055f, 0.055f, length + 0.055f), "Lab_SafetyYellow");
            Box("Kick Plate", t, new Vector3(x, 0.090f, zCentre), new Vector3(0.020f, 0.160f, length + 0.055f), "Lab_SafetyYellow");
        }

        /// <summary>One bay of steel racking with blue stacking crates on it.</summary>
        static void Racking(Transform root, string name, Vector3 centre, float yaw)
        {
            var t = Group(name, root, centre, new Vector3(0f, yaw, 0f));

            for (var i = 0; i < 4; i++)
            {
                var x = (i % 2 == 0 ? -1f : 1f) * 0.435f;
                var z = (i < 2 ? -1f : 1f) * 0.200f;
                Box($"Upright {i + 1}", t, new Vector3(x, 1.000f, z), new Vector3(0.050f, 2.000f, 0.050f), "Lab_MetalDark");
            }

            var shelfHeights = new[] { 0.20f, 0.72f, 1.24f, 1.76f };
            for (var i = 0; i < shelfHeights.Length; i++)
                Box($"Shelf {i + 1}", t, new Vector3(0f, shelfHeights[i], 0f), new Vector3(0.920f, 0.035f, 0.460f), "Lab_MetalDark");

            // Crates rest on a shelf: shelf centre + half its 35 mm board + half the
            // 240 mm crate. The first pass offset them by hand and left one hanging
            // 20 cm above a shelf with nothing under it.
            const float rest = 0.0175f + 0.120f;
            Crate(t, "Crate 1", -0.230f, shelfHeights[0] + rest, 0.010f);
            Crate(t, "Crate 2", -0.230f, shelfHeights[1] + rest, -0.008f);
            Crate(t, "Crate 3", 0.230f, shelfHeights[1] + rest, 0.012f);
            Crate(t, "Crate 4", 0.230f, shelfHeights[2] + rest, -0.010f);
            Crate(t, "Crate 5", 0.230f, shelfHeights[2] + rest + 0.240f, 0.004f);
        }

        static void Crate(Transform bay, string name, float x, float y, float z)
        {
            Box(name, bay, new Vector3(x, y, z), new Vector3(0.400f, 0.240f, 0.400f), "Lab_CrateBlue");
        }

        /// <summary>
        /// Wall control cabinet: green display, red / amber / green status lamps.
        ///
        /// It sits at x = -3.30 because that is far enough left to clear the work
        /// order's shadow from the participant pose — at 6 m the brief covers roughly
        /// -5 deg to -21 deg, and the cabinet subtends -23 deg to -30 deg.
        /// The lamps are lit by the emissive palette materials the benches already
        /// use, so the cabinet adds no material the scenes do not already load.
        /// </summary>
        static void ControlCabinet(Transform root)
        {
            var t = Group("Control Cabinet", root, new Vector3(-3.30f, 0f, 0f));
            const float face = k_WallNorth - 0.21f;

            Box("Body", t, new Vector3(0f, 1.680f, face - 0.100f), new Vector3(0.860f, 1.080f, 0.200f), "Lab_MetalDark");
            Box("Door", t, new Vector3(0f, 1.680f, face - 0.207f), new Vector3(0.780f, 0.980f, 0.020f), "Lab_Navy");
            Box("Screen Bezel", t, new Vector3(0f, 1.900f, face - 0.222f), new Vector3(0.500f, 0.340f, 0.008f), "Lab_PlasticDark");
            Box("Screen", t, new Vector3(0f, 1.900f, face - 0.228f), new Vector3(0.430f, 0.270f, 0.010f), "Lab_StatusGreen");

            Cyl("Lamp Red", t, new Vector3(-0.190f, 1.480f, face - 0.226f), new Vector3(0.085f, 0.010f, 0.085f), "Lab_StatusRed", new Vector3(90f, 0f, 0f));
            Cyl("Lamp Amber", t, new Vector3(0f, 1.480f, face - 0.226f), new Vector3(0.085f, 0.010f, 0.085f), "Lab_Warning", new Vector3(90f, 0f, 0f));
            Cyl("Lamp Green", t, new Vector3(0.190f, 1.480f, face - 0.226f), new Vector3(0.085f, 0.010f, 0.085f), "Lab_StatusGreen", new Vector3(90f, 0f, 0f));

            Box("Data Plate", t, new Vector3(0f, 1.300f, face - 0.224f), new Vector3(0.420f, 0.080f, 0.006f), "Lab_LabelPlate");
            // Runs the whole way to the ceiling slab at 3.00; stopping short left the
            // cabinet with a length of pipe ending in mid-air.
            Cyl("Conduit", t, new Vector3(0f, 2.610f, face - 0.060f), new Vector3(0.050f, 0.390f, 0.050f), "Lab_MetalDark");
        }

        /// <summary>
        /// Three wall signs in the ISO shapes — hazard diamond, mandatory disc, safe
        /// condition square — and no glyph on any of them.
        ///
        /// Deliberately wordless. The room is read from anywhere in it, and this study
        /// measures how a participant seeks information; a legible line of English on
        /// the wall is another information source in a task whose sources are the
        /// controlled variable.
        /// </summary>
        static void SafetySigns(Transform root)
        {
            var t = Group("Safety Signs", root);
            const float z = k_WallNorth - 0.015f;

            // High on the middle band of the far wall — the one stretch neither the
            // work order nor the notice board casts a shadow across from the
            // participant pose, and a metre above the sight line to the bench.
            const float y = 2.400f;

            Box("Hazard Backing", t, new Vector3(-0.25f, y, z), new Vector3(0.400f, 0.400f, 0.010f), "Lab_PlasticDark", new Vector3(0f, 0f, 45f));
            Box("Hazard Face", t, new Vector3(-0.25f, y, z - 0.006f), new Vector3(0.300f, 0.300f, 0.012f), "Lab_Warning", new Vector3(0f, 0f, 45f));

            // Dark backings on all three: a white sign plate on a white wall is an
            // invisible border, which left the first pass looking like coloured decals.
            Cyl("Mandatory Backing", t, new Vector3(0.38f, y, z), new Vector3(0.420f, 0.005f, 0.420f), "Lab_PlasticDark", new Vector3(90f, 0f, 0f));
            Cyl("Mandatory Face", t, new Vector3(0.38f, y, z - 0.006f), new Vector3(0.330f, 0.006f, 0.330f), "Lab_Accent", new Vector3(90f, 0f, 0f));

            // 1.00 and no further right: the notice board's left edge is at 1.21.
            Box("Condition Backing", t, new Vector3(1.00f, y, z), new Vector3(0.400f, 0.400f, 0.010f), "Lab_PlasticDark");
            Box("Condition Face", t, new Vector3(1.00f, y, z - 0.006f), new Vector3(0.310f, 0.310f, 0.012f), "Lab_StatusGreen");
        }

        /// <summary>
        /// Green ESD matting the length of the bench. It sits 2 mm above the bench top
        /// and 2 mm below the per-scene Service Mat, so the darker service pad still
        /// reads as the service area and neither surface z-fights the other.
        ///
        /// Sized to 2.60 m so it fits the 3.60 m bench the scenes use, not the 4.60 m
        /// one authored in this prefab.
        /// </summary>
        static void BenchMatting(Transform root)
        {
            Box("ESD Bench Matting", Group("Bench", root), new Vector3(0f, 0.9225f, 0.900f),
                new Vector3(2.600f, 0.004f, 0.640f), "Lab_Pcb");
        }

        static void MarkStatic(Transform root)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic);
        }

        /// <summary>
        /// A single collider here would put decoration in front of a task part for an
        /// XR ray, which is exactly the defect 743b1c3 and its follow-up were about.
        /// </summary>
        static void AssertInert(Transform root)
        {
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                Debug.LogError($"[IndustrialDressing] '{collider.name}' carries a {collider.GetType().Name}; dressing must never answer a ray.");
            foreach (var interactable in root.GetComponentsInChildren<ResearchInteractable>(true))
                Debug.LogError($"[IndustrialDressing] '{interactable.name}' carries a ResearchInteractable; dressing must never be a task object.");
        }
    }
}
