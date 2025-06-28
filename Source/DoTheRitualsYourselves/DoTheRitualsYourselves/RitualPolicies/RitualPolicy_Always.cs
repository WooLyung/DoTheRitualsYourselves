using Verse;

namespace DoTheRitualsYourselves.RitualPolicies
{
    public class RitualPolicy_Always : RitualPolicy
    {
        public RitualPolicy_Always() : base("DoTheRitualsYourselves.Policy.Always".Translate(), new IntRange(0, 24), new FloatRange(0f, 1f), 1, 0.0f, false) { }

        public override bool IsStatic => true;
    }
}
