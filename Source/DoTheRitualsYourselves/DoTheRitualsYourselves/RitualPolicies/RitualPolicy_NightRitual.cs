using Verse;

namespace DoTheRitualsYourselves.RitualPolicies
{
    public class RitualPolicy_NightRitual : RitualPolicy
    {
        public RitualPolicy_NightRitual() : base("DoTheRitualsYourselves.Policy.NightRitual".Translate(), new IntRange(6, 22), new FloatRange(0f, 1f), 1, 0.0f, true) { }

        public override bool IsStatic => false;
    }
}