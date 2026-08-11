using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// The committed record of how well every part could be pointed at, and the gate that
    /// fails when a part gets worse.
    ///
    /// == the hole this closes ==
    ///
    /// <see cref="ResearchRayAimReport"/> answers "which part does a ray select". Its
    /// gate failed on one thing only: a part that answers *none* of its 135 aims. That
    /// catches a part becoming unselectable and nothing else, and "unselectable" is the
    /// far end of a long slope.
    ///
    /// The slope was walked in this project and measured. Putting the fan's motor module
    /// in the wrong place took `fan.power-plug` from 94 of 135 aims to 49, and
    /// `fan.power-cord` from 49 to 20 — the plug lost 48 per cent of the surface a
    /// participant could point at, and the cord 59 per cent. Both still answered some
    /// aims, so both stayed out of the failure list, the WARN tier printed the new
    /// numbers with nothing to compare them against, and Scene Integrity stayed green.
    /// It was caught because a person read the numbers, which is not a check.
    ///
    /// So the numbers are committed, and the gate compares against them:
    ///
    /// * FAIL — a part answers no aim from any pose. Unchanged; it can never appear in
    ///   the event stream.
    /// * FAIL — a part answers fewer than <see cref="k_KeepFraction"/> of the aims it
    ///   answered when the baseline was recorded. A quarter of a part's pointable
    ///   surface is far more than bench noise: rebuilding both benches and moving the
    ///   racking, the guardrails and the matting moved no part by more than a few per
    ///   cent.
    /// * FAIL — the set of parts changed. A baselined part that is gone, or a part with
    ///   no baseline row, both mean the file no longer describes the benches.
    ///
    /// == why it never updates itself ==
    ///
    /// A gate that re-records its own baseline passes forever: each regression becomes
    /// the new normal and the next one is measured from it, so a part can walk from 135
    /// to 0 in twenty green runs. Nothing writes
    /// <see cref="k_Path"/> except <see cref="Record"/>, and nothing calls
    /// <see cref="Record"/> except the human who picks its menu item. The report and the
    /// gate only ever read.
    ///
    /// When a part legitimately changes — a bench is rebuilt, a part is re-modelled —
    /// the run fails, a person reads what moved and by how much, and *then* re-records.
    /// The failure is the point at which someone decides, and it is not skippable.
    /// </summary>
    public static class ResearchRayAimBaseline
    {
        const string k_Path = "Assets/VRMaintenanceResearch/Docs/Verification/Ray_Aim_Baseline.tsv";

        /// <summary>
        /// The share of its baseline aims a part must still answer. At 0.75 a part may
        /// lose a quarter of its pointable surface to ordinary bench rearrangement
        /// before the gate calls it a regression; the two drops that prompted this were
        /// 48 and 59 per cent.
        /// </summary>
        const float k_KeepFraction = 0.75f;

        public readonly struct Row
        {
            public Row(int hits, int aims) { Hits = hits; Aims = aims; }
            public int Hits { get; }
            public int Aims { get; }
        }

        public sealed class Table
        {
            public bool Exists;
            public string Criterion = "";
            public string Recorded = "";
            public readonly Dictionary<string, Row> Rows = new();
        }

        static string Key(string scene, string id) => scene + "\t" + id;

        // ---------------------------------------------------------------- reading

        public static Table Load()
        {
            var table = new Table();
            if (!File.Exists(k_Path))
                return table;

            table.Exists = true;
            foreach (var line in File.ReadAllLines(k_Path))
            {
                if (line.StartsWith("# criterion:"))
                    table.Criterion = line.Substring("# criterion:".Length).Trim();
                else if (line.StartsWith("# recorded:"))
                    table.Recorded = line.Substring("# recorded:".Length).Trim();

                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("scene\t"))
                    continue;

                var parts = line.Split('\t');
                if (parts.Length < 4)
                    continue;
                if (!int.TryParse(parts[2], out var hits) || !int.TryParse(parts[3], out var aims))
                    continue;

                table.Rows[Key(parts[0], parts[1])] = new Row(hits, aims);
            }

            return table;
        }

        /// <summary>
        /// How one part now compares with its baseline row: the text the report prints
        /// and, when <see cref="IsRegression"/>, the text the gate fails with.
        /// </summary>
        public readonly struct Verdict
        {
            public Verdict(string what, bool regression) { What = what; IsRegression = regression; }
            public string What { get; }
            public bool IsRegression { get; }
        }

        public static Verdict Compare(Table baseline, string scene, ResearchRayAimReport.AimVerdict verdict)
        {
            if (!baseline.Exists)
                return new Verdict("no baseline file", false);

            if (!baseline.Rows.TryGetValue(Key(scene, verdict.Id), out var row))
                return new Verdict("NOT IN BASELINE — the benches carry a part the baseline does not", true);

            var delta = verdict.Hits - row.Hits;
            var percent = row.Hits == 0 ? 0f : delta * 100f / row.Hits;
            var moved = delta == 0 ? "same as baseline" : $"baseline {row.Hits}, {(delta > 0 ? "+" : "")}{delta} ({percent:+0.#;-0.#;0}%)";

            // Integer comparison so the threshold cannot drift with float rounding:
            // fail when hits/baseline < 0.75, i.e. hits*100 < baseline*75.
            if (verdict.Hits * 100 < row.Hits * (int)(k_KeepFraction * 100))
                return new Verdict($"DROPPED — {moved}, past the {(1f - k_KeepFraction) * 100f:0}% allowance", true);

            return new Verdict(moved, false);
        }

        /// <summary>
        /// Baselined parts that no scene produced a verdict for. A part that has left the
        /// benches is the strongest regression there is, and the loop over live parts
        /// cannot see it.
        /// </summary>
        public static List<string> Missing(Table baseline, IEnumerable<(string Scene, ResearchRayAimReport.AimVerdict Verdict)> seen)
        {
            if (!baseline.Exists)
                return new List<string>();

            var live = new HashSet<string>(seen.Select(entry => Key(entry.Scene, entry.Verdict.Id)));
            return baseline.Rows.Keys
                .Where(key => !live.Contains(key))
                .Select(key => key.Replace("\t", ": '") + "' is in the baseline but no longer answers any aim in its scene")
                .OrderBy(line => line)
                .ToList();
        }

        /// <summary>
        /// Whether the baseline was recorded under the criterion now in force. Counts
        /// taken under a different criterion are not comparable, and silently comparing
        /// them would report the criterion change as a scene regression.
        /// </summary>
        public static bool CriterionMatches(Table baseline) =>
            !baseline.Exists || baseline.Criterion == ResearchRayAimReport.Criterion;

        public static string CriterionComplaint(Table baseline) =>
            $"the baseline was recorded under a different aim criterion, so its counts are not comparable.\n" +
            $"    baseline: {baseline.Criterion}\n" +
            $"    now:      {ResearchRayAimReport.Criterion}\n" +
            $"    Re-record deliberately: Tools > VR Maintenance Research > Visual Audit > {k_MenuLeaf}";

        public const string k_MenuLeaf = "UPDATE Ray Aim Baseline (overwrites the committed file)";

        // ---------------------------------------------------------------- writing

        /// <summary>
        /// The only thing in the project that writes the baseline, and it runs only when
        /// a person picks it from the menu.
        ///
        /// It prints every row it changed before it writes, so an accidental run is
        /// visible in the console as well as in the commit diff.
        /// </summary>
        [MenuItem("Tools/VR Maintenance Research/Visual Audit/" + k_MenuLeaf, priority = 11)]
        public static void Record()
        {
            var previous = Load();
            var evaluated = ResearchRayAimReport.EvaluateAllScenes();

            var file = new StringBuilder();
            file.AppendLine("# Ray aim baseline — THIS FILE IS AN INPUT TO A TEST, NOT A REPORT.");
            file.AppendLine("#");
            file.AppendLine("# How many of its aim points each part answers, recorded on purpose. The scene");
            file.AppendLine("# integrity gate fails a part that answers no aim at all, or that answers fewer");
            file.AppendLine("# than 75% of the aims it answers here, or that is not listed here at all.");
            file.AppendLine("#");
            file.AppendLine("# Do not regenerate this to make a red run go green. A drop is either a scene");
            file.AppendLine("# regression to fix, or a deliberate change to the benches whose new numbers a");
            file.AppendLine("# person has looked at and accepted. Rewrite it only from the menu item named");
            file.AppendLine($"# in the line below, and say why in the commit message.");
            file.AppendLine("#");
            file.AppendLine($"# rewritten by: Tools > VR Maintenance Research > Visual Audit > {k_MenuLeaf}");
            file.AppendLine($"# criterion: {ResearchRayAimReport.Criterion}");
            file.AppendLine($"# recorded: {System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}");
            file.AppendLine("scene\tstable_object_id\thits\taims");

            var changes = new List<string>();
            foreach (var (scene, verdict) in evaluated.OrderBy(e => e.Scene).ThenBy(e => e.Verdict.Id))
            {
                file.AppendLine($"{scene}\t{verdict.Id}\t{verdict.Hits}\t{verdict.Aims}");

                if (!previous.Rows.TryGetValue(Key(scene, verdict.Id), out var was))
                    changes.Add($"  NEW    {scene}: '{verdict.Id}' {verdict.Hits} of {verdict.Aims}");
                else if (was.Hits != verdict.Hits)
                    changes.Add($"  MOVED  {scene}: '{verdict.Id}' {was.Hits} -> {verdict.Hits} of {verdict.Aims}");
            }

            foreach (var gone in Missing(previous, evaluated))
                changes.Add("  GONE   " + gone);

            Directory.CreateDirectory(Path.GetDirectoryName(k_Path)!);
            File.WriteAllText(k_Path, file.ToString());
            AssetDatabase.ImportAsset(k_Path);

            var summary = previous.Exists
                ? (changes.Count == 0 ? "no row changed" : $"{changes.Count} row(s) changed:\n" + string.Join("\n", changes))
                : $"first baseline, {evaluated.Count} rows";
            Debug.Log($"[RayAimBaseline] wrote {k_Path}\n  criterion: {ResearchRayAimReport.Criterion}\n{summary}");
        }
    }
}
