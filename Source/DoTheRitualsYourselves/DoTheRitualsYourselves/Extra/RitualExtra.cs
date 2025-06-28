using UnityEngine;
using Verse;

namespace DoTheRitualsYourselves.Extra
{
    public class RitualExtra : IExposable
    {
        public bool autoStart;
        public int policy;
        public int nextCheckTick;

        public void ExposeData()
        {
            Scribe_Values.Look(ref autoStart, "DoTheRitualsYourselves.AutoStart", false);
            Scribe_Values.Look(ref policy, "DoTheRitualsYourselves.Policy", 1);
            Scribe_Values.Look(ref nextCheckTick, "DoTheRitualsYourselves.NextCheckTick", Random.Range(2500, 7500));
        }
    }
}
