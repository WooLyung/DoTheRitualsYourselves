using DoTheRitualsYourselves.WorldComponents;
using DoTheRitualsYourselves.RitualPolicies;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace DoTheRitualsYourselves.Windows
{
    public class Dialog_EditRitualPolicy : Window
    {
        private RitualPolicy selectedPolicy;
        private Vector2 scrollPosition = Vector2.zero;

        public override Vector2 InitialSize => new Vector2(600f, 600f);

        public Dialog_EditRitualPolicy()
        {
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            float topSpace = 100f;
            float listWidth = 140f;
            float spacing = 30f;

            // X
            Rect closeRect = new Rect(inRect.xMax - 24f, inRect.y, 24f, 24f);
            if (Widgets.ButtonImage(closeRect, TexButton.CloseXSmall))
                Close();

            // +
            Rect addRect = new Rect(inRect.x + listWidth + 10f, inRect.y + inRect.height - 20f, 16f, 16f);
            TooltipHandler.TipRegion(addRect, "DoTheRitualsYourselves.UI.Add".Translate());
            if (Widgets.ButtonImage(addRect, TexButton.Plus))
            {
                int number = WorldComponent_RitualPolicy.Instance.AllPolicyIds.Count() + 1;
                WorldComponent_RitualPolicy.Instance.InsertPolicy(new RitualPolicy("DoTheRitualsYourselves.NewPolicy".Translate() + " " + number));
            }

            // title & desc
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30f), "DoTheRitualsYourselves.UI.Policy.Title".Translate());
            Text.Font = GameFont.Small; 
            Widgets.Label(new Rect(inRect.x, inRect.y + 40f, inRect.width, 70f), "DoTheRitualsYourselves.UI.Policy.Desc".Translate());

            // list
            Rect listRect = new Rect(inRect.x, inRect.y + topSpace, listWidth, inRect.height - topSpace);
            Widgets.DrawMenuSection(listRect);

            GUI.enabled = true;
            float listInnerHeight = WorldComponent_RitualPolicy.Instance.AllPolicyIds.Count() * 30f + 30f;
            Rect viewRect = new Rect(0f, 0f, listRect.width, listInnerHeight);
            Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect, showScrollbars: false);
            Listing_Standard list = new Listing_Standard();
            list.Begin(viewRect);
            foreach (var id in WorldComponent_RitualPolicy.Instance.AllPolicyIds)
            {
                var policy = WorldComponent_RitualPolicy.Instance.GetPolicy(id);
                if (list.ButtonText(policy.label))
                    selectedPolicy = policy;
            }
            list.End();
            Widgets.EndScrollView();

            if (selectedPolicy == null)
                return;

            float curY = inRect.y + 10f + topSpace;
            float labelWidth = 130f;
            float settingX = inRect.x + listWidth + spacing;

            int timeControlId = selectedPolicy.GetHashCode();
            int moodControlId = selectedPolicy.GetHashCode() ^ 0xABCDEF;

            // label
            Text.Font = GameFont.Medium;
            string label = selectedPolicy.label;
            if (selectedPolicy.IsStatic)
                label += $" ({"DoTheRitualsYourselves.UI.NotEditable".Translate()})";
            Widgets.Label(new Rect(settingX - 10f, curY, inRect.width - settingX - 30f, 30f), label);
            Text.Font = GameFont.Small;
            curY += 40f;

            float sliderWidth = inRect.width - settingX - 150f;

            // min pawn
            var rect1 = new Rect(settingX, curY, labelWidth, 24f);
            TooltipHandler.TipRegion(rect1, "DoTheRitualsYourselves.UI.Policy.PawnCount.Tip".Translate());
            Widgets.Label(rect1, "DoTheRitualsYourselves.UI.Policy.PawnCount".Translate());
            GUI.enabled = !selectedPolicy.IsStatic;
            float pawnSlider = Widgets.HorizontalSlider(
                new Rect(settingX + labelWidth, curY, sliderWidth, 24f),
                selectedPolicy.minPawnCount, 1, 50, true, selectedPolicy.minPawnCount.ToString(), roundTo: 1f);
            selectedPolicy.minPawnCount = (int)pawnSlider;
            GUI.enabled = true;
            curY += 40f;

            // mood
            var rect2 = new Rect(settingX, curY, labelWidth, 24f);
            TooltipHandler.TipRegion(rect2, "DoTheRitualsYourselves.UI.Policy.AvgMood.Tip".Translate());
            Widgets.Label(rect2, "DoTheRitualsYourselves.UI.Policy.AvgMood".Translate());
            GUI.enabled = !selectedPolicy.IsStatic;
            Widgets.FloatRange(new Rect(settingX + labelWidth, curY, sliderWidth, 24f), moodControlId, ref selectedPolicy.allowedAverageMood, 0f, 1f, $"{(selectedPolicy.allowedAverageMood.min * 100).ToString("F0")}-{(selectedPolicy.allowedAverageMood.max * 100).ToString("F0")}%");
            GUI.enabled = true;
            curY += 40f;

            // time
            var rect3 = new Rect(settingX, curY, labelWidth, 24f);
            TooltipHandler.TipRegion(rect3, "DoTheRitualsYourselves.UI.Policy.Time.Tip".Translate());
            Widgets.Label(rect3, "DoTheRitualsYourselves.UI.Policy.Time".Translate());
            GUI.enabled = !selectedPolicy.IsStatic;
            Widgets.IntRange(new Rect(settingX + labelWidth, curY, sliderWidth, 24f), timeControlId, ref selectedPolicy.allowedHours, 0, 24);
            GUI.enabled = true;
            curY += 40f;

            // flip time
            var rect4 = new Rect(settingX, curY, labelWidth, 24f);
            TooltipHandler.TipRegion(rect4, "DoTheRitualsYourselves.UI.Policy.FlipTime.Tip".Translate());
            Widgets.Label(rect4, "DoTheRitualsYourselves.UI.Policy.FlipTime".Translate());
            GUI.enabled = !selectedPolicy.IsStatic;
            Widgets.Checkbox(settingX + labelWidth, curY, ref selectedPolicy.flipTime, 16);
            GUI.enabled = true;
            curY += 80f;

            // participants

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(settingX - 10f, curY, inRect.width - settingX - 30f, 30f), "DoTheRitualsYourselves.UI.Policy.Participants".Translate());
            Text.Font = GameFont.Small;
            curY += 40f;

            // min health
            var rect5 = new Rect(settingX, curY, labelWidth, 24f);
            TooltipHandler.TipRegion(rect5, "DoTheRitualsYourselves.UI.Policy.MinHealth.Tip".Translate());
            Widgets.Label(rect5, "DoTheRitualsYourselves.UI.Policy.MinHealth".Translate());
            GUI.enabled = !selectedPolicy.IsStatic;
            float healthSlider = Widgets.HorizontalSlider(
                new Rect(settingX + labelWidth, curY, sliderWidth, 24f),
                selectedPolicy.minHealth, 0f, 1f, true, (selectedPolicy.minHealth * 100).ToString("F0") + "%", roundTo: 0.05f);
            selectedPolicy.minHealth = healthSlider;
            GUI.enabled = true;
            curY += 40f;

            // delete, rename

            if (!selectedPolicy.IsStatic)
            {
                float buttonY = inRect.yMax - 40f;
                float buttonSize = 30f;
                float iconSpacing = 4f;
                float rightAlignX = inRect.xMax - buttonSize - iconSpacing;

                TooltipHandler.TipRegion(new Rect(rightAlignX, buttonY, buttonSize, buttonSize), "DoTheRitualsYourselves.UI.Delete".Translate());
                TooltipHandler.TipRegion(new Rect(rightAlignX - buttonSize - iconSpacing, buttonY, buttonSize, buttonSize), "DoTheRitualsYourselves.UI.Rename".Translate());

                if (Widgets.ButtonImage(new Rect(rightAlignX - buttonSize - iconSpacing, buttonY, buttonSize, buttonSize), TexButton.Rename))
                    Find.WindowStack.Add(new Dialog_RenameRitualPolicy(selectedPolicy));

                if (Widgets.ButtonImage(new Rect(rightAlignX, buttonY, buttonSize, buttonSize), TexButton.Delete))
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("DoTheRitualsYourselves.UI.DeleteConfirm".Translate(), () =>
                    {
                        WorldComponent_RitualPolicy.Instance.RemovePolicy(selectedPolicy);
                        selectedPolicy = null;
                    }));
                }
            }
        }
    }
}
