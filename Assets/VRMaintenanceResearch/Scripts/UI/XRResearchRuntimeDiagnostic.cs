using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Emits one runtime check for the participant UI input path.
    /// </summary>
    public sealed class XRResearchRuntimeDiagnostic : MonoBehaviour
    {
        void Start()
        {
            Report();
        }

        void Report()
        {
            var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            var xrModules = FindObjectsByType<XRUIInputModule>(FindObjectsSortMode.None).Length;
            var baseModules = FindObjectsByType<BaseInputModule>(FindObjectsSortMode.None).Length;
            var legacyModules = baseModules - xrModules;
            var trackedRaycasters = FindObjectsByType<TrackedDeviceGraphicRaycaster>(FindObjectsSortMode.None).Length;
            var canvasRaycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None).Length;
            Debug.Log($"[VRMaintenanceResearch] XR UI diagnostic: eventSystems={eventSystems}; xrModules={xrModules}; legacyModules={legacyModules}; trackedRaycasters={trackedRaycasters}; graphicRaycasters={canvasRaycasters}.", this);
            if (eventSystems != 1 || xrModules != 1 || legacyModules != 0)
                Debug.LogError("[VRMaintenanceResearch] XR UI diagnostic failed: expected one EventSystem with one XRUIInputModule and no competing input module.", this);
        }
    }
}
