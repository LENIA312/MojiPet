using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mojipet.Core;
using Mojipet.Events;
using Mojipet.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mojipet.UI.Views
{
    public sealed class PetToken : MonoBehaviour
    {
        private static readonly TimeSpan StatusPollInterval = TimeSpan.FromSeconds(5);

        private RectTransform _rectTransform;
        private RectTransform _worldBounds;
        private int _characterId;
        private string _character;
        private Action<int> _onClicked;
        private CancellationTokenSource _cts;
        private Transform _visual;
        private TextMeshProUGUI _statusIcon;
        private Image _researchGauge;

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
            _character = character;
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

            RefreshVisual();
            CreateStatusIcon();
            RefreshStatusIcon();

            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.EventBus.Subscribe<OnHandwritingSaved>(HandleHandwritingSaved);
                gameManager.EventBus.Subscribe<OnPetFed>(HandlePetFed);
                gameManager.EventBus.Subscribe<OnResearchStarted>(HandleResearchStarted);
                gameManager.EventBus.Subscribe<OnResearchCompleted>(HandleResearchCompleted);
                gameManager.EventBus.Subscribe<OnResearchCanceled>(HandleResearchCanceled);
            }

            _cts = new CancellationTokenSource();
            WanderLoopAsync(_cts.Token).Forget();
            StatusPollLoopAsync(_cts.Token).Forget();
        }

        private void HandleHandwritingSaved(OnHandwritingSaved e)
        {
            if (e.CharacterId == _characterId)
            {
                RefreshVisual();
            }
        }

        private void HandlePetFed(OnPetFed e)
        {
            if (e.CharacterId == _characterId)
            {
                RefreshStatusIcon();
            }
        }

        private void HandleResearchStarted(OnResearchStarted e)
        {
            if (e.CharacterId == _characterId)
            {
                RefreshStatusIcon();
            }
        }

        private void HandleResearchCompleted(OnResearchCompleted e)
        {
            if (e.CharacterId == _characterId)
            {
                RefreshStatusIcon();
            }
        }

        private void HandleResearchCanceled(OnResearchCanceled e)
        {
            if (e.CharacterId == _characterId)
            {
                RefreshStatusIcon();
            }
        }

        private async UniTaskVoid StatusPollLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(StatusPollInterval, cancellationToken: token).SuppressCancellationThrow();
                if (token.IsCancellationRequested)
                {
                    return;
                }

                // Hunger decays continuously with no dedicated per-tick event, so
                // poll it (in addition to the event-driven refreshes above, which
                // cover research/feeding immediately).
                RefreshStatusIcon();
            }
        }

        private void CreateStatusIcon()
        {
            // Ring gauge sits behind (and slightly larger than) the icon glyph so
            // it visually surrounds it; only shown/filled while researching.
            _researchGauge = UiFactory.CreateRadialProgress(_rectTransform, UiTheme.Primary);
            var gaugeRect = (RectTransform)_researchGauge.transform;
            gaugeRect.anchorMin = new Vector2(1f, 1f);
            gaugeRect.anchorMax = new Vector2(1f, 1f);
            gaugeRect.pivot = new Vector2(0.5f, 0.5f);
            gaugeRect.sizeDelta = new Vector2(54f, 54f);
            gaugeRect.anchoredPosition = new Vector2(-6f, -6f);
            _researchGauge.gameObject.SetActive(false);

            _statusIcon = UiFactory.CreateText(_rectTransform, string.Empty, 32, TextAlignmentOptions.Center);
            _statusIcon.raycastTarget = false;
            var iconRect = (RectTransform)_statusIcon.transform;
            iconRect.anchorMin = new Vector2(1f, 1f);
            iconRect.anchorMax = new Vector2(1f, 1f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(44f, 44f);
            iconRect.anchoredPosition = new Vector2(-6f, -6f);
        }

        private void RefreshStatusIcon()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || _statusIcon == null)
            {
                return;
            }

            if (!gameManager.PetSystem.IsUnlocked(_characterId))
            {
                return;
            }

            var hunger = gameManager.PetSystem.GetPet(_characterId).Hunger;
            var isResearching = gameManager.ResearchSystem.IsResearching(_characterId);

            if (hunger <= 0f)
            {
                _statusIcon.text = "🍖";
                _researchGauge.gameObject.SetActive(false);
            }
            else if (isResearching)
            {
                // Deliberately the bare U+270D glyph, not the "✍️" VS16 emoji-presentation
                // sequence -- the variation selector itself has no visible glyph in
                // Noto Emoji and could render as a stray tofu box next to the pencil.
                _statusIcon.text = "✍";
                _researchGauge.gameObject.SetActive(true);
                _researchGauge.fillAmount = gameManager.ResearchSystem.GetProgressRate(_characterId);
            }
            else
            {
                _statusIcon.text = string.Empty;
                _researchGauge.gameObject.SetActive(false);
            }
        }

        private void RefreshVisual()
        {
            if (_visual != null)
            {
                Destroy(_visual.gameObject);
            }

            var visualGo = new GameObject("Visual", typeof(RectTransform));
            visualGo.transform.SetParent(_rectTransform, false);
            _visual = visualGo.transform;
            UiFactory.StretchFull((RectTransform)_visual);

            if (!TryShowHandwriting(_visual))
            {
                var label = UiFactory.CreateText(_visual, _character, 48, TextAlignmentOptions.Center);
                var labelRect = (RectTransform)label.transform;
                UiFactory.StretchFull(labelRect);
            }
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

        private bool TryShowHandwriting(Transform parent)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || !gameManager.HandwritingSystem.HasDrawing(_characterId))
            {
                return false;
            }

            var texture = gameManager.HandwritingSystem.LoadDrawing(_characterId);
            if (texture == null)
            {
                return false;
            }

            var rawImageGo = new GameObject("Handwriting", typeof(RectTransform), typeof(RawImage));
            rawImageGo.transform.SetParent(parent, false);
            var rawImage = rawImageGo.GetComponent<RawImage>();
            rawImage.texture = texture;
            rawImage.raycastTarget = false;

            var rawImageRect = (RectTransform)rawImageGo.transform;
            UiFactory.StretchFull(rawImageRect);
            rawImageRect.offsetMin = new Vector2(8f, 8f);
            rawImageRect.offsetMax = new Vector2(-8f, -8f);

            return true;
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

            var gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.EventBus != null)
            {
                gameManager.EventBus.Unsubscribe<OnHandwritingSaved>(HandleHandwritingSaved);
                gameManager.EventBus.Unsubscribe<OnPetFed>(HandlePetFed);
                gameManager.EventBus.Unsubscribe<OnResearchStarted>(HandleResearchStarted);
                gameManager.EventBus.Unsubscribe<OnResearchCompleted>(HandleResearchCompleted);
                gameManager.EventBus.Unsubscribe<OnResearchCanceled>(HandleResearchCanceled);
            }
        }
    }
}
