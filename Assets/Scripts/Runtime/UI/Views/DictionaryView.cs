using Mojipet.UI.Components;
using Mojipet.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mojipet.UI.Views
{
    public sealed class DictionaryView : MonoBehaviour
    {
        private DictionaryPresenter _presenter;
        private RectTransform _listContent;
        private TextMeshProUGUI _completionText;

        public static DictionaryView Create(Transform parent, DictionaryPresenter presenter)
        {
            var go = new GameObject("DictionaryWindow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<DictionaryView>();
            view.Initialize(presenter);
            return view;
        }

        private void Initialize(DictionaryPresenter presenter)
        {
            _presenter = presenter;
            Build();
            Refresh();
        }

        private void Build()
        {
            var rect = (RectTransform)transform;
            UiFactory.StretchFull(rect);

            var background = UiFactory.CreatePanel(rect, UiTheme.WindowBackground);
            var backgroundRect = (RectTransform)background.transform;
            UiFactory.StretchFull(backgroundRect);

            var title = UiFactory.CreateText(backgroundRect, "図鑑", 36, TextAlignmentOptions.Center);
            var titleRect = (RectTransform)title.transform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 80f);
            titleRect.anchoredPosition = Vector2.zero;

            _completionText = UiFactory.CreateText(backgroundRect, string.Empty, 24, TextAlignmentOptions.Center);
            var completionRect = (RectTransform)_completionText.transform;
            completionRect.anchorMin = new Vector2(0f, 1f);
            completionRect.anchorMax = new Vector2(1f, 1f);
            completionRect.pivot = new Vector2(0.5f, 1f);
            completionRect.sizeDelta = new Vector2(0f, 50f);
            completionRect.anchoredPosition = new Vector2(0f, -80f);

            var scrollView = UiFactory.CreateScrollView(backgroundRect, out _listContent);
            scrollView.anchorMin = new Vector2(0f, 0f);
            scrollView.anchorMax = new Vector2(1f, 1f);
            scrollView.offsetMin = new Vector2(20f, 90f);
            scrollView.offsetMax = new Vector2(-20f, -150f);

            var closeButton = UiFactory.CreateButton(backgroundRect, "閉じる", Close, ButtonStyle.Secondary);
            var closeRect = (RectTransform)closeButton.transform;
            closeRect.anchorMin = new Vector2(0.5f, 0f);
            closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.pivot = new Vector2(0.5f, 0f);
            closeRect.sizeDelta = new Vector2(200f, 60f);
            closeRect.anchoredPosition = new Vector2(0f, 30f);
        }

        public void Refresh()
        {
            _completionText.text =
                $"{_presenter.GetUnlockedCount()} / {_presenter.GetTotalCount()} ({_presenter.GetCompletionRate() * 100f:F1}%)";

            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            foreach (var row in _presenter.GetRows())
            {
                CreateRow(row);
            }
        }

        private void CreateRow(DictionaryRowData row)
        {
            var label = row.Unlocked
                ? $"{row.DisplayWord}（{row.Reading}） 難易度{row.Difficulty} {row.Category}"
                : row.DisplayWord;

            var text = UiFactory.CreateText(_listContent, label, 22, TextAlignmentOptions.Left);
            var layoutElement = text.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 40f;
            layoutElement.minHeight = 40f;
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}
