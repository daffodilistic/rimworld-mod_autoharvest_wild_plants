using UnityEngine;
using Verse;
using RimWorld;

namespace Daffodilistic.RimWorld.AutoHarvestWildPlants
{
    public class AutoHarvestWildPlantsMod : Mod
    {

        AutoHarvestWildPlantsModSettings _settings;

        public AutoHarvestWildPlantsMod(ModContentPack content)
            : base(content)
            => this._settings = GetSettings<AutoHarvestWildPlantsModSettings>();

        public override string SettingsCategory() => "Autoharvest Wild Crops";

        public override void DoSettingsWindowContents(Rect rect)
        {
            var listing = new Listing_Standard();
            listing.Begin(rect);
            {
                listing.CheckboxLabeled("Allow", ref _settings.allow, "Is auto-allow enabled?");
                listing.CheckboxLabeled("Notify", ref _settings.notify, "Are notifications enabled?");
            }
            listing.End();
            base.DoSettingsWindowContents(rect);
        }

    }
}
