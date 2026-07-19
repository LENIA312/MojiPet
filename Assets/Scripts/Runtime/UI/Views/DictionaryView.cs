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
        private TextMeshProUGUI _pageText;
        private Button _prevPageButton;
        private Button _nextPageButton;
        private int _page;

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

            var pageNavGo = new GameObject("PageNav", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            pageNavGo.transform.SetParent(backgroundRect, false);
            var pageNavRect = (RectTransform)pageNavGo.transform;
            pageNavRect.anchorMin = new Vector2(0f, 1f);
            pageNavRect.anchorMax = new Vector2(1f, 1f);
            pageNavRect.pivot = new Vector2(0.5f, 1f);
            pageNavRect.sizeDelta = new Vector2(0f, 50f);
            pageNavRect.anchoredPosition = new Vector2(0f, -130f);

            var pageNavLayout = pageNavGo.GetComponent<HorizontalLayoutGroup>();
            pageNavLayout.childForceExpandWidth = false;
            pageNavLayout.childForceExpandHeight = true;
            pageNavLayout.childAlignment = TextAnchor.MiddleCenter;
            pageNavLayout.spacing = 12f;
            pageNavLayout.padding = new RectOffset(20, 20, 0, 0);

            _prevPageButton = UiFactory.CreateButton(pageNavGo.transform, "＜前へ", OnPrevPageClicked, ButtonStyle.Secondary);
            var prevLayout = _prevPageButton.gameObject.AddComponent<LayoutElement>();
            prevLayout.preferredWidth = 140f;

            _pageText = UiFactory.CreateText(pageNavGo.transform, string.Empty, 22, TextAlignmentOptions.Center);
            var pageTextLayout = _pageText.gameObject.AddComponent<LayoutElement>();
            pageTextLayout.preferredWidth = 160f;

            _nextPageButton = UiFactory.CreateButton(pageNavGo.transform, "次へ＞", OnNextPageClicked, ButtonStyle.Secondary);
            var nextLayout = _nextPageButton.gameObject.AddComponent<LayoutElement>();
            nextLayout.preferredWidth = 140f;

            var scrollView = UiFactory.CreateScrollView(backgroundRect, out _listContent);
            scrollView.anchorMin = new Vector2(0f, 0f);
            scrollView.anchorMax = new Vector2(1f, 1f);
            scrollView.offsetMin = new Vector2(20f, 90f);
            scrollView.offsetMax = new Vector2(-20f, -190f);

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

            var pageCount = _presenter.GetPageCount();
            if (_page >= pageCount)
            {
                _page = pageCount - 1;
            }

            if (_page < 0)
            {
                _page = 0;
            }

            _pageText.text = $"{_page + 1} / {pageCount}";
            _prevPageButton.interactable = _page > 0;
            _nextPageButton.interactable = _page < pageCount - 1;

            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            foreach (var row in _presenter.GetRows(_page))
            {
                CreateRow(row);
            }
        }

        private void OnPrevPageClicked()
        {
            _page--;
            Refresh();
        }

        private void OnNextPageClicked()
        {
            _page++;
            Refresh();
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
