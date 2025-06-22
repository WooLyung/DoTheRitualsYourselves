using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace DoTheRitualsYourselves.WorldComponents
{
    public class WorldComponent_AutoRituals : WorldComponent
    {
        private Dictionary<int, bool> autoStart = new Dictionary<int, bool>();

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
        }

        public void Update(int key, bool value)
        {
            autoStart[key] = value;
        }

        public bool IsAutoStart(int key)
        {
            return autoStart.TryGetValue(key, false);
        }
    }
}
