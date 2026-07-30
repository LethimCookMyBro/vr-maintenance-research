using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TMUVR.MaintenanceResearch
{
    public sealed class InformationSourceController : MonoBehaviour
    {
        [SerializeField] InformationSourceDefinition definition;
        [SerializeField] MaintenanceTaskController task;
        [SerializeField] GameObject contentPanel;
        [SerializeField] int currentPage;
        XRBaseInteractable xrInteractable;
        bool isOpen;

        public InformationSourceDefinition Definition => definition;

        void Awake()
        {
            if (task == null)
                task = FindFirstObjectByType<MaintenanceTaskController>();
            xrInteractable = GetComponent<XRBaseInteractable>();
            if (contentPanel != null)
                contentPanel.SetActive(false);
        }

        void OnEnable()
        {
            if (xrInteractable == null)
                xrInteractable = GetComponent<XRBaseInteractable>();
            if (xrInteractable == null)
                return;
            xrInteractable.hoverEntered.AddListener(OnHoverEntered);
            xrInteractable.selectEntered.AddListener(OnSelectEntered);
        }

        void OnDisable()
        {
            if (xrInteractable == null)
                return;
            xrInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
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

        public void VideoPlay() => task?.NotifyInformation(definition, ResearchEventType.VideoPlayed, "video_played");
        public void VideoPause() => task?.NotifyInformation(definition, ResearchEventType.VideoPaused, "video_paused");
        public void VideoStop() => task?.NotifyInformation(definition, ResearchEventType.VideoStopped, "video_stopped");
        public void VideoSeek(float seconds) => task?.NotifyInformation(definition, ResearchEventType.VideoSeeked, "video_seconds=" + seconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));
    }
}
