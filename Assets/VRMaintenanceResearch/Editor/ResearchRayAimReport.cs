using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Asks the one question no existing check asks: if a participant points at the
    /// part they can see, which part does the software decide they pointed at?
    ///
    /// Every other check reaches the interactable by name or by reference. The scene
    /// integrity tests read components, the visual validator reads appearance, and
    /// the play-mode runtime checks call MaintenanceTaskController.RecordInteraction
    /// directly with the object they looked up. None of them casts a ray, so none of
    /// them can see a collider that is the wrong size for the part it belongs to —
    /// and a collider that is the wrong size is exactly what decides which
    /// stableObjectId lands in the event stream.
    ///
    /// This writes a report rather than asserting, because changing a collider
    /// changes which object a hover is logged against, which is a research variable
    /// and not a thing to fix without the supervisor. Promote it into
    /// ResearchSceneIntegrityTests once the collider sizes are settled.
    /// </summary>
    public static class ResearchRayAimReport
    {
        const string k_Path = "Assets/VRMaintenanceResearch/Docs/Verification/Ray_Aim_Attribution.txt";

        // The pose the participant spawns at, and a pose leaning in over the bench.
        // Both are places a participant actually points from, so a part that cannot be
        // aimed at from either cannot be aimed at.
        static readonly (string Name, Vector3 Eye)[] k_Poses =
        {
            ("start pose (0, 1.361, -1.6)", new Vector3(0f, 1.361f, -1.6f)),
            ("bench pose (0, 1.500, 0.30)", new Vector3(0f, 1.5f, 0.3f)),
        };

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Report Ray Aim Attribution", priority = 8)]
        public static void Run()
        {
            var report = new StringBuilder("=== ray aim attribution ===\n");
            report.AppendLine("A ray is cast from the eye pose to the centre of what the part actually");
            report.AppendLine("renders. MISATTRIBUTED means the first interactable the ray meets is a");
            report.AppendLine("different one, so pointing at this part logs the other part's id.\n");

            var misattributed = 0;
            var total = 0;
            foreach (var scenePath in new[] { ResearchSceneSet.Computer, ResearchSceneSet.Fan, ResearchSceneSet.Training })
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                report.AppendLine("=== " + scene.name);

                var parts = Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .OrderBy(part => part.StableObjectId)
                    .ToArray();

                foreach (var (poseName, eye) in k_Poses)
                {
                    report.AppendLine("--- " + poseName);
                    foreach (var part in parts)
                    {
                        if (!TryVisibleCentre(part, out var centre))
                        {
                            report.AppendLine($"{part.StableObjectId}\tSKIPPED nothing rendered");
                            continue;
                        }

                        total++;
                        var hit = Physics.RaycastAll(eye, (centre - eye).normalized, 12f)
                            .OrderBy(candidate => candidate.distance)
                            .Select(candidate => candidate.collider.GetComponentInParent<ResearchInteractable>())
                            .FirstOrDefault(candidate => candidate != null);

                        var hitId = hit == null ? "(nothing)" : hit.StableObjectId;
                        if (hitId == part.StableObjectId)
                        {
                            report.AppendLine($"{part.StableObjectId}\tOK");
                            continue;
                        }

                        misattributed++;
                        report.AppendLine($"{part.StableObjectId}\tMISATTRIBUTED -> {hitId}\t" +
                                          $"(its own collider spans {Extent(part)})");
                    }
                }
            }

            report.AppendLine();
            report.AppendLine(misattributed == 0
                ? $"ALL {total} AIMS RESOLVE TO THE PART AIMED AT"
                : $"{misattributed} of {total} aims resolve to a different part");

            System.IO.File.WriteAllText(k_Path, report.ToString());
            AssetDatabase.ImportAsset(k_Path);
            Debug.Log("[RayAim]\n" + report);
        }

        /// <summary>Centre of what the part draws, which is what a participant aims at.</summary>
        static bool TryVisibleCentre(ResearchInteractable part, out Vector3 centre)
        {
            centre = default;
            var found = false;
            var bounds = new Bounds();
            foreach (var renderer in part.GetComponentsInChildren<Renderer>(false))
            {
                if (!renderer.enabled)
                    continue;
                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (found)
                centre = bounds.center;
            return found;
        }

        static string Extent(ResearchInteractable part)
        {
            var collider = part.GetComponent<Collider>();
            if (collider == null)
                return "no collider";
            var size = collider.bounds.size;
            return $"{size.x * 1000f:0} x {size.y * 1000f:0} x {size.z * 1000f:0} mm";
        }
    }
}
