using Cysharp.Threading.Tasks;
using Mojipet.Events;
using Mojipet.Managers;
using Mojipet.Save;
using Mojipet.Systems;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Mojipet.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        private const string HomeSceneName = "Home";

        public static GameManager Instance { get; private set; }

        public EventBus EventBus { get; private set; }
        public MasterManager MasterManager { get; private set; }
        public SaveSystem SaveSystem { get; private set; }
        public CurrencySystem CurrencySystem { get; private set; }
        public PetSystem PetSystem { get; private set; }
        public WordSystem WordSystem { get; private set; }
        public DictionarySystem DictionarySystem { get; private set; }
        public ResearchSystem ResearchSystem { get; private set; }
        public IdleSystem IdleSystem { get; private set; }
        public ItemSystem ItemSystem { get; private set; }
        public ShopSystem ShopSystem { get; private set; }
        public FacilitySystem FacilitySystem { get; private set; }
        public HandwritingSystem HandwritingSystem { get; private set; }

        private GameTicker _ticker;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            BootstrapAsync().Forget();
        }

        private async UniTaskVoid BootstrapAsync()
        {
            await Addressables.InitializeAsync().ToUniTask();

            EventBus = new EventBus();

            MasterManager = new MasterManager();
            await MasterManager.InitializeAsync();

            SaveSystem = new SaveSystem(new SaveRepository(), EventBus);
            SaveSystem.Load();

            CurrencySystem = new CurrencySystem(SaveSystem, EventBus);
            FacilitySystem = new FacilitySystem(SaveSystem, MasterManager, CurrencySystem, EventBus);
            PetSystem = new PetSystem(SaveSystem, MasterManager, FacilitySystem, EventBus);
            ItemSystem = new ItemSystem(SaveSystem, PetSystem, MasterManager, EventBus);
            ShopSystem = new ShopSystem(MasterManager, CurrencySystem, ItemSystem, EventBus);
            HandwritingSystem = new HandwritingSystem(SaveSystem, EventBus);

            GrantInitialItemsIfNewGame();
            WordSystem = new WordSystem(MasterManager);
            DictionarySystem = new DictionarySystem(SaveSystem, WordSystem, EventBus);
            ResearchSystem = new ResearchSystem(SaveSystem, MasterManager, WordSystem, PetSystem, DictionarySystem, FacilitySystem, EventBus);
            IdleSystem = new IdleSystem(SaveSystem, PetSystem, ResearchSystem, CurrencySystem, MasterManager, EventBus);

            IdleSystem.CalculateOfflineProgress();
            IdleSystem.ApplyOfflineReward();

            _ticker = new GameTicker(PetSystem, ResearchSystem, CurrencySystem);
            _ticker.Start();

            await SceneManager.LoadSceneAsync(HomeSceneName).ToUniTask();
        }

        private void GrantInitialItemsIfNewGame()
        {
            if (!SaveSystem.WasNewGame)
            {
                return;
            }

            foreach (var itemEntry in MasterManager.ItemMaster.Entries)
            {
                if (itemEntry.ItemType == Mojipet.Models.ItemType.Seed)
                {
                    ItemSystem.AddItem(itemEntry.Id, MasterManager.GameBalanceMaster.InitialSeedCount);
                    break;
                }
            }
        }

        private void OnApplicationQuit()
        {
            _ticker?.Stop();

            if (IdleSystem != null)
            {
                IdleSystem.SaveLoginTime();
            }
            else
            {
                SaveSystem?.Save();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                _ticker?.Stop();

                if (IdleSystem != null)
                {
                    IdleSystem.SaveLoginTime();
                }
                else
                {
                    SaveSystem?.Save();
                }

                return;
            }

            if (IdleSystem == null)
            {
                return;
            }

            IdleSystem.CalculateOfflineProgress();
            IdleSystem.ApplyOfflineReward();
            _ticker?.Start();
        }
    }
}
