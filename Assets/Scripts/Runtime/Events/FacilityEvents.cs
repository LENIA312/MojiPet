using Mojipet.Models;

namespace Mojipet.Events
{
    public readonly struct OnFacilityUpgraded
    {
        public readonly FacilityId FacilityId;
        public readonly int Level;

        public OnFacilityUpgraded(FacilityId facilityId, int level)
        {
            FacilityId = facilityId;
            Level = level;
        }
    }

    public readonly struct OnFacilityMaxLevel
    {
        public readonly FacilityId FacilityId;

        public OnFacilityMaxLevel(FacilityId facilityId)
        {
            FacilityId = facilityId;
        }
    }
}
