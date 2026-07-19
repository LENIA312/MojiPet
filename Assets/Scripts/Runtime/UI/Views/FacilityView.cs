using Mojipet.UI.Components;
using Mojipet.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mojipet.UI.Views
{
    public sealed class FacilityView : MonoBehaviour
    {
        private FacilityPresenter _presenter;
        private RectTransform _listContent;

        public static FacilityView Create(Transform parent, FacilityPresenter presenter)
        {
            var go = new GameObject("FacilityWindow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<FacilityView>();
            view.Initialize(presenter);
            return view;
        }

        private void Initialize(FacilityPresenter presenter)
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

            var title = UiFactory.CreateText(backgroundRect, "施設", 36, TextAlignmentOptions.Center);
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

        public void Refresh()
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

        private void CreateRow(FacilityRowData row)
        {
            var rowGo = new GameObject(row.DisplayName + "Row", typeof(RectTransform));
            rowGo.transform.SetParent(_listContent, false);
            var rowRect = (RectTransform)rowGo.transform;

            var layoutElement = rowGo.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 90f;
            layoutElement.minHeight = 90f;

            var statusLine = row.IsMaxLevel
                ? $"{row.DisplayName}  Lv{row.Level}（最大）  効果 x{row.EffectValue:F2}"
                : $"{row.DisplayName}  Lv{row.Level}  効果 x{row.EffectValue:F2}  強化費用 {row.UpgradeCost:N0}";

            var text = UiFactory.CreateText(rowRect, statusLine, 22, TextAlignmentOptions.Left);
            var textRect = (RectTransform)text.transform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(0.7f, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            if (!row.IsMaxLevel)
            {
                var facilityId = row.FacilityId;
                var upgradeButton = UiFactory.CreateButton(rowRect, "強化", () => OnUpgradeClicked(facilityId));
                var buttonRect = (RectTransform)upgradeButton.transform;
                buttonRect.anchorMin = new Vector2(0.72f, 0.15f);
                buttonRect.anchorMax = new Vector2(1f, 0.85f);
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;

                upgradeButton.interactable = row.CanUpgrade;
            }
        }

        private void OnUpgradeClicked(Mojipet.Models.FacilityId facilityId)
        {
            _presenter.Upgrade(facilityId);
            Refresh();
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}
