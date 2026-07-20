using System;
using Cysharp.Threading.Tasks;
using Mojipet.UI.Components;
using TMPro;
using UnityEngine;

namespace Mojipet.UI.Views
{
    public sealed class Toast : MonoBehaviour
    {
        private const float BaseDisplaySeconds = 2.5f;
        private const float ExtraSecondsPerLine = 1f;
        private const float MaxDisplaySeconds = 6f;
        private const float BaseHeight = 70f;
        private const float ExtraHeightPerLine = 32f;

        public static void Show(Transform parent, string message)
        {
            var go = new GameObject("Toast", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var toast = go.AddComponent<Toast>();
            toast.Initialize(message);
        }

        private void Initialize(string message)
        {
            var lineCount = Mathf.Max(1, message.Split('\n').Length);

            // Anchored below the top of Screen.safeArea (not the raw canvas top) so
            // it clears the notch/Dynamic Island/status bar and the header bar.
            var safeAreaTop = UiFactory.GetSafeAreaAnchorMax().y;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(0.5f, safeAreaTop);
            rect.anchorMax = new Vector2(0.5f, safeAreaTop);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(600f, BaseHeight + (lineCount - 1) * ExtraHeightPerLine);
            rect.anchoredPosition = new Vector2(0f, -20f);

            var panel = UiFactory.CreatePanel(rect, UiTheme.Surface);
            var panelRect = (RectTransform)panel.transform;
            UiFactory.StretchFull(panelRect);

            var text = UiFactory.CreateText(panelRect, message, 24, TextAlignmentOptions.Center);
            UiFactory.StretchFull((RectTransform)text.transform);

            var displaySeconds = Mathf.Min(MaxDisplaySeconds, BaseDisplaySeconds + (lineCount - 1) * ExtraSecondsPerLine);
            DismissAfterDelayAsync(displaySeconds).Forget();
        }

        private async UniTaskVoid DismissAfterDelayAsync(float displaySeconds)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(displaySeconds)).SuppressCancellationThrow();
            if (this != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
