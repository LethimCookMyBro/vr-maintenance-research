using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Asks the one question no other check asks: if a participant points at the part
    /// they can see, which part does the software decide they pointed at?
    ///
    /// Every other check reaches the interactable by name or by reference. The scene
    /// integrity tests read components, the visual validator reads appearance, and the
    /// play-mode runtime checks call MaintenanceTaskController.RecordInteraction
    /// directly with the object they looked up. None of them casts a ray, so none of
    /// them can see a collider that is the wrong size for the part it belongs to — and
    /// a collider that is the wrong size is exactly what decides which stableObjectId
    /// lands in the event stream.
    ///
    /// This file is the human-readable report. The gate that fails a build lives in
    /// ResearchSceneIntegrityTests.EveryInteractableAnswersTheRayAimedAtIt, and both
    /// run through the shared helpers below so the report and the test can never
    /// disagree about what a ray hits.
    /// </summary>
    public static class ResearchRayAimReport
    {
        const string k_Path = "Assets/VRMaintenanceResearch/Docs/Verification/Ray_Aim_Attribution.txt";

        /// <summary>
        /// The pose the participant spawns at, and a pose leaning in over the bench.
        /// Both are places a participant actually points from, so a part that cannot be
        /// aimed at from either cannot be aimed at.
        /// </summary>
        public static readonly (string Name, Vector3 Eye)[] Poses =
        {
            ("start pose (0, 1.361, -1.6)", new Vector3(0f, 1.361f, -1.6f)),
            ("bench pose (0, 1.500, 0.30)", new Vector3(0f, 1.5f, 0.3f)),
        };

        public static readonly string[] Scenes = { ResearchSceneSet.Computer, ResearchSceneSet.Fan, ResearchSceneSet.Training };

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Report Ray Aim Attribution", priority = 8)]
        public static void Run()
        {
            var report = new StringBuilder("=== ray aim attribution ===\n");
            report.AppendLine("A ray is cast from the eye pose to the centre of what the part actually");
            report.AppendLine("renders, and resolved the way XRRayInteractor resolves one: the nearest");
            report.AppendLine("collider wins, and a collider belonging to no interactable blocks");
            report.AppendLine("everything behind it. MISATTRIBUTED means pointing at this part logs a");
            report.AppendLine("different part's id.\n");

            var misattributed = 0;
            var total = 0;
            foreach (var scenePath in Scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                report.AppendLine("=== " + scene.name);

                using var aim = new AimProbe();
                foreach (var (poseName, eye) in Poses)
                {
                    report.AppendLine("--- " + poseName);
                    foreach (var part in aim.Parts)
                    {
                        if (!TryVisibleCentre(part, out var centre))
                        {
                            report.AppendLine($"{part.StableObjectId}\tSKIPPED nothing rendered");
                            continue;
                        }

                        total++;
                        var hit = aim.Resolve(eye, centre, out var blocker);
                        if (hit == part)
                        {
                            report.AppendLine($"{part.StableObjectId}\tOK");
                            continue;
                        }

                        misattributed++;
                        report.AppendLine($"{part.StableObjectId}\tMISATTRIBUTED -> " +
                                          $"{(hit == null ? "(nothing)" : hit.StableObjectId)}\t{blocker}");
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

        /// <summary>
        /// Centre of what the part draws, which is what a participant aims at.
        ///
        /// Renderers belonging to a nested interactable are not this part's. Both
        /// builders reparent the in-machine parts under their device, so counting every
        /// renderer in the subtree would aim at the case "including its motherboard" and
        /// at the fan "including its blade".
        /// </summary>
        public static bool TryVisibleCentre(ResearchInteractable part, out Vector3 centre)
        {
            centre = default;
            var found = false;
            var bounds = new Bounds();
            foreach (var renderer in part.GetComponentsInChildren<Renderer>(false))
            {
                if (!renderer.enabled)
                    continue;
                if (renderer.GetComponentInParent<ResearchInteractable>() != part)
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

        /// <summary>
        /// One scene's worth of ray resolution, answering the way the running game does.
        ///
        /// XRInteractionManager only knows which interactable owns a collider once the
        /// interactable has registered, and registration happens at OnEnable — which
        /// never runs in the editor. So the probe registers the scene's interactables
        /// with a real XRInteractionManager, asks it the same
        /// TryGetInteractableForCollider question XRRayInteractor asks, and unregisters
        /// on dispose. Nothing is saved: the manager it may have created is destroyed
        /// with it.
        /// </summary>
        public sealed class AimProbe : System.IDisposable
        {
            readonly XRInteractionManager m_Manager;
            readonly bool m_OwnsManager;
            readonly List<IXRInteractable> m_Registered = new();

            public ResearchInteractable[] Parts { get; }

            public AimProbe()
            {
                Parts = Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .OrderBy(part => part.StableObjectId)
                    .ToArray();

                m_Manager = Object.FindFirstObjectByType<XRInteractionManager>();
                if (m_Manager == null)
                {
                    m_Manager = new GameObject("ray aim probe manager").AddComponent<XRInteractionManager>();
                    m_OwnsManager = true;
                }

                foreach (var xr in Object.FindObjectsByType<XRBaseInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                {
                    // The list BindOwnColliders wrote. Awake has not run in the editor,
                    // so the runtime colliders list is still empty and registering would
                    // map nothing; filling it here is what Awake would have done.
                    if (xr.colliders.Count == 0)
                        xr.colliders.AddRange(xr.GetComponents<Collider>());
                    m_Manager.RegisterInteractable((IXRInteractable)xr);
                    m_Registered.Add(xr);
                }

                // Colliders were resized by a builder in this same editor session, and
                // the physics scene is only rebuilt on demand outside play mode.
                Physics.SyncTransforms();
            }

            /// <summary>
            /// What a ray from <paramref name="eye"/> through <paramref name="centre"/>
            /// selects, and — when that is not the part aimed at — what stopped it.
            ///
            /// The nearest collider decides. XRRayInteractor.GetValidTargets walks its
            /// hits in distance order and breaks on the first one that belongs to no
            /// interactable, so geometry in front of a part blocks it just as surely as
            /// another part's oversized collider does.
            /// </summary>
            public ResearchInteractable Resolve(Vector3 eye, Vector3 centre, out string blocker)
            {
                blocker = "nothing on the ray";
                var direction = centre - eye;
                // The interactor's own ray length, not the distance to the aim point:
                // XRRayInteractor casts maxRaycastDistance and takes the nearest hit, so
                // truncating at the target would report "nothing" for a part whose
                // collider starts a centimetre further on.
                var hits = Physics.RaycastAll(eye, direction.normalized, 12f)
                    .OrderBy(hit => hit.distance)
                    .ToArray();
                if (hits.Length == 0)
                    return null;

                var first = hits[0];
                if (!m_Manager.TryGetInteractableForCollider(first.collider, out var interactable))
                {
                    blocker = $"blocked at {first.distance * 1000f:0} mm by '{first.collider.name}', which no interactable owns";
                    return null;
                }

                var found = (interactable as Component)?.GetComponent<ResearchInteractable>();
                blocker = $"blocked at {first.distance * 1000f:0} mm by '{first.collider.name}'";
                return found;
            }

            public void Dispose()
            {
                foreach (var registered in m_Registered)
                    m_Manager.UnregisterInteractable(registered);
                if (m_OwnsManager && m_Manager != null)
                    Object.DestroyImmediate(m_Manager.gameObject);
            }
        }
    }
}
