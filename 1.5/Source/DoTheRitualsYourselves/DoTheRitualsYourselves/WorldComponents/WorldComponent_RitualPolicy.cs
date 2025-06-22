using DoTheRitualsYourselves.RitualPolicy;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DoTheRitualsYourselves.WorldComponents
{
    public class WorldComponent_RitualPolicy : WorldComponent
    {
        private Dictionary<int, RitualPolicyBase> policies = new Dictionary<int, RitualPolicyBase>()
        {
            { 0, new RitualPolicy_Always() },
            { 1, new RitualPolicyBase("ASDF") }
        };
        private int nextId = 2;

        private static WorldComponent_RitualPolicy _instance = null;
        public static WorldComponent_RitualPolicy Instance {
            get
            {
                if (_instance == null)
                    _instance = Find.World.GetComponent<WorldComponent_RitualPolicy>();
                return _instance;
            }
        }

        public WorldComponent_RitualPolicy(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref policies, "DoTheRitualsYourselves.RitualPolicies", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref nextId, "DoTheRitualsYourselves.NextID");
        }

        public void InsertPolicy(RitualPolicyBase policy)
        {
            policies.Add(nextId++, policy);
        }

        public void RemovePolicy(RitualPolicyBase policy)
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

        public RitualPolicyBase GetPolicy(int key)
        {
            if (!policies.ContainsKey(key))
                return null;
            return policies[key];
        }
    }
}
