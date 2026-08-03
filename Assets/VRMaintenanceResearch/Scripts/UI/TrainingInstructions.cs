using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Fixed world-space training board. Replaces the IMGUI debug panel with a
    /// readable task title, controller instructions, live progress indicators and a
    /// single Continue button that unlocks once the training requirements are met.
    /// Progress is observed from existing interactor/interactable state only: no
    /// research event, log row or task state is produced by this component.
    /// </summary>
    public sealed class TrainingInstructions : MonoBehaviour
    {
        [SerializeField] Vector3 boardPosition = new Vector3(-0.70f, 1.58f, 1.35f);
        [SerializeField] Vector2 boardSize = new Vector2(1.10f, 0.62f);
        [SerializeField] Sprite satisfiedIcon;
        [SerializeField] Sprite outstandingIcon;

        readonly List<XRGrabInteractable> grabbables = new List<XRGrabInteractable>();
        XRSocketInteractor socket;
        XRGrabInteractable knob;
        InformationSourceController informationSource;
        TrainingResetController resetControl;

        bool grabSatisfied;
        bool socketSatisfied;
        bool knobSatisfied;
        bool informationSatisfied;
        Quaternion knobStartRotation;

        Requirement grabRequirement;
        Requirement socketRequirement;
        Requirement knobRequirement;
        Requirement informationRequirement;
        Button continueButton;
        TextMeshProUGUI continueLabel;
        Image continueBody;
        TextMeshProUGUI hintLabel;

        struct Requirement
        {
            public Image Marker;
            public TextMeshProUGUI Check;
            public TextMeshProUGUI Text;
        }

        void Start()
        {
            grabbables.AddRange(FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None));
            for (var i = 0; i < grabbables.Count; i++)
                if (grabbables[i] != null && grabbables[i].name == "Training Cylinder")
                    knob = grabbables[i];
            if (knob != null)
                knobStartRotation = knob.transform.localRotation;
            socket = FindFirstObjectByType<XRSocketInteractor>();
            informationSource = FindFirstObjectByType<InformationSourceController>();
            resetControl = FindFirstObjectByType<TrainingResetController>();
            if (resetControl != null)
                resetControl.ResetPerformed += OnTrainingReset;
            Build();
            Refresh();
        }

        void OnDestroy()
        {
            if (resetControl != null)
                resetControl.ResetPerformed -= OnTrainingReset;
        }

        void Update()
        {
            if (!grabSatisfied)
            {
                for (var i = 0; i < grabbables.Count; i++)
                {
                    if (grabbables[i] != null && grabbables[i].isSelected)
                    {
                        grabSatisfied = true;
                        break;
                    }
                }
            }

            if (!socketSatisfied && socket != null && socket.hasSelection)
                socketSatisfied = true;

            if (!knobSatisfied && knob != null && knob.isSelected &&
                Quaternion.Angle(knobStartRotation, knob.transform.localRotation) >= 25f)
                knobSatisfied = true;

            if (!informationSatisfied && informationSource != null && informationSource.IsOpen)
                informationSatisfied = true;

            Refresh();
        }

        void Build()
        {
            var canvasObject = new GameObject("Training Board", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);
            // Identity rotation faces the participant, who stands on the -Z side looking towards +Z.
            canvasObject.transform.SetPositionAndRotation(boardPosition, Quaternion.identity);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            var rect = (RectTransform)canvasObject.transform;
            rect.sizeDelta = new Vector2(1280f, 1280f * (boardSize.y / boardSize.x));
            rect.localScale = Vector3.one * (boardSize.x / 1280f);
            canvasObject.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 3f;
            canvasObject.AddComponent<TrackedDeviceGraphicRaycaster>();

            var background = ResearchUiKit.Panel("Background", canvasObject.transform, ResearchUiKit.Navy);
            ResearchUiKit.Stretch(background.rectTransform);

            var accent = ResearchUiKit.Panel("Accent", background.transform, ResearchUiKit.Accent);
            accent.rectTransform.anchorMin = new Vector2(0f, 1f);
            accent.rectTransform.anchorMax = new Vector2(1f, 1f);
            accent.rectTransform.pivot = new Vector2(0.5f, 1f);
            accent.rectTransform.sizeDelta = new Vector2(0f, 8f);
            accent.rectTransform.anchoredPosition = Vector2.zero;

            var title = ResearchUiKit.Label("Title", background.transform, "Neutral XR Training", 46f, ResearchUiKit.OnDark, TextAlignmentOptions.Left, FontStyles.Bold);
            ResearchUiKit.Place(title.rectTransform, 44f, 14f, 900f, 54f);

            var controls = ResearchUiKit.Label("Controls", background.transform,
                "<b>Look</b> headset   <b>Point</b> controller ray   <b>Grip</b> grab and release   <b>Trigger</b> select",
                22f, ResearchUiKit.OnDarkMuted, TextAlignmentOptions.Left);
            ResearchUiKit.Place(controls.rectTransform, 44f, 66f, 1000f, 28f);

            var subtitle = ResearchUiKit.Label("Subtitle", background.transform,
                "This scene contains no Computer or Fan maintenance solution.",
                22f, ResearchUiKit.OnDarkMuted, TextAlignmentOptions.Left);
            ResearchUiKit.Place(subtitle.rectTransform, 44f, 94f, 1000f, 28f);

            var column = ResearchUiKit.Rect("Requirements", background.transform);
            ResearchUiKit.Place(column, 44f, 132f, 880f, 156f);

            grabRequirement = RequirementRow(column, 0f, "Pick up a training object");
            socketRequirement = RequirementRow(column, 36f, "Place an object in the comparison tray");
            knobRequirement = RequirementRow(column, 72f, "Turn the training dial");
            informationRequirement = RequirementRow(column, 108f, "Open the neutral information panel");

            hintLabel = ResearchUiKit.Label("Reset Hint", background.transform,
                "Complete all four skills. <b>RESET</b> returns the training objects.",
                21f, ResearchUiKit.OnDarkMuted, TextAlignmentOptions.Left);
            ResearchUiKit.Place(hintLabel.rectTransform, 44f, 292f, 880f, 28f);

            continueButton = ResearchUiKit.TextButton("Continue", background.transform, "Continue", 28f, ResearchUiKit.SlateSoft, ResearchUiKit.OnDarkMuted, out continueLabel);
            continueBody = continueButton.image;
            ResearchUiKit.Place(continueBody.rectTransform, 960f, 266f, 276f, 62f);
            continueButton.onClick.AddListener(Continue);
        }

        Requirement RequirementRow(RectTransform parent, float y, string caption)
        {
            var marker = ResearchUiKit.Panel("Marker", parent, ResearchUiKit.SlateSoft);
            ResearchUiKit.Place(marker.rectTransform, 0f, y, 28f, 28f);
            marker.preserveAspect = true;

            var check = ResearchUiKit.Label("Check", marker.transform, string.Empty, 24f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            ResearchUiKit.Stretch(check.rectTransform);

            var text = ResearchUiKit.Label("Caption", parent, caption, 24f, ResearchUiKit.OnDarkMuted, TextAlignmentOptions.Left);
            ResearchUiKit.Place(text.rectTransform, 34f, y, 820f, 28f);

            return new Requirement { Marker = marker, Check = check, Text = text };
        }

        void Refresh()
        {
            Apply(grabRequirement, grabSatisfied);
            Apply(socketRequirement, socketSatisfied);
            Apply(knobRequirement, knobSatisfied);
            Apply(informationRequirement, informationSatisfied);

            var ready = grabSatisfied && socketSatisfied && knobSatisfied && informationSatisfied;
            if (continueButton == null)
                return;

            continueButton.interactable = ready;
            continueButton.gameObject.SetActive(ready);
            continueBody.color = ready ? ResearchUiKit.Accent : ResearchUiKit.SlateSoft;
            continueLabel.color = ready ? Color.white : ResearchUiKit.OnDarkMuted;
            continueLabel.text = "Continue";
            hintLabel.text = ready ? "All four skills complete." : "Complete all four skills. <b>RESET</b> returns the training objects.";
        }

        void Apply(Requirement requirement, bool satisfied)
        {
            if (requirement.Marker == null)
                return;

            var sprite = satisfied ? satisfiedIcon : outstandingIcon;
            requirement.Marker.sprite = sprite;
            requirement.Marker.color = satisfied ? ResearchUiKit.Accent : ResearchUiKit.SlateSoft;
            if (requirement.Check != null)
                requirement.Check.text = satisfied ? "✓" : string.Empty;
            requirement.Text.color = satisfied ? ResearchUiKit.OnDark : ResearchUiKit.OnDarkMuted;
        }

        void OnTrainingReset()
        {
            if (informationSource != null && informationSource.IsOpen)
                informationSource.Close();
            grabSatisfied = false;
            socketSatisfied = false;
            knobSatisfied = false;
            informationSatisfied = false;
            if (knob != null)
                knobStartRotation = knob.transform.localRotation;
            Refresh();
        }

        void Continue()
        {
            var session = ResearchSessionManager.Instance;
            FindFirstObjectByType<TrainingMaintenanceTask>()?.CompleteTraining();
            if (session != null)
                session.StartFirstTaskAfterTraining();
        }
    }
}
