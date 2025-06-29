using UnityEngine;
using Verse;

namespace DoTheRitualsYourselves.Extra
{
    public class RitualExtraData : IExposable
    {
        public bool autoStart;
        public int policyID;
        public int nextCheckTick;

        public void ExposeData()
        {
            Scribe_Values.Look(ref autoStart, "DoTheRitualsYourselves.AutoStart", false);
            Scribe_Values.Look(ref policyID, "DoTheRitualsYourselves.PolicyID", 1);
            Scribe_Values.Look(ref nextCheckTick, "DoTheRitualsYourselves.NextCheckTick", Random.Range(2500, 7500));
        }
    }
}
