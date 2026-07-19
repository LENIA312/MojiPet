using Mojipet.UI.Components;
using Mojipet.UI.Presenters;
using TMPro;
using UnityEngine;

namespace Mojipet.UI.Views
{
    public sealed class SettingsView : MonoBehaviour
    {
        private SettingsPresenter _presenter;
        private TextMeshProUGUI _qualityText;
        private int _qualityIndex;

        public static SettingsView Create(Transform parent, SettingsPresenter presenter)
        {
            var go = new GameObject("SettingsWindow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<SettingsView>();
            view.Initialize(presenter);
            return view;
        }

        private void Initialize(SettingsPresenter presenter)
        {
            _presenter = presenter;
            _qualityIndex = presenter.Quality;
            Build();
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

            var title = UiFactory.CreateText(backgroundRect, "設定", 32, TextAlignmentOptions.Center);
            var titleRect = (RectTransform)title.transform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 60f);
            titleRect.anchoredPosition = new Vector2(0f, -10f);

            var bgmLabel = UiFactory.CreateText(backgroundRect, "BGM音量", 22, TextAlignmentOptions.Left);
            PlaceRow(bgmLabel.transform, 0.62f, 0.78f, 0f, 0.4f);

            var bgmSlider = UiFactory.CreateSlider(backgroundRect, 0f, 1f, _presenter.BgmVolume, _presenter.SetBgmVolume);
            PlaceRow(bgmSlider.transform, 0.62f, 0.78f, 0.42f, 1f);

            var seLabel = UiFactory.CreateText(backgroundRect, "SE音量", 22, TextAlignmentOptions.Left);
            PlaceRow(seLabel.transform, 0.42f, 0.58f, 0f, 0.4f);

            var seSlider = UiFactory.CreateSlider(backgroundRect, 0f, 1f, _presenter.SeVolume, _presenter.SetSeVolume);
            PlaceRow(seSlider.transform, 0.42f, 0.58f, 0.42f, 1f);

            var qualityLabel = UiFactory.CreateText(backgroundRect, "画質", 22, TextAlignmentOptions.Left);
            PlaceRow(qualityLabel.transform, 0.22f, 0.38f, 0f, 0.3f);

            var qualityPrevButton = UiFactory.CreateButton(backgroundRect, "<", () => ChangeQuality(-1));
            PlaceRow(qualityPrevButton.transform, 0.22f, 0.38f, 0.32f, 0.48f);

            _qualityText = UiFactory.CreateText(backgroundRect, GetQualityLabel(), 20, TextAlignmentOptions.Center);
            PlaceRow(_qualityText.transform, 0.22f, 0.38f, 0.5f, 0.82f);

            var qualityNextButton = UiFactory.CreateButton(backgroundRect, ">", () => ChangeQuality(1));
            PlaceRow(qualityNextButton.transform, 0.22f, 0.38f, 0.84f, 1f);

            var closeButton = UiFactory.CreateButton(backgroundRect, "閉じる", Close);
            var closeRect = (RectTransform)closeButton.transform;
            closeRect.anchorMin = new Vector2(0.3f, 0f);
            closeRect.anchorMax = new Vector2(0.7f, 0.12f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
        }

        private static void PlaceRow(Transform target, float anchorYMin, float anchorYMax, float anchorXMin, float anchorXMax)
        {
            var rect = (RectTransform)target;
            rect.anchorMin = new Vector2(anchorXMin, anchorYMin);
            rect.anchorMax = new Vector2(anchorXMax, anchorYMax);
            rect.offsetMin = new Vector2(10f, 4f);
            rect.offsetMax = new Vector2(-10f, -4f);
        }

        private void ChangeQuality(int delta)
        {
            var names = _presenter.QualityNames;
            if (names == null || names.Length == 0)
            {
                return;
            }

            _qualityIndex = (_qualityIndex + delta + names.Length) % names.Length;
            _presenter.SetQuality(_qualityIndex);
            _qualityText.text = GetQualityLabel();
        }

        private string GetQualityLabel()
        {
            var names = _presenter.QualityNames;
            if (names == null || names.Length == 0 || _qualityIndex < 0 || _qualityIndex >= names.Length)
            {
                return "-";
            }

            return names[_qualityIndex];
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}
