using System;
using Mojipet.UI.Components;
using Mojipet.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mojipet.UI.Views
{
    public sealed class ResearchSelectView : MonoBehaviour
    {
        private ResearchSelectPresenter _presenter;
        private RectTransform _listContent;
        private int _characterId;
        private Action _onStarted;

        public static ResearchSelectView Create(
            Transform parent,
            ResearchSelectPresenter presenter,
            int characterId,
            Action onStarted)
        {
            var go = new GameObject("ResearchSelectWindow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<ResearchSelectView>();
            view.Initialize(presenter, characterId, onStarted);
            return view;
        }

        private void Initialize(ResearchSelectPresenter presenter, int characterId, Action onStarted)
        {
            _presenter = presenter;
            _characterId = characterId;
            _onStarted = onStarted;
            Build();
            Refresh();
        }

        private void Build()
        {
            var rect = (RectTransform)transform;
            UiFactory.StretchFull(rect);

            var background = UiFactory.CreatePanel(rect, new Color(0f, 0f, 0f, 0.9f));
            var backgroundRect = (RectTransform)background.transform;
            UiFactory.StretchFull(backgroundRect);

            var title = UiFactory.CreateText(backgroundRect, "研究する単語を選ぶ", 32, TextAlignmentOptions.Center);
            var titleRect = (RectTransform)title.transform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 80f);
            titleRect.anchoredPosition = Vector2.zero;

            var scrollView = UiFactory.CreateScrollView(backgroundRect, out _listContent);
            scrollView.anchorMin = new Vector2(0f, 0f);
            scrollView.anchorMax = new Vector2(1f, 1f);
            scrollView.offsetMin = new Vector2(20f, 90f);
            scrollView.offsetMax = new Vector2(-20f, -90f);

            var closeButton = UiFactory.CreateButton(backgroundRect, "閉じる", Close);
            var closeRect = (RectTransform)closeButton.transform;
            closeRect.anchorMin = new Vector2(0.5f, 0f);
            closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.pivot = new Vector2(0.5f, 0f);
            closeRect.sizeDelta = new Vector2(200f, 60f);
            closeRect.anchoredPosition = new Vector2(0f, 30f);
        }

        private void Refresh()
        {
            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            var candidates = _presenter.GetCandidates(_characterId);
            if (candidates.Count == 0)
            {
                var empty = UiFactory.CreateText(_listContent, "研究できる単語がありません", 22, TextAlignmentOptions.Left);
                var emptyLayout = empty.gameObject.AddComponent<LayoutElement>();
                emptyLayout.preferredHeight = 40f;
                return;
            }

            foreach (var word in candidates)
            {
                var wordId = word.WordId;
                var label = $"{word.Word}（{word.Reading}） 難易度{word.Difficulty}";

                var button = UiFactory.CreateButton(_listContent, label, () => OnWordClicked(wordId));
                var layoutElement = button.gameObject.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 60f;
                layoutElement.minHeight = 60f;
            }
        }

        private void OnWordClicked(int wordId)
        {
            if (_presenter.StartResearch(_characterId, wordId))
            {
                _onStarted?.Invoke();
                Close();
            }
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}
