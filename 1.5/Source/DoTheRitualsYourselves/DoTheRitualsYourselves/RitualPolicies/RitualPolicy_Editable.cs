using Verse;

namespace DoTheRitualsYourselves.RitualPolicies
{
    public class RitualPolicy_Editable : RitualPolicy
    {

        public RitualPolicy_Editable() : base() 
        {
        }

        public RitualPolicy_Editable(string label) : base()
        {
            this.label = label;
        }

        public RitualPolicy_Editable(
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
            this.label = label;
        }

        public override bool IsConst => false;

        public override string Label => label;
    }
}
