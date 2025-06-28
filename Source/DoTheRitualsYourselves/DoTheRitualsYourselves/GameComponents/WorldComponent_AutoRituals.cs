using DoTheRitualsYourselves.Extra;
using DoTheRitualsYourselves.RitualPolicies;
using DoTheRitualsYourselves.Tool;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DoTheRitualsYourselves.WorldComponents
{
    public class WorldComponent_AutoRituals : WorldComponent
    {
        private Dictionary<int, RitualExtra> extra = new Dictionary<int, RitualExtra>();

        public static WorldComponent_AutoRituals Instance => Find.World.GetComponent<WorldComponent_AutoRituals>();

        public WorldComponent_AutoRituals(World world) : base(world) { }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref extra, "DoTheRitualsYourselves.AutoStart", LookMode.Value, LookMode.Deep);
        }

        public void SetAutoStart(int key, bool value)
        {
            extra[key].autoStart = value;
        }

        public void SetPolicy(int key, int value)
        {
            extra[key].policy = value;
        }

        public bool IsAutoStart(int key)
        {
            return extra[key]?.autoStart ?? false;
        }

        public RitualPolicy GetRitualPolicy(int key)
        {
            if (!extra.ContainsKey(key))
                extra.Add(key, new RitualExtra());
            int policyId = extra[key].policy;

            RitualPolicy policy = WorldComponent_RitualPolicy.Instance.GetPolicy(policyId);
            if (policy == null)
            {
                extra[key].policy = 0;
                return WorldComponent_RitualPolicy.Instance.GetPolicy(0);
            }
            return policy;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
            {
                foreach (Precept_Ritual ritual in RitualStarter.GetRituals(ideo))
                {
                    int id = ritual.Id;
                    if (!extra.ContainsKey(id))
                        extra.Add(id, new RitualExtra());
                    RitualExtra ex = extra[id];

                    ex.nextCheckTick--;
                    if (ex.nextCheckTick <= 0)
                    {
                        if (RitualStarter.TryStart(ritual, ex))
                            ex.nextCheckTick = Random.Range(60000, 180000);
                        else
                            ex.nextCheckTick = Random.Range(2500, 7500);
                    }
                }
            }
        }
    }
}
