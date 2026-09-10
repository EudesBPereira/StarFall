using Starfall.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Starfall.EditorTools
{
    /// <summary>Helpers to build uGUI + TextMeshPro hierarchies from code (used by the scene generator).</summary>
    public static class UiBuilder
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        public static readonly Color Accent = new Color(0.35f, 0.9f, 1f);
        public static readonly Color Accent2 = new Color(1f, 0.35f, 0.75f);
        public static readonly Color PanelColor = new Color(0.02f, 0.03f, 0.08f, 0.9f);
        public static readonly Color ButtonColor = new Color(0.08f, 0.16f, 0.3f, 0.95f);
        public static readonly Color ButtonHighlight = new Color(0.16f, 0.4f, 0.65f, 1f);
        public static readonly Color TextColor = new Color(0.92f, 0.96f, 1f);

        public static Sprite PanelSprite;
        /// <summary>Plain white sprite for bar fills and highlights (never the decorated panel, which is dark).</summary>
        public static Sprite FillSprite;
        public static Material HologramMaterial;

        /// <summary>Default TMP font, or null when the TMP essentials are not imported (never throws).</summary>
        private static TMP_FontAsset _finalFont;
        private static bool _finalFontChecked;

        /// <summary>Final font from Art/Final/Fonts when delivered, else the TMP default (null when TMP essentials are missing).</summary>
        public static TMP_FontAsset DefaultFont
        {
            get
            {
                if (!_finalFontChecked) { _finalFont = FinalAssets.TryLoadFont(); _finalFontChecked = true; }
                if (_finalFont != null) return _finalFont;
                var settings = Resources.Load<TMP_Settings>("TMP Settings");
                return settings != null ? TMP_Settings.defaultFontAsset : null;
            }
        }

        public static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static GameObject CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            return go;
        }

        public static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform Stretch(RectTransform rt, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        public static Image CreateImage(Transform parent, string name, Color color, Sprite sprite = null, bool raycast = false)
        {
            var rt = CreateRect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            img.raycastTarget = raycast;
            if (sprite != null && sprite.border != Vector4.zero) img.type = Image.Type.Sliced;
            return img;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center, FontStyles style = FontStyles.Normal)
        {
            var rt = CreateRect(parent, name);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            var font = DefaultFont;
            if (font != null) tmp.font = font;
            // Static labels translate themselves; dynamic ones are left alone because they never equal a table key.
            if (!string.IsNullOrEmpty(text)) rt.gameObject.AddComponent<LocalizedText>().key = text;
            return tmp;
        }

        public static Button CreateButton(Transform parent, string name, string label, Vector2 size, float fontSize = 40f, Color? color = null)
        {
            var rt = CreateRect(parent, name);
            rt.sizeDelta = size;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color ?? ButtonColor;
            img.sprite = PanelSprite;
            img.type = Image.Type.Sliced;
            img.raycastTarget = true;
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.6f, 1.8f, 2f, 1f);
            colors.selectedColor = new Color(1.6f, 1.8f, 2f, 1f);
            colors.pressedColor = new Color(0.7f, 0.8f, 1f, 1f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            rt.gameObject.AddComponent<UiButtonSfx>();
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = size.x;
            le.preferredHeight = size.y;

            var text = CreateText(rt, "Label", label, fontSize, TextColor, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(text.rectTransform, 12f, 12f, 4f, 4f);
            // Translated labels can be longer than the English source: shrink instead of wrapping inside the button.
            text.enableAutoSizing = true;
            text.fontSizeMax = fontSize;
            text.fontSizeMin = Mathf.Max(18f, fontSize * 0.55f);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return button;
        }

        public static Image CreateBar(Transform parent, string name, Vector2 size, Color background, Color fill, out Image fillImage)
        {
            var bg = CreateImage(parent, name, background, PanelSprite);
            bg.rectTransform.sizeDelta = size;
            bg.type = Image.Type.Sliced;
            fillImage = CreateImage(bg.transform, "Fill", fill, FillSprite != null ? FillSprite : PanelSprite);
            Stretch(fillImage.rectTransform, 3f, 3f, 3f, 3f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;
            return bg;
        }

        public static Slider CreateSlider(Transform parent, string name, string label, float min, float max, out TextMeshProUGUI labelText)
        {
            var row = CreateRect(parent, name);
            row.sizeDelta = new Vector2(760f, 110f);
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 110f;
            le.preferredWidth = 760f;

            labelText = CreateText(row, "Label", label, 32f, TextColor, TextAlignmentOptions.Left);
            Place(labelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(760f, 44f));

            var sliderRt = CreateRect(row, "Slider");
            Place(sliderRt, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 4f), new Vector2(760f, 50f));
            var slider = sliderRt.gameObject.AddComponent<Slider>();

            var bg = CreateImage(sliderRt, "Background", new Color(0.1f, 0.12f, 0.2f, 1f), PanelSprite, true);
            Stretch(bg.rectTransform, 0f, 0f, 15f, 15f);

            var fillArea = CreateRect(sliderRt, "Fill Area");
            Stretch(fillArea, 10f, 10f, 15f, 15f);
            var fill = CreateImage(fillArea, "Fill", Accent, FillSprite != null ? FillSprite : PanelSprite);
            Stretch(fill.rectTransform);

            var handleArea = CreateRect(sliderRt, "Handle Slide Area");
            Stretch(handleArea, 15f, 15f, 0f, 0f);
            var handle = CreateImage(handleArea, "Handle", Color.white, PanelSprite, true);
            handle.rectTransform.sizeDelta = new Vector2(34f, 0f);
            Stretch(handle.rectTransform);
            handle.rectTransform.sizeDelta = new Vector2(34f, 0f);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = max;
            sliderRt.gameObject.AddComponent<UiButtonSfx>();
            return slider;
        }

        public static Toggle CreateToggle(Transform parent, string name, string label)
        {
            var row = CreateRect(parent, name);
            row.sizeDelta = new Vector2(760f, 70f);
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 70f;
            le.preferredWidth = 760f;
            var toggle = row.gameObject.AddComponent<Toggle>();

            var bg = CreateImage(row, "Background", new Color(0.1f, 0.12f, 0.2f, 1f), PanelSprite, true);
            Place(bg.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(56f, 56f));
            var check = CreateImage(bg.transform, "Checkmark", Accent, FillSprite != null ? FillSprite : PanelSprite);
            Stretch(check.rectTransform, 10f, 10f, 10f, 10f);

            var text = CreateText(row, "Label", label, 32f, TextColor, TextAlignmentOptions.Left);
            Place(text.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(76f, 0f), new Vector2(680f, 60f));

            toggle.targetGraphic = bg;
            toggle.graphic = check;
            toggle.isOn = true;
            row.gameObject.AddComponent<UiButtonSfx>();
            return toggle;
        }

        /// <summary>Full-screen dim panel with a centered vertical column.</summary>
        public static RectTransform CreatePanelRoot(Transform canvas, string name, out RectTransform column, float columnWidth = 820f)
        {
            var root = CreateImage(canvas, name, PanelColor, null, true);
            Stretch(root.rectTransform);
            var col = CreateRect(root.transform, "Column");
            Place(col, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(columnWidth, 0f));
            var layout = col.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 22f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(20, 20, 20, 20);
            var fitter = col.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            column = col;
            return root.rectTransform;
        }

        public static TextMeshProUGUI AddColumnText(RectTransform column, string name, string text, float size, Color color, float height, FontStyles style = FontStyles.Normal)
        {
            var t = CreateText(column, name, text, size, color, TextAlignmentOptions.Center, style);
            var le = t.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.preferredWidth = 780f;
            return t;
        }

        /// <summary>Selectable row: icon (optional), title, detail line and a right-aligned cost label.</summary>
        public static ListRow CreateListRow(Transform parent, string name, Vector2 size, bool withIcon)
        {
            var button = CreateButton(parent, name, "", size, 24f, new Color(0.06f, 0.1f, 0.2f, 0.92f));
            var rt = button.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            var row = button.gameObject.AddComponent<ListRow>();
            row.button = button;
            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) Object.DestroyImmediate(label.gameObject);

            var highlight = CreateImage(rt, "Highlight", new Color(0.35f, 0.9f, 1f, 0.25f), FillSprite != null ? FillSprite : PanelSprite);
            highlight.type = Image.Type.Sliced;
            Stretch(highlight.rectTransform, 2f, 2f, 2f, 2f);
            highlight.enabled = false;
            row.highlight = highlight;

            float left = 14f;
            if (withIcon)
            {
                var icon = CreateImage(rt, "Icon", Color.white, null);
                float iconSize = size.y - 12f;
                Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(iconSize, iconSize));
                icon.preserveAspect = true;
                row.icon = icon;
                left = iconSize + 22f;
            }

            float titleSize = size.y >= 80f ? 30f : 24f;
            float detailSize = size.y >= 80f ? 22f : 18f;
            bool twoLines = size.y >= 54f;
            var title = CreateText(rt, "Title", "", titleSize, TextColor, TextAlignmentOptions.Left, FontStyles.Bold);
            Place(title.rectTransform, new Vector2(0f, twoLines ? 1f : 0.5f), new Vector2(0f, twoLines ? 1f : 0.5f), new Vector2(left, twoLines ? -4f : 0f), new Vector2(size.x - left - 200f, twoLines ? size.y * 0.5f : size.y));
            row.titleText = title;
            var detail = CreateText(rt, "Detail", "", detailSize, new Color(0.7f, 0.82f, 0.95f), TextAlignmentOptions.Left);
            Place(detail.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(left, 2f), new Vector2(size.x - left - 200f, twoLines ? size.y * 0.5f : 0f));
            if (!twoLines) detail.enabled = false;
            row.detailText = detail;
            var cost = CreateText(rt, "Cost", "", detailSize + 2f, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Right, FontStyles.Bold);
            Place(cost.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(190f, size.y));
            row.costText = cost;
            return row;
        }
    }
}
