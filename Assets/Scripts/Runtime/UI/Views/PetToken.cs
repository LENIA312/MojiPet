using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mojipet.Core;
using Mojipet.Events;
using Mojipet.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mojipet.UI.Views
{
    public sealed class PetToken : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
    {
        private static readonly TimeSpan StatusPollInterval = TimeSpan.FromSeconds(5);

        // Level-based visual growth: a continuous, always-visible sign that this
        // specific character has been invested in, since numbers alone (Lv/Exp)
        // don't read as "raising" something unless you open its detail screen.
        private const float MinScale = 1f;
        private const float MaxScale = 1.5f;
        private const int MaxScaleLevel = 100;
        private const int StarBadgeLevel = 10;
        private const int CrownBadgeLevel = 50;

        // Tap-vs-long-press: a quick tap pats the character in place (no menu);
        // holding briefly opens the full detail screen. Three quick pats in a
        // row register as an actual "stroke" (PetSystem.Stroke). This replaces
        // routing every interaction through a menu button -- direct touch reads
        // as "petting", a button press doesn't.
        private const float LongPressSeconds = 0.45f;
        private const float TapStreakWindowSeconds = 1.2f;
        private const int StrokeTapThreshold = 3;

        private RectTransform _rectTransform;
        private RectTransform _worldBounds;
        private int _characterId;
        private string _character;
        private Action<int> _onClicked;
        private CancellationTokenSource _cts;
        private Transform _visual;
        private TextMeshProUGUI _statusIcon;
        private Image _researchGauge;
        private TextMeshProUGUI _growthBadge;

        private float _pointerDownTime;
        private float _lastTapTime;
        private int _tapStreak;
        private bool _isGathering;

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

            RefreshVisual();
            CreateStatusIcon();
            CreateGrowthBadge();
            RefreshStatusIcon();
            RefreshGrowth();

            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.EventBus.Subscribe<OnHandwritingSaved>(HandleHandwritingSaved);
                gameManager.EventBus.Subscribe<OnPetFed>(HandlePetFed);
                gameManager.EventBus.Subscribe<OnResearchStarted>(HandleResearchStarted);
                gameManager.EventBus.Subscribe<OnResearchCompleted>(HandleResearchCompleted);
                gameManager.EventBus.Subscribe<OnResearchCanceled>(HandleResearchCanceled);
                gameManager.EventBus.Subscribe<OnPetLevelUp>(HandlePetLevelUp);
            }

            _cts = new CancellationTokenSource();
            WanderLoopAsync(_cts.Token).Forget();
            StatusPollLoopAsync(_cts.Token).Forget();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDownTime = Time.unscaledTime;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Unity only invokes OnPointerClick for a genuine press+release that
            // never exceeded the drag threshold, so a swipe used to pan the
            // garden's ScrollRect never lands here -- no manual drag-vs-tap
            // disambiguation needed.
            var pressDuration = Time.unscaledTime - _pointerDownTime;
            if (pressDuration >= LongPressSeconds)
            {
                _onClicked?.Invoke(_characterId);
                return;
            }

            RegisterQuickTap();
        }

        private void RegisterQuickTap()
        {
            var now = Time.unscaledTime;
            if (now - _lastTapTime > TapStreakWindowSeconds)
            {
                _tapStreak = 0;
            }

            _lastTapTime = now;
            _tapStreak++;

            if (_tapStreak >= StrokeTapThreshold)
            {
                _tapStreak = 0;
                TryStroke();
            }
        }

        private void TryStroke()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            // Always show the affectionate reaction -- the cooldown only gates
            // the exp reward (PetSystem.Stroke), not whether petting "does"
            // anything the player can see.
            gameManager.PetSystem.Stroke(_characterId);
            ShowReactionAsync("💗").Forget();
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

        private void HandlePetLevelUp(OnPetLevelUp e)
        {
            if (e.CharacterId == _characterId)
            {
                RefreshGrowth();
                PlayLevelUpEffectAsync().Forget();
            }
        }

        private async UniTaskVoid PlayLevelUpEffectAsync()
        {
            if (_cts == null)
            {
                return;
            }

            var token = _cts.Token;
            ShowReactionAsync("✨").Forget();

            var baseScale = _rectTransform.localScale;
            var bounceScale = baseScale * 1.3f;

            await AnimateScaleAsync(baseScale, bounceScale, 0.15f, token);
            await AnimateScaleAsync(bounceScale, baseScale, 0.2f, token);
        }

        private async UniTask AnimateScaleAsync(Vector3 from, Vector3 to, float duration, CancellationToken token)
        {
            var elapsed = 0f;
            const float step = 0.02f;

            while (elapsed < duration)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(step), cancellationToken: token).SuppressCancellationThrow();
                if (token.IsCancellationRequested)
                {
                    return;
                }

                elapsed += step;
                _rectTransform.localScale = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            }

            _rectTransform.localScale = to;
        }

        // Short-lived emoji that pops up and fades -- used for pats, level-ups,
        // and (from HomeWorldView) the word-completion celebration.
        public async UniTaskVoid ShowReactionAsync(string emoji)
        {
            if (_cts == null)
            {
                return;
            }

            var token = _cts.Token;
            var reactionGo = new GameObject("Reaction", typeof(RectTransform));
            reactionGo.transform.SetParent(_rectTransform, false);
            var reactionText = UiFactory.CreateText(reactionGo.transform, emoji, 36, TextAlignmentOptions.Center);
            reactionText.raycastTarget = false;
            var reactionRect = (RectTransform)reactionGo.transform;
            reactionRect.anchorMin = new Vector2(0.5f, 0.5f);
            reactionRect.anchorMax = new Vector2(0.5f, 0.5f);
            reactionRect.pivot = new Vector2(0.5f, 0.5f);
            reactionRect.sizeDelta = new Vector2(50f, 50f);
            reactionRect.anchoredPosition = new Vector2(0f, 60f);

            const float duration = 0.8f;
            const float step = 0.04f;
            var elapsed = 0f;
            var startPos = reactionRect.anchoredPosition;
            var endPos = startPos + new Vector2(0f, 40f);

            while (elapsed < duration)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(step), cancellationToken: token).SuppressCancellationThrow();
                if (token.IsCancellationRequested || reactionGo == null)
                {
                    if (reactionGo != null)
                    {
                        Destroy(reactionGo);
                    }

                    return;
                }

                elapsed += step;
                var t = Mathf.Clamp01(elapsed / duration);
                reactionRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                var color = reactionText.color;
                color.a = 1f - t;
                reactionText.color = color;
            }

            if (reactionGo != null)
            {
                Destroy(reactionGo);
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

        private void CreateGrowthBadge()
        {
            // Opposite corner from the research/hunger status icon.
            _growthBadge = UiFactory.CreateText(_rectTransform, string.Empty, 28, TextAlignmentOptions.Center);
            _growthBadge.raycastTarget = false;
            var badgeRect = (RectTransform)_growthBadge.transform;
            badgeRect.anchorMin = new Vector2(0f, 1f);
            badgeRect.anchorMax = new Vector2(0f, 1f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.sizeDelta = new Vector2(40f, 40f);
            badgeRect.anchoredPosition = new Vector2(6f, -6f);
        }

        private void RefreshGrowth()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || !gameManager.PetSystem.IsUnlocked(_characterId))
            {
                return;
            }

            var level = gameManager.PetSystem.GetPet(_characterId).Level;

            var t = Mathf.Clamp01((level - 1) / (float)(MaxScaleLevel - 1));
            var scale = Mathf.Lerp(MinScale, MaxScale, t);
            _rectTransform.localScale = new Vector3(scale, scale, 1f);

            if (level >= CrownBadgeLevel)
            {
                _growthBadge.text = "👑";
            }
            else if (level >= StarBadgeLevel)
            {
                _growthBadge.text = "⭐";
            }
            else
            {
                _growthBadge.text = string.Empty;
            }
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

            // Deterministic per-character pacing (same character always has the
            // same "personality" across sessions): 0.7x-1.3x speed, using a
            // different seed multiplier than the position random above so the
            // two don't correlate.
            var personality = new System.Random(_characterId * 104729 + 17);
            var speedFactor = 0.7f + (float)personality.NextDouble() * 0.6f;

            while (!token.IsCancellationRequested)
            {
                var waitSeconds = (3f + (float)random.NextDouble() * 3f) / speedFactor;
                await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: token).SuppressCancellationThrow();

                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (_isGathering)
                {
                    // A word-completion gather is driving position externally;
                    // don't fight it -- just wait for the next cycle.
                    continue;
                }

                await GlideToAsync(RandomPositionInBounds(), 1.5f / speedFactor, token);
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

        public Vector2 GetPosition()
        {
            return _rectTransform.anchoredPosition;
        }

        // Called externally (HomeWorldView) when this character is part of a
        // just-completed word: glide to a shared point, hold briefly, then let
        // the normal wander loop resume. Characters visibly "becoming" a word
        // is the one thing this game can do that a generic pet game can't.
        public async UniTask GatherAsync(Vector2 targetPosition, float holdSeconds)
        {
            if (_cts == null)
            {
                return;
            }

            var token = _cts.Token;
            _isGathering = true;
            try
            {
                await GlideToAsync(targetPosition, 1f, token);
                await UniTask.Delay(TimeSpan.FromSeconds(holdSeconds), cancellationToken: token).SuppressCancellationThrow();
            }
            finally
            {
                _isGathering = false;
            }
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
                gameManager.EventBus.Unsubscribe<OnPetLevelUp>(HandlePetLevelUp);
            }
        }
    }
}
