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
        private const int GoalMilestoneStep = 50;

        private Transform _windowLayer;
        private Transform _toastLayer;
        private TextMeshProUGUI _moneyText;
        private TextMeshProUGUI _goalText;

        private void Start()
        {
            UiFactory.CreateEventSystem();

            var canvas = UiFactory.CreateCanvasRoot("Canvas");
            canvas.transform.SetParent(transform, false);

            HomeWorldView.Create(canvas.transform, OpenPetDetail);

            // Anchored to the top of Screen.safeArea so it clears the notch/Dynamic
            // Island/status bar instead of drawing underneath it.
            var safeAreaMax = UiFactory.GetSafeAreaAnchorMax();
            var header = UiFactory.CreatePanel(canvas.transform, UiTheme.Surface);
            header.raycastTarget = false;
            var headerRect = (RectTransform)header.transform;
            headerRect.anchorMin = new Vector2(0f, safeAreaMax.y);
            headerRect.anchorMax = new Vector2(1f, safeAreaMax.y);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, UiTheme.HeaderHeight);
            headerRect.anchoredPosition = Vector2.zero;

            _moneyText = UiFactory.CreateText(header.transform, "言霊 0", 28, TextAlignmentOptions.Left);
            var moneyRect = (RectTransform)_moneyText.transform;
            moneyRect.anchorMin = new Vector2(0f, 0.5f);
            moneyRect.anchorMax = new Vector2(1f, 1f);
            moneyRect.offsetMin = new Vector2(20f, 0f);
            moneyRect.offsetMax = new Vector2(-20f, 0f);

            // Lightweight "what am I working toward" indicator -- the loop is
            // otherwise fully passive (research is automatic) with no visible
            // target to aim for.
            _goalText = UiFactory.CreateText(header.transform, string.Empty, 18, TextAlignmentOptions.Left);
            _goalText.color = UiTheme.TextMuted;
            var goalRect = (RectTransform)_goalText.transform;
            goalRect.anchorMin = new Vector2(0f, 0f);
            goalRect.anchorMax = new Vector2(1f, 0.5f);
            goalRect.offsetMin = new Vector2(20f, 0f);
            goalRect.offsetMax = new Vector2(-20f, 0f);

            // Anchored to the bottom of Screen.safeArea so it clears the home
            // indicator instead of drawing underneath it. Footer placement (rather
            // than header) keeps navigation within thumb reach on a handheld device.
            var safeAreaMin = UiFactory.GetSafeAreaAnchorMin();
            var footer = UiFactory.CreatePanel(canvas.transform, UiTheme.Surface);
            footer.raycastTarget = false;
            var footerRect = (RectTransform)footer.transform;
            footerRect.anchorMin = new Vector2(0f, safeAreaMin.y);
            footerRect.anchorMax = new Vector2(1f, safeAreaMin.y);
            footerRect.pivot = new Vector2(0.5f, 0f);
            footerRect.sizeDelta = new Vector2(0f, UiTheme.FooterHeight);
            footerRect.anchoredPosition = Vector2.zero;

            var buttonRowGo = new GameObject("ButtonRow", typeof(RectTransform), typeof(UnityEngine.UI.HorizontalLayoutGroup));
            buttonRowGo.transform.SetParent(footer.transform, false);
            var buttonRowRect = (RectTransform)buttonRowGo.transform;
            UiFactory.StretchFull(buttonRowRect);
            buttonRowRect.offsetMin = new Vector2(10f, 10f);
            buttonRowRect.offsetMax = new Vector2(-10f, -10f);

            var buttonRowLayout = buttonRowGo.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            buttonRowLayout.childForceExpandWidth = true;
            buttonRowLayout.childForceExpandHeight = true;
            buttonRowLayout.spacing = 8f;

            UiFactory.CreateButton(buttonRowGo.transform, "図鑑", OpenDictionary);
            UiFactory.CreateButton(buttonRowGo.transform, "施設", OpenFacility);
            UiFactory.CreateButton(buttonRowGo.transform, "ショップ", OpenShop);
            UiFactory.CreateButton(buttonRowGo.transform, "持ち物", OpenInventory);
            UiFactory.CreateButton(buttonRowGo.transform, "設定", OpenSettings);

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
                gameManager.EventBus.Subscribe<OnPetUnlocked>(HandlePetUnlocked);
                _moneyText.text = $"言霊 {gameManager.CurrencySystem.GetMoney():N0}";
                RefreshGoalText(gameManager);

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
                gameManager.EventBus.Unsubscribe<OnPetUnlocked>(HandlePetUnlocked);
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
            RefreshGoalText(gameManager);
        }

        private void RefreshGoalText(GameManager gameManager)
        {
            var unlocked = gameManager.DictionarySystem.GetUnlockedCount();
            var total = gameManager.DictionarySystem.GetTotalWordCount();

            if (unlocked >= total)
            {
                _goalText.text = "目標: 図鑑コンプリート達成！";
                return;
            }

            var nextMilestone = (unlocked / GoalMilestoneStep + 1) * GoalMilestoneStep;
            if (nextMilestone > total)
            {
                nextMilestone = total;
            }

            _goalText.text = $"目標: 図鑑{nextMilestone}語まであと{nextMilestone - unlocked}語";
        }

        private void HandlePetLevelUp(OnPetLevelUp e)
        {
            Toast.Show(_toastLayer, $"レベルアップ！ Lv{e.NewLevel}");
        }

        private void HandlePetUnlocked(OnPetUnlocked e)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            var character = "?";
            foreach (var entry in gameManager.MasterManager.PetMaster.Entries)
            {
                if (entry.CharacterId == e.CharacterId)
                {
                    character = entry.Character;
                    break;
                }
            }

            HandwritingView.Create(_windowLayer, e.CharacterId, character);
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

        private void OpenSettings()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            var presenter = new SettingsPresenter(gameManager.SaveSystem);
            SettingsView.Create(_windowLayer, presenter);
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
                gameManager.WordSystem,
                gameManager.CurrencySystem);

            PetDetailView.Create(_windowLayer, presenter, characterId);
        }
    }
}
