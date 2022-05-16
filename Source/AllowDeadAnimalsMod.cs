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

                var section = listing.BeginSection(24 * 4);
                section.CheckboxLabeled("Animal", ref _settings.allowAnimal);
                section.CheckboxLabeled("Insect", ref _settings.allowInsect);
                section.CheckboxLabeled("Humanlike", ref _settings.allowHumanlike);
                section.CheckboxLabeled("Mechanoid", ref _settings.allowMechanoid);
                listing.EndSection(section);

                string mass_threshold_label = _settings.mass_threshold.ToString();
                listing.TextFieldNumericLabeled("Ignore corpses lighter than:", ref _settings.mass_threshold, ref mass_threshold_label);
            }
            listing.End();
            base.DoSettingsWindowContents(rect);
        }

    }
}
