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

        // --- glass surfaces -------------------------------------------------
        //
        // The participant-facing panels moved to the industrial reference look: a
        // dark, slightly transparent slab with a rounded edge, rather than a flat
        // opaque rectangle. They live here rather than in each board so that the
        // status board, the training board and the heads-up display cannot drift
        // apart. The researcher setup screen deliberately keeps the light Shell /
        // Surface tokens above: it is a desktop form, not a panel in the room.
        public static readonly Color Glass = Hex("#0D141FDB");        // ~86 % opaque
        public static readonly Color GlassRaised = Hex("#18233468");  // inner card over Glass
        public static readonly Color GlassEdge = Hex("#42536DD9");
        public static readonly Color Ok = Hex("#3FB27F");

        /// <summary>8-digit hex is accepted, so a token can carry its own alpha.</summary>
        public static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out var color);
            return color;
        }

        // ponytail: one 48 px sprite generated once beats shipping a 9-slice PNG
        // plus its meta and import settings. Sliced, so the corner radius is the
        // same on a 1 040 px board and a 200 px chip.
        const int k_RoundedSize = 48;
        const int k_RoundedRadius = 14;
        static Sprite roundedSprite;

        public static Sprite RoundedSprite
        {
            get
            {
                if (roundedSprite == null)
                    roundedSprite = BuildRoundedSprite();
                return roundedSprite;
            }
        }

        static Sprite BuildRoundedSprite()
        {
            var texture = new Texture2D(k_RoundedSize, k_RoundedSize, TextureFormat.RGBA32, false)
            {
                name = "Research Rounded Panel",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            var pixels = new Color32[k_RoundedSize * k_RoundedSize];
            for (var y = 0; y < k_RoundedSize; y++)
            for (var x = 0; x < k_RoundedSize; x++)
            {
                // Distance past the corner arc, in pixels; one pixel of feather so
                // the edge is not stepped at the sizes these panels render at.
                var dx = Mathf.Max(0f, Mathf.Max(k_RoundedRadius - 0.5f - x, x - (k_RoundedSize - k_RoundedRadius - 0.5f)));
                var dy = Mathf.Max(0f, Mathf.Max(k_RoundedRadius - 0.5f - y, y - (k_RoundedSize - k_RoundedRadius - 0.5f)));
                var outside = Mathf.Sqrt(dx * dx + dy * dy) - k_RoundedRadius;
                var alpha = Mathf.Clamp01(0.5f - outside);
                pixels[y * k_RoundedSize + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            var border = Vector4.one * (k_RoundedRadius + 2);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, k_RoundedSize, k_RoundedSize), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            sprite.name = "Research Rounded Panel";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        /// <summary>A rounded slab. <paramref name="radius"/> is in canvas units.</summary>
        public static Image Rounded(string name, Transform parent, Color color, float radius = 16f)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = RoundedSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = (k_RoundedRadius + 2f) / Mathf.Max(1f, radius);
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>
        /// The standard participant panel: a rounded edge ring with the glass body
        /// inset inside it. Returns the body, which is what callers parent into.
        /// </summary>
        public static RectTransform GlassPanel(string name, Transform parent, float radius = 16f, float edge = 2f)
        {
            var ring = Rounded(name, parent, GlassEdge, radius);
            var body = Rounded("Glass", ring.transform, Glass, Mathf.Max(1f, radius - edge));
            Stretch(body.rectTransform, edge);
            return ring.rectTransform;
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
