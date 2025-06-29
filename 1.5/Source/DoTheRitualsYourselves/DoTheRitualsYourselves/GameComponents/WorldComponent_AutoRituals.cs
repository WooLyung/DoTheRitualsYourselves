using DoTheRitualsYourselves.Core;
using DoTheRitualsYourselves.Extra;
using DoTheRitualsYourselves.RitualPolicies;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DoTheRitualsYourselves.WorldComponents
{
    public class WorldComponent_AutoRituals : WorldComponent
    {
        private Dictionary<int, RitualExtraData> extraDatas = new Dictionary<int, RitualExtraData>();

        public static WorldComponent_AutoRituals Instance => Find.World.GetComponent<WorldComponent_AutoRituals>();

        public WorldComponent_AutoRituals(World world) : base(world) { }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref extraDatas, "DoTheRitualsYourselves.ExtraDatas", LookMode.Value, LookMode.Deep);
        }

        public void SetAutoStart(int ritualID, bool value)
        {
            extraDatas[ritualID].autoStart = value;
        }

        public void SetPolicy(int ritualID, int value)
        {
            extraDatas[ritualID].policyID = value;
        }

        public bool IsAutoStart(int ritualID)
        {
            return extraDatas[ritualID]?.autoStart ?? false;
        }

        public RitualPolicy GetRitualPolicy(int ritualID)
        {
            if (!extraDatas.ContainsKey(ritualID))
                extraDatas.Add(ritualID, new RitualExtraData());
            int policyID = extraDatas[ritualID].policyID;

            RitualPolicy policy = WorldComponent_RitualPolicy.Instance.GetPolicy(policyID);
            if (policy == null)
            {
                extraDatas[ritualID].policyID = 0;
                return WorldComponent_RitualPolicy.Instance.GetPolicy(0);
            }
            return policy;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
            {
                foreach (Precept_Ritual ritual in ideo.GetRituals())
                {
                    int id = ritual.Id;
                    if (!extraDatas.ContainsKey(id))
                        extraDatas.Add(id, new RitualExtraData());
                    RitualExtraData ex = extraDatas[id];

                    ex.nextCheckTick--;
                    if (ex.nextCheckTick <= 0)
                    {
                        if (ritual.TryStart())
                            ex.nextCheckTick = Random.Range(60000, 180000);
                        else
                            ex.nextCheckTick = Random.Range(2500, 7500);
                    }
                }
            }
        }
    }
}
