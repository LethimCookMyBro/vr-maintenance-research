using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TMUVR.MaintenanceResearch
{
    // Keep existing values stable: scenes serialize these enum values directly.
    public enum InformationSourceControlAction { PreviousPage, NextPage, VideoPlay, VideoPause, VideoStop, VideoSeekForward, Close, VideoRestart }

    [RequireComponent(typeof(Collider))]
    public sealed class InformationSourceControlButton : MonoBehaviour
    {
        [SerializeField] InformationSourceController source;
        [SerializeField] InformationSourceControlAction action;
        [SerializeField, Min(0f)] float seekSeconds = 10f;
        XRBaseInteractable xrInteractable;

        void Awake() => xrInteractable = GetComponent<XRBaseInteractable>();

        void OnEnable()
        {
            if (xrInteractable == null)
                xrInteractable = GetComponent<XRBaseInteractable>();
            if (xrInteractable != null)
                xrInteractable.selectEntered.AddListener(OnSelectEntered);
        }

        void OnDisable()
        {
            if (xrInteractable != null)
                xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }

        void OnMouseDown() => Activate();
        void OnSelectEntered(SelectEnterEventArgs args) => Activate();

        void Activate()
        {
            if (source == null)
                return;
            switch (action)
            {
                case InformationSourceControlAction.PreviousPage: source.PreviousPage(); break;
                case InformationSourceControlAction.NextPage: source.NextPage(); break;
                case InformationSourceControlAction.VideoPlay: source.VideoPlay(); break;
                case InformationSourceControlAction.VideoPause: source.VideoPause(); break;
                case InformationSourceControlAction.VideoStop: source.VideoStop(); break;
                case InformationSourceControlAction.VideoSeekForward: source.VideoSeek(seekSeconds); break;
                case InformationSourceControlAction.Close: source.Close(); break;
                case InformationSourceControlAction.VideoRestart: source.VideoRestart(); break;
            }
        }
    }
}
