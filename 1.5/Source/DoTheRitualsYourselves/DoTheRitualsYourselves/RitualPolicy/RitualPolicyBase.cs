using Verse;

namespace DoTheRitualsYourselves.RitualPolicy
{
    public class RitualPolicyBase : IExposable, IRenameable
    {
        public string label;
        public IntRange allowedHours = new IntRange(0, 24);
        public FloatRange allowedAverageMood = new FloatRange(0f, 1f);
        public int minPawnCount = 1;

        public virtual bool IsStatic => false;

        public string RenamableLabel { get => label; set => label = value; }

        public string BaseLabel => label;

        public string InspectLabel => label;

        public RitualPolicyBase()
        {
        }

        public RitualPolicyBase(string label)
        {
            this.label = label; 
        }

        public RitualPolicyBase(string label, IntRange hours, FloatRange mood, int minCount)
        {
            this.label = label;
            allowedHours = hours;
            allowedAverageMood = mood;
            minPawnCount = minCount;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref label, "DoTheRitualsYourselves.Label", "policy");
            Scribe_Values.Look(ref allowedHours.min, "DoTheRitualsYourselves.allowedHoursMin", 0);
            Scribe_Values.Look(ref allowedHours.max, "DoTheRitualsYourselves.allowedHoursMax", 24);
            Scribe_Values.Look(ref allowedAverageMood.min, "DoTheRitualsYourselves.allowedAverageMoodMin", 0f);
            Scribe_Values.Look(ref allowedAverageMood.max, "DoTheRitualsYourselves.allowedAverageMoodMood", 1f);
            Scribe_Values.Look(ref minPawnCount, "minPawnCount", 1);
        }
    }
}
