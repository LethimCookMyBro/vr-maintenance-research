using UnityEditor;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Runs the whole visual rebuild in dependency order. Materials first (the
    /// builders load them by name), then per-scene geometry, then lighting last
    /// so the reflection probe bakes against the finished scene.
    /// </summary>
    public static class ResearchVisualPipeline
    {
        [MenuItem("Tools/VR Maintenance Research/Visual Audit/REBUILD ALL", priority = 0)]
        public static void RebuildAll()
        {
            ResearchMaterialPalette.Apply();
            // Prefab first: the four scenes instance LabEnvironment, so the room has to
            // be finished before any scene is opened and normalised against it.
            LabIndustrialDressing.Build();
            LabLayoutBuilder.ApplyToAllScenes();
            ComputerWorkstationBuilder.Build();
            FanWorkstationBuilder.Build();
            TrainingSceneBuilder.Build();
            ResearcherSetupBuilder.Build();
            InformationDockBuilder.BuildAll();
            LabNoticeBoardBuilder.BuildAll();
            ParticipantHudBuilder.AttachAll();
            ResearchUiStyleBuilder.ApplyAll();
            ResearchLightingRig.ApplyToAllScenes();
            Debug.Log("[VisualPipeline] full rebuild complete");
        }
    }
}
