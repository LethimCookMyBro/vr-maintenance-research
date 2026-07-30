using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    [CreateAssetMenu(menuName = "VR Maintenance Research/Information Source Definition", fileName = "InformationSourceDefinition")]
    public sealed class InformationSourceDefinition : ScriptableObject
    {
        public string sourceId = "source.product-manual";
        public InformationSourceType sourceType;
        public string sourceSlot = "A";
        [TextArea] public string englishTitle = "Development placeholder";
        [TextArea(3, 12)] public string englishContent = "Development content pending advisor review.";
        [TextArea] public string thaiTitle = "";
        [TextArea] public string japaneseTitle = "";
        [TextArea(3, 12)] public string thaiContent = "";
        [TextArea(3, 12)] public string japaneseContent = "";
        public bool developmentPlaceholder = true;
    }
}
