using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Renders the participant viewpoint of a research scene to a PNG so that
    /// before/after documentation screenshots are reproducible from the same pose.
    /// Editor-only documentation utility; it does not affect runtime behaviour.
    /// </summary>
    public static class ResearchSceneCapture
    {
        public const string OutputFolder = "Assets/VRMaintenanceResearch/Docs/Screenshots";

        [MenuItem("Tools/VR Maintenance Research/Capture Participant Views")]
        public static void CaptureAll()
        {
            CaptureScene("Assets/VRMaintenanceResearch/Scenes/ResearcherSetup.unity", "ResearcherSetup");
            CaptureScene("Assets/VRMaintenanceResearch/Scenes/VRTraining.unity", "VRTraining");
            CaptureScene("Assets/VRMaintenanceResearch/Scenes/ComputerRepairTask.unity", "Computer");
            CaptureScene("Assets/VRMaintenanceResearch/Scenes/FanRepairTask.unity", "Fan");
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Captures whatever is running now, without opening a scene.
        ///
        /// The runtime-built interfaces — the status board, the training board and the
        /// participant heads-up display — exist only in play mode, so an edit-mode
        /// capture cannot show them. This is how the _Play screenshots are taken.
        /// </summary>
        [MenuItem("Tools/VR Maintenance Research/Capture Play Mode View")]
        public static void CapturePlayModeView()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log("[SceneCapture] " + CaptureActiveScene(scene + "_Play", "PolishAfter"));
        }

        public static string CaptureScene(string scenePath, string label, string prefix = "After", int width = 1600, int height = 900)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            return CaptureActiveScene(label, prefix, width, height);
        }

        /// <summary>Renders the first enabled camera of the open scene from the participant pose.</summary>
        public static string CaptureActiveScene(string label, string prefix = "After", int width = 1600, int height = 900)
        {
            var source = FindParticipantCamera();
            if (source == null)
                return "no camera in " + label;

            Directory.CreateDirectory(OutputFolder);
            var path = OutputFolder + "/" + prefix + "_" + label + ".png";

            var holder = new GameObject("~CaptureCamera");
            var camera = holder.AddComponent<Camera>();
            camera.CopyFrom(source);
            camera.enabled = false;
            camera.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            camera.aspect = (float)width / height;

            var texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 2 };
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

        static Camera FindParticipantCamera()
        {
            var main = Camera.main;
            if (main != null)
                return main;

            foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (camera.name.Contains("Main"))
                    return camera;
            }

            return null;
        }
    }
}
