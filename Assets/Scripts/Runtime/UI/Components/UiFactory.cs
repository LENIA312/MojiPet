using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Mojipet.UI.Components
{
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
            tmp.color = Color.white;

            var japaneseFont = GetJapaneseFont();
            if (japaneseFont != null)
            {
                tmp.font = japaneseFont;
            }

            return tmp;
        }

        private static TMP_FontAsset GetJapaneseFont()
        {
            if (_japaneseFontInitialized)
            {
                return _japaneseFont;
            }

            _japaneseFontInitialized = true;

            // Editor/Windows実機確認用のフォールバック。iOS/Androidリリース時はOS標準フォント名が異なるため、
            // 埋め込み日本語フォントアセットへの差し替えが別途必要になる。
            string[] candidates =
            {
                "Yu Gothic UI",
                "Yu Gothic",
                "Meiryo UI",
                "Meiryo",
                "MS Gothic",
                "Noto Sans CJK JP",
                "Noto Sans JP",
                "Hiragino Sans",
                "Hiragino Kaku Gothic ProN"
            };

            foreach (var family in candidates)
            {
                var asset = TMP_FontAsset.CreateFontAsset(family, "Regular", 90);
                if (asset != null)
                {
                    _japaneseFont = asset;
                    break;
                }
            }

            return _japaneseFont;
        }

        public static Button CreateButton(Transform parent, string label, Action onClick)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.2f, 0.5f, 0.9f, 1f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            var text = CreateText(go.transform, label, 24, TextAlignmentOptions.Center);
            StretchFull((RectTransform)text.transform);

            return button;
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
            scrollImage.color = new Color(1f, 1f, 1f, 0.05f);

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
