using Verse;

namespace DoTheRitualsYourselves.RitualPolicies
{
    public class RitualPolicy_Always : RitualPolicy
    {
        public RitualPolicy_Always() : base()
        {
        }


        public RitualPolicy_Always(
            string label,
            int minPawnCount,
            FloatRange avgMood,
            IntRange time,
            bool invertTime,
            FloatRange pawnHealth,
            FloatRange pawnMood,
            bool exceptResting) 
            : base(minPawnCount, avgMood, time, invertTime, pawnHealth, pawnMood, exceptResting)
        {
        }

        public override bool IsConst => true;

        public override string Label => "DoTheRitualsYourselves.Policy.Always".Translate();
    }
}
