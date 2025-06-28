using DoTheRitualsYourselves.RitualPolicies;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DoTheRitualsYourselves.WorldComponents
{
    public class WorldComponent_RitualPolicy : WorldComponent
    {
        private Dictionary<int, RitualPolicy> policies = new Dictionary<int, RitualPolicy>()
        {
            { 0, new RitualPolicy_Always() },
            { 1, new RitualPolicy_DaytimeRitual() },
            { 2, new RitualPolicy_NightRitual() }
        };
        private int nextId = 3;

        public static WorldComponent_RitualPolicy Instance => Find.World.GetComponent<WorldComponent_RitualPolicy>();

        public WorldComponent_RitualPolicy(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref policies, "DoTheRitualsYourselves.RitualPolicies", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref nextId, "DoTheRitualsYourselves.NextID");
        }

        public void InsertPolicy(RitualPolicy policy)
        {
            policies.Add(nextId++, policy);
        }

        public void RemovePolicy(RitualPolicy policy)
        {
            int key = -1;
            foreach (var k in policies.Keys)
            {
                if (policies[k] == policy)
                {
                    key = k;
                    break;
                }    
            }

            if (key != -1)
                policies.Remove(key);
        }

        public IEnumerable<int> AllPolicyIds
        {
            get
            {
                List<int> keys = policies.Keys.ToList();
                keys.Sort();
                foreach (int key in keys)
                    yield return key;
            }
        }

        public RitualPolicy GetPolicy(int key)
        {
            if (!policies.ContainsKey(key))
                return null;
            return policies[key];
        }
    }
}
