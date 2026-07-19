using Cysharp.Threading.Tasks;
using Mojipet.Master;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Mojipet.Managers
{
    public sealed class MasterManager
    {
        public WordMasterSO WordMaster { get; private set; }
        public PetMasterSO PetMaster { get; private set; }
        public ExpMasterSO ExpMaster { get; private set; }
        public ItemMasterSO ItemMaster { get; private set; }
        public FacilityMasterSO FacilityMaster { get; private set; }
        public ShopMasterSO ShopMaster { get; private set; }
        public ResearchMasterSO ResearchMaster { get; private set; }
        public GameBalanceMasterSO GameBalanceMaster { get; private set; }
        public CategoryMasterSO CategoryMaster { get; private set; }

        public async UniTask InitializeAsync()
        {
            WordMaster = await LoadAsync<WordMasterSO>("Master/WordMaster");
            PetMaster = await LoadAsync<PetMasterSO>("Master/PetMaster");
            ExpMaster = await LoadAsync<ExpMasterSO>("Master/ExpMaster");
            ItemMaster = await LoadAsync<ItemMasterSO>("Master/ItemMaster");
            FacilityMaster = await LoadAsync<FacilityMasterSO>("Master/FacilityMaster");
            ShopMaster = await LoadAsync<ShopMasterSO>("Master/ShopMaster");
            ResearchMaster = await LoadAsync<ResearchMasterSO>("Master/ResearchMaster");
            GameBalanceMaster = await LoadAsync<GameBalanceMasterSO>("Master/GameBalanceMaster");
            CategoryMaster = await LoadAsync<CategoryMasterSO>("Master/CategoryMaster");
        }

        private static async UniTask<T> LoadAsync<T>(string address) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            return await handle.ToUniTask();
        }
    }
}
