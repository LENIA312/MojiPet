using Mojipet.Core;
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
        private Button _researchButton;
        private Button _cancelButton;

        public static PetDetailView Create(Transform parent, PetDetailPresenter presenter, int characterId)
        {
            var go = new GameObject("PetDetailWindow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<PetDetailView>();
            view.Initialize(presenter, characterId);
            return view;
        }

        private void Initialize(PetDetailPresenter presenter, int characterId)
        {
            _presenter = presenter;
            _characterId = characterId;
            Build();
            Refresh();
        }

        private void Build()
        {
            var rect = (RectTransform)transform;
            UiFactory.StretchFull(rect);

            var background = UiFactory.CreatePanel(rect, new Color(0f, 0f, 0f, 0.9f));
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.anchorMin = new Vector2(0.1f, 0.25f);
            backgroundRect.anchorMax = new Vector2(0.9f, 0.75f);
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            _statusText = UiFactory.CreateText(backgroundRect, string.Empty, 24, TextAlignmentOptions.TopLeft);
            var statusRect = (RectTransform)_statusText.transform;
            statusRect.anchorMin = new Vector2(0f, 0.3f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.offsetMin = new Vector2(20f, 0f);
            statusRect.offsetMax = new Vector2(-20f, -10f);

            var feedButton = UiFactory.CreateButton(backgroundRect, "エサをあげる", OnFeedClicked);
            var feedRect = (RectTransform)feedButton.transform;
            feedRect.anchorMin = new Vector2(0f, 0.1f);
            feedRect.anchorMax = new Vector2(0.33f, 0.28f);
            feedRect.offsetMin = new Vector2(10f, 0f);
            feedRect.offsetMax = new Vector2(-5f, 0f);

            _researchButton = UiFactory.CreateButton(backgroundRect, "研究する", OnResearchClicked);
            var researchRect = (RectTransform)_researchButton.transform;
            researchRect.anchorMin = new Vector2(0.33f, 0.1f);
            researchRect.anchorMax = new Vector2(0.66f, 0.28f);
            researchRect.offsetMin = new Vector2(5f, 0f);
            researchRect.offsetMax = new Vector2(-5f, 0f);

            _cancelButton = UiFactory.CreateButton(backgroundRect, "研究中止", OnCancelClicked);
            var cancelRect = (RectTransform)_cancelButton.transform;
            cancelRect.anchorMin = new Vector2(0.66f, 0.1f);
            cancelRect.anchorMax = new Vector2(1f, 0.28f);
            cancelRect.offsetMin = new Vector2(5f, 0f);
            cancelRect.offsetMax = new Vector2(-10f, 0f);

            var closeButton = UiFactory.CreateButton(backgroundRect, "閉じる", Close);
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

            var researchLine = data.IsResearching
                ? $"研究中: {data.ResearchingWordDisplay}（{data.ResearchProgress * 100f:F0}%）"
                : "研究中の単語はありません";

            var boostLine = data.IsResearchBoostActive
                ? $"研究速度アップ中（残り{(int)data.ResearchBoostRemaining.TotalMinutes}分）\n"
                : string.Empty;

            _statusText.text =
                $"{data.Character}\n" +
                $"Lv {data.Level}\n" +
                $"{expLine}\n" +
                $"満腹度 {data.Hunger:F0}\n" +
                $"言霊生産 {data.ProductionRate}/秒\n" +
                boostLine +
                $"{researchLine}";

            _researchButton.interactable = !data.IsResearching;
            _cancelButton.interactable = data.IsResearching;
        }

        private void OnFeedClicked()
        {
            _presenter.Feed(_characterId);
            Refresh();
        }

        private void OnResearchClicked()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            var researchPresenter = new ResearchSelectPresenter(
                gameManager.WordSystem,
                gameManager.DictionarySystem,
                gameManager.ResearchSystem,
                gameManager.PetSystem,
                gameManager.FacilitySystem,
                gameManager.MasterManager);

            ResearchSelectView.Create(transform.parent, researchPresenter, _characterId, Refresh);
        }

        private void OnCancelClicked()
        {
            _presenter.CancelResearch(_characterId);
            Refresh();
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}
