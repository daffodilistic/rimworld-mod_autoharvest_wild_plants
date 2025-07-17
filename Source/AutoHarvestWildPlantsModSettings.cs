using Verse;

namespace Daffodilistic.RimWorld.AutoHarvestWildPlants
{
    public class AutoHarvestWildPlantsModSettings : ModSettings
    {

        public bool allow = true;
        public bool notify = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref allow, nameof(allow), defaultValue: true);
            Scribe_Values.Look(ref notify, nameof(notify), defaultValue: true);
            base.ExposeData();
        }

    }
}
