using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TMUVR.MaintenanceResearch
{
    public enum ResearchInteractionKind { Device, Component, Tool, RepairAction, DeviceTest }

    [RequireComponent(typeof(Collider))]
    public sealed class ResearchInteractable : MonoBehaviour
    {
        [SerializeField] string stableObjectId = "object.development";
        [SerializeField] string objectCategory = "component";
        [SerializeField] ResearchInteractionKind kind = ResearchInteractionKind.Component;
        [SerializeField] bool isCorrect = true;
        [SerializeField] MaintenanceTaskController task;
        XRBaseInteractable xrInteractable;

        public string StableObjectId => stableObjectId;
        public string ObjectCategory => objectCategory;
        public ResearchInteractionKind Kind => kind;
        public bool IsCorrect => isCorrect;

        void Awake()
        {
            if (task == null)
                task = FindFirstObjectByType<MaintenanceTaskController>();
            xrInteractable = GetComponent<XRBaseInteractable>();
        }

        void OnEnable()
        {
            if (xrInteractable == null)
                xrInteractable = GetComponent<XRBaseInteractable>();
            if (xrInteractable == null)
                return;
            xrInteractable.hoverEntered.AddListener(OnHoverEntered);
            xrInteractable.hoverExited.AddListener(OnHoverExited);
            xrInteractable.selectEntered.AddListener(OnSelectEntered);
        }

        void OnDisable()
        {
            if (xrInteractable == null)
                return;
            xrInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            xrInteractable.hoverExited.RemoveListener(OnHoverExited);
            xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }

        void OnMouseEnter() => task?.RecordHover(this, "mouse");
        void OnMouseExit() => task?.RecordHoverExit(this, "mouse");
        void OnMouseDown() => task?.RecordInteraction(this);
        void OnHoverEntered(HoverEnterEventArgs args) => task?.RecordHover(this, args.interactorObject?.transform == null ? "unknown" : args.interactorObject.transform.name);
        void OnHoverExited(HoverExitEventArgs args) => task?.RecordHoverExit(this, args.interactorObject?.transform == null ? "unknown" : args.interactorObject.transform.name);
        void OnSelectEntered(SelectEnterEventArgs args) => task?.RecordInteraction(this);
    }
}
