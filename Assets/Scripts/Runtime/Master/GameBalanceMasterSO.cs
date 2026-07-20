using UnityEngine;

namespace Mojipet.Master
{
    [CreateAssetMenu(fileName = "GameBalanceMaster", menuName = "Mojipet/Master/GameBalanceMaster")]
    public sealed class GameBalanceMasterSO : ScriptableObject
    {
        [SerializeField] private int _maxOfflineHours = 8;
        [SerializeField] private int _maxPetLevel = 100;
        [SerializeField] private int _maxFood = 100;
        [SerializeField] private int _initialSeedCount = 1;
        [SerializeField] private int _defaultResearchSlots = 1;
        [SerializeField] private float _hungerDecayPerHour = 5f;
        [SerializeField] private float _hungerLowThreshold = 40f;
        [SerializeField] private float _hungerLowMultiplier = 0.8f;
        [SerializeField] private float _hungerStarvingMultiplier = 0.5f;
        [SerializeField] private int _baseExp = 20;
        [SerializeField] private float _lengthBonusMultiplier2 = 1.0f;
        [SerializeField] private float _lengthBonusMultiplier3 = 1.1f;
        [SerializeField] private float _lengthBonusMultiplier4 = 1.2f;
        [SerializeField] private float _lengthBonusMultiplier5Plus = 1.3f;
        [SerializeField] private int _cheerCost = 200;
        [SerializeField] private float _cheerMultiplier = 1.3f;
        [SerializeField] private int _cheerDurationSeconds = 180;
        [SerializeField] private int _milestonePercentStep = 5;
        [SerializeField] private int _milestoneBonusPerStep = 2000;

        public int MaxOfflineHours { get => _maxOfflineHours; set => _maxOfflineHours = value; }
        public int MaxPetLevel { get => _maxPetLevel; set => _maxPetLevel = value; }
        public int MaxFood { get => _maxFood; set => _maxFood = value; }
        public int InitialSeedCount { get => _initialSeedCount; set => _initialSeedCount = value; }
        public int DefaultResearchSlots { get => _defaultResearchSlots; set => _defaultResearchSlots = value; }
        public float HungerDecayPerHour { get => _hungerDecayPerHour; set => _hungerDecayPerHour = value; }
        public float HungerLowThreshold { get => _hungerLowThreshold; set => _hungerLowThreshold = value; }
        public float HungerLowMultiplier { get => _hungerLowMultiplier; set => _hungerLowMultiplier = value; }
        public float HungerStarvingMultiplier { get => _hungerStarvingMultiplier; set => _hungerStarvingMultiplier = value; }
        public int BaseExp { get => _baseExp; set => _baseExp = value; }
        public float LengthBonusMultiplier2 { get => _lengthBonusMultiplier2; set => _lengthBonusMultiplier2 = value; }
        public float LengthBonusMultiplier3 { get => _lengthBonusMultiplier3; set => _lengthBonusMultiplier3 = value; }
        public float LengthBonusMultiplier4 { get => _lengthBonusMultiplier4; set => _lengthBonusMultiplier4 = value; }
        public float LengthBonusMultiplier5Plus { get => _lengthBonusMultiplier5Plus; set => _lengthBonusMultiplier5Plus = value; }
        public int CheerCost { get => _cheerCost; set => _cheerCost = value; }
        public float CheerMultiplier { get => _cheerMultiplier; set => _cheerMultiplier = value; }
        public int CheerDurationSeconds { get => _cheerDurationSeconds; set => _cheerDurationSeconds = value; }
        public int MilestonePercentStep { get => _milestonePercentStep; set => _milestonePercentStep = value; }
        public int MilestoneBonusPerStep { get => _milestoneBonusPerStep; set => _milestoneBonusPerStep = value; }
    }
}
