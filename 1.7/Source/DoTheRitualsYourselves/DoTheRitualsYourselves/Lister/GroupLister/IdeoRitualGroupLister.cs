using DoTheRitualsYourselves.Lister.Group;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DoTheRitualsYourselves.Lister.GroupLister
{
    public class IdeoRitualGroupLister : RitualGroupLister
    {
        public override bool IsSkipped => !ModsConfig.IsActive("ludeon.rimworld.ideology");
        private List<ThingDef> buildingDefs = new List<ThingDef>();

        public IdeoRitualGroupLister()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
                if (IsIdeoRitualBuilding(def))
                    buildingDefs.Add(def);
        }

        private bool IsIdeoRitualBuilding(ThingDef def)
        {
            if (def?.building?.buildingTags?.Contains("RitualFocus") ?? false)
                return true;
            if (def.HasComp<CompGatherSpot>())
                return true;
            if (def.HasComp<CompLightball>())
                return true;
            return false;
        }

        public override IEnumerable<RitualGroup> GetGroups()
        {
            int priority = 1;
            foreach (Ideo ideo in Find.IdeoManager.IdeosListForReading)
                if (Faction.OfPlayer.ideos.AllIdeos.Contains(ideo))
                    yield return new IdeoRitualGroup(this, priority++, ideo);
        }

        public override IEnumerable<ThingDef> GetBuildingDefs()
        {
            return buildingDefs;
        }
    }
}
