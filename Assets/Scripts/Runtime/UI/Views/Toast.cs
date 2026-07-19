using System;
using Cysharp.Threading.Tasks;
using Mojipet.UI.Components;
using TMPro;
using UnityEngine;

namespace Mojipet.UI.Views
{
    public sealed class Toast : MonoBehaviour
    {
        private const float DisplaySeconds = 2.5f;

        public static void Show(Transform parent, string message)
        {
            var go = new GameObject("Toast", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var toast = go.AddComponent<Toast>();
            toast.Initialize(message);
        }

        private void Initialize(string message)
        {
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(600f, 70f);
            rect.anchoredPosition = new Vector2(0f, -120f);

            var panel = UiFactory.CreatePanel(rect, UiTheme.Surface);
            var panelRect = (RectTransform)panel.transform;
            UiFactory.StretchFull(panelRect);

            var text = UiFactory.CreateText(panelRect, message, 24, TextAlignmentOptions.Center);
            UiFactory.StretchFull((RectTransform)text.transform);

            DismissAfterDelayAsync().Forget();
        }

        private async UniTaskVoid DismissAfterDelayAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(DisplaySeconds)).SuppressCancellationThrow();
            if (this != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
