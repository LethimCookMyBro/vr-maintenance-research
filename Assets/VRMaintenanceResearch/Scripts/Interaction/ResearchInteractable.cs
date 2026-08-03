using System.Collections.Generic;
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

        [Header("Affordance")]
        [Tooltip("Tints this object's renderers while an interactor is on it. The tint is " +
                 "identical for every interactable, so it signals 'this responds' without " +
                 "signalling 'this is the answer'.")]
        [SerializeField] bool highlightOnFocus = true;

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
            xrInteractable.selectExited.AddListener(OnSelectExited);
        }

        void OnDisable()
        {
            focusCount = 0;
            SetFocusTint(false);
            if (xrInteractable == null)
                return;
            xrInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            xrInteractable.hoverExited.RemoveListener(OnHoverExited);
            xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
            xrInteractable.selectExited.RemoveListener(OnSelectExited);
        }

        void OnMouseEnter()
        {
            task?.RecordHover(this, "mouse");
            Focus(+1);
        }

        void OnMouseExit()
        {
            task?.RecordHoverExit(this, "mouse");
            Focus(-1);
        }

        void OnMouseDown() => task?.RecordInteraction(this);

        void OnHoverEntered(HoverEnterEventArgs args)
        {
            task?.RecordHover(this, args.interactorObject?.transform == null ? "unknown" : args.interactorObject.transform.name);
            Focus(+1);
        }

        void OnHoverExited(HoverExitEventArgs args)
        {
            task?.RecordHoverExit(this, args.interactorObject?.transform == null ? "unknown" : args.interactorObject.transform.name);
            Focus(-1);
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            task?.RecordInteraction(this);
            Focus(+1);
        }

        void OnSelectExited(SelectExitEventArgs args) => Focus(-1);

        // --- focus tint ------------------------------------------------------
        //
        // Participants could not tell which objects respond to the controller, so
        // every interactable now lifts its own colour while focused. It is drawn
        // with a MaterialPropertyBlock: no material instances are created, shared
        // palette materials are never written to, and nothing here touches the
        // stable id, the task events or the collider.

        static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly Color k_FocusTint = new Color(0.55f, 0.80f, 1f);   // one tint for all, so it never marks the answer
        const float k_FocusStrength = 0.30f;

        readonly List<Renderer> tinted = new List<Renderer>();
        readonly List<Color> restColors = new List<Color>();
        MaterialPropertyBlock block;
        int focusCount;

        void Start() => CacheTintTargets();

        void CacheTintTargets()
        {
            block = new MaterialPropertyBlock();
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                // Stop at a nested interactable. The case owns the PSU, the board and
                // the fan as children, so tinting the whole subtree would light up six
                // separately grabbable parts whenever the ray touched the case.
                if (renderer.GetComponentInParent<ResearchInteractable>() != this)
                    continue;

                // Skip text and anything on a shader without a base colour.
                var material = renderer.sharedMaterial;
                if (material == null || !material.HasProperty(k_BaseColor) || renderer.GetComponent<TMPro.TMP_Text>() != null)
                    continue;
                tinted.Add(renderer);
                restColors.Add(material.GetColor(k_BaseColor));
            }
        }

        /// <summary>Reference-counted so two interactors cannot leave the tint stuck on.</summary>
        void Focus(int delta)
        {
            if (!highlightOnFocus)
                return;
            var was = focusCount > 0;
            focusCount = Mathf.Max(0, focusCount + delta);
            if (was != focusCount > 0)
                SetFocusTint(focusCount > 0);
        }

        void SetFocusTint(bool on)
        {
            if (block == null)
                return;
            for (var i = 0; i < tinted.Count; i++)
            {
                var renderer = tinted[i];
                if (renderer == null)
                    continue;
                renderer.GetPropertyBlock(block);
                block.SetColor(k_BaseColor, on ? Color.Lerp(restColors[i], k_FocusTint, k_FocusStrength) : restColors[i]);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
