using RimWorld;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Daffodilistic.RimWorld.AutoHarvestWildPlants
{
    public class ObjectCopier
    {
        public static object ShallowCopy(object o)
        {
            return o?.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(o, null);
        }
    }
    public class AutoHarvestWildPlantsMapComponent : MapComponent
    {
        const int k_ticks_threshold = 1000;
        int _ticks = 0;

        AutoHarvestWildPlantsModSettings _settings = null;
        TickManager _tickManager = null;
        ColorDef _autoharvestColor = null;
        DesignationDef _autoharvestDesignation = null;

        public AutoHarvestWildPlantsMapComponent(Map map)
            : base(map)
        {
            _settings = LoadedModManager
                .GetMod<AutoHarvestWildPlantsMod>()
                .GetSettings<AutoHarvestWildPlantsModSettings>();
            _tickManager = Find.TickManager;
            _autoharvestColor = new ColorDef
            {
                colorType = ColorType.Misc,
                color = UnityEngine.Color.magenta,
                displayOrder = int.MaxValue,
                displayInStylingStationUI = false,
                randomlyPickable = false
            };
            _autoharvestDesignation = (DesignationDef)ObjectCopier.ShallowCopy(DesignationDefOf.CutPlant);
            _autoharvestDesignation.defName = "AutoharvestWildPlants";
            _autoharvestDesignation.ResolveDefNameHash();

            // 
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
            LogMessage("BEGIN");
            Faction playerFaction = Faction.OfPlayer;
            int ticksGame = _tickManager.TicksGame;

            bool allow = _settings.allow;
            bool notify = _settings.notify;

            if (!allow && !notify) return;

            var list = map.listerThings.ThingsInGroup(ThingRequestGroup.HarvestablePlant);
            LogMessage("Things count:" + list.Count);
            var plantTargets = new LookTargets();
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
                        var exists = false;
                        foreach (var designation in map.designationManager.AllDesignationsOn(wildPlant))
                        {
                            if (designation.def.defName == _autoharvestDesignation.defName)
                            {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists)
                        {
                            var newDesignation = new Designation(wildPlant, _autoharvestDesignation, _autoharvestColor);
                            map.designationManager.AddDesignation(newDesignation);
                        }
                    }

                    plantTargets.targets.Add(new TargetInfo(wildPlant));
                }
            }

            if (notify && plantTargets.targets.Count > 0)
            {
                Messages.Message(text: "RipePlantSpottedMultiple".Translate(), lookTargets: plantTargets, def: MessageTypeDefOf.NeutralEvent);
            }

            LogMessage("END");
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
            bool isNotBlighted = !plant.Blighted;

            if (
                (isFood || isMedicine)
                && isNotZoned
                && isNotDesignated
                && isNotBlighted
            )
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
