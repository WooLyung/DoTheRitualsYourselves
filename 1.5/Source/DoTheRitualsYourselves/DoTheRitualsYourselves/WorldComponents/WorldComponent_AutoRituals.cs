using DoTheRitualsYourselves.RitualPolicy;
using DoTheRitualsYourselves.Tool;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DoTheRitualsYourselves.WorldComponents
{
    public class WorldComponent_AutoRituals : WorldComponent
    {
        private Dictionary<int, bool> autoStart = new Dictionary<int, bool>();
        private Dictionary<int, int> policies = new Dictionary<int, int>();

        private static WorldComponent_AutoRituals _instance = null;
        public static WorldComponent_AutoRituals Instance {
            get
            {
                if (_instance == null)
                    _instance = Find.World.GetComponent<WorldComponent_AutoRituals>();
                return _instance;
            }
        }

        public WorldComponent_AutoRituals(World world) : base(world) { }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref autoStart, "DoTheRitualsYourselves.AutoStart", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref policies, "DoTheRitualsYourselves.Policies", LookMode.Value, LookMode.Value);
        }

        public void SetAutoStart(int key, bool value)
        {
            autoStart[key] = value;
        }

        public void SetPolicy(int key, int value)
        {
            policies[key] = value;
        }

        public bool IsAutoStart(int key)
        {
            return autoStart.TryGetValue(key, false);
        }

        public RitualPolicyBase GetRitualPolicy(int key)
        {
            if (!policies.ContainsKey(key))
                policies.Add(key, 0);
            int policyId = policies[key];

            RitualPolicyBase policy = WorldComponent_RitualPolicy.Instance.GetPolicy(policyId);
            if (policy == null)
            {
                policies[key] = 0;
                return WorldComponent_RitualPolicy.Instance.GetPolicy(0);
            }
            return policy;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (Find.TickManager.TicksGame % 15000 == 0) // 6시간마다
                RitualStarter.TryStart();
        }
    }
}
