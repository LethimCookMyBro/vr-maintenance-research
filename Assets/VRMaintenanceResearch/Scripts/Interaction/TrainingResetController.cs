using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TMUVR.MaintenanceResearch
{
    public sealed class TrainingResetController : MonoBehaviour
    {
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
            foreach (var pair in startingPoses)
            {
                if (pair.Key == null)
                    continue;
                var body = pair.Key.GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                pair.Key.SetPositionAndRotation(pair.Value.position, pair.Value.rotation);
            }
        }
    }
}
