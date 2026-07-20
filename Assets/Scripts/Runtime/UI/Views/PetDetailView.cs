using Mojipet.Core;
using Mojipet.Events;
using Mojipet.UI.Components;
using Mojipet.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mojipet.UI.Views
{
    public sealed class PetDetailView : MonoBehaviour
    {
        private PetDetailPresenter _presenter;
        private int _characterId;
        private TextMeshProUGUI _statusText;
        private Button _cheerButton;
        private TextMeshProUGUI _cheerButtonLabel;
        private Button _strokeButton;
        private TextMeshProUGUI _strokeButtonLabel;
        private Transform _toastLayer;

        public static PetDetailView Create(Transform parent, PetDetailPresenter presenter, int characterId, Transform toastLayer)
        {
            var go = new GameObject("PetDetailWindow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<PetDetailView>();
            view.Initialize(presenter, characterId, toastLayer);
            return view;
        }

        private void Initialize(PetDetailPresenter presenter, int characterId, Transform toastLayer)
        {
            _presenter = presenter;
            _characterId = characterId;
            _toastLayer = toastLayer;
            Build();
            Refresh();

            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.EventBus.Subscribe<OnMoneyChanged>(HandleMoneyChanged);
            }
        }

        private void OnDestroy()
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.EventBus != null)
            {
                gameManager.EventBus.Unsubscribe<OnMoneyChanged>(HandleMoneyChanged);
            }
        }

        private void HandleMoneyChanged(OnMoneyChanged e)
        {
            Refresh();
        }

        private void Build()
        {
            var rect = (RectTransform)transform;
            UiFactory.StretchFull(rect);

            var background = UiFactory.CreatePanel(rect, UiTheme.WindowBackground);
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.anchorMin = new Vector2(0.1f, 0.25f);
            backgroundRect.anchorMax = new Vector2(0.9f, 0.75f);
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            _statusText = UiFactory.CreateText(backgroundRect, string.Empty, 24, TextAlignmentOptions.TopLeft);
            var statusRect = (RectTransform)_statusText.transform;
            statusRect.anchorMin = new Vector2(0f, 0.3f);
            statusRect.anchorMax = new Vector2(0.78f, 1f);
            statusRect.offsetMin = new Vector2(20f, 0f);
            statusRect.offsetMax = new Vector2(-10f, -10f);

            var redrawButton = UiFactory.CreateButton(backgroundRect, "描き直す", OnRedrawClicked, ButtonStyle.Secondary);
            var redrawRect = (RectTransform)redrawButton.transform;
            redrawRect.anchorMin = new Vector2(0.78f, 0.86f);
            redrawRect.anchorMax = new Vector2(1f, 1f);
            redrawRect.offsetMin = new Vector2(0f, 0f);
            redrawRect.offsetMax = new Vector2(-10f, -10f);

            var feedButton = UiFactory.CreateButton(backgroundRect, "エサをあげる", OnFeedClicked);
            var feedRect = (RectTransform)feedButton.transform;
            feedRect.anchorMin = new Vector2(0f, 0.1f);
            feedRect.anchorMax = new Vector2(0.32f, 0.28f);
            feedRect.offsetMin = new Vector2(10f, 0f);
            feedRect.offsetMax = new Vector2(-4f, 0f);

            _strokeButton = UiFactory.CreateButton(backgroundRect, "なでる", OnStrokeClicked);
            _strokeButtonLabel = _strokeButton.GetComponentInChildren<TextMeshProUGUI>();
            var strokeRect = (RectTransform)_strokeButton.transform;
            strokeRect.anchorMin = new Vector2(0.34f, 0.1f);
            strokeRect.anchorMax = new Vector2(0.66f, 0.28f);
            strokeRect.offsetMin = new Vector2(4f, 0f);
            strokeRect.offsetMax = new Vector2(-4f, 0f);

            _cheerButton = UiFactory.CreateButton(backgroundRect, "応援する", OnCheerClicked);
            _cheerButtonLabel = _cheerButton.GetComponentInChildren<TextMeshProUGUI>();
            var cheerRect = (RectTransform)_cheerButton.transform;
            cheerRect.anchorMin = new Vector2(0.68f, 0.1f);
            cheerRect.anchorMax = new Vector2(1f, 0.28f);
            cheerRect.offsetMin = new Vector2(4f, 0f);
            cheerRect.offsetMax = new Vector2(-10f, 0f);

            var closeButton = UiFactory.CreateButton(backgroundRect, "閉じる", Close, ButtonStyle.Secondary);
            var closeRect = (RectTransform)closeButton.transform;
            closeRect.anchorMin = new Vector2(0.3f, 0f);
            closeRect.anchorMax = new Vector2(0.7f, 0.09f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
        }

        private void Refresh()
        {
            var data = _presenter.GetData(_characterId);

            var expLine = data.RequiredExp >= 0
                ? $"経験値 {data.Exp} / {data.RequiredExp}"
                : $"経験値 {data.Exp}（最大レベル）";

            string researchLine;
            if (data.IsResearching)
            {
                researchLine = $"研究中: {data.ResearchingWordDisplay}（{data.ResearchProgress * 100f:F0}%）";
            }
            else if (data.Hunger <= 0f)
            {
                researchLine = "お腹が空いていて研究できません";
            }
            else
            {
                researchLine = "つぎの研究をさがしています…";
            }

            var boostLine = data.IsResearchBoostActive
                ? $"研究速度アップ中（残り{(int)data.ResearchBoostRemaining.TotalMinutes}分）\n"
                : string.Empty;

            var cheerLine = data.IsCheerActive
                ? $"応援効果中（残り{(int)data.CheerRemaining.TotalMinutes}分{(int)data.CheerRemaining.TotalSeconds % 60}秒）\n"
                : string.Empty;

            _statusText.text =
                $"{data.Character}\n" +
                $"Lv {data.Level}\n" +
                $"{expLine}\n" +
                $"満腹度 {data.Hunger:F0}\n" +
                $"言霊生産 {data.ProductionRate}/秒\n" +
                boostLine +
                cheerLine +
                $"{researchLine}";

            _cheerButtonLabel.text = data.IsCheerActive ? "応援中" : $"応援する（{data.CheerCost}）";
            _cheerButton.interactable = !data.IsCheerActive && data.CanAffordCheer;

            _strokeButtonLabel.text = data.CanStroke
                ? "なでる"
                : $"また今度（{(int)data.StrokeCooldownRemaining.TotalMinutes}分）";
            _strokeButton.interactable = data.CanStroke;
        }

        private static readonly string[] StrokeReactions =
        {
            "うれしそう！",
            "なでなで、きもちよさそう",
            "よろこんでいる…気がする",
            "ふるえて反応した！"
        };

        private void OnFeedClicked()
        {
            _presenter.Feed(_characterId);
            Refresh();
        }

        private void OnCheerClicked()
        {
            _presenter.Cheer(_characterId);
            Refresh();
        }

        private void OnStrokeClicked()
        {
            if (_presenter.Stroke(_characterId))
            {
                var reaction = StrokeReactions[Random.Range(0, StrokeReactions.Length)];
                Toast.Show(_toastLayer, reaction);
            }

            Refresh();
        }

        private void OnRedrawClicked()
        {
            var data = _presenter.GetData(_characterId);
            HandwritingView.Create(transform.parent, _characterId, data.Character);
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}
