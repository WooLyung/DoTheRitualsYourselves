using RimWorld;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace DoTheRitualsYourselves.RitualPolicies
{
    public abstract class RitualPolicy : IExposable, IRenameable
    {
        public string label = "";

        public int minPawnCount = 1;
        public FloatRange avgMood = new FloatRange(0f, 1f);
        public IntRange time = new IntRange(0, 24);
        public bool invertTime = false;

        public FloatRange pawnHealth = new FloatRange(0f, 1f);
        public FloatRange pawnMood = new FloatRange(0f, 1f);
        public bool exceptResting = false;

        public abstract string Label { get; }
        public abstract bool IsConst { get; }

        public string BaseLabel => Label;

        public string InspectLabel => Label;

        public string RenamableLabel
        {
            get => label;
            set => label = value;
        }

        public RitualPolicy()
        {
        }

        public RitualPolicy(
            int minPawnCount,
            FloatRange avgMood,
            IntRange time,
            bool invertTime,
            FloatRange pawnHealth,
            FloatRange pawnMood,
            bool exceptResting)
        {
            this.minPawnCount = minPawnCount;
            this.avgMood = avgMood;
            this.time = time;
            this.invertTime = invertTime;

            this.pawnHealth = pawnHealth;
            this.pawnMood = pawnMood;
            this.exceptResting = exceptResting;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref label, "DoTheRitualsYourselves.Label", "policy");
            Scribe_Values.Look(ref minPawnCount, "DoTheRitualsYourselves.MinPawnCount", 1);
            Scribe_Values.Look(ref avgMood.min, "DoTheRitualsYourselves.AvgMoodMin", 0f);
            Scribe_Values.Look(ref avgMood.max, "DoTheRitualsYourselves.AvgMoodMax", 1f);
            Scribe_Values.Look(ref time.min, "DoTheRitualsYourselves.TimeMin", 0);
            Scribe_Values.Look(ref time.max, "DoTheRitualsYourselves.TimeMax", 24);
            Scribe_Values.Look(ref invertTime, "DoTheRitualsYourselves.InvertTime", false);
            Scribe_Values.Look(ref pawnHealth.min, "DoTheRitualsYourselves.PawnHealthMin", 0f);
            Scribe_Values.Look(ref pawnHealth.max, "DoTheRitualsYourselves.PawnHealthMax", 0f);
            Scribe_Values.Look(ref pawnMood.min, "DoTheRitualsYourselves.PawnMoodMin", 0f);
            Scribe_Values.Look(ref pawnMood.max, "DoTheRitualsYourselves.PawnMoodMax", 0f);
            Scribe_Values.Look(ref exceptResting, "DoTheRitualsYourselves.ExceptResting", false);
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
            if (!pawnHealth.Includes(pawn?.health?.summaryHealth?.SummaryHealthPercent ?? 0))
                return false;
            if (!pawnMood.Includes(pawn?.needs?.mood?.CurInstantLevel ?? 0))
                return false;
            if (exceptResting && !pawn.Awake())
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
            if (invertTime != (time.min > currentHour || currentHour >= time.max))
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
            if (!this.avgMood.Includes(avgMood))
            {
                reason = "DoTheRitualsYourselves.Reason.WrongMood".Translate();
                return false;
            }

            return true;
        }
    }
}
