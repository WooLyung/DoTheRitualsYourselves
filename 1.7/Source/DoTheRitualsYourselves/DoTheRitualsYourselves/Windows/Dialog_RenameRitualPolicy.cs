using DoTheRitualsYourselves.RitualPolicies;
using RimWorld;
using System.Text.RegularExpressions;
using Verse;

namespace DoTheRitualsYourselves.Windows
{
    public class Dialog_RenameRitualPolicy : Dialog_Rename<RitualPolicy>
    {
        private static readonly Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-]*$");

        public Dialog_RenameRitualPolicy(RitualPolicy policy)
            : base(policy)
        {
        }

        protected override AcceptanceReport NameIsValid(string name)
        {
            AcceptanceReport result = base.NameIsValid(name);
            if (!result.Accepted)
            {
                return result;
            }

            if (!ValidNameRegex.IsMatch(name))
            {
                return "InvalidName".Translate();
            }

            return true;
        }

        protected override void OnRenamed(string name)
        {
            Messages.Message("DoTheRitualsYourselves.Message.RenameRitualPolicy".Translate(name), MessageTypeDefOf.NeutralEvent);
        }
    }
}
