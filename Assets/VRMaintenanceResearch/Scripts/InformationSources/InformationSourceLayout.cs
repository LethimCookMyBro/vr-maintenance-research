using System.Linq;
using System.Text;
using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Which information source sits in which slot of the dock, and who decides.
    ///
    /// == the confound this exists to remove ==
    ///
    /// The dock sorts its four cards by <see cref="InformationSourceType"/>, so the row
    /// reads manual, troubleshooting, video, visual guide — left to right, in both
    /// tasks, for every participant, always. Position in the row is therefore perfectly
    /// confounded with source type: a participant who reaches for the leftmost card
    /// because it is leftmost is indistinguishable from one who reaches for the manual
    /// because it is the manual, and *which source a participant chooses* is a primary
    /// outcome under proposal 10.2.2. With one layout in the build there is nothing to
    /// model the bias out with afterwards.
    ///
    /// == what this does, and what it deliberately does not do ==
    ///
    /// It provides four orders, each with its own `information_source_layout_id`, and
    /// assigns one to a participant deterministically from their participant code, so
    /// the assignment is reproducible from the recorded data alone and needs no separate
    /// schedule file to be kept in step with the sessions.
    ///
    /// **It is off.** <see cref="ResearchSessionConfig.counterbalanceInformationSourceOrder"/>
    /// defaults to false, and with it false nothing here runs, the row keeps the fixed
    /// order it has always had, and the recorded id stays
    /// <see cref="FixedLayoutId"/> — so today's build behaves exactly as it did before
    /// this file existed. Turning it on is a protocol decision, it changes what the
    /// study measures, and it belongs to the supervisor. See PROTOCOL_CHANGE_LOG.md.
    ///
    /// Nothing here moves, resizes or re-angles the dock. The four slot poses are
    /// whatever the dock builder authored; all that changes is which card occupies
    /// which of them, so distance, apparent size and reach are untouched and the
    /// equidistance the dock was built for still holds.
    /// </summary>
    public static class InformationSourceLayouts
    {
        /// <summary>
        /// The id recorded when counterbalancing is off: the fixed sourceType order the
        /// build has always used. Unchanged so that data collected before and after this
        /// file can be told apart by nothing at all — because nothing differs.
        /// </summary>
        public const string FixedLayoutId = "sources-layout-development-a";

        /// <summary>
        /// A cyclic Latin square on the four source types. Each type appears exactly
        /// once in each of the four slots across the four layouts, which is what breaks
        /// the type/position confound; row 1 is the order the build already used, so the
        /// fixed condition is a member of the set rather than something outside it.
        ///
        /// Four orders, not all twenty-four: the proposal recruits 8 participants per
        /// group (8.2.1), so four layouts divide both groups evenly and twenty-four
        /// cannot. A cyclic square balances *position* but preserves relative adjacency
        /// — the manual is always immediately left of the troubleshooting guide, wrapping
        /// round. A Williams square would balance adjacency too. **Which design to use
        /// is a research decision and is not settled here**; changing it means editing
        /// this table and nothing else.
        /// </summary>
        public static readonly Layout[] Counterbalanced =
        {
            new Layout("sources-layout-counterbalanced-1",
                InformationSourceType.ProductManual, InformationSourceType.TextTroubleshootingGuide,
                InformationSourceType.InstructionalVideo, InformationSourceType.VisualStepByStepGuide),
            new Layout("sources-layout-counterbalanced-2",
                InformationSourceType.TextTroubleshootingGuide, InformationSourceType.InstructionalVideo,
                InformationSourceType.VisualStepByStepGuide, InformationSourceType.ProductManual),
            new Layout("sources-layout-counterbalanced-3",
                InformationSourceType.InstructionalVideo, InformationSourceType.VisualStepByStepGuide,
                InformationSourceType.ProductManual, InformationSourceType.TextTroubleshootingGuide),
            new Layout("sources-layout-counterbalanced-4",
                InformationSourceType.VisualStepByStepGuide, InformationSourceType.ProductManual,
                InformationSourceType.TextTroubleshootingGuide, InformationSourceType.InstructionalVideo),
        };

        public sealed class Layout
        {
            public Layout(string id, params InformationSourceType[] slots)
            {
                Id = id;
                Slots = slots;
            }

            public string Id { get; }

            /// <summary>Source type per slot, slot 0 leftmost as the dock builder lays them out.</summary>
            public InformationSourceType[] Slots { get; }
        }

        /// <summary>The fixed order: source types in enum order, which is what the dock builder sorts by.</summary>
        public static readonly Layout Fixed = new Layout(FixedLayoutId,
            InformationSourceType.ProductManual, InformationSourceType.TextTroubleshootingGuide,
            InformationSourceType.InstructionalVideo, InformationSourceType.VisualStepByStepGuide);

        /// <summary>
        /// The layout a session runs under. Honours the switch, so with counterbalancing
        /// off this is always <see cref="Fixed"/> whatever the participant code is.
        /// </summary>
        public static Layout Resolve(ResearchSessionConfig config)
        {
            if (config == null || !config.counterbalanceInformationSourceOrder)
                return Fixed;
            return Counterbalanced[(int)(Fingerprint(config.participantCode) % (uint)Counterbalanced.Length)];
        }

        /// <summary>
        /// FNV-1a over the uppercased participant code, modulo the number of layouts.
        ///
        /// Deliberately not <c>string.GetHashCode()</c>: that is explicitly documented as
        /// unstable between runtimes and is randomised per process on some of them, so
        /// the same participant code could draw a different layout from a different
        /// build of the same scene — which would make the assignment unreproducible and
        /// `information_source_layout_id` unauditable.
        ///
        /// Uppercased first, so that a code re-typed as `p01` instead of `P01` between
        /// the two tasks cannot silently move that participant to another layout. Codes
        /// are already restricted to `[A-Za-z0-9_-]` by
        /// <see cref="ResearchSessionConfig.Validate"/>, so the bytes are plain ASCII and
        /// this is reproducible in any language for the analysis.
        /// </summary>
        public static uint Fingerprint(string participantCode)
        {
            unchecked
            {
                var hash = 2166136261u;
                foreach (var b in Encoding.UTF8.GetBytes((participantCode ?? string.Empty).ToUpperInvariant()))
                {
                    hash ^= b;
                    hash *= 16777619u;
                }

                return hash;
            }
        }
    }

    /// <summary>
    /// Puts the dock's cards into the slots the session's layout asks for, once, at
    /// scene load.
    ///
    /// The dock is authored by an editor tool and baked into the three task scenes, so
    /// there was no runtime code positioning these cards at all. This is the smallest
    /// thing that can change the order without touching the authored geometry: it reads
    /// the four slot poses the builder produced, then hands each pose to whichever card
    /// the layout puts there. It never computes a pose, so the dock cannot drift.
    ///
    /// Added to the dock root by <c>InformationDockBuilder</c>, so every task scene gets
    /// it without anyone hand-editing a scene. With the switch off — which is how it
    /// ships — <see cref="Apply"/> returns before touching anything.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InformationSourceLayoutApplier : MonoBehaviour
    {
        void Awake() => Apply();

        public void Apply()
        {
            var config = ResearchSessionManager.Instance == null ? null : ResearchSessionManager.Instance.Configuration;
            var layout = InformationSourceLayouts.Resolve(config);

            // The fixed layout is what the scene already holds. Returning here rather
            // than reassigning every card to the pose it is already in keeps the "off"
            // path incapable of moving anything at all, including by float rounding.
            if (ReferenceEquals(layout, InformationSourceLayouts.Fixed))
                return;

            var sources = Object.FindObjectsByType<InformationSourceController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(source => source.Definition != null)
                .ToList();

            // Slot poses, in the order the builder laid them out: it sorts by source
            // type, so sorting by source type recovers slot 0..3 left to right.
            var slots = sources
                .OrderBy(source => (int)source.Definition.sourceType)
                .Select(source => (source.transform.position, source.transform.rotation))
                .ToList();

            // The training room carries one source, not four, so there is no row to
            // permute there. Expected, and not a warning.
            if (slots.Count != layout.Slots.Length)
            {
                Debug.Log($"[SourceLayout] this scene has {slots.Count} source(s), not the {layout.Slots.Length} " +
                          $"{layout.Id} orders; the dock is left as authored.");
                return;
            }

            for (var slot = 0; slot < layout.Slots.Length; slot++)
            {
                var wanted = layout.Slots[slot];
                var card = sources.FirstOrDefault(source => source.Definition.sourceType == wanted);
                if (card == null)
                {
                    Debug.LogWarning($"[SourceLayout] {layout.Id} wants {wanted} in slot {slot + 1} " +
                                     "but this scene has no such source; the dock is left as authored.");
                    return;
                }

                card.transform.SetPositionAndRotation(slots[slot].position, slots[slot].rotation);
            }

            Debug.Log($"[SourceLayout] {layout.Id}: " + string.Join(" | ", layout.Slots));
        }
    }
}
