using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TMUVR.MaintenanceResearch
{
    public sealed class InformationSourceController : MonoBehaviour
    {
        [SerializeField] InformationSourceDefinition definition;
        [SerializeField] MaintenanceTaskController task;
        [SerializeField] GameObject contentPanel;
        [SerializeField] VideoPlayer videoPlayer;
        [SerializeField] int currentPage;
        XRBaseInteractable xrInteractable;
        bool isOpen;
        bool isVideoPlaying;
        readonly HashSet<string> activeHoverInteractors = new HashSet<string>();

        public InformationSourceDefinition Definition => definition;
        public bool IsOpen => isOpen;
        public int CurrentPage => currentPage;

        void Awake()
        {
            if (task == null)
                task = FindFirstObjectByType<MaintenanceTaskController>();
            if (videoPlayer == null)
                videoPlayer = GetComponent<VideoPlayer>();
            xrInteractable = GetComponent<XRBaseInteractable>();
            if (contentPanel != null)
                contentPanel.SetActive(false);
            RefreshLocalizedContent();
        }

        void OnEnable()
        {
            if (xrInteractable == null)
                xrInteractable = GetComponent<XRBaseInteractable>();
            if (xrInteractable != null)
            {
                xrInteractable.hoverEntered.AddListener(OnHoverEntered);
                xrInteractable.hoverExited.AddListener(OnHoverExited);
                xrInteractable.selectEntered.AddListener(OnSelectEntered);
            }
            if (videoPlayer != null)
                videoPlayer.loopPointReached += OnVideoCompleted;
        }

        void OnDisable()
        {
            if (xrInteractable != null)
            {
                xrInteractable.hoverEntered.RemoveListener(OnHoverEntered);
                xrInteractable.hoverExited.RemoveListener(OnHoverExited);
                xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
            }
            if (videoPlayer != null)
                videoPlayer.loopPointReached -= OnVideoCompleted;
        }

        void OnMouseEnter() => RecordHover("mouse");
        void OnMouseExit() => RecordHoverExit("mouse");
        void OnMouseDown() => Toggle();
        void OnHoverEntered(HoverEnterEventArgs args) => RecordHover(args.interactorObject?.transform == null ? "unknown" : args.interactorObject.transform.name);
        void OnHoverExited(HoverExitEventArgs args) => RecordHoverExit(args.interactorObject?.transform == null ? "unknown" : args.interactorObject.transform.name);
        void OnSelectEntered(SelectEnterEventArgs args) => Toggle();

        void RecordHover(string interactorId)
        {
            interactorId = string.IsNullOrEmpty(interactorId) ? "unknown" : interactorId;
            if (!activeHoverInteractors.Add(interactorId))
                return;
            task?.NotifyInformation(definition, ResearchEventType.InformationSourceHovered, "raw_hover_enter;interactor=" + interactorId);
        }

        void RecordHoverExit(string interactorId)
        {
            interactorId = string.IsNullOrEmpty(interactorId) ? "unknown" : interactorId;
            if (!activeHoverInteractors.Remove(interactorId))
                return;
            task?.NotifyInformation(definition, ResearchEventType.ControllerRayExited, "raw_hover_exit;interactor=" + interactorId);
        }

        public void Toggle()
        {
            if (isOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (isOpen || definition == null)
                return;
            isOpen = true;
            RefreshLocalizedContent();
            if (contentPanel != null)
            {
                var follow = contentPanel.GetComponent<ComfortFollowPanel>() ?? contentPanel.AddComponent<ComfortFollowPanel>();
                contentPanel.SetActive(true);
                follow.Configure(1.45f, -0.10f, 25f);
                follow.Recenter();
            }
            task?.NotifyInformation(definition, ResearchEventType.InformationSourceOpened, "opened");
        }

        public void Close()
        {
            if (!isOpen || definition == null)
                return;
            if (isVideoPlaying)
                VideoStop();
            isOpen = false;
            if (contentPanel != null)
                contentPanel.SetActive(false);
            task?.NotifyInformation(definition, ResearchEventType.InformationSourceClosed, "closed");
        }

        void RefreshLocalizedContent()
        {
            if (definition == null)
                return;

            var language = ResearchLanguage.English;
            var session = ResearchSessionManager.Instance;
            if (session != null)
                language = session.Configuration.language;

            var title = language == ResearchLanguage.Thai && !string.IsNullOrEmpty(definition.thaiTitle) ? definition.thaiTitle :
                language == ResearchLanguage.Japanese && !string.IsNullOrEmpty(definition.japaneseTitle) ? definition.japaneseTitle : definition.englishTitle;
            var body = language == ResearchLanguage.Thai && !string.IsNullOrEmpty(definition.thaiContent) ? definition.thaiContent :
                language == ResearchLanguage.Japanese && !string.IsNullOrEmpty(definition.japaneseContent) ? definition.japaneseContent : definition.englishContent;

            if (contentPanel != null)
            {
                foreach (var label in contentPanel.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (label.name == "GEN Title")
                        label.text = title;
                    else if (label.name == "GEN Body")
                        label.text = body;
                }
            }

            foreach (var label in GetComponentsInChildren<TMP_Text>(true))
                if (label.name == "GEN Caption")
                    label.text = SourceLabel(language);
        }

        string SourceLabel(ResearchLanguage language)
        {
            if (language == ResearchLanguage.Thai)
            {
                switch (definition.sourceType)
                {
                    case InformationSourceType.ProductManual: return "\u0E04\u0E39\u0E48\u0E21\u0E37\u0E2D\u0E1C\u0E25\u0E34\u0E15\u0E20\u0E31\u0E13\u0E11\u0E4C";
                    case InformationSourceType.TextTroubleshootingGuide: return "\u0E04\u0E39\u0E48\u0E21\u0E37\u0E2D\u0E41\u0E01\u0E49\u0E44\u0E02\u0E1B\u0E31\u0E0D\u0E2B\u0E32";
                    case InformationSourceType.InstructionalVideo: return "\u0E27\u0E34\u0E14\u0E35\u0E42\u0E2D\u0E41\u0E19\u0E30\u0E19\u0E33";
                    default: return "\u0E04\u0E39\u0E48\u0E21\u0E37\u0E2D\u0E20\u0E32\u0E1E\u0E02\u0E31\u0E49\u0E19\u0E15\u0E2D\u0E19";
                }
            }
            if (language == ResearchLanguage.Japanese)
            {
                switch (definition.sourceType)
                {
                    case InformationSourceType.ProductManual: return "\u88FD\u54C1\u30DE\u30CB\u30E5\u30A2\u30EB";
                    case InformationSourceType.TextTroubleshootingGuide: return "\u30C8\u30E9\u30D6\u30EB\u30B7\u30E5\u30FC\u30C6\u30A3\u30F3\u30B0\u30AC\u30A4\u30C9";
                    case InformationSourceType.InstructionalVideo: return "\u624B\u9806\u30D3\u30C7\u30AA";
                    default: return "\u624B\u9806\u30D3\u30B8\u30E5\u30A2\u30EB\u30AC\u30A4\u30C9";
                }
            }
            switch (definition.sourceType)
            {
                case InformationSourceType.ProductManual: return "Product Manual";
                case InformationSourceType.TextTroubleshootingGuide: return "Text Troubleshooting Guide";
                case InformationSourceType.InstructionalVideo: return "Instructional Video";
                default: return "Visual Step-by-Step Guide";
            }
        }

        public void ChangePage(int page)
        {
            if (!isOpen)
                return;
            currentPage = Mathf.Max(0, page);
            task?.NotifyInformation(definition, ResearchEventType.InformationPageChanged, "page=" + currentPage);
        }

        public void NextPage() => ChangePage(currentPage + 1);
        public void PreviousPage() => ChangePage(Mathf.Max(0, currentPage - 1));

        public void VideoPlay()
        {
            if (!isOpen || definition == null || videoPlayer == null || videoPlayer.clip == null)
                return;
            videoPlayer.Play();
            isVideoPlaying = true;
            task?.NotifyInformation(definition, ResearchEventType.VideoPlayed, "video_played");
        }

        public void VideoPause()
        {
            if (!isOpen || definition == null || videoPlayer == null || !isVideoPlaying)
                return;
            videoPlayer.Pause();
            isVideoPlaying = false;
            task?.NotifyInformation(definition, ResearchEventType.VideoPaused, "video_paused");
        }

        public void VideoStop()
        {
            if (!isOpen || definition == null || videoPlayer == null || !isVideoPlaying)
                return;
            videoPlayer.Stop();
            isVideoPlaying = false;
            task?.NotifyInformation(definition, ResearchEventType.VideoStopped, "video_stopped");
        }

        public void VideoSeek(float seconds)
        {
            if (!isOpen || definition == null || videoPlayer == null || videoPlayer.clip == null)
                return;
            var duration = (float)videoPlayer.clip.length;
            var targetSeconds = Mathf.Clamp((float)videoPlayer.time + seconds, 0f, Mathf.Max(0f, duration - 0.001f));
            videoPlayer.time = targetSeconds;
            task?.NotifyInformation(definition, ResearchEventType.VideoSeeked, "video_seconds=" + targetSeconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));
        }

        void OnVideoCompleted(VideoPlayer completedPlayer)
        {
            if (!isOpen || !isVideoPlaying || definition == null)
                return;
            isVideoPlaying = false;
            task?.NotifyInformation(definition, ResearchEventType.VideoCompleted, "video_completed");
        }
    }
}
