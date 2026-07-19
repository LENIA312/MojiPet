using System;
using Mojipet.UI.Components;
using Mojipet.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mojipet.UI.Views
{
    public sealed class InventoryView : MonoBehaviour
    {
        private InventoryPresenter _presenter;
        private RectTransform _listContent;
        private Transform _toastLayer;

        public static InventoryView Create(Transform parent, InventoryPresenter presenter, Transform toastLayer)
        {
            var go = new GameObject("InventoryWindow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<InventoryView>();
            view.Initialize(presenter, toastLayer);
            return view;
        }

        private void Initialize(InventoryPresenter presenter, Transform toastLayer)
        {
            _presenter = presenter;
            _toastLayer = toastLayer;
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

            var title = UiFactory.CreateText(backgroundRect, "持ち物", 36, TextAlignmentOptions.Center);
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

            var closeButton = UiFactory.CreateButton(backgroundRect, "閉じる", Close, ButtonStyle.Secondary);
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

            var rows = _presenter.GetRows();
            if (rows.Count == 0)
            {
                var empty = UiFactory.CreateText(_listContent, "何も持っていません", 22, TextAlignmentOptions.Left);
                var emptyLayout = empty.gameObject.AddComponent<LayoutElement>();
                emptyLayout.preferredHeight = 40f;
                return;
            }

            foreach (var row in rows)
            {
                CreateRow(row);
            }
        }

        private void CreateRow(InventoryRowData row)
        {
            var rowGo = new GameObject(row.Name + "Row", typeof(RectTransform));
            rowGo.transform.SetParent(_listContent, false);
            var rowRect = (RectTransform)rowGo.transform;

            var layoutElement = rowGo.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80f;
            layoutElement.minHeight = 80f;

            var label = $"{row.Name}  所持{row.Count}個";
            var text = UiFactory.CreateText(rowRect, label, 22, TextAlignmentOptions.Left);
            var textRect = (RectTransform)text.transform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(0.7f, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            if (row.CanUseDirectly)
            {
                var itemId = row.ItemId;
                var useButton = UiFactory.CreateButton(rowRect, "使う", () => OnUseClicked(itemId));
                var buttonRect = (RectTransform)useButton.transform;
                buttonRect.anchorMin = new Vector2(0.72f, 0.1f);
                buttonRect.anchorMax = new Vector2(1f, 0.9f);
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;
            }
        }

        private void OnUseClicked(int itemId)
        {
            try
            {
                _presenter.UseDirectly(itemId);
            }
            catch (InvalidOperationException e)
            {
                Toast.Show(_toastLayer, e.Message);
            }

            Refresh();
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}
