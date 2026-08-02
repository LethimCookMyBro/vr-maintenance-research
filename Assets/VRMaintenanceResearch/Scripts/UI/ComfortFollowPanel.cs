using UnityEngine;
using UnityEngine.EventSystems;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Keeps an opened research panel in a comfortable yaw-only field of view.
    /// Pointer and drag callbacks pause the follow while a control is active.
    /// </summary>
    public sealed class ComfortFollowPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] Transform trackingCamera;
        [SerializeField, Min(0.5f)] float distance = 1.4f;
        [SerializeField] float verticalOffset = -0.12f;
        [SerializeField, Range(5f, 60f)] float recenterAngle = 25f;
        [SerializeField, Min(1f)] float followSharpness = 10f;

        Vector3 targetPosition;
        Quaternion targetRotation;
        bool interacting;

        void OnEnable()
        {
            ResolveCamera();
            Recenter();
        }

        void LateUpdate()
        {
            ResolveCamera();
            if (trackingCamera == null || interacting)
                return;

            var toPanel = transform.position - trackingCamera.position;
            toPanel.y = 0f;
            var viewForward = trackingCamera.forward;
            viewForward.y = 0f;
            if (toPanel.sqrMagnitude > 0.001f && viewForward.sqrMagnitude > 0.001f && Vector3.Angle(viewForward, toPanel) > recenterAngle)
                SetTargetFromCamera();

            var t = 1f - Mathf.Exp(-followSharpness * Time.unscaledDeltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
        }

        public void Configure(float panelDistance, float panelVerticalOffset, float angleThreshold)
        {
            distance = Mathf.Max(0.5f, panelDistance);
            verticalOffset = panelVerticalOffset;
            recenterAngle = Mathf.Clamp(angleThreshold, 5f, 60f);
        }

        public void Recenter()
        {
            ResolveCamera();
            if (trackingCamera == null)
                return;
            SetTargetFromCamera();
            transform.SetPositionAndRotation(targetPosition, targetRotation);
        }

        public void OnPointerDown(PointerEventData eventData) => interacting = true;
        public void OnPointerUp(PointerEventData eventData) => interacting = false;
        public void OnBeginDrag(PointerEventData eventData) => interacting = true;
        public void OnEndDrag(PointerEventData eventData) => interacting = false;

        void ResolveCamera()
        {
            if (trackingCamera == null && Camera.main != null)
                trackingCamera = Camera.main.transform;
        }

        void SetTargetFromCamera()
        {
            var forward = trackingCamera.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            forward.Normalize();
            targetPosition = trackingCamera.position + forward * distance + Vector3.up * verticalOffset;
            targetRotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }
}
