using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mojipet.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mojipet.UI.Views
{
    public sealed class PetToken : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private RectTransform _worldBounds;
        private int _characterId;
        private Action<int> _onClicked;
        private CancellationTokenSource _cts;

        public static PetToken Create(
            Transform parent,
            RectTransform worldBounds,
            int characterId,
            string character,
            Action<int> onClicked)
        {
            var go = new GameObject($"Pet_{character}", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var token = go.AddComponent<PetToken>();
            token.Initialize(worldBounds, characterId, character, onClicked);
            return token;
        }

        private void Initialize(RectTransform worldBounds, int characterId, string character, Action<int> onClicked)
        {
            _worldBounds = worldBounds;
            _characterId = characterId;
            _onClicked = onClicked;

            _rectTransform = (RectTransform)transform;
            _rectTransform.sizeDelta = new Vector2(120f, 120f);
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = RandomPositionInBounds();

            var image = gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.02f);

            var button = gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => _onClicked?.Invoke(_characterId));

            var label = UiFactory.CreateText(_rectTransform, character, 48, TextAlignmentOptions.Center);
            var labelRect = (RectTransform)label.transform;
            UiFactory.StretchFull(labelRect);

            _cts = new CancellationTokenSource();
            WanderLoopAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid WanderLoopAsync(CancellationToken token)
        {
            var random = new System.Random(_characterId * 7919 + 13);

            while (!token.IsCancellationRequested)
            {
                var waitSeconds = 3f + (float)random.NextDouble() * 3f;
                await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: token).SuppressCancellationThrow();

                if (token.IsCancellationRequested)
                {
                    return;
                }

                await GlideToAsync(RandomPositionInBounds(), 1.5f, token);
            }
        }

        private async UniTask GlideToAsync(Vector2 target, float duration, CancellationToken token)
        {
            var start = _rectTransform.anchoredPosition;
            var elapsed = 0f;
            const float step = 0.1f;

            while (elapsed < duration)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(step), cancellationToken: token).SuppressCancellationThrow();
                if (token.IsCancellationRequested)
                {
                    return;
                }

                elapsed += step;
                var t = Mathf.Clamp01(elapsed / duration);
                _rectTransform.anchoredPosition = Vector2.Lerp(start, target, t);
            }

            _rectTransform.anchoredPosition = target;
        }

        private Vector2 RandomPositionInBounds()
        {
            var size = _worldBounds.rect.size;
            var halfWidth = Mathf.Max(0f, size.x * 0.5f - 80f);
            var halfHeight = Mathf.Max(0f, size.y * 0.5f - 80f);

            var x = UnityEngine.Random.Range(-halfWidth, halfWidth);
            var y = UnityEngine.Random.Range(-halfHeight, halfHeight);
            return new Vector2(x, y);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
