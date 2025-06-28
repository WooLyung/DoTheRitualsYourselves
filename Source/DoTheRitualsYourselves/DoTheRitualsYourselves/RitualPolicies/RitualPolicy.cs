using RimWorld;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace DoTheRitualsYourselves.RitualPolicies
{
    public class RitualPolicy : IExposable, IRenameable
    {
        public string label;
        public IntRange allowedHours = new IntRange(0, 24);
        public FloatRange allowedAverageMood = new FloatRange(0f, 1f);
        public int minPawnCount = 1;
        public float minHealth = 0.0f;
        public bool flipTime = false;

        public virtual bool IsStatic => false;

        public string RenamableLabel { get => label; set => label = value; }

        public string BaseLabel => label;

        public string InspectLabel => label;

        public RitualPolicy()
        {
        }

        public RitualPolicy(string label)
        {
            this.label = label; 
        }

        public RitualPolicy(string label, IntRange hours, FloatRange mood, int minCount, float minHealth, bool flipTime)
        {
            this.label = label;
            allowedHours = hours;
            allowedAverageMood = mood;
            minPawnCount = minCount;
            this.minHealth = minHealth;
            this.flipTime = flipTime;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref label, "DoTheRitualsYourselves.Label", "policy");
            Scribe_Values.Look(ref allowedHours.min, "DoTheRitualsYourselves.AllowedHoursMin", 0);
            Scribe_Values.Look(ref allowedHours.max, "DoTheRitualsYourselves.AllowedHoursMax", 24);
            Scribe_Values.Look(ref allowedAverageMood.min, "DoTheRitualsYourselves.AllowedAverageMoodMin", 0f);
            Scribe_Values.Look(ref allowedAverageMood.max, "DoTheRitualsYourselves.AllowedAverageMoodMood", 1f);
            Scribe_Values.Look(ref minHealth, "DoTheRitualsYourselves.MinHealth", 0f);
            Scribe_Values.Look(ref minPawnCount, "DoTheRitualsYourselves.MinPawnCount", 1);
            Scribe_Values.Look(ref flipTime, "DoTheRitualsYourselves.FlipTime", false);
        }

        public virtual bool IsCanJoin(Precept_Ritual ritual, Pawn pawn, bool voluntary = false, bool allowOtherIdeos = false)
        {
            if (pawn.GetLord() != null)
                return false;
            if (pawn.IsMutant)
                return false;
            if (pawn.InMentalState)
                return false;
            if (pawn.Drafted)
                return false;
            if (pawn?.health?.summaryHealth?.SummaryHealthPercent < minHealth)
                return false;
            return !ritual.ritualOnlyForIdeoMembers || ritual.def.allowSpectatorsFromOtherIdeos || pawn.Ideo == ritual.ideo || !voluntary || allowOtherIdeos || pawn.IsPrisonerOfColony || pawn.RaceProps.Animal;
        }

        public virtual bool IsAccept(Precept_Ritual ritual, Map map, ref string reason)
        {
            if (ritual.RepeatPenaltyActive)
            {
                reason = "DoTheRitualsYourselves.Reason.RepeatPenaltyActive".Translate();
                return false;
            }

            int currentHour = GenLocalDate.HourOfDay(map);
            if (flipTime != (allowedHours.min > currentHour || currentHour >= allowedHours.max))
            {
                reason = "DoTheRitualsYourselves.Reason.WrongTime".Translate();
                return false;
            }

            var candidates = map.mapPawns.AllHumanlike.Where(pawn => IsCanJoin(ritual, pawn));
            if (candidates.Count() < minPawnCount)
            {
                reason = "DoTheRitualsYourselves.Reason.InsufficientPawn".Translate();
                return false;
            }

            float avgMood = candidates.Average(pawn => pawn?.needs?.mood?.CurLevel ?? 0f);
            if (!allowedAverageMood.Includes(avgMood))
            {
                reason = "DoTheRitualsYourselves.Reason.WrongMood".Translate();
                return false;
            }

            return true;
        }
    }
}
