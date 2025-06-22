using DoTheRitualsYourselves.RitualPolicy;
using DoTheRitualsYourselves.WorldComponents;
using RimWorld;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace DoTheRitualsYourselves.Windows
{
    public class Dialog_RenameRitualPolicy : Dialog_Rename<RitualPolicyBase>
    {
        private static readonly Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-]*$");

        public Dialog_RenameRitualPolicy(RitualPolicyBase policy)
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
    }
}
