using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Gives the lab directional lighting and real reflections.
    /// Before: two directional lights plus flat trilight ambient at full
    /// intensity, and no reflection probe, so metals had nothing to reflect
    /// and every surface rendered as the same chalky grey.
    /// Re-runnable: named objects are reused, not duplicated.
    /// </summary>
    public static class ResearchLightingRig
    {
        const string k_RigName = "Research Lighting Rig";

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Apply Lighting Rig")]
        public static void ApplyToAllScenes()
        {
            foreach (var scenePath in ResearchSceneSet.AllScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Apply();
                // Edit-mode GameObject creation and RenderSettings writes do not
                // dirty the scene on their own, so SaveOpenScenes silently skipped.
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            Debug.Log("[LightingRig] applied to all research scenes");
        }

        public static void Apply()
        {
            TuneAmbient();
            TuneDirectionalLights();
            var rig = BuildRig();
            BuildCeilingFixtures(rig);
            BuildBenchLights(rig);
            BuildReflectionProbe(rig);
        }

        static void TuneAmbient()
        {
            // Flat ambient at 1.0 was washing out every shadow. Drop it and let
            // the lights carry the shaping; keep a cool bounce off the floor.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ResearchMaterialPalette.Hex("#AEB8C3");
            RenderSettings.ambientEquatorColor = ResearchMaterialPalette.Hex("#929BA5");
            RenderSettings.ambientGroundColor = ResearchMaterialPalette.Hex("#5C6268");
            RenderSettings.ambientIntensity = 0.78f;
            RenderSettings.reflectionIntensity = 1f;
            RenderSettings.fog = false;
        }

        static void TuneDirectionalLights()
        {
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (light.type != LightType.Directional)
                    continue;

                if (light.name.Contains("Key"))
                {
                    light.intensity = 0.95f;
                    light.color = ResearchMaterialPalette.Hex("#FFF4E2");
                    light.shadows = LightShadows.Soft;
                    light.shadowStrength = 0.62f;
                    light.transform.rotation = Quaternion.Euler(48f, 152f, 0f);
                }
                else
                {
                    light.intensity = 0.32f;
                    light.color = ResearchMaterialPalette.Hex("#C9DAFF");
                    light.shadows = LightShadows.None;
                    light.transform.rotation = Quaternion.Euler(20f, -35f, 0f);
                }
            }
        }

        static Transform BuildRig()
        {
            var existing = GameObject.Find(k_RigName);
            if (existing != null)
                Object.DestroyImmediate(existing);
            return new GameObject(k_RigName).transform;
        }

        // URP's per-object additional-light limit here is 4, so the rig stays at
        // three: two room fills plus one bench key. The outer ceiling panels are
        // emissive geometry only — they read as lit without costing a light slot.
        static void BuildCeilingFixtures(Transform rig)
        {
            var positions = new[] { new Vector3(-1.9f, 2.80f, 0.6f), new Vector3(1.9f, 2.80f, 0.6f) };

            for (var i = 0; i < positions.Length; i++)
            {
                var go = new GameObject($"Ceiling Fixture {i + 1}");
                go.transform.SetParent(rig, false);
                go.transform.position = positions[i];
                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = ResearchMaterialPalette.Hex("#FFF7EA");
                light.intensity = 2.1f;
                light.range = 8f;
                light.shadows = LightShadows.None;
            }
        }

        /// <summary>Task lighting over the bench: this is what makes the workstation the focal point.</summary>
        static void BuildBenchLights(Transform rig)
        {
            var key = new GameObject("Bench Key");
            key.transform.SetParent(rig, false);
            key.transform.position = new Vector3(-0.35f, 2.35f, -0.15f);
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Spot;
            keyLight.color = ResearchMaterialPalette.Hex("#FFFDF7");
            keyLight.intensity = 6.0f;
            keyLight.range = 6f;
            keyLight.spotAngle = 95f;
            keyLight.innerSpotAngle = 40f;
            keyLight.shadows = LightShadows.Soft;
            keyLight.shadowStrength = 0.45f;
            key.transform.rotation = Quaternion.Euler(52f, 16f, 0f);
        }

        static void BuildReflectionProbe(Transform rig)
        {
            var go = new GameObject("Lab Reflection Probe");
            go.transform.SetParent(rig, false);
            go.transform.position = new Vector3(0f, 1.4f, 0.9f);

            var probe = go.AddComponent<ReflectionProbe>();
            // Baked probes need a full lighting bake, which BakeReflectionProbe
            // kept refusing on this scene. Realtime-on-awake renders once when the
            // scene loads at runtime; the capture tool renders it explicitly.
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
            probe.size = new Vector3(9.4f, 3.2f, 8.4f);
            probe.resolution = 128;
            probe.cullingMask = ~0;
            probe.clearFlags = ReflectionProbeClearFlags.SolidColor;
            probe.backgroundColor = ResearchMaterialPalette.Hex("#9BA5B0");
            probe.intensity = 1f;
            probe.boxProjection = true;
        }

        /// <summary>Renders the realtime probe so edit-mode captures show reflections.</summary>
        public static void RenderProbes()
        {
            foreach (var probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                probe.RenderProbe();
        }
    }

    public static class ResearchSceneSet
    {
        public const string Computer = "Assets/VRMaintenanceResearch/Scenes/ComputerRepairTask.unity";
        public const string Fan = "Assets/VRMaintenanceResearch/Scenes/FanRepairTask.unity";
        public const string Training = "Assets/VRMaintenanceResearch/Scenes/VRTraining.unity";
        public const string ResearcherSetup = "Assets/VRMaintenanceResearch/Scenes/ResearcherSetup.unity";

        public static readonly string[] AllScenes = { Computer, Fan, Training, ResearcherSetup };
    }
}
