using Verse;

namespace Daffodilistic.RimWorld.AutoHarvestWildPlants
{
    public class AutoHarvestWildPlantsModSettings : ModSettings
    {

        public bool allow = true;
        public bool allowAnimal = true;
        public bool allowInsect = false;
        public bool allowHumanlike = false;
        public bool allowMechanoid = false;
        public bool notify = true;
        public float mass_threshold = 10;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref allow, nameof(allow), defaultValue: true);
            Scribe_Values.Look(ref notify, nameof(notify), defaultValue: true);
            base.ExposeData();
        }

    }
}
