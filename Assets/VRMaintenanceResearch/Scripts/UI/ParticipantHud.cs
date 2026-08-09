using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Head-referenced display of elapsed time, completion percentage and the three
    /// milestones the task already tracks.
    ///
    /// It is read-only in the strongest sense: it writes no research event, changes
    /// no task state, adds no measurement, and reads only
    /// <see cref="MaintenanceTaskController.ActiveElapsed"/>,
    /// <see cref="MaintenanceTaskController.DeviceTested"/>,
    /// <see cref="MaintenanceTaskController.RepairPerformed"/> and
    /// <see cref="MaintenanceTaskController.State"/> — the same three transitions the
    /// event stream already records as DeviceTestStarted, the correct RepairAction and
    /// TaskCompleted. Nothing here invents a step: both tasks are diagnostic and have
    /// exactly one repair action, so the checklist is a record of what has happened,
    /// never an instruction, and it never names a part, a tool or a procedure.
    ///
    /// Every element is off-switchable from <see cref="ResearchSessionConfig"/>, and the
    /// three switches are written into session_manifest.csv, because a session's timing
    /// and search data is only interpretable against the display it was collected under.
    /// What each one costs in interpretation is set out in KNOWN_LIMITATIONS.md.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ParticipantHud : MonoBehaviour
    {
        [SerializeField] MaintenanceTaskController task;
        [SerializeField] float distance = 1.15f;
        [SerializeField] Vector2 panelSize = new Vector2(1.60f, 0.94f);

        const float CanvasWidth = 1600f;

        Transform head;
        GameObject root;
        TextMeshProUGUI timeValue;
        TextMeshProUGUI progressValue;
        Image progressFill;
        readonly TextMeshProUGUI[] objectiveChecks = new TextMeshProUGUI[3];
        readonly TextMeshProUGUI[] objectiveText = new TextMeshProUGUI[3];
        readonly Image[] objectiveChips = new Image[3];
        bool showTime, showProgress, showObjectives;
        int lastDone = -1;
        int lastSecond = -1;

        void Start()
        {
            if (task == null)
                task = FindFirstObjectByType<MaintenanceTaskController>();

            var config = ResearchSessionManager.Instance == null ? null : ResearchSessionManager.Instance.Configuration;
            var definition = task == null ? null : task.Definition;

            // Two gates on the clock, deliberately: showTimerToParticipant is per task
            // and already existed, showTimer is the session-wide switch this pass added.
            showTime = (config == null || config.showTimer) && definition != null && definition.showTimerToParticipant;
            showProgress = config == null || config.showProgress;
            showObjectives = config == null || config.showObjectives;

            if (!showTime && !showProgress && !showObjectives)
                return;

            InformationSourceController.EnsureLocalizedFontFallbacks();
            Build(config == null ? ResearchLanguage.English : config.language);
            Refresh(true);
        }

        void LateUpdate()
        {
            if (root == null)
                return;

            // The rig swaps its camera between the simulator and a headset, so the head
            // is resolved every frame rather than cached at Start.
            if (head == null || !head.gameObject.activeInHierarchy)
            {
                if (Camera.main == null)
                    return;
                head = Camera.main.transform;
                root.transform.SetParent(head, false);
                root.transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, distance), Quaternion.identity);
            }

            Refresh(false);
        }

        void Build(ResearchLanguage language)
        {
            root = new GameObject("Participant HUD", typeof(RectTransform));
            root.transform.SetParent(transform, false);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = new Vector2(CanvasWidth, CanvasWidth * (panelSize.y / panelSize.x));
            rect.localScale = Vector3.one * (panelSize.x / CanvasWidth);
            root.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 3f;
            // No GraphicRaycaster: the display carries no control, so a controller ray
            // must pass straight through it to the bench behind.

            if (showTime)
                BuildTime(rect);
            if (showProgress)
                BuildProgress(rect);
            if (showObjectives)
                BuildObjectives(rect, language);
        }

        void BuildTime(RectTransform parent)
        {
            var panel = Corner(parent, new Vector2(0f, 0f), new Vector2(48f, 40f), new Vector2(300f, 118f));
            Caption(panel, "TIME");
            timeValue = ResearchUiKit.Label("Value", panel, "00:00", 62f, ResearchUiKit.OnDark, TextAlignmentOptions.Left, FontStyles.Bold);
            timeValue.rectTransform.anchorMin = new Vector2(0f, 0f);
            timeValue.rectTransform.anchorMax = new Vector2(1f, 0f);
            timeValue.rectTransform.pivot = new Vector2(0.5f, 0f);
            timeValue.rectTransform.sizeDelta = new Vector2(-48f, 70f);
            timeValue.rectTransform.anchoredPosition = new Vector2(0f, 12f);
        }

        void BuildProgress(RectTransform parent)
        {
            var panel = Corner(parent, new Vector2(1f, 0f), new Vector2(-48f, 40f), new Vector2(340f, 118f));
            Caption(panel, "PROGRESS");

            progressValue = ResearchUiKit.Label("Value", panel, "0%", 62f, ResearchUiKit.OnDark, TextAlignmentOptions.Right, FontStyles.Bold);
            progressValue.rectTransform.anchorMin = new Vector2(0f, 0f);
            progressValue.rectTransform.anchorMax = new Vector2(1f, 0f);
            progressValue.rectTransform.pivot = new Vector2(0.5f, 0f);
            progressValue.rectTransform.sizeDelta = new Vector2(-48f, 70f);
            progressValue.rectTransform.anchoredPosition = new Vector2(0f, 22f);

            var track = ResearchUiKit.Rounded("Track", panel, ResearchUiKit.GlassRaised, 4f);
            track.rectTransform.anchorMin = new Vector2(0f, 0f);
            track.rectTransform.anchorMax = new Vector2(1f, 0f);
            track.rectTransform.pivot = new Vector2(0.5f, 0f);
            track.rectTransform.sizeDelta = new Vector2(-48f, 8f);
            track.rectTransform.anchoredPosition = new Vector2(0f, 14f);

            progressFill = ResearchUiKit.Rounded("Fill", track.transform, ResearchUiKit.Accent, 4f);
            progressFill.rectTransform.anchorMin = Vector2.zero;
            progressFill.rectTransform.anchorMax = new Vector2(0f, 1f);
            progressFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            progressFill.rectTransform.sizeDelta = new Vector2(0f, 0f);
            progressFill.rectTransform.anchoredPosition = Vector2.zero;
        }

        void BuildObjectives(RectTransform parent, ResearchLanguage language)
        {
            var panel = Corner(parent, new Vector2(1f, 1f), new Vector2(-48f, -40f), new Vector2(560f, 250f));
            Caption(panel, "OBJECTIVES");

            var lines = Objectives(language);
            for (var i = 0; i < 3; i++)
            {
                var chip = ResearchUiKit.Rounded("Chip " + (i + 1), panel, ResearchUiKit.SlateSoft, 8f);
                ResearchUiKit.Place(chip.rectTransform, 26f, 74f + i * 54f, 32f, 32f);
                objectiveChips[i] = chip;

                objectiveChecks[i] = ResearchUiKit.Label("Check", chip.transform, string.Empty, 26f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
                ResearchUiKit.Stretch(objectiveChecks[i].rectTransform);

                objectiveText[i] = ResearchUiKit.Label("Objective " + (i + 1), panel, lines[i], 28f, ResearchUiKit.OnDarkMuted, TextAlignmentOptions.Left);
                ResearchUiKit.Place(objectiveText[i].rectTransform, 70f, 76f + i * 54f, 466f, 34f);
            }
        }

        /// <summary>A glass block pinned to one corner of the display.</summary>
        static RectTransform Corner(RectTransform parent, Vector2 anchor, Vector2 offset, Vector2 size)
        {
            var panel = ResearchUiKit.GlassPanel("Block", parent, 18f);
            panel.anchorMin = anchor;
            panel.anchorMax = anchor;
            panel.pivot = anchor;
            panel.sizeDelta = size;
            panel.anchoredPosition = offset;
            return panel;
        }

        static void Caption(RectTransform panel, string text)
        {
            var caption = ResearchUiKit.Label("Caption", panel, text, 24f, ResearchUiKit.OnDarkMuted, TextAlignmentOptions.Left, FontStyles.Bold);
            caption.characterSpacing = 8f;
            ResearchUiKit.Place(caption.rectTransform, 26f, 18f, 480f, 28f);
        }

        /// <summary>
        /// The three milestones, in the participant's own language.
        ///
        /// Same hardcoded-switch shape as <see cref="TaskStatusBoard.FinishedMessage"/>.
        /// They are written as states reached, not as instructions: naming an action to
        /// perform, or the part to perform it on, would turn an open diagnostic task
        /// into a checklist and hand over what the study is measuring.
        /// </summary>
        static string[] Objectives(ResearchLanguage language)
        {
            switch (language)
            {
                case ResearchLanguage.Thai:
                    return new[] { "ทดสอบเครื่องแล้ว", "ดำเนินการซ่อมแล้ว", "ทดสอบผ่านแล้ว" };
                case ResearchLanguage.Japanese:
                    return new[] { "動作確認を実施", "修理を実施", "動作確認に合格" };
                default:
                    return new[] { "Device tested", "Repair performed", "Device test passed" };
            }
        }

        void Refresh(bool force)
        {
            if (task == null)
                return;

            if (showTime && timeValue != null)
            {
                var seconds = Mathf.FloorToInt(task.ActiveElapsed);
                if (force || seconds != lastSecond)
                {
                    lastSecond = seconds;
                    timeValue.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
                }
            }

            var done = (task.DeviceTested ? 1 : 0) + (task.RepairPerformed ? 1 : 0) + (task.State == TaskState.Completed ? 1 : 0);
            if (!force && done == lastDone)
                return;
            lastDone = done;

            if (showProgress && progressValue != null)
            {
                progressValue.text = Mathf.RoundToInt(done / 3f * 100f) + "%";
                progressFill.rectTransform.anchorMax = new Vector2(done / 3f, 1f);
                progressFill.color = done == 3 ? ResearchUiKit.Ok : ResearchUiKit.Accent;
            }

            if (!showObjectives || objectiveChips[0] == null)
                return;

            var satisfied = new[] { task.DeviceTested, task.RepairPerformed, task.State == TaskState.Completed };
            for (var i = 0; i < 3; i++)
            {
                objectiveChips[i].color = satisfied[i] ? ResearchUiKit.Ok : ResearchUiKit.SlateSoft;
                objectiveChecks[i].text = satisfied[i] ? "✓" : string.Empty;
                objectiveText[i].color = satisfied[i] ? ResearchUiKit.OnDark : ResearchUiKit.OnDarkMuted;
            }
        }
    }
}
