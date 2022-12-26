using System.Threading.Tasks;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Daffodilistic.RimWorld.AutoHarvestWildPlants
{
    public class AutoHarvestWildPlantsMapComponent : MapComponent
    {
        const int k_ticks_threshold = 1000;
        int _ticks = 0;

        AutoHarvestWildPlantsModSettings _settings = null;
        TickManager _tickManager = null;

        public AutoHarvestWildPlantsMapComponent(Map map)
            : base(map)
        {
            _settings = LoadedModManager
                .GetMod<AutoHarvestWildPlantsMod>()
                .GetSettings<AutoHarvestWildPlantsModSettings>();
            _tickManager = Find.TickManager;
        }

        public override void MapComponentTick()
        {
            if (++_ticks == k_ticks_threshold)
            {
                Task.Run(AutoHarvestWildPlants);
                _ticks = 0;
            }
        }

        private static void LogMessage(string message)
        {
#if DEBUG
            Log.Message("[AutoHarvestWildPlants] " + message);
#endif
        }

        /// <remarks> Things returned from ThingsInGroup() is NOT thread-safe so EXPECT it can be changed by diffent CPU thread, mid-execution, anytime here.</remarks>
        void AutoHarvestWildPlants()
        {
            LogMessage("[AutoHarvestWildPlants] AutoHarvestWildPlants() BEGIN");
            Faction playerFaction = Faction.OfPlayer;
            float massThreshold = _settings.mass_threshold;
            int ticksGame = _tickManager.TicksGame;

            bool allow = _settings.allow;
            bool notify = _settings.notify;

            if (!allow && !notify) return;

            var list = map.listerThings.ThingsInGroup(ThingRequestGroup.HarvestablePlant);
            LogMessage("[AutoHarvestWildPlants] AutoHarvestWildPlants() list.Count:" + list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                if (
                    (Plant)list[i] is Plant wildPlant
                    && wildPlant.Growth == 1.0f
                    && validHarvestTarget(wildPlant)
                )
                {
                    if (allow)
                    {
                        map.designationManager.AddDesignation(new Designation(wildPlant, DesignationDefOf.HarvestPlant));
                    }

                    if (notify)
                    {
                        Messages.Message(text: "RipePlantSpotted".Translate((NamedArgument)wildPlant.LabelShort), lookTargets: wildPlant, def: MessageTypeDefOf.NeutralEvent);
                    }
                }
            }
            LogMessage("[AutoHarvestWildPlants] AutoHarvestWildPlants() END");
        }

        bool validHarvestTarget(Plant plant)
        {
            // LogMessage("[AutoHarvestWildPlants] validHarvestTarget() wildPlant: " + plant.Label +
            // "\nPurpose:" + plant.def.plant.purpose.ToString().ToLower());

            bool isFood = plant.def.plant.purpose.ToString().ToLower() == "food";
            bool isNotZoned = !plant.IsCrop;
            // NOTE Wild Healroots do not have the "health" purpose set
            bool isMedicine = plant.def.plant.purpose.ToString().ToLower() == "health" || plant.Label.ToLower() == "wild healroot";
            // Check if plant is not designated already
            bool isNotDesignated = map.designationManager.DesignationOn(plant, DesignationDefOf.HarvestPlant) == null;

            if ((isFood || isMedicine) && isNotZoned && isNotDesignated)
            {
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            // No data is exposed since  allowedAlready should
            // not be saved to savegame file
            base.ExposeData();
        }

    }
}
