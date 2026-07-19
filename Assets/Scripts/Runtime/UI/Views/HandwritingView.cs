using System;
using Mojipet.Core;
using Mojipet.UI.Components;
using TMPro;
using UnityEngine;

namespace Mojipet.UI.Views
{
    public sealed class HandwritingView : MonoBehaviour
    {
        private int _characterId;
        private DrawingCanvas _canvas;
        private Action _onClosed;

        public static HandwritingView Create(Transform parent, int characterId, string character, Action onClosed = null)
        {
            var go = new GameObject("HandwritingWindow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<HandwritingView>();
            view.Initialize(characterId, character, onClosed);
            return view;
        }

        private void Initialize(int characterId, string character, Action onClosed)
        {
            _characterId = characterId;
            _onClosed = onClosed;
            Build(character);
        }

        private void Build(string character)
        {
            var rect = (RectTransform)transform;
            UiFactory.StretchFull(rect);

            var background = UiFactory.CreatePanel(rect, UiTheme.WindowBackground);
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.anchorMin = new Vector2(0.08f, 0.1f);
            backgroundRect.anchorMax = new Vector2(0.92f, 0.9f);
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            var title = UiFactory.CreateText(
                backgroundRect, "あたらしいもじ！\nなぞって描いてあげよう", 26, TextAlignmentOptions.Center);
            var titleRect = (RectTransform)title.transform;
            titleRect.anchorMin = new Vector2(0f, 0.86f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(10f, 0f);
            titleRect.offsetMax = new Vector2(-10f, -10f);

            // Faint trace guide behind the drawing canvas showing the actual glyph shape.
            var guide = UiFactory.CreateText(backgroundRect, character, 220, TextAlignmentOptions.Center);
            guide.color = new Color(1f, 1f, 1f, 0.12f);
            guide.raycastTarget = false;
            var guideRect = (RectTransform)guide.transform;
            guideRect.anchorMin = new Vector2(0.15f, 0.24f);
            guideRect.anchorMax = new Vector2(0.85f, 0.84f);
            guideRect.offsetMin = Vector2.zero;
            guideRect.offsetMax = Vector2.zero;

            _canvas = DrawingCanvas.Create(backgroundRect, 256, UiTheme.SurfaceLight, UiTheme.TextPrimary);
            var canvasRect = (RectTransform)_canvas.transform;
            canvasRect.anchorMin = new Vector2(0.15f, 0.24f);
            canvasRect.anchorMax = new Vector2(0.85f, 0.84f);
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            var clearButton = UiFactory.CreateButton(backgroundRect, "消す", () => _canvas.Clear(), ButtonStyle.Secondary);
            var clearRect = (RectTransform)clearButton.transform;
            clearRect.anchorMin = new Vector2(0.1f, 0.08f);
            clearRect.anchorMax = new Vector2(0.45f, 0.2f);
            clearRect.offsetMin = Vector2.zero;
            clearRect.offsetMax = Vector2.zero;

            var doneButton = UiFactory.CreateButton(backgroundRect, "できた！", OnDoneClicked);
            var doneRect = (RectTransform)doneButton.transform;
            doneRect.anchorMin = new Vector2(0.55f, 0.08f);
            doneRect.anchorMax = new Vector2(0.9f, 0.2f);
            doneRect.offsetMin = Vector2.zero;
            doneRect.offsetMax = Vector2.zero;
        }

        private void OnDoneClicked()
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null && _canvas.HasDrawn)
            {
                gameManager.HandwritingSystem.SaveDrawing(_characterId, _canvas.GetTexture());
            }

            Close();
        }

        private void Close()
        {
            _onClosed?.Invoke();
            Destroy(gameObject);
        }
    }
}
