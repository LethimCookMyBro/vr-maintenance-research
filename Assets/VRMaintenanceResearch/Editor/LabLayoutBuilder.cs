using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Normalises the room across scenes.
    ///
    /// Two problems this fixes. First, the 4.6 m bench dwarfed every task object,
    /// leaving most of the frame empty — it is cut to 3.6 m and the test station
    /// pulled in to match. Second, the Information Station had been dragged onto
    /// the bench's back edge in the computer scene, where it sat as a blank 1.65 m
    /// slab across the work area; it goes back to the wall.
    /// </summary>
    public static class LabLayoutBuilder
    {
        public const float BenchHalfWidth = 1.80f;
        public const float TestStationX = 1.50f;
        public const float DockClampX = -1.72f;

        /// <summary>Canonical wall position for the Station Backing panel.</summary>
        // Right of centre on the far wall: the information dock already occupies the
        // participant's left, and stacking both there read as one cluttered mass.
        public static readonly Vector3 StationBackingHome = new Vector3(2.10f, 1.34f, 3.22f);

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Normalise Lab Layout")]
        public static void ApplyToAllScenes()
        {
            foreach (var scenePath in ResearchSceneSet.AllScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Apply();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            Debug.Log("[LabLayout] normalised in all scenes");
        }

        public static void Apply()
        {
            ResizeBench();
            MoveInformationStationHome();
            MoveTestStation();
            BuildPostProcessing();
        }

        static void ResizeBench()
        {
            SetScaleX("Workbench Top", BenchHalfWidth * 2f);
            SetScaleX("Workbench Apron", BenchHalfWidth * 2f - 0.14f);
            SetScaleX("Workbench Shelf", BenchHalfWidth * 2f - 0.30f);
            SetScaleX("Work Zone Line Front", BenchHalfWidth * 2f + 0.60f);

            // Legs: keep the original front/back z, pull the x inboard.
            var legX = BenchHalfWidth - 0.14f;
            var index = 0;
            foreach (var t in FindAll("Workbench Leg"))
            {
                var p = t.position;
                t.position = new Vector3(index < 2 ? -legX : legX, p.y, p.z);
                index++;
            }

            foreach (var t in FindAll("Work Zone Line Left"))
                t.position = new Vector3(-(BenchHalfWidth + 0.30f), t.position.y, t.position.z);
            foreach (var t in FindAll("Work Zone Line Right"))
                t.position = new Vector3(BenchHalfWidth + 0.30f, t.position.y, t.position.z);
        }

        /// <summary>
        /// Shifts the whole Information Station group by whatever delta puts its
        /// backing panel back on the wall, so it works regardless of where a given
        /// scene had dragged it.
        /// </summary>
        static void MoveInformationStationHome()
        {
            var backing = GameObject.Find("Station Backing");
            if (backing == null)
                return;

            var group = backing.transform.parent;
            if (group == null)
                return;

            var delta = StationBackingHome - backing.transform.position;
            if (delta.sqrMagnitude < 0.0001f)
                return;

            group.position += delta;
            Debug.Log($"[LabLayout] Information Station moved by {delta}");
        }

        static void MoveTestStation()
        {
            // Control Pedestal parts share an x; slide the group in with the bench.
            foreach (var name in new[] { "Base", "Column", "Cap", "Cap Accent" })
            {
                foreach (var t in FindAll(name))
                {
                    if (t.parent == null || t.parent.name != "Control Pedestal")
                        continue;
                    t.position = new Vector3(TestStationX, t.position.y, t.position.z);
                }
            }

            foreach (var name in new[] { "Computer Power Button", "Fan Speed Selector" })
            {
                var go = GameObject.Find(name);
                if (go != null)
                    go.transform.position = new Vector3(TestStationX, go.transform.position.y, go.transform.position.z);
            }
        }

        /// <summary>
        /// URP was rendering with no tonemapper, so every lit surface clipped to
        /// white. A global volume with neutral tonemapping restores the highlights.
        /// </summary>
        static void BuildPostProcessing()
        {
            var existing = GameObject.Find("Lab Post Processing");
            if (existing != null)
                Object.DestroyImmediate(existing);

            var profile = LoadOrCreateProfile();
            var go = new GameObject("Lab Post Processing");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.sharedProfile = profile;

            // A volume alone does nothing: URP cameras have post-processing off by
            // default, which is why the lab was clipping to white.
            foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var data = camera.GetUniversalAdditionalCameraData();
                if (data == null)
                    continue;
                data.renderPostProcessing = true;
                // The rig cameras are prefab instances: dirtying the Camera does
                // not record an override on UniversalAdditionalCameraData, which is
                // the component that actually holds this flag.
                EditorUtility.SetDirty(data);
                PrefabUtility.RecordPrefabInstancePropertyModifications(data);
            }
        }

        const string k_ProfilePath = "Assets/VRMaintenanceResearch/Materials/LabPostProcessing.asset";

        static VolumeProfile LoadOrCreateProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(k_ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, k_ProfilePath);
            }

            // Rebuild the components so re-running cannot stack duplicates.
            for (var i = profile.components.Count - 1; i >= 0; i--)
                Object.DestroyImmediate(profile.components[i], true);
            profile.components.Clear();

            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.overrideState = true;
            tonemapping.mode.value = TonemappingMode.Neutral;

            var exposure = profile.Add<ColorAdjustments>(true);
            exposure.postExposure.overrideState = true;
            exposure.postExposure.value = -0.35f;
            exposure.contrast.overrideState = true;
            exposure.contrast.value = 12f;
            exposure.saturation.overrideState = true;
            exposure.saturation.value = 6f;

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 1.05f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.35f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.62f;

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.18f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.5f;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        static void SetScaleX(string name, float x)
        {
            foreach (var t in FindAll(name))
            {
                var s = t.localScale;
                t.localScale = new Vector3(x, s.y, s.z);
            }
        }

        static System.Collections.Generic.List<Transform> FindAll(string name)
        {
            var found = new System.Collections.Generic.List<Transform>();
            var lab = GameObject.Find("Lab Environment");
            if (lab == null)
                return found;
            foreach (var t in lab.GetComponentsInChildren<Transform>(true))
                if (t.name == name)
                    found.Add(t);
            return found;
        }
    }
}
