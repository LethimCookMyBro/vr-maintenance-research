using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Renders every research scene from a fixed set of poses so before/after
    /// visual-audit screenshots are directly comparable. Editor-only.
    /// </summary>
    public static class VisualAuditCapture
    {
        public const string OutputFolder = "Assets/VRMaintenanceResearch/Docs/Screenshots/Audit";

        struct Pose
        {
            public string Name;
            public Vector3 Position;
            public Vector3 Euler;
            public Vector3 Target;
            public bool LookAt;
            public float Fov;
        }

        // ponytail: fixed poses, not a camera-rig asset. Scene layout is stable.
        static readonly Pose[] k_TaskPoses =
        {
            new Pose { Name = "ParticipantEye", Position = new Vector3(0f, 1.36f, -1.6f), Euler = new Vector3(6f, 0f, 0f), Fov = 60f },
            new Pose { Name = "Workstation", Position = new Vector3(0f, 1.55f, -0.9f), Euler = new Vector3(20f, 0f, 0f), Fov = 55f },
            new Pose { Name = "Overview", Position = new Vector3(2.6f, 2.1f, -2.6f), Euler = new Vector3(18f, -38f, 0f), Fov = 60f },
            new Pose { Name = "SideProfile", Position = new Vector3(-2.9f, 1.5f, -0.6f), Euler = new Vector3(8f, 62f, 0f), Fov = 55f },
        };

        static readonly Pose[] k_RoomPoses =
        {
            new Pose { Name = "ParticipantEye", Position = new Vector3(0f, 1.36f, -1.6f), Euler = new Vector3(6f, 0f, 0f), Fov = 60f },
            new Pose { Name = "Overview", Position = new Vector3(2.6f, 2.1f, -2.6f), Euler = new Vector3(18f, -38f, 0f), Fov = 60f },
        };

        static readonly (string scene, string label, bool taskPoses)[] k_Scenes =
        {
            ("Assets/VRMaintenanceResearch/Scenes/ComputerRepairTask.unity", "Computer", true),
            ("Assets/VRMaintenanceResearch/Scenes/FanRepairTask.unity", "Fan", true),
            ("Assets/VRMaintenanceResearch/Scenes/VRTraining.unity", "Training", true),
            ("Assets/VRMaintenanceResearch/Scenes/ResearcherSetup.unity", "ResearcherSetup", false),
        };

        /// <summary>
        /// Walk-up poses for the comprehension check.
        ///
        /// The four audit poses all stand 1.6 m back, which is where a participant
        /// starts, not where they work. A reader shown only those shots can say the
        /// bench looks tidy and still have no idea what the components are. These are
        /// the views a participant actually gets once they step in: the open side of
        /// the case, the front of the fan, and the service bay each fill the frame.
        /// </summary>
        static readonly (string scene, string label, Pose[] poses)[] k_ApproachSets =
        {
            ("Assets/VRMaintenanceResearch/Scenes/ComputerRepairTask.unity", "Computer", new[]
            {
                new Pose { Name = "OpenSide", Position = new Vector3(-0.40f, 1.28f, 0.32f), Euler = new Vector3(10f, 20f, 0f), Fov = 52f },
                new Pose { Name = "BoardDetail", Position = new Vector3(-0.30f, 1.20f, 0.62f), Euler = new Vector3(4f, 26f, 0f), Fov = 34f },
                LookAt("AtxConnector", new Vector3(-0.34f, 1.21f, 0.63f), new Vector3(-0.01f, 1.19f, 0.98f), 28f),
                LookAt("TaskBrief", new Vector3(-0.68f, 1.54f, 0.25f), new Vector3(-0.68f, 1.54f, 1.30f), 42f),
                new Pose { Name = "SparesTray", Position = new Vector3(-1.02f, 1.30f, 0.42f), Euler = new Vector3(34f, -14f, 0f), Fov = 46f },
                new Pose { Name = "InspectControl", Position = new Vector3(1.34f, 1.34f, -0.62f), Euler = new Vector3(26f, 12f, 0f), Fov = 44f },
            }),
            ("Assets/VRMaintenanceResearch/Scenes/FanRepairTask.unity", "Fan", new[]
            {
                new Pose { Name = "Front", Position = new Vector3(0.36f, 1.30f, 0.30f), Euler = new Vector3(8f, -28f, 0f), Fov = 52f },
                new Pose { Name = "ServiceBay", Position = new Vector3(-0.46f, 1.34f, 0.72f), Euler = new Vector3(12f, 62f, 0f), Fov = 40f },
                LookAt("FuseDetail", new Vector3(-0.41f, 1.34f, 0.84f), new Vector3(-0.13f, 1.33f, 1.01f), 25f),
                LookAt("TaskBrief", new Vector3(-0.68f, 1.54f, 0.25f), new Vector3(-0.68f, 1.54f, 1.30f), 42f),
                new Pose { Name = "SparesTray", Position = new Vector3(-1.02f, 1.30f, 0.42f), Euler = new Vector3(34f, -14f, 0f), Fov = 46f },
                new Pose { Name = "InspectControl", Position = new Vector3(1.34f, 1.34f, -0.62f), Euler = new Vector3(26f, 12f, 0f), Fov = 44f },
            }),
            ("Assets/VRMaintenanceResearch/Scenes/VRTraining.unity", "Training", new[]
            {
                LookAt("WorkstationDetail", new Vector3(0.18f, 1.24f, 0.10f), new Vector3(0.18f, 0.98f, 0.95f), 55f),
            }),
        };

        static Pose LookAt(string name, Vector3 position, Vector3 target, float fov) =>
            new Pose { Name = name, Position = position, Target = target, LookAt = true, Fov = fov };

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Capture Comprehension Set")]
        public static void CaptureComprehension()
        {
            Directory.CreateDirectory(OutputFolder);
            var count = 0;
            foreach (var (scenePath, label, poses) in k_ApproachSets)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (var pose in poses)
                {
                    Render(pose, $"Approach_{label}_{pose.Name}");
                    count++;
                }
            }
            count += CaptureTrainingFeedback();
            AssetDatabase.Refresh();
            Debug.Log($"[VisualAudit] wrote {count} approach captures to {OutputFolder}");
        }

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Capture BEFORE")]
        public static void CaptureBefore() => CaptureAll("Before");

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Capture AFTER")]
        public static void CaptureAfter() => CaptureAll("After");

        public static void CaptureAll(string prefix)
        {
            Directory.CreateDirectory(OutputFolder);
            var written = new List<string>();

            foreach (var (scenePath, label, taskPoses) in k_Scenes)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (var pose in taskPoses ? k_TaskPoses : k_RoomPoses)
                    written.Add(Render(pose, $"{prefix}_{label}_{pose.Name}"));

                if (taskPoses)
                    written.Add(CaptureReaderOpen(prefix, label));
            }

            AssetDatabase.Refresh();
            Debug.Log($"[VisualAudit] wrote {written.Count} captures to {OutputFolder}");
        }

        /// <summary>
        /// Renders the participant pose with one information reader open. This is the
        /// evidence that the manuals no longer block the task: the workstation must
        /// stay fully visible while a guide is being read.
        /// </summary>
        static string CaptureReaderOpen(string prefix, string label)
        {
            // Prefer the product manual: it is the text-bearing source, so the shot
            // shows real guide content rather than an image-only panel.
            var sources = Object.FindObjectsByType<InformationSourceController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var source = sources.FirstOrDefault(s => s.Definition != null && s.Definition.sourceType == InformationSourceType.ProductManual)
                         ?? sources.FirstOrDefault();
            GameObject panel = null;
            if (source != null)
            {
                var prop = new SerializedObject(source).FindProperty("contentPanel");
                panel = prop != null ? prop.objectReferenceValue as GameObject : null;
            }

            var wasActive = panel != null && panel.activeSelf;
            if (panel != null)
                panel.SetActive(true);

            var path = Render(
                new Pose { Name = "ReaderOpen", Position = new Vector3(0f, 1.36f, -1.6f), Euler = new Vector3(6f, -12f, 0f), Fov = 68f },
                $"{prefix}_{label}_ReaderOpen");

            if (panel != null)
                panel.SetActive(wasActive);
            return path;
        }

        /// <summary>Renders one pose of the currently open scene.</summary>
        public static string Render(string poseName, Vector3 position, Vector3 euler, float fov, string fileName,
            int width = 1600, int height = 900)
        {
            return Render(new Pose { Name = poseName, Position = position, Euler = euler, Fov = fov }, fileName, width, height);
        }

        static string Render(Pose pose, string fileName, int width = 1600, int height = 900)
        {
            Directory.CreateDirectory(OutputFolder);
            var path = $"{OutputFolder}/{fileName}.png";

            var holder = new GameObject("~AuditCamera");
            var camera = holder.AddComponent<Camera>();
            camera.enabled = false;
            camera.transform.SetPositionAndRotation(
                pose.Position,
                pose.LookAt ? Quaternion.LookRotation(pose.Target - pose.Position, Vector3.up) : Quaternion.Euler(pose.Euler));
            camera.fieldOfView = pose.Fov;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 200f;
            camera.aspect = (float)width / height;

            // URP cameras default to post-processing off, so without this the
            // capture bypasses tonemapping and every lit surface clips to white.
            var urp = camera.GetUniversalAdditionalCameraData();
            urp.renderPostProcessing = true;
            urp.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;

            // Realtime probes are not rendered in edit mode until asked.
            ResearchLightingRig.RenderProbes();

            var texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            var readback = new Texture2D(width, height, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = texture;
                camera.Render();
                RenderTexture.active = texture;
                readback.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                readback.Apply();
                File.WriteAllBytes(path, readback.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                camera.targetTexture = null;
                Object.DestroyImmediate(holder);
                Object.DestroyImmediate(readback);
                texture.Release();
                Object.DestroyImmediate(texture);
            }

            return path;
        }

        /// <summary>Uses the real focus branch for one non-persistent visual proof.</summary>
        static int CaptureTrainingFeedback()
        {
            EditorSceneManager.OpenScene(ResearchSceneSet.Training, OpenSceneMode.Single);
            var target = GameObject.Find("Training Cube A")?.GetComponent<ResearchInteractable>();
            var focus = typeof(ResearchInteractable).GetMethod("Focus", BindingFlags.Instance | BindingFlags.NonPublic);
            var cache = typeof(ResearchInteractable).GetMethod("CacheTintTargets", BindingFlags.Instance | BindingFlags.NonPublic);
            if (target == null || focus == null || cache == null)
                return 0;

            cache.Invoke(target, null);
            focus.Invoke(target, new object[] { 1 });
            try
            {
                Render(
                    LookAt("Feedback", new Vector3(-0.18f, 1.16f, 0.32f), new Vector3(-0.10f, 0.98f, 0.95f), 45f),
                    "Approach_Training_Feedback");
            }
            finally
            {
                focus.Invoke(target, new object[] { -1 });
            }
            return 1;
        }
    }
}
