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

        /// <remarks> TODO should use a list with weak references for automatic GC? </remarks>
        HashSet<Plant> _allowedAlready;

        AutoHarvestWildPlantsModSettings _settings = null;
        TickManager _tickManager = null;

        public AutoHarvestWildPlantsMapComponent(Map map)
            : base(map)
        {
            _settings = LoadedModManager
                .GetMod<AutoHarvestWildPlantsMod>()
                .GetSettings<AutoHarvestWildPlantsModSettings>();
            _allowedAlready = new HashSet<Plant>();
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

        /// <remarks> Things returned from ThingsInGroup() is NOT thread-safe so EXPECT it can be changed by diffent CPU thread, mid-execution, anytime here.</remarks>
        void AutoHarvestWildPlants()
        {
            // Log.Message("[AutoHarvestWildPlants] AutoHarvestWildPlants() BEGIN");
            Faction playerFaction = Faction.OfPlayer;
            float massThreshold = _settings.mass_threshold;
            int ticksGame = _tickManager.TicksGame;

            bool allow = _settings.allow;
            bool notify = _settings.notify;
            bool allowAnimal = _settings.allowAnimal;
            bool allowInsect = _settings.allowInsect;
            bool allowHumanlike = _settings.allowHumanlike;
            bool allowMechanoid = _settings.allowMechanoid;

            if (!allow && !notify) return;// lets not fill _allowedAlready for nothing

            var list = map.listerThings.ThingsInGroup(ThingRequestGroup.HarvestablePlant);
            // Log.Message("[AutoHarvestWildPlants] AutoHarvestWildPlants() list.Count:" + list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                if (
                    (Plant)list[i] is Plant wildPlant
                    && !_allowedAlready.Contains(wildPlant)
                    && wildPlant.Growth == 1.0f
                    && validHarvestTarget(wildPlant)
                )
                {
                    _allowedAlready.Add(wildPlant);

                    if (allow)
                        map.designationManager.AddDesignation(new Designation(wildPlant, DesignationDefOf.HarvestPlant));
                    else
                    {
                        if (notify)
                            Messages.Message(
                            $"{wildPlant.LabelCap} is not allowed to be harvested.",
                            wildPlant,
                            MessageTypeDefOf.NegativeEvent
                            );
                    }

                    if (notify)
                    Messages.Message(text: "RipePlantSpotted".Translate((NamedArgument)wildPlant.LabelShort), lookTargets: wildPlant, def: MessageTypeDefOf.NeutralEvent);
                }
            }
            // Log.Message("[AutoHarvestWildPlants] AutoHarvestWildPlants() END");
        }

        bool validHarvestTarget(Plant plant)
        {
            // Log.Message("[AutoHarvestWildPlants] validHarvestTarget() wildPlant: " + plant.Label +
            // "\nPurpose:" + plant.def.plant.purpose.ToString().ToLower());

            bool isFood = plant.def.plant.purpose.ToString().ToLower() == "food";
            bool isNotZoned = !plant.IsCrop;
            // NOTE Wild Healroots do not have the "health" purpose set
            bool isMedicine = plant.def.plant.purpose.ToString().ToLower() == "health" || plant.Label.ToLower() == "wild healroot";
            
            if ((isFood || isMedicine) && isNotZoned)
            {
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref _allowedAlready, false, nameof(_allowedAlready), LookMode.Reference);
            base.ExposeData();
        }

    }
}
