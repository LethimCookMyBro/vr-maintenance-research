using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TMUVR.MaintenanceResearch
{
    public sealed class TrainingResetController : MonoBehaviour
    {
        public event Action ResetPerformed;

        [SerializeField] Transform[] resetTargets;
        readonly Dictionary<Transform, Pose> startingPoses = new Dictionary<Transform, Pose>();
        XRSimpleInteractable interactable;

        void Awake()
        {
            foreach (var target in resetTargets)
                if (target != null)
                    startingPoses[target] = new Pose(target.position, target.rotation);
            interactable = GetComponent<XRSimpleInteractable>();
        }

        void OnEnable()
        {
            if (interactable == null)
                interactable = GetComponent<XRSimpleInteractable>();
            if (interactable != null)
                interactable.selectEntered.AddListener(OnSelected);
        }

        void OnDisable()
        {
            if (interactable != null)
                interactable.selectEntered.RemoveListener(OnSelected);
        }

        void OnMouseDown() => ResetTrainingObjects();
        void OnSelected(SelectEnterEventArgs args) => ResetTrainingObjects();

        public void ResetTrainingObjects()
        {
            var interactionManager = FindFirstObjectByType<XRInteractionManager>();
            foreach (var pair in startingPoses)
            {
                if (pair.Key == null)
                    continue;

                var xr = pair.Key.GetComponent<XRBaseInteractable>();
                if (interactionManager != null && xr != null)
                    for (var i = xr.interactorsSelecting.Count - 1; i >= 0; i--)
                        interactionManager.SelectExit(xr.interactorsSelecting[i], xr);

                var body = pair.Key.GetComponent<Rigidbody>();
                if (body != null && !body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                pair.Key.SetPositionAndRotation(pair.Value.position, pair.Value.rotation);
            }
            ResetPerformed?.Invoke();
        }
    }
}
