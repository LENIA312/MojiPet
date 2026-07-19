using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Mojipet.UI.Components
{
    public enum ButtonStyle
    {
        Primary,
        Secondary,
        Danger
    }

    public static class UiFactory
    {
        public static Canvas CreateCanvasRoot(string name)
        {
            var go = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        public static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        public static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // Screen.safeArea excludes notches/Dynamic Island/status bar (top) and the
        // home indicator (bottom). Expressed as normalized (0-1) anchor coordinates
        // so it stays correct regardless of CanvasScaler scaling -- a pixel-based
        // offset would need a separate scale-factor conversion and drift on
        // resolutions other than the reference resolution.
        public static Vector2 GetSafeAreaAnchorMin()
        {
            var safeArea = Screen.safeArea;
            return new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        }

        public static Vector2 GetSafeAreaAnchorMax()
        {
            var safeArea = Screen.safeArea;
            return new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
        }

        public static Image CreatePanel(Transform parent, Color color)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_FontAsset _japaneseFont;
        private static bool _japaneseFontInitialized;

        public static TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = UiTheme.TextPrimary;

            var japaneseFont = GetJapaneseFont();
            if (japaneseFont != null)
            {
                tmp.font = japaneseFont;
            }

            return tmp;
        }

        private const string JapaneseFontResourcePath = "NotoSansJP-Regular SDF";

        private static TMP_FontAsset GetJapaneseFont()
        {
            if (_japaneseFontInitialized)
            {
                return _japaneseFont;
            }

            _japaneseFontInitialized = true;

            // Runtime OS font lookup (TMP_FontAsset.CreateFontAsset(familyName, ...)) is
            // unreliable on iOS/Android -- it works in the Windows Editor but silently
            // returns no usable font on device, causing Japanese text to render as
            // tofu/mojibake. Use a prebaked font asset embedded in the project instead
            // (Assets/Fonts/NotoSansJP-Regular.ttf via Tools > Generate Japanese Font
            // Asset), which works identically on every platform.
            _japaneseFont = Resources.Load<TMP_FontAsset>(JapaneseFontResourcePath);
            if (_japaneseFont == null)
            {
                Debug.LogError(
                    $"Japanese font asset not found at Resources/{JapaneseFontResourcePath}. " +
                    "Run Tools > Generate Japanese Font Asset in the Unity Editor.");
            }

            return _japaneseFont;
        }

        public static Button CreateButton(Transform parent, string label, Action onClick, ButtonStyle style = ButtonStyle.Primary)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = GetButtonColor(style);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            var text = CreateText(go.transform, label, 24, TextAlignmentOptions.Center);
            text.color = UiTheme.TextOnPrimary;
            StretchFull((RectTransform)text.transform);

            return button;
        }

        private static Color GetButtonColor(ButtonStyle style)
        {
            switch (style)
            {
                case ButtonStyle.Secondary:
                    return UiTheme.Secondary;
                case ButtonStyle.Danger:
                    return UiTheme.Danger;
                default:
                    return UiTheme.Primary;
            }
        }

        public static TMP_InputField CreateInputField(Transform parent, string placeholderText)
        {
            var go = new GameObject("InputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);

            var background = go.GetComponent<Image>();
            background.color = UiTheme.SurfaceLight;

            var viewportGo = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(go.transform, false);
            var viewportRect = (RectTransform)viewportGo.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(16f, 6f);
            viewportRect.offsetMax = new Vector2(-16f, -6f);

            var placeholder = CreateText(viewportRect, placeholderText, 24, TextAlignmentOptions.Left);
            placeholder.color = UiTheme.TextMuted;
            placeholder.fontStyle = FontStyles.Italic;
            StretchFull((RectTransform)placeholder.transform);

            var text = CreateText(viewportRect, string.Empty, 24, TextAlignmentOptions.Left);
            StretchFull((RectTransform)text.transform);

            var inputField = go.GetComponent<TMP_InputField>();
            inputField.textViewport = viewportRect;
            inputField.textComponent = text;
            inputField.placeholder = placeholder;
            inputField.targetGraphic = background;

            return inputField;
        }

        public static Slider CreateSlider(Transform parent, float minValue, float maxValue, float initialValue, Action<float> onValueChanged)
        {
            var go = new GameObject("Slider", typeof(RectTransform), typeof(Image), typeof(Slider));
            go.transform.SetParent(parent, false);

            var background = go.GetComponent<Image>();
            background.color = UiTheme.SurfaceLight;

            var fillAreaGo = new GameObject("FillArea", typeof(RectTransform));
            fillAreaGo.transform.SetParent(go.transform, false);
            var fillAreaRect = (RectTransform)fillAreaGo.transform;
            fillAreaRect.anchorMin = new Vector2(0f, 0f);
            fillAreaRect.anchorMax = new Vector2(1f, 1f);
            fillAreaRect.offsetMin = new Vector2(6f, 6f);
            fillAreaRect.offsetMax = new Vector2(-6f, -6f);

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRect = (RectTransform)fillGo.transform;
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fillGo.GetComponent<Image>();
            fillImage.color = UiTheme.Primary;

            var slider = go.GetComponent<Slider>();
            slider.targetGraphic = background;
            slider.fillRect = fillRect;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = initialValue;

            if (onValueChanged != null)
            {
                slider.onValueChanged.AddListener(value => onValueChanged(value));
            }

            return slider;
        }

        public static RectTransform CreateScrollView(Transform parent, out RectTransform content)
        {
            var scrollGo = new GameObject(
                "ScrollView",
                typeof(RectTransform),
                typeof(Image),
                typeof(ScrollRect),
                typeof(Mask));
            scrollGo.transform.SetParent(parent, false);
            var scrollRectTransform = (RectTransform)scrollGo.transform;

            var scrollImage = scrollGo.GetComponent<Image>();
            scrollImage.color = UiTheme.SurfaceLight;

            var mask = scrollGo.GetComponent<Mask>();
            mask.showMaskGraphic = true;

            var contentGo = new GameObject(
                "Content",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            contentGo.transform.SetParent(scrollGo.transform, false);
            content = (RectTransform)contentGo.transform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            var layoutGroup = contentGo.GetComponent<VerticalLayoutGroup>();
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.spacing = 4f;
            layoutGroup.padding = new RectOffset(8, 8, 8, 8);

            var fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = scrollGo.GetComponent<ScrollRect>();
            scrollRect.content = content;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            return scrollRectTransform;
        }
    }
}
