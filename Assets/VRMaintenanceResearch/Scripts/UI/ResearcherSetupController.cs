using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Researcher setup screen. Session behaviour is unchanged from the validated
    /// baseline: the same Configure -> StartSession -> StartConfiguredFlow sequence,
    /// the same validation messages, and the same pseudonymity warning. Only the
    /// presentation moved from IMGUI to a centred TextMeshPro desktop interface.
    /// The layout is built in code so there is no prefab wiring to drift.
    /// </summary>
    public sealed class ResearcherSetupController : MonoBehaviour
    {
        const float FormWidth = 1420f;
        const float ColumnWidth = 690f;
        const float ColumnGap = 40f;
        const float Gutter = 28f;
        const float Content = ColumnWidth - (Gutter * 2f);

        ResearchSessionManager session;
        ResearchSessionConfig config;
        TextMeshProUGUI statusLabel;
        Image statusStrip;
        Canvas canvas;

        readonly List<Action> refreshers = new List<Action>();

        void Start()
        {
            session = ResearchSessionManager.Instance;
            if (session == null)
            {
                var sessionObject = new GameObject("Research Session Manager");
                session = sessionObject.AddComponent<ResearchSessionManager>();
            }

            config = session.Configuration;
            Build();
            SetStatus("Enter a pseudonymous Participant Code. Do not enter names, phone numbers, email addresses, or student IDs.", false);
        }

        void Build()
        {
            EnsureEventSystem();

            var canvasObject = new GameObject("Researcher Setup Canvas", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 1000f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var background = ResearchUiKit.Panel("Background", canvas.transform, ResearchUiKit.Shell);
            ResearchUiKit.Stretch(background.rectTransform);

            var band = ResearchUiKit.Panel("Header Band", canvas.transform, ResearchUiKit.Navy);
            band.rectTransform.anchorMin = new Vector2(0f, 1f);
            band.rectTransform.anchorMax = new Vector2(1f, 1f);
            band.rectTransform.pivot = new Vector2(0.5f, 1f);
            band.rectTransform.sizeDelta = new Vector2(0f, 96f);
            band.rectTransform.anchoredPosition = Vector2.zero;

            var title = ResearchUiKit.Label("Title", band.transform, "VR Maintenance Research", 34f, ResearchUiKit.OnDark, TextAlignmentOptions.Left, FontStyles.Bold);
            ResearchUiKit.Place(title.rectTransform, 48f, 20f, 700f, 40f);
            var subtitle = ResearchUiKit.Label("Subtitle", band.transform, "Researcher Session Setup", 19f, ResearchUiKit.OnDarkMuted, TextAlignmentOptions.Left);
            ResearchUiKit.Place(subtitle.rectTransform, 48f, 58f, 700f, 26f);

            var form = ResearchUiKit.Rect("Form", canvas.transform);
            form.anchorMin = new Vector2(0.5f, 1f);
            form.anchorMax = new Vector2(0.5f, 1f);
            form.pivot = new Vector2(0.5f, 1f);
            form.sizeDelta = new Vector2(FormWidth, 0f);
            form.anchoredPosition = new Vector2(0f, -128f);

            const float right = ColumnWidth + ColumnGap;

            var left = Section(form, 0f, 0f, "1", "Session", out var sessionBody, 200f);
            BuildSessionSection(sessionBody);
            Section(form, 0f, left, "2", "Experimental condition", out var conditionBody, 212f);
            BuildConditionSection(conditionBody);

            var rightY = Section(form, right, 0f, "3", "Session options", out var optionsBody, 246f);
            BuildOptionsSection(optionsBody);
            rightY = Section(form, right, rightY, "4", "Technical notes", out var notesBody, 100f);
            BuildNotesSection(notesBody);
            BuildFooter(form, right, rightY);
        }

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            // The project runs the Input System package only, so the legacy
            // StandaloneInputModule would throw on the first mouse read.
            var go = new GameObject("Event System", typeof(EventSystem));
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        float Section(RectTransform parent, float x, float y, string index, string heading, out RectTransform body, float bodyHeight)
        {
            var card = ResearchUiKit.Card("Section " + index, parent, ResearchUiKit.Surface, ResearchUiKit.Accent);
            ResearchUiKit.Place(card.rectTransform, x, y, ColumnWidth, bodyHeight + 54f);

            var badge = ResearchUiKit.Panel("Index", card.transform, ResearchUiKit.Navy);
            ResearchUiKit.Place(badge.rectTransform, 20f, 18f, 26f, 26f);
            var badgeLabel = ResearchUiKit.Label("Number", badge.transform, index, 15f, ResearchUiKit.OnDark, TextAlignmentOptions.Center, FontStyles.Bold);
            ResearchUiKit.Stretch(badgeLabel.rectTransform);

            var headingLabel = ResearchUiKit.Label("Heading", card.transform, heading, 20f, ResearchUiKit.InkStrong, TextAlignmentOptions.Left, FontStyles.Bold);
            ResearchUiKit.Place(headingLabel.rectTransform, 58f, 18f, 500f, 28f);

            body = ResearchUiKit.Rect("Body", card.transform);
            ResearchUiKit.Place(body, Gutter, 54f, Content, bodyHeight);

            return y + bodyHeight + 54f + 18f;
        }

        void BuildSessionSection(RectTransform body)
        {
            var warning = ResearchUiKit.Panel("Pseudonymity Warning", body, new Color(0.949f, 0.635f, 0.173f, 0.16f));
            ResearchUiKit.Place(warning.rectTransform, 0f, 0f, Content, 48f);
            var stripe = ResearchUiKit.Panel("Stripe", warning.transform, ResearchUiKit.Warning);
            ResearchUiKit.Place(stripe.rectTransform, 0f, 0f, 4f, 48f);
            var warningLabel = ResearchUiKit.Label("Text", warning.transform,
                "Pseudonymous codes only. Do not enter names, phone numbers, email addresses, or student IDs.",
                15f, ResearchUiKit.InkStrong, TextAlignmentOptions.Left);
            ResearchUiKit.Place(warningLabel.rectTransform, 16f, 8f, Content - 32f, 40f);

            const float fieldWidth = (Content - 24f) / 2f;
            Field(body, 0f, 62f, fieldWidth, "Participant Code", "required",
                () => config.participantCode, v => config.participantCode = v);
            Field(body, fieldWidth + 24f, 62f, fieldWidth, "Session ID", "auto",
                () => config.sessionId, v => config.sessionId = v);
            Field(body, 0f, 132f, fieldWidth, "Experiment Site", string.Empty,
                () => config.experimentSite, v => config.experimentSite = v);
            Field(body, fieldWidth + 24f, 132f, fieldWidth, "Researcher Code", string.Empty,
                () => config.researcherCode, v => config.researcherCode = v);
        }

        void BuildConditionSection(RectTransform body)
        {
            Choice(body, 0f, "Participant group", new[] { "Thai group", "Japanese group" },
                () => (int)config.participantGroup, v => config.participantGroup = (ParticipantGroup)v);
            Choice(body, 70f, "Instruction language", new[] { "Thai", "Japanese", "English" },
                () => (int)config.language, v => config.language = (ResearchLanguage)v);
            Choice(body, 140f, "Task order", new[] { "Computer → Fan", "Fan → Computer" },
                () => (int)config.taskOrder, v => config.taskOrder = (TaskOrder)v);
        }

        void BuildOptionsSection(RectTransform body)
        {
            Toggle(body, 0f, "Training required",
                () => config.trainingRequired, v => config.trainingRequired = v);
            Toggle(body, 30f, "DEVELOPMENT_TEST mode (separate output folder)",
                () => config.developmentMode, v => config.developmentMode = v);
            Toggle(body, 60f, "XR simulator mode",
                () => config.simulatorMode, v => config.simulatorMode = v);
            Toggle(body, 90f, "First-person recording consent recorded",
                () => config.firstPersonRecordingConsent, v => config.firstPersonRecordingConsent = v);
            Toggle(body, 120f, "Enable first-person recording (capture pipeline is not implemented)",
                () => config.firstPersonRecordingConsent && config.firstPersonRecordingEnabled,
                v => config.firstPersonRecordingEnabled = v);

            // The participant display. Each switch is written into session_manifest.csv,
            // so a later analysis can tell which sessions ran with which of them; turning
            // one off here is how a pilot's "too much on screen" is answered without a
            // rebuild. See KNOWN_LIMITATIONS.md for what each one costs in interpretation.
            Toggle(body, 150f, "Participant display: elapsed time (also needs the task's own timer flag)",
                () => config.showTimer, v => config.showTimer = v);
            Toggle(body, 180f, "Participant display: completion percentage",
                () => config.showProgress, v => config.showProgress = v);
            Toggle(body, 210f, "Participant display: objectives checklist",
                () => config.showObjectives, v => config.showObjectives = v);
        }

        void BuildNotesSection(RectTransform body)
        {
            var hint = ResearchUiKit.Label("Hint", body, "No personal information.", 14f, ResearchUiKit.InkMuted, TextAlignmentOptions.Left);
            ResearchUiKit.Place(hint.rectTransform, 0f, 0f, 600f, 20f);

            var frame = ResearchUiKit.Panel("Field", body, ResearchUiKit.Hex("#F4F6F8"));
            ResearchUiKit.Place(frame.rectTransform, 0f, 22f, Content, 74f);
            var underline = ResearchUiKit.Panel("Underline", frame.transform, ResearchUiKit.Line);
            underline.rectTransform.anchorMin = new Vector2(0f, 0f);
            underline.rectTransform.anchorMax = new Vector2(1f, 0f);
            underline.rectTransform.pivot = new Vector2(0.5f, 0f);
            underline.rectTransform.sizeDelta = new Vector2(0f, 2f);
            underline.rectTransform.anchoredPosition = Vector2.zero;

            var input = CreateInput(frame.transform, () => config.technicalNotes, v => config.technicalNotes = v, 300);
            input.lineType = TMP_InputField.LineType.MultiLineNewline;
        }

        void BuildFooter(RectTransform parent, float x, float y)
        {
            var footer = ResearchUiKit.Rect("Footer", parent);
            ResearchUiKit.Place(footer, x, y, ColumnWidth, 142f);

            statusStrip = ResearchUiKit.Panel("Status", footer, ResearchUiKit.Hex("#E4E8EC"));
            ResearchUiKit.Place(statusStrip.rectTransform, 0f, 0f, ColumnWidth, 66f);
            statusLabel = ResearchUiKit.Label("Status Text", statusStrip.transform, string.Empty, 15f, ResearchUiKit.InkStrong, TextAlignmentOptions.TopLeft);
            ResearchUiKit.Place(statusLabel.rectTransform, 16f, 8f, ColumnWidth - 32f, 56f);

            var start = ResearchUiKit.TextButton("Start Session", footer, "Start Session", 22f, ResearchUiKit.Accent, Color.white, out _);
            ResearchUiKit.Place(start.image.rectTransform, 0f, 80f, ColumnWidth, 58f);
            start.onClick.AddListener(StartSession);
        }

        void StartSession()
        {
            if (!session.Configure(config, out var error) || !session.StartSession(out error))
            {
                SetStatus(error, true);
                return;
            }

            SetStatus("Session created at: " + session.Logger.CurrentDataFolder, false);
            session.StartConfiguredFlow();
        }

        void SetStatus(string message, bool isError)
        {
            if (statusLabel == null)
                return;

            statusLabel.text = message;
            statusLabel.color = isError ? ResearchUiKit.Danger : ResearchUiKit.InkStrong;
            statusStrip.color = isError ? new Color(0.753f, 0.224f, 0.169f, 0.14f) : ResearchUiKit.Hex("#E4E8EC");
        }

        // ---------- controls ----------

        void Field(RectTransform parent, float x, float y, float width, string caption, string hint, Func<string> get, Action<string> set)
        {
            var label = ResearchUiKit.Label(caption + " Caption", parent, caption, 14f, ResearchUiKit.InkMuted, TextAlignmentOptions.Left, FontStyles.Bold);
            ResearchUiKit.Place(label.rectTransform, x, y, width, 18f);

            if (!string.IsNullOrEmpty(hint))
            {
                var hintLabel = ResearchUiKit.Label(caption + " Hint", parent, hint, 13f, ResearchUiKit.InkMuted, TextAlignmentOptions.Right);
                ResearchUiKit.Place(hintLabel.rectTransform, x, y, width, 18f);
            }

            var frame = ResearchUiKit.Panel(caption + " Field", parent, ResearchUiKit.Hex("#F4F6F8"));
            ResearchUiKit.Place(frame.rectTransform, x, y + 20f, width, 40f);
            var underline = ResearchUiKit.Panel("Underline", frame.transform, ResearchUiKit.Line);
            underline.rectTransform.anchorMin = new Vector2(0f, 0f);
            underline.rectTransform.anchorMax = new Vector2(1f, 0f);
            underline.rectTransform.pivot = new Vector2(0.5f, 0f);
            underline.rectTransform.sizeDelta = new Vector2(0f, 2f);
            underline.rectTransform.anchoredPosition = Vector2.zero;

            CreateInput(frame.transform, get, set, 64);
        }

        TMP_InputField CreateInput(Transform frame, Func<string> get, Action<string> set, int characterLimit)
        {
            var image = frame.GetComponent<Image>();
            image.raycastTarget = true;

            var viewport = ResearchUiKit.Rect("Text Area", frame);
            ResearchUiKit.Stretch(viewport, 10f);
            viewport.gameObject.AddComponent<RectMask2D>();

            var text = ResearchUiKit.Label("Text", viewport, string.Empty, 16f, ResearchUiKit.InkStrong, TextAlignmentOptions.TopLeft);
            ResearchUiKit.Stretch(text.rectTransform);

            var input = frame.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = viewport;
            input.textComponent = text;
            input.characterLimit = characterLimit;
            input.text = get() ?? string.Empty;
            input.onValueChanged.AddListener(value => set(value));
            input.onEndEdit.AddListener(value => set(value));
            return input;
        }

        void Choice(RectTransform parent, float y, string caption, string[] options, Func<int> get, Action<int> set)
        {
            var label = ResearchUiKit.Label(caption + " Caption", parent, caption, 14f, ResearchUiKit.InkMuted, TextAlignmentOptions.Left, FontStyles.Bold);
            ResearchUiKit.Place(label.rectTransform, 0f, y, 400f, 18f);

            const float segmentGap = 10f;
            var width = (Content - (segmentGap * (options.Length - 1))) / options.Length;
            var images = new Image[options.Length];
            var labels = new TextMeshProUGUI[options.Length];

            for (var i = 0; i < options.Length; i++)
            {
                var index = i;
                var button = ResearchUiKit.TextButton(caption + " " + options[i], parent, options[i], 16f, ResearchUiKit.Hex("#F4F6F8"), ResearchUiKit.InkStrong, out var buttonLabel);
                ResearchUiKit.Place(button.image.rectTransform, i * (width + segmentGap), y + 22f, width, 40f);
                images[i] = button.image;
                labels[i] = buttonLabel;
                button.onClick.AddListener(() => { set(index); RefreshAll(); });
            }

            Action refresh = () =>
            {
                var selected = get();
                for (var i = 0; i < images.Length; i++)
                {
                    var on = i == selected;
                    images[i].color = on ? ResearchUiKit.Accent : ResearchUiKit.Hex("#F4F6F8");
                    labels[i].color = on ? Color.white : ResearchUiKit.InkStrong;
                    labels[i].fontStyle = on ? FontStyles.Bold : FontStyles.Normal;
                }
            };
            refreshers.Add(refresh);
            refresh();
        }

        void Toggle(RectTransform parent, float y, string caption, Func<bool> get, Action<bool> set)
        {
            var button = ResearchUiKit.TextButton(caption, parent, string.Empty, 1f, new Color(0f, 0f, 0f, 0f), Color.white, out _);
            ResearchUiKit.Place(button.image.rectTransform, 0f, y, Content, 26f);

            var box = ResearchUiKit.Panel("Box", button.transform, ResearchUiKit.Hex("#F4F6F8"));
            ResearchUiKit.Place(box.rectTransform, 0f, 3f, 20f, 20f);
            var tick = ResearchUiKit.Panel("Tick", box.transform, ResearchUiKit.Accent);
            ResearchUiKit.Place(tick.rectTransform, 4f, 4f, 12f, 12f);

            var label = ResearchUiKit.Label("Caption", button.transform, caption, 15f, ResearchUiKit.InkStrong, TextAlignmentOptions.Left);
            ResearchUiKit.Place(label.rectTransform, 32f, 3f, Content - 32f, 22f);

            button.onClick.AddListener(() => { set(!get()); RefreshAll(); });

            Action refresh = () =>
            {
                var on = get();
                tick.enabled = on;
                box.color = on ? ResearchUiKit.Hex("#DCE9FB") : ResearchUiKit.Hex("#F4F6F8");
            };
            refreshers.Add(refresh);
            refresh();
        }

        void RefreshAll()
        {
            for (var i = 0; i < refreshers.Count; i++)
                refreshers[i]();
        }
    }
}
