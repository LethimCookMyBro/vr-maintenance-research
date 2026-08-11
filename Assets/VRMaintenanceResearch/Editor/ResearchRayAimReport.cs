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
    ///
    /// == what changed, and why the first criterion was wrong ==
    ///
    /// The first version fired one ray per part per pose, at the centre of the part's
    /// renderer bounds, and called anything else a defect. That criterion was never a
    /// statement about the scene; it was a statement about solid convex parts seen
    /// from head height, and this bench holds neither.
    ///
    /// * A hollow part has no geometry at its centre. computer.case is an open shell,
    ///   so the centre of its bounds is the air inside it and the ray leaves through
    ///   the missing side panel and lands on the motherboard — which is the correct
    ///   answer to the question that was asked, and the wrong question. A participant
    ///   aims at the skin of the case, never at its middle.
    /// * A part on the lower shelf is behind the bench top from a standing pose. The
    ///   criterion only stood the participant up, so computer.side-panel and
    ///   fan.front-cover could not be aimed at by construction — a participant looking
    ///   for a part on the bottom shelf bends down, and no pose in the set did.
    ///
    /// So the criterion now spreads aim points over the part's own surface, on the
    /// faces that turn toward the eye, and stands the participant in the places they
    /// actually stand, including crouched at the bench. Two tiers come out of it:
    ///
    /// * FAIL — not one aim point from any pose resolves to the part. The part cannot
    ///   be selected at all, and no amount of participant skill fixes that.
    /// * WARN — the part answers, but no pose gets a majority of its surface. It is
    ///   reachable and fiddly. Reported as a proportion, and does not fail a build:
    ///   a part half-behind another part is a real bench, not a broken scene.
    ///
    /// The bar is higher than the old one, not lower. The old criterion asked each
    /// part one question from two places; this asks it 27 from five, and a part that
    /// answers none of the 135 is genuinely unreachable.
    /// </summary>
    public static class ResearchRayAimReport
    {
        const string k_Path = "Assets/VRMaintenanceResearch/Docs/Verification/Ray_Aim_Attribution.txt";

        /// <summary>
        /// How far across each half-extent the outer ring of aim points sits. At 1.0
        /// they would sit on the silhouette edge, where a ray grazes the collider and
        /// the answer is decided by float noise rather than by the scene.
        /// </summary>
        const float k_Inset = 0.6f;

        /// <summary>
        /// The share of a pose's aim points that must resolve to the part before that
        /// pose counts as a place the part can comfortably be pointed at. Below this
        /// the part is reachable but fiddly, which is a WARN, not a failure.
        /// </summary>
        const float k_Reliable = 0.5f;

        /// <summary>
        /// Poses a participant actually holds. The first two are the ones the original
        /// single-centre criterion used and are kept unchanged so the two criteria can
        /// be compared on the same ground; the other three are the postures that
        /// criterion left out.
        ///
        /// The crouch is what makes the lower shelf reachable. The bench top's collider
        /// undersides sit at y = 0.84 and its front edge at z = 0.45, so a sight line
        /// from 1.10 m at z = -0.60 passes below the slab and reaches the shelf at
        /// y = 0.28. Standing, it cannot: that is a fact about benches, not about this
        /// scene.
        ///
        /// The two side poses are a step to either end of the 3.6 m bench, which is
        /// how anyone gets a look past a part standing in front of another part.
        /// </summary>
        public static readonly (string Name, Vector3 Eye)[] Poses =
        {
            ("start pose (0, 1.361, -1.6)", new Vector3(0f, 1.361f, -1.6f)),
            ("bench pose (0, 1.500, 0.30)", new Vector3(0f, 1.5f, 0.3f)),
            ("crouched at the bench (0, 1.100, -0.60)", new Vector3(0f, 1.1f, -0.6f)),
            ("step left (-0.90, 1.400, -0.30)", new Vector3(-0.9f, 1.4f, -0.3f)),
            ("step right (0.90, 1.400, -0.30)", new Vector3(0.9f, 1.4f, -0.3f)),
        };

        /// <summary>The two poses the superseded criterion used, for the comparison section.</summary>
        static IEnumerable<(string Name, Vector3 Eye)> LegacyPoses => Poses.Take(2);

        public static readonly string[] Scenes = { ResearchSceneSet.Computer, ResearchSceneSet.Fan, ResearchSceneSet.Training };

        /// <summary>
        /// The criterion in one line, recorded into the baseline file and checked against
        /// it on every run.
        ///
        /// Aim counts only mean something relative to the criterion that produced them.
        /// Add a sixth pose and every part's count jumps; widen the inset and the corner
        /// samples slide off the part and every count falls. Either would look exactly
        /// like a scene regression to a comparison against yesterday's numbers, so the
        /// baseline carries this string and
        /// <see cref="ResearchRayAimBaseline"/> refuses to compare across a change to it.
        /// </summary>
        public static string Criterion =>
            $"poses={Poses.Length};inset={k_Inset:0.00};reliable={k_Reliable:0.00};grid=3x3;faces=3";

        /// <summary>
        /// Every part in every task scene, evaluated once. The report, the baseline
        /// recorder and the scene integrity gate all walk the scenes through here, so
        /// none of the three can be measuring something the others are not.
        ///
        /// The verdicts outlive the scenes they came from. Only their value fields are
        /// safe to read afterwards — <see cref="AimVerdict.Part"/> is destroyed with its
        /// scene, and nothing downstream of this method touches it.
        /// </summary>
        public static List<(string Scene, AimVerdict Verdict)> EvaluateAllScenes()
        {
            var all = new List<(string, AimVerdict)>();
            foreach (var scenePath in Scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                using var probe = new AimProbe();
                foreach (var verdict in Evaluate(probe))
                    all.Add((scene.name, verdict));
            }

            return all;
        }

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Report Ray Aim Attribution", priority = 8)]
        public static void Run()
        {
            var report = new StringBuilder("=== ray aim attribution ===\n");
            report.AppendLine("For every interactable, aim points are spread over the faces of the part");
            report.AppendLine("that turn toward the eye and pulled onto the part's own collider, then a");
            report.AppendLine("ray is cast at each of them and resolved the way XRRayInteractor resolves");
            report.AppendLine("one: the nearest collider wins, and a collider belonging to no interactable");
            report.AppendLine("blocks everything behind it.");
            report.AppendLine();
            report.AppendLine($"  FAIL  not one of the aims resolves to the part, from any of the {Poses.Length} poses.");
            report.AppendLine($"  WARN  it answers, but no single pose reaches {k_Reliable * 100f:0}% of its surface.");
            report.AppendLine("  OK    at least one pose points at it and gets it back.");
            report.AppendLine();
            report.AppendLine("Poses:");
            foreach (var (poseName, _) in Poses)
                report.AppendLine("  " + poseName);
            report.AppendLine();

            var baseline = ResearchRayAimBaseline.Load();
            report.AppendLine(baseline.Exists
                ? $"Compared against the committed baseline recorded {baseline.Recorded}."
                : "NO BASELINE FILE — every part below is unmeasured against anything.");
            if (baseline.Exists && !ResearchRayAimBaseline.CriterionMatches(baseline))
                report.AppendLine("WARNING: " + ResearchRayAimBaseline.CriterionComplaint(baseline));
            report.AppendLine();

            var failed = new List<string>();
            var warned = new List<string>();
            var regressed = new List<string>();
            var hits = 0;
            var aims = 0;
            var legacy = new StringBuilder();
            var evaluated = new List<(string Scene, AimVerdict Verdict)>();

            foreach (var scenePath in Scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                report.AppendLine("=== " + scene.name);
                legacy.AppendLine("=== " + scene.name);

                using var aim = new AimProbe();
                foreach (var verdict in Evaluate(aim))
                {
                    hits += verdict.Hits;
                    aims += verdict.Aims;
                    evaluated.Add((scene.name, verdict));

                    var against = ResearchRayAimBaseline.Compare(baseline, scene.name, verdict);
                    report.AppendLine($"{verdict.Id}\t{verdict.Tier}\t{verdict.Hits} of {verdict.Aims} aims\t" +
                                      $"{verdict.PosesAnswering} of {Poses.Length} poses\tbest pose {verdict.BestReach * 100f:0}%\t" +
                                      against.What +
                                      (verdict.Obstruction.Length == 0 ? "" : "\t" + verdict.Obstruction));

                    if (against.IsRegression)
                        regressed.Add($"{scene.name} '{verdict.Id}': {verdict.Hits} of {verdict.Aims} aims — {against.What}");
                    if (verdict.Unreachable)
                        failed.Add($"{scene.name} '{verdict.Id}': {verdict.Obstruction}");
                    else if (verdict.HardToAim)
                        warned.Add($"{scene.name} '{verdict.Id}': {verdict.Hits} of {verdict.Aims} aims, " +
                                   $"{verdict.PosesAnswering} of {Poses.Length} poses, best pose {verdict.BestReach * 100f:0}% — {verdict.Obstruction}");
                }

                AppendLegacy(legacy, aim);
            }

            regressed.AddRange(ResearchRayAimBaseline.Missing(baseline, evaluated));

            report.AppendLine();
            report.AppendLine($"=== summary: {hits} of {aims} aims resolve to the part aimed at");
            report.AppendLine(failed.Count == 0
                ? "NO PART IS UNREACHABLE"
                : $"{failed.Count} PART(S) UNREACHABLE:");
            foreach (var line in failed)
                report.AppendLine("  FAIL " + line);
            report.AppendLine(warned.Count == 0
                ? "no part is hard to aim at"
                : $"{warned.Count} part(s) reachable but hard to aim at:");
            foreach (var line in warned)
                report.AppendLine("  WARN " + line);

            report.AppendLine(regressed.Count == 0
                ? "no part has fallen away from its baseline"
                : $"{regressed.Count} PART(S) WORSE THAN BASELINE:");
            foreach (var line in regressed)
                report.AppendLine("  REGRESSION " + line);

            report.AppendLine();
            report.AppendLine("=== superseded criterion, kept so the two can be compared ===");
            report.AppendLine("One ray per part per pose, at the centre of the part's renderer bounds,");
            report.AppendLine("from the two standing poses only. This is what the file recorded before");
            report.AppendLine("the criterion changed; the counts below are recomputed on the current");
            report.AppendLine("scenes, so it stays a live comparison rather than a stale copy.");
            report.AppendLine();
            report.Append(legacy);

            System.IO.File.WriteAllText(k_Path, report.ToString());
            AssetDatabase.ImportAsset(k_Path);
            Debug.Log("[RayAim]\n" + report);
        }

        /// <summary>The old single-centre criterion, run alongside so the report can show what changed.</summary>
        static void AppendLegacy(StringBuilder legacy, AimProbe aim)
        {
            var misattributed = 0;
            var total = 0;
            foreach (var (poseName, eye) in LegacyPoses)
            {
                legacy.AppendLine("--- " + poseName);
                foreach (var part in aim.Parts)
                {
                    if (!TryVisibleCentre(part, out var centre))
                    {
                        legacy.AppendLine($"{part.StableObjectId}\tSKIPPED nothing rendered");
                        continue;
                    }

                    total++;
                    var hit = aim.Resolve(eye, centre, out var blocker, out _);
                    if (hit == part)
                    {
                        legacy.AppendLine($"{part.StableObjectId}\tOK");
                        continue;
                    }

                    misattributed++;
                    legacy.AppendLine($"{part.StableObjectId}\tMISATTRIBUTED -> " +
                                      $"{(hit == null ? "(nothing)" : hit.StableObjectId)}\t{blocker}");
                }
            }

            legacy.AppendLine($"    {misattributed} of {total} centre aims resolve to a different part");
        }

        /// <summary>
        /// What the two tiers are decided from, for every part in the open scene.
        /// The report prints it and the scene integrity test asserts on it, so the two
        /// cannot drift apart.
        /// </summary>
        public static List<AimVerdict> Evaluate(AimProbe probe)
        {
            var verdicts = new List<AimVerdict>();
            foreach (var part in probe.Parts)
            {
                var verdict = new AimVerdict(part);
                var blockers = new Dictionary<string, (int Count, float Nearest)>();

                foreach (var (_, eye) in Poses)
                {
                    var points = AimPoints(part, eye);
                    if (points.Length == 0)
                        continue;

                    var hits = 0;
                    foreach (var point in points)
                    {
                        if (probe.Resolve(eye, point, out var blocker, out var distance) == part)
                        {
                            hits++;
                            continue;
                        }

                        var seen = blockers.TryGetValue(blocker, out var previous) ? previous : (Count: 0, Nearest: float.MaxValue);
                        blockers[blocker] = (seen.Count + 1, distance < 0f ? seen.Nearest : Mathf.Min(seen.Nearest, distance));
                    }

                    verdict.Add(hits, points.Length);
                }

                // Nothing rendered: there is no surface to aim at and nothing to judge.
                if (verdict.Aims == 0)
                    continue;

                verdict.Obstruction = Describe(blockers, verdict.Aims - verdict.Hits);
                verdicts.Add(verdict);
            }

            return verdicts;
        }

        static string Describe(Dictionary<string, (int Count, float Nearest)> blockers, int missed)
        {
            if (missed == 0 || blockers.Count == 0)
                return "";

            var worst = blockers.OrderByDescending(entry => entry.Value.Count).First();
            var where = worst.Value.Nearest < float.MaxValue ? $", nearest {worst.Value.Nearest * 1000f:0} mm" : "";
            return $"{worst.Value.Count} of {missed} missed aims stop at {worst.Key}{where}";
        }

        /// <summary>
        /// Where a participant would point at this part: a three-by-three grid on each
        /// face of what the part draws that turns toward the eye, pulled onto the
        /// part's own collider with <see cref="Collider.ClosestPoint"/>.
        ///
        /// The pull is what makes this work on the two shapes the centre aim could not
        /// handle. On a hollow part it moves the sample off the air inside the shell
        /// and onto the skin; on a part whose collider is smaller than its bounds — a
        /// cylinder inscribed in a box, a plug on the end of a coiled lead — it moves
        /// the corner samples off the empty corners and onto the part.
        ///
        /// Faces on the far side are not sampled. A participant cannot point at the
        /// back of something, and counting the back as a missed aim would report every
        /// part in the scene as half unreachable.
        /// </summary>
        public static Vector3[] AimPoints(ResearchInteractable part, Vector3 eye)
        {
            if (!TryVisibleBounds(part, out var bounds))
                return System.Array.Empty<Vector3>();

            var colliders = part.GetComponents<Collider>();
            if (colliders.Length == 0)
                return System.Array.Empty<Vector3>();

            var points = new List<Vector3>();
            var extents = bounds.extents;
            for (var axis = 0; axis < 3; axis++)
            {
                var toward = eye[axis] >= bounds.center[axis] ? 1f : -1f;
                var u = (axis + 1) % 3;
                var v = (axis + 2) % 3;

                for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                {
                    var point = bounds.center;
                    point[axis] += toward * extents[axis];
                    point[u] += i * extents[u] * k_Inset;
                    point[v] += j * extents[v] * k_Inset;
                    points.Add(Nearest(colliders, point));
                }
            }

            return points.ToArray();
        }

        static Vector3 Nearest(Collider[] colliders, Vector3 point)
        {
            var best = colliders[0].ClosestPoint(point);
            for (var i = 1; i < colliders.Length; i++)
            {
                var candidate = colliders[i].ClosestPoint(point);
                if ((candidate - point).sqrMagnitude < (best - point).sqrMagnitude)
                    best = candidate;
            }

            return best;
        }

        /// <summary>
        /// What the part draws, which is what a participant aims at.
        ///
        /// Renderers belonging to a nested interactable are not this part's. Both
        /// builders reparent the in-machine parts under their device, so counting every
        /// renderer in the subtree would aim at the case "including its motherboard" and
        /// at the fan "including its blade".
        /// </summary>
        public static bool TryVisibleBounds(ResearchInteractable part, out Bounds bounds)
        {
            bounds = default;
            var found = false;
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

            return found;
        }

        /// <summary>The superseded criterion's aim point, kept for the comparison section.</summary>
        public static bool TryVisibleCentre(ResearchInteractable part, out Vector3 centre)
        {
            var found = TryVisibleBounds(part, out var bounds);
            centre = found ? bounds.center : default;
            return found;
        }

        /// <summary>What one part's aims came to, across every pose.</summary>
        public sealed class AimVerdict
        {
            public AimVerdict(ResearchInteractable part)
            {
                Part = part;
                Id = part.StableObjectId;
            }

            public ResearchInteractable Part { get; }
            public string Id { get; }
            public int Hits { get; private set; }
            public int Aims { get; private set; }
            public int PosesAnswering { get; private set; }
            public float BestReach { get; private set; }
            public string Obstruction { get; set; } = "";

            public void Add(int hits, int aims)
            {
                Hits += hits;
                Aims += aims;
                if (hits > 0)
                    PosesAnswering++;
                BestReach = Mathf.Max(BestReach, hits / (float)aims);
            }

            /// <summary>Nothing selects it, from anywhere. The event stream can never carry its id.</summary>
            public bool Unreachable => Hits == 0;

            /// <summary>It answers, but only from a sliver of its surface.</summary>
            public bool HardToAim => Hits > 0 && BestReach < k_Reliable;

            public string Tier => Unreachable ? "FAIL" : HardToAim ? "WARN" : "OK";
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
            /// What a ray from <paramref name="eye"/> through <paramref name="target"/>
            /// selects, and — when that is not the part aimed at — what stopped it and
            /// how far away that was. <paramref name="distance"/> is negative when the
            /// ray hits nothing at all.
            ///
            /// The nearest collider decides. XRRayInteractor.GetValidTargets walks its
            /// hits in distance order and breaks on the first one that belongs to no
            /// interactable, so geometry in front of a part blocks it just as surely as
            /// another part's oversized collider does.
            /// </summary>
            public ResearchInteractable Resolve(Vector3 eye, Vector3 target, out string blocker, out float distance)
            {
                blocker = "nothing on the ray";
                distance = -1f;

                var direction = target - eye;
                // The interactor's own ray length, not the distance to the aim point:
                // XRRayInteractor casts maxRaycastDistance and takes the nearest hit, so
                // truncating at the target would report "nothing" for a part whose
                // collider starts a centimetre further on.
                if (!Physics.Raycast(eye, direction.normalized, out var first, 12f))
                    return null;

                distance = first.distance;
                if (!m_Manager.TryGetInteractableForCollider(first.collider, out var interactable))
                {
                    blocker = $"'{first.collider.name}', which no interactable owns";
                    return null;
                }

                var found = (interactable as Component)?.GetComponent<ResearchInteractable>();
                blocker = $"'{first.collider.name}' ({(found == null ? "no stable id" : found.StableObjectId)})";
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
