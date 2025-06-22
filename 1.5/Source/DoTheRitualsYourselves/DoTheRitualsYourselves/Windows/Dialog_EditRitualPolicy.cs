using DoTheRitualsYourselves.RitualPolicy;
using DoTheRitualsYourselves.WorldComponents;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace DoTheRitualsYourselves.Windows
{
    public class Dialog_EditRitualPolicy : Window
    {
        private RitualPolicyBase selectedPolicy;
        private Vector2 scrollPosition = Vector2.zero;

        public override Vector2 InitialSize => new Vector2(600f, 400f);

        public Dialog_EditRitualPolicy()
        {
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            float listWidth = 140f;
            float spacing = 30f;

            // X 버튼
            Rect closeRect = new Rect(inRect.xMax - 24f, inRect.y, 24f, 24f);
            if (Widgets.ButtonImage(closeRect, TexButton.CloseXSmall))
                Close();

            // + 버튼
            Rect addRect = new Rect(inRect.x + listWidth + 10f, inRect.y + inRect.height - 20f, 16f, 16f);
            TooltipHandler.TipRegion(addRect, "DoTheRitualsYourselves.UI.Add".Translate());
            if (Widgets.ButtonImage(addRect, TexButton.Plus))
            {
                int number = WorldComponent_RitualPolicy.Instance.AllPolicyIds.Count() + 1;
                WorldComponent_RitualPolicy.Instance.InsertPolicy(new RitualPolicyBase("DoTheRitualsYourselves.NewPolicy".Translate() + " " + number));
            }

            Rect listRect = new Rect(inRect.x, inRect.y, listWidth, inRect.height);
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

            float curY = inRect.y + 10f;
            float labelWidth = 130f;
            float settingX = inRect.x + listWidth + spacing;

            int timeControlId = selectedPolicy.GetHashCode();
            int moodControlId = selectedPolicy.GetHashCode() ^ 0xABCDEF;

            // 정책 이름 (굵고 크게)
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(settingX - 10f, curY, inRect.width - settingX - 30f, 30f), selectedPolicy.label);
            Text.Font = GameFont.Small;

            curY += 40f;

            float sliderWidth = inRect.width - settingX - 150f;

            // 시간 구간 설정
            Widgets.Label(new Rect(settingX, curY, labelWidth, 24f), "DoTheRitualsYourselves.UI.Policy.Time".Translate());
            GUI.enabled = !selectedPolicy.IsStatic;
            Widgets.IntRange(new Rect(settingX + labelWidth, curY, sliderWidth, 24f), timeControlId, ref selectedPolicy.allowedHours, 0, 24);
            GUI.enabled = true;
            curY += 40f;

            // 무드 구간
            Widgets.Label(new Rect(settingX, curY, labelWidth, 24f), "DoTheRitualsYourselves.UI.Policy.Mood".Translate());
            GUI.enabled = !selectedPolicy.IsStatic;
            Widgets.FloatRange(new Rect(settingX + labelWidth, curY, sliderWidth, 24f), moodControlId, ref selectedPolicy.allowedAverageMood, 0f, 1f);
            GUI.enabled = true;
            curY += 40f;

            // 최소 인원
            Widgets.Label(new Rect(settingX, curY, labelWidth, 24f), "DoTheRitualsYourselves.UI.Policy.PawnCount".Translate());
            float pawnSlider = Widgets.HorizontalSlider(
                new Rect(settingX + labelWidth, curY, sliderWidth, 24f),
                selectedPolicy.minPawnCount, 1, 50, true, selectedPolicy.minPawnCount.ToString(), roundTo: 1f);
            if (!selectedPolicy.IsStatic)
                selectedPolicy.minPawnCount = (int)pawnSlider;

            // 삭제 및 이름 변경 아이콘 버튼 (우측 하단)
            float buttonY = inRect.yMax - 40f;
            float buttonSize = 30f;
            float iconSpacing = 4f;
            float rightAlignX = inRect.xMax - buttonSize - iconSpacing;

            TooltipHandler.TipRegion(new Rect(rightAlignX, buttonY, buttonSize, buttonSize), "DoTheRitualsYourselves.UI.Delete".Translate());
            TooltipHandler.TipRegion(new Rect(rightAlignX - buttonSize - iconSpacing, buttonY, buttonSize, buttonSize), "DoTheRitualsYourselves.UI.Rename".Translate());
            GUI.enabled = !selectedPolicy.IsStatic;

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
            GUI.enabled = true;
        }
    }
}
