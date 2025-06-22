using Verse;

namespace DoTheRitualsYourselves.RitualPolicy
{
    public class RitualPolicy_Always : RitualPolicyBase
    {
        public RitualPolicy_Always() : base("Always", new IntRange(0, 24), new FloatRange(0f, 1f), 1) { }

        public override bool IsStatic => true;
    }
}
