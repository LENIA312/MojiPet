using Mojipet.UI.Components;
using Mojipet.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mojipet.UI.Views
{
    public sealed class ShopView : MonoBehaviour
    {
        private ShopPresenter _presenter;
        private RectTransform _listContent;

        public static ShopView Create(Transform parent, ShopPresenter presenter)
        {
            var go = new GameObject("ShopWindow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<ShopView>();
            view.Initialize(presenter);
            return view;
        }

        private void Initialize(ShopPresenter presenter)
        {
            _presenter = presenter;
            Build();
            Refresh();
        }

        private void Build()
        {
            var rect = (RectTransform)transform;
            UiFactory.StretchFull(rect);

            var background = UiFactory.CreatePanel(rect, new Color(0f, 0f, 0f, 0.85f));
            var backgroundRect = (RectTransform)background.transform;
            UiFactory.StretchFull(backgroundRect);

            var title = UiFactory.CreateText(backgroundRect, "ショップ", 36, TextAlignmentOptions.Center);
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

            foreach (var row in _presenter.GetRows())
            {
                CreateRow(row);
            }
        }

        private void CreateRow(ShopRowData row)
        {
            var rowGo = new GameObject(row.ItemName + "Row", typeof(RectTransform));
            rowGo.transform.SetParent(_listContent, false);
            var rowRect = (RectTransform)rowGo.transform;

            var layoutElement = rowGo.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80f;
            layoutElement.minHeight = 80f;

            var label = $"{row.ItemName}  所持{row.OwnedCount}個  価格{row.Price:N0}";
            var text = UiFactory.CreateText(rowRect, label, 22, TextAlignmentOptions.Left);
            var textRect = (RectTransform)text.transform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(0.7f, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var shopEntryId = row.ShopEntryId;
            var buyButton = UiFactory.CreateButton(rowRect, "購入", () => OnBuyClicked(shopEntryId));
            var buttonRect = (RectTransform)buyButton.transform;
            buttonRect.anchorMin = new Vector2(0.72f, 0.1f);
            buttonRect.anchorMax = new Vector2(1f, 0.9f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            buyButton.interactable = row.CanPurchase;
        }

        private void OnBuyClicked(int shopEntryId)
        {
            _presenter.Purchase(shopEntryId);
            Refresh();
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}
