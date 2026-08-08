using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Read-only world-space status board mounted above and behind the workbench. It
    /// mirrors <see cref="MaintenanceTaskController.State"/> and the attempt number,
    /// and once the task ends it tells the participant, in their own language, that
    /// the work is done and to wait.
    ///
    /// It never writes a research event, never changes task state and never names the
    /// faulty component — and it deliberately carries no control. The participant
    /// removes the headset between the two tasks for NASA-TLX, so a Continue button
    /// here would let them load the second task before that questionnaire was
    /// administered. Advancing is the researcher's, from the desktop panel.
    /// </summary>
    public sealed class TaskStatusBoard : MonoBehaviour
    {
        [SerializeField] MaintenanceTaskController task;
        [SerializeField] string taskTitle = "Maintenance Task";
        [SerializeField] Vector3 boardPosition = new Vector3(2.1f, 1.48f, 1.25f);
        [SerializeField] Vector2 boardSize = new Vector2(0.95f, 0.34f);

        TextMeshProUGUI stateLabel;
        TextMeshProUGUI attemptLabel;
        TextMeshProUGUI finishedLabel;
        Image statePip;
        TaskState lastState = (TaskState)(-1);
        int lastAttempt = -1;

        void Start()
        {
            if (task == null)
                task = FindFirstObjectByType<MaintenanceTaskController>();
            Build();
            Refresh(true);
        }

        void Update() => Refresh(false);

        void Build()
        {
            var canvasObject = new GameObject("Task Status Board", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);
            // Identity rotation faces the participant, who stands on the -Z side looking towards +Z.
            canvasObject.transform.SetPositionAndRotation(boardPosition, Quaternion.identity);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            var rect = (RectTransform)canvasObject.transform;
            rect.sizeDelta = new Vector2(1040f, 1040f * (boardSize.y / boardSize.x));
            rect.localScale = Vector3.one * (boardSize.x / 1040f);
            canvasObject.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 3f;
            canvasObject.AddComponent<TrackedDeviceGraphicRaycaster>();

            var background = ResearchUiKit.Panel("Background", canvasObject.transform, ResearchUiKit.Navy);
            ResearchUiKit.Stretch(background.rectTransform);

            var accent = ResearchUiKit.Panel("Accent", background.transform, ResearchUiKit.Accent);
            accent.rectTransform.anchorMin = new Vector2(0f, 1f);
            accent.rectTransform.anchorMax = new Vector2(1f, 1f);
            accent.rectTransform.pivot = new Vector2(0.5f, 1f);
            accent.rectTransform.sizeDelta = new Vector2(0f, 6f);
            accent.rectTransform.anchoredPosition = Vector2.zero;

            var title = ResearchUiKit.Label("Title", background.transform, taskTitle, 40f, ResearchUiKit.OnDark, TextAlignmentOptions.Left, FontStyles.Bold);
            ResearchUiKit.Place(title.rectTransform, 44f, 24f, 900f, 52f);

            statePip = ResearchUiKit.Panel("Pip", background.transform, ResearchUiKit.Accent);
            ResearchUiKit.Place(statePip.rectTransform, 44f, 100f, 18f, 18f);

            stateLabel = ResearchUiKit.Label("State", background.transform, string.Empty, 32f, ResearchUiKit.OnDarkMuted, TextAlignmentOptions.Left);
            ResearchUiKit.Place(stateLabel.rectTransform, 74f, 94f, 600f, 36f);

            attemptLabel = ResearchUiKit.Label("Attempt", background.transform, string.Empty, 32f, ResearchUiKit.OnDarkMuted, TextAlignmentOptions.Right);
            ResearchUiKit.Place(attemptLabel.rectTransform, 340f, 94f, 656f, 36f);

            var finishedPanel = ResearchUiKit.Panel("Finished", background.transform, ResearchUiKit.Slate);
            ResearchUiKit.Place(finishedPanel.rectTransform, 44f, 160f, 952f, 170f);
            finishedLabel = ResearchUiKit.Label("Finished Text", finishedPanel.transform, string.Empty, 40f, ResearchUiKit.OnDark, TextAlignmentOptions.Center);
            ResearchUiKit.Stretch(finishedLabel.rectTransform, 16f);
            finishedPanel.gameObject.SetActive(false);
        }

        /// <summary>
        /// The one thing on this board addressed to the participant rather than the
        /// researcher, so it is the one thing that has to be in their language. Same
        /// hardcoded-switch shape as InformationSourceController.CompactSourceLabel.
        /// It says the work is over and to wait; it does not say what happens next,
        /// because what happens next is the researcher's script, not the software's.
        /// </summary>
        static string FinishedMessage(ResearchLanguage language)
        {
            switch (language)
            {
                case ResearchLanguage.Thai: return "งานนี้เสร็จแล้ว\nกรุณารอผู้วิจัย";
                case ResearchLanguage.Japanese: return "この作業は終了しました。\n研究者をお待ちください。";
                default: return "This task is finished.\nPlease wait for the researcher.";
            }
        }

        static bool IsTerminal(TaskState state) =>
            state == TaskState.Completed || state == TaskState.TimedOut ||
            state == TaskState.Aborted || state == TaskState.SafetyStopped;

        void Refresh(bool force)
        {
            if (stateLabel == null)
                return;

            if (task == null)
            {
                stateLabel.text = "Status: unavailable";
                return;
            }

            if (!force && task.State == lastState && task.AttemptId == lastAttempt)
                return;

            lastState = task.State;
            lastAttempt = task.AttemptId;
            stateLabel.text = "Status: " + Readable(task.State);
            attemptLabel.text = "Attempt " + task.AttemptId;
            statePip.color = PipColor(task.State);

            if (finishedLabel == null)
                return;

            var terminal = IsTerminal(task.State);
            finishedLabel.transform.parent.gameObject.SetActive(terminal);
            if (!terminal)
                return;

            InformationSourceController.EnsureLocalizedFontFallbacks();
            var session = ResearchSessionManager.Instance;
            finishedLabel.text = FinishedMessage(session == null ? ResearchLanguage.English : session.Configuration.language);
        }

        static string Readable(TaskState state)
        {
            switch (state)
            {
                case TaskState.NotStarted: return "Not started";
                case TaskState.Active: return "In progress";
                case TaskState.Paused: return "Paused";
                case TaskState.Completed: return "Completed";
                case TaskState.TimedOut: return "Time limit reached";
                case TaskState.Aborted: return "Stopped by researcher";
                case TaskState.SafetyStopped: return "Safety stop";
                case TaskState.Reset: return "Reset";
                default: return state.ToString();
            }
        }

        static Color PipColor(TaskState state)
        {
            switch (state)
            {
                case TaskState.Active: return ResearchUiKit.Accent;
                case TaskState.Completed: return ResearchUiKit.Hex("#3FB27F");
                case TaskState.Paused:
                case TaskState.Reset: return ResearchUiKit.Warning;
                case TaskState.SafetyStopped:
                case TaskState.Aborted:
                case TaskState.TimedOut: return ResearchUiKit.Danger;
                default: return ResearchUiKit.SlateSoft;
            }
        }
    }
}
