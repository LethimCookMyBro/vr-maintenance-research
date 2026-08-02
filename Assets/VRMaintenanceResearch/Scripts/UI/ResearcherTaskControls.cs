using UnityEngine;
using UnityEngine.InputSystem;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Researcher-facing operator controls. Every function of the validated baseline is
    /// preserved and calls the same task methods: Pause, Resume, Retry, Reset Task,
    /// Abort Task, Safety Stop, Continue to Next Task, and the two training controls.
    ///
    /// Changes are presentation only:
    ///  - still gated on developmentMode, and now additionally collapsed by default so
    ///    it does not sit over the participant view during normal operation,
    ///  - opened with the F9 key,
    ///  - Safety Stop separated by a rule and given the reserved amber/red treatment.
    /// </summary>
    public sealed class ResearcherTaskControls : MonoBehaviour
    {
        const float PanelWidth = 236f;

        [SerializeField] MaintenanceTaskController task;
        [SerializeField] bool trainingScene;
        // F9 is fixed by the research protocol. Older scenes may serialize a
        // different numeric Key value, so never use that value as an index into
        // Keyboard; Input System throws for values outside its key-control table.
        [SerializeField] Key toggleKey = Key.F9;

        bool expanded;
        GUIStyle headingStyle;
        GUIStyle captionStyle;
        GUIStyle normalButton;
        GUIStyle dangerButton;
        Texture2D panelTexture;
        Texture2D ruleTexture;
        Texture2D dangerTexture;

        void Awake()
        {
            if (task == null)
                task = FindFirstObjectByType<MaintenanceTaskController>();

            if (toggleKey != Key.F9)
                toggleKey = Key.F9;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f9Key != null && keyboard.f9Key.wasPressedThisFrame)
                expanded = !expanded;
        }

        void OnDestroy()
        {
            Destroy(panelTexture);
            Destroy(ruleTexture);
            Destroy(dangerTexture);
        }

        void OnGUI()
        {
            var session = ResearchSessionManager.Instance;
            if (session == null || !session.Configuration.developmentMode)
                return;

            EnsureStyles();

            if (!expanded)
                return;

            var height = trainingScene ? 116f : 268f;
            GUILayout.BeginArea(new Rect(Screen.width - PanelWidth - 12f, 12f, PanelWidth, height), GUIContent.none, PanelStyle());
            GUILayout.Space(8f);

            if (trainingScene)
            {
                GUILayout.Label("Development Training Controls", headingStyle);
                if (GUILayout.Button("Continue to First Task", normalButton))
                {
                    FindFirstObjectByType<TrainingMaintenanceTask>()?.CompleteTraining();
                    session.StartFirstTaskAfterTraining();
                }
                if (GUILayout.Button("Skip Training (development only)", normalButton))
                {
                    FindFirstObjectByType<TrainingMaintenanceTask>()?.CompleteTraining();
                    session.SkipTrainingWhenPermitted();
                }
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label("Researcher Controls (Development)", headingStyle);
            GUILayout.Label(task == null ? "Task unavailable" : "State: " + task.State, captionStyle);
            GUILayout.Space(4f);

            if (GUILayout.Button("Pause", normalButton)) task?.PauseTask();
            if (GUILayout.Button("Resume", normalButton)) task?.ResumeTask();
            if (GUILayout.Button("Retry", normalButton)) task?.Retry();
            if (GUILayout.Button("Reset Task", normalButton)) task?.ResetDevelopmentTask();
            if (GUILayout.Button("Abort Task", normalButton)) task?.AbortTask();

            if (task != null && (task.State == TaskState.Completed || task.State == TaskState.Aborted || task.State == TaskState.TimedOut || task.State == TaskState.SafetyStopped))
            {
                if (GUILayout.Button("Continue to Next Task", normalButton))
                    session.CompleteCurrentTaskAndAdvance();
            }

            GUILayout.Space(8f);
            GUILayout.Box(GUIContent.none, RuleStyle(), GUILayout.Height(2f), GUILayout.ExpandWidth(true));
            GUILayout.Space(6f);
            if (GUILayout.Button("SAFETY STOP", dangerButton)) task?.SafetyStop();

            GUILayout.EndArea();
        }

        // ---------- styling ----------

        void EnsureStyles()
        {
            if (headingStyle != null)
                return;

            panelTexture = Solid(new Color(0.078f, 0.10f, 0.13f, 0.93f));
            ruleTexture = Solid(new Color(0.35f, 0.40f, 0.46f, 0.7f));
            dangerTexture = Solid(new Color(0.62f, 0.16f, 0.12f, 0.95f));

            headingStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 11,
                padding = new RectOffset(10, 10, 2, 4),
            };
            headingStyle.normal.textColor = new Color(0.95f, 0.96f, 0.97f);

            captionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                padding = new RectOffset(10, 10, 0, 2),
            };
            captionStyle.normal.textColor = new Color(0.66f, 0.70f, 0.75f);

            normalButton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(12, 8, 4, 4),
                margin = new RectOffset(10, 10, 2, 2),
            };

            dangerButton = new GUIStyle(normalButton)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            dangerButton.normal.background = dangerTexture;
            dangerButton.hover.background = dangerTexture;
            dangerButton.active.background = dangerTexture;
            dangerButton.normal.textColor = Color.white;
            dangerButton.hover.textColor = new Color(1f, 0.90f, 0.62f);
            dangerButton.active.textColor = Color.white;

        }

        GUIStyle PanelStyle()
        {
            var style = new GUIStyle(GUIStyle.none);
            style.normal.background = panelTexture;
            return style;
        }

        GUIStyle RuleStyle()
        {
            var style = new GUIStyle(GUIStyle.none) { margin = new RectOffset(10, 10, 0, 0) };
            style.normal.background = ruleTexture;
            return style;
        }

        static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
