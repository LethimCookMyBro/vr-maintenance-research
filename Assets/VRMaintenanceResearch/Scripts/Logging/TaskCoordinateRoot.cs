using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    [DisallowMultipleComponent]
    public sealed class TaskCoordinateRoot : MonoBehaviour
    {
        public const string CoordinateSpaceId = "task-local-v2";

        public Vector3 InverseTransformPoint(Vector3 worldPosition) => transform.InverseTransformPoint(worldPosition);
        public Quaternion InverseTransformRotation(Quaternion worldRotation) => Quaternion.Inverse(transform.rotation) * worldRotation;
    }
}
