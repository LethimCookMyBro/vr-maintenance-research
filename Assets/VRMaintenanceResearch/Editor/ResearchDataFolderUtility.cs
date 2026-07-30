using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    public static class ResearchDataFolderUtility
    {
        [MenuItem("VR Maintenance Research/Open Data Folder")]
        static void OpenDataFolder()
        {
            var path = Path.Combine(Application.persistentDataPath, "VRMaintenanceResearchData");
            Directory.CreateDirectory(path);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}
