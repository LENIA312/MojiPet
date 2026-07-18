using Mojipet.Core;
using Mojipet.Events;
using Mojipet.UI.Components;
using Mojipet.UI.Presenters;
using Mojipet.UI.Views;
using TMPro;
using UnityEngine;

namespace Mojipet.UI
{
    public sealed class HomeUIRoot : MonoBehaviour
    {
        private Transform _windowLayer;
        private Transform _toastLayer;
        private TextMeshProUGUI _moneyText;

        private void Start()
        {
            UiFactory.CreateEventSystem();

            var canvas = UiFactory.CreateCanvasRoot("Canvas");
            canvas.transform.SetParent(transform, false);

            HomeWorldView.Create(canvas.transform, OpenPetDetail);

            var header = UiFactory.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0.6f));
            var headerRect = (RectTransform)header.transform;
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, 100f);
            headerRect.anchoredPosition = Vector2.zero;

            _moneyText = UiFactory.CreateText(header.transform, "言霊 0", 28, TextAlignmentOptions.Left);
            var moneyRect = (RectTransform)_moneyText.transform;
            moneyRect.anchorMin = new Vector2(0f, 0f);
            moneyRect.anchorMax = new Vector2(0.6f, 1f);
            moneyRect.offsetMin = new Vector2(20f, 0f);
            moneyRect.offsetMax = Vector2.zero;

            var dictionaryButton = UiFactory.CreateButton(header.transform, "図鑑", OpenDictionary);
            var dictionaryButtonRect = (RectTransform)dictionaryButton.transform;
            dictionaryButtonRect.anchorMin = new Vector2(1f, 0.5f);
            dictionaryButtonRect.anchorMax = new Vector2(1f, 0.5f);
            dictionaryButtonRect.pivot = new Vector2(1f, 0.5f);
            dictionaryButtonRect.sizeDelta = new Vector2(160f, 70f);
            dictionaryButtonRect.anchoredPosition = new Vector2(-20f, 0f);

            var facilityButton = UiFactory.CreateButton(header.transform, "施設", OpenFacility);
            var facilityButtonRect = (RectTransform)facilityButton.transform;
            facilityButtonRect.anchorMin = new Vector2(1f, 0.5f);
            facilityButtonRect.anchorMax = new Vector2(1f, 0.5f);
            facilityButtonRect.pivot = new Vector2(1f, 0.5f);
            facilityButtonRect.sizeDelta = new Vector2(160f, 70f);
            facilityButtonRect.anchoredPosition = new Vector2(-200f, 0f);

            var shopButton = UiFactory.CreateButton(header.transform, "ショップ", OpenShop);
            var shopButtonRect = (RectTransform)shopButton.transform;
            shopButtonRect.anchorMin = new Vector2(1f, 0.5f);
            shopButtonRect.anchorMax = new Vector2(1f, 0.5f);
            shopButtonRect.pivot = new Vector2(1f, 0.5f);
            shopButtonRect.sizeDelta = new Vector2(160f, 70f);
            shopButtonRect.anchoredPosition = new Vector2(-380f, 0f);

            var inventoryButton = UiFactory.CreateButton(header.transform, "持ち物", OpenInventory);
            var inventoryButtonRect = (RectTransform)inventoryButton.transform;
            inventoryButtonRect.anchorMin = new Vector2(1f, 0.5f);
            inventoryButtonRect.anchorMax = new Vector2(1f, 0.5f);
            inventoryButtonRect.pivot = new Vector2(1f, 0.5f);
            inventoryButtonRect.sizeDelta = new Vector2(160f, 70f);
            inventoryButtonRect.anchoredPosition = new Vector2(-560f, 0f);

            var toastLayerGo = new GameObject("Toasts", typeof(RectTransform));
            toastLayerGo.transform.SetParent(canvas.transform, false);
            _toastLayer = toastLayerGo.transform;
            UiFactory.StretchFull((RectTransform)_toastLayer);

            var windowLayerGo = new GameObject("Windows", typeof(RectTransform));
            windowLayerGo.transform.SetParent(canvas.transform, false);
            _windowLayer = windowLayerGo.transform;
            UiFactory.StretchFull((RectTransform)_windowLayer);

            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.EventBus.Subscribe<OnMoneyChanged>(HandleMoneyChanged);
                gameManager.EventBus.Subscribe<OnResearchCompleted>(HandleResearchCompleted);
                gameManager.EventBus.Subscribe<OnPetLevelUp>(HandlePetLevelUp);
                _moneyText.text = $"言霊 {gameManager.CurrencySystem.GetMoney():N0}";

                ShowOfflineRewardToastIfAny(gameManager);
            }
        }

        private void ShowOfflineRewardToastIfAny(GameManager gameManager)
        {
            var idleSystem = gameManager.IdleSystem;
            if (idleSystem == null || idleSystem.ElapsedTime <= System.TimeSpan.Zero)
            {
                return;
            }

            var hours = (int)idleSystem.ElapsedTime.TotalHours;
            var minutes = idleSystem.ElapsedTime.Minutes;
            Toast.Show(_toastLayer, $"{hours}時間{minutes}分放置しました！ 言霊 +{idleSystem.RewardMoney:N0}");
        }

        private void OnDestroy()
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.EventBus != null)
            {
                gameManager.EventBus.Unsubscribe<OnMoneyChanged>(HandleMoneyChanged);
                gameManager.EventBus.Unsubscribe<OnResearchCompleted>(HandleResearchCompleted);
                gameManager.EventBus.Unsubscribe<OnPetLevelUp>(HandlePetLevelUp);
            }
        }

        private void HandleMoneyChanged(OnMoneyChanged e)
        {
            _moneyText.text = $"言霊 {e.CurrentMoney:N0}";
        }

        private void HandleResearchCompleted(OnResearchCompleted e)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            var word = gameManager.WordSystem.GetWord(e.WordId);
            Toast.Show(_toastLayer, $"新しいことば！ {word.Word}");
        }

        private void HandlePetLevelUp(OnPetLevelUp e)
        {
            Toast.Show(_toastLayer, $"レベルアップ！ Lv{e.NewLevel}");
        }

        private void OpenDictionary()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            var presenter = new DictionaryPresenter(gameManager.DictionarySystem, gameManager.WordSystem);
            DictionaryView.Create(_windowLayer, presenter);
        }

        private void OpenFacility()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            var presenter = new FacilityPresenter(gameManager.FacilitySystem);
            FacilityView.Create(_windowLayer, presenter);
        }

        private void OpenShop()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            var presenter = new ShopPresenter(gameManager.ShopSystem, gameManager.ItemSystem, gameManager.MasterManager);
            ShopView.Create(_windowLayer, presenter);
        }

        private void OpenInventory()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            var presenter = new InventoryPresenter(gameManager.ItemSystem, gameManager.MasterManager);
            InventoryView.Create(_windowLayer, presenter, _toastLayer);
        }

        private void OpenPetDetail(int characterId)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            var presenter = new PetDetailPresenter(
                gameManager.PetSystem,
                gameManager.ItemSystem,
                gameManager.MasterManager,
                gameManager.ResearchSystem,
                gameManager.WordSystem);

            PetDetailView.Create(_windowLayer, presenter, characterId);
        }
    }
}
