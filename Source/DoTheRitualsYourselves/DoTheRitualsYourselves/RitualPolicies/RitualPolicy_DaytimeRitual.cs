using Verse;

namespace DoTheRitualsYourselves.RitualPolicies
{
    public class RitualPolicy_DaytimeRitual : RitualPolicy
    {
        public RitualPolicy_DaytimeRitual() : base("DoTheRitualsYourselves.Policy.DaytimeRitual".Translate(), new IntRange(6, 22), new FloatRange(0f, 1f), 1, 0.0f, false) { }

        public override bool IsStatic => false;
    }
}
