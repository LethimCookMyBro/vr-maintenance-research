using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Shared visual language for every research interface (60-30-10: neutral shell,
    /// navy/slate surfaces, blue accent, amber reserved for warnings).
    /// Builds plain uGUI + TextMeshPro objects so no prefab wiring can drift.
    /// </summary>
    public static class ResearchUiKit
    {
        public static readonly Color Shell = Hex("#EDEDEA");
        public static readonly Color Surface = Hex("#FFFFFF");
        public static readonly Color Navy = Hex("#1E2A3A");
        public static readonly Color Slate = Hex("#3C4655");
        public static readonly Color SlateSoft = Hex("#5A6675");
        public static readonly Color Accent = Hex("#2E7BE6");
        public static readonly Color AccentDim = Hex("#1F5AA8");
        public static readonly Color Warning = Hex("#F2A22C");
        public static readonly Color Danger = Hex("#C0392B");
        public static readonly Color InkStrong = Hex("#12181F");
        public static readonly Color InkMuted = Hex("#5A6675");
        public static readonly Color OnDark = Hex("#F2F5F8");
        public static readonly Color OnDarkMuted = Hex("#A8B3C0");
        public static readonly Color Line = Hex("#D3D7DC");

        public static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out var color);
            return color;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static Image Panel(string name, Transform parent, Color color)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>A filled block plus a 2 px accent rule along its top edge.</summary>
        public static Image Card(string name, Transform parent, Color body, Color rule)
        {
            var image = Panel(name, parent, body);
            var line = Panel("Rule", image.transform, rule);
            Stretch(line.rectTransform);
            line.rectTransform.anchorMin = new Vector2(0f, 1f);
            line.rectTransform.anchorMax = new Vector2(1f, 1f);
            line.rectTransform.pivot = new Vector2(0.5f, 1f);
            line.rectTransform.sizeDelta = new Vector2(0f, 3f);
            line.rectTransform.anchoredPosition = Vector2.zero;
            return image;
        }

        public static TextMeshProUGUI Label(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.TopLeft, FontStyles style = FontStyles.Normal)
        {
            var rect = Rect(name, parent);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = align;
            label.fontStyle = style;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            return label;
        }

        public static Button TextButton(string name, Transform parent, string text, float size, Color body, Color textColor, out TextMeshProUGUI label)
        {
            var image = Panel(name, parent, body);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.14f, 1.14f, 1.14f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.06f;
            button.colors = colors;

            label = Label("Label", image.transform, text, size, textColor, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            return button;
        }

        public static void Stretch(RectTransform rect, float padding = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        public static RectTransform Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
    }
}
