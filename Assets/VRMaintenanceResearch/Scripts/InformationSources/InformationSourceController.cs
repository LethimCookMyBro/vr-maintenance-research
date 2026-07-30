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
        }

        void OnEnable()
        {
            if (xrInteractable == null)
                xrInteractable = GetComponent<XRBaseInteractable>();
            if (xrInteractable != null)
            {
                xrInteractable.hoverEntered.AddListener(OnHoverEntered);
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
                xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
            }
            if (videoPlayer != null)
                videoPlayer.loopPointReached -= OnVideoCompleted;
        }

        void OnMouseEnter() => task?.NotifyInformation(definition, ResearchEventType.InformationSourceHovered, "mouse_hover");
        void OnMouseDown() => Toggle();
        void OnHoverEntered(HoverEnterEventArgs args) => task?.NotifyInformation(definition, ResearchEventType.InformationSourceHovered, "controller_ray_hover");
        void OnSelectEntered(SelectEnterEventArgs args) => Toggle();

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
            if (contentPanel != null)
                contentPanel.SetActive(true);
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
