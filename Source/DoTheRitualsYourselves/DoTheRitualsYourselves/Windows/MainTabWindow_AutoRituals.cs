using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using DoTheRitualsYourselves.Tool;
using RimWorld.Planet;
using DoTheRitualsYourselves.WorldComponents;

namespace DoTheRitualsYourselves.Windows
{
    public class MainTabWindow_AutoRituals : MainTabWindow
    {
        private Ideo selectedIdeo;

        private Ideo DefaultIdeo => Faction.OfPlayer.ideos?.PrimaryIdeo ?? Find.IdeoManager.IdeosListForReading.FirstOrDefault();

        public override Vector2 InitialSize
        {
            get
            {
                int ritualCount = (selectedIdeo ?? DefaultIdeo).GetRituals().Count();
                float height = 80f + ritualCount * 34f + 40f;
                return new Vector2(800f, height);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            if (WorldRendererUtility.WorldRenderedNow && Find.CurrentMap != null)
                Find.World.renderer.wantedMode = WorldRenderMode.None;

            if (selectedIdeo == null)
                selectedIdeo = DefaultIdeo;
        }

        public override void DoWindowContents(Rect inRect)
        {
            const float lineHeight = 28f;
            const float spacing = 6f;
            const float nameWidth = 350f;

            Text.Font = GameFont.Small;
            float curY = inRect.y + spacing;

            if (Widgets.ButtonText(new Rect(inRect.x + spacing, curY, nameWidth, lineHeight), selectedIdeo.name))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (Ideo ideo in Find.IdeoManager.IdeosListForReading)
                    if (Faction.OfPlayer.ideos.AllIdeos.Contains(ideo))
                        options.Add(new FloatMenuOption(ideo.name, () => selectedIdeo = ideo, ideo.Icon, ideo.Color));
                Find.WindowStack.Add(new FloatMenu(options));
            }

            TextAnchor prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(inRect.x + nameWidth + 10f, curY, 100f, lineHeight), "DoTheRitualsYourselves.UI.StartNow".Translate());
            Widgets.Label(new Rect(inRect.x + nameWidth + 120f, curY, 100f, lineHeight), "DoTheRitualsYourselves.UI.AutoStart".Translate());
            Widgets.Label(new Rect(inRect.x + nameWidth + 240f, curY, 150f, lineHeight), "DoTheRitualsYourselves.UI.RitualPolicy".Translate()); // 정책 열 추가
            Text.Anchor = prevAnchor;

            curY += lineHeight + spacing * 2f;
            windowRect.y += windowRect.height - InitialSize.y;
            windowRect.height = InitialSize.y;
            foreach (Precept_Ritual ritual in selectedIdeo.GetRituals())
            {
                Widgets.Label(new Rect(inRect.x + 20f, curY, nameWidth, lineHeight), new GUIContent($"{ritual.LabelCap} ({ritual.def.label})", ritual.Icon));

                // start now
                string reason = "";
                Rect rect = new Rect(inRect.x + nameWidth + 10f, curY, 100f, lineHeight);
                if (!ritual.CanStartNow(ref reason, true))
                {
                    Widgets.ButtonText(rect, "DoTheRitualsYourselves.UI.CannotStart".Translate(), active: false);
                    TooltipHandler.TipRegion(rect, reason != "" ? reason : "DoTheRitualsYourselves.Reason.CantUnknown".Translate().RawText);
                }
                else
                {
                    if (Widgets.ButtonText(new Rect(inRect.x + nameWidth + 10f, curY, 100f, lineHeight), "DoTheRitualsYourselves.UI.Start".Translate()))
                        ritual.StartNow();
                    TooltipHandler.TipRegion(rect, "DoTheRitualsYourselves.UI.StartTooltip".Translate());
                }

                // auto start
                bool auto = WorldComponent_AutoRituals.Instance.IsAutoStart(ritual.Id);
                Widgets.Checkbox(new Vector2(inRect.x + nameWidth + 160f, curY + 5f), ref auto);
                WorldComponent_AutoRituals.Instance.SetAutoStart(ritual.Id, auto);

                // policy
                Rect policyRect = new Rect(inRect.x + nameWidth + 240f, curY, 150f, lineHeight);
                var currentPolicy = WorldComponent_AutoRituals.Instance.GetRitualPolicy(ritual.Id);
                if (Widgets.ButtonText(policyRect, currentPolicy.label))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (var policyId in WorldComponent_RitualPolicy.Instance.AllPolicyIds)
                    {
                        var policy = WorldComponent_RitualPolicy.Instance.GetPolicy(policyId);
                        options.Add(new FloatMenuOption(policy.label, () =>
                        {
                            WorldComponent_AutoRituals.Instance.SetPolicy(ritual.Id, policyId);
                        }));
                    }
                    options.Add(new FloatMenuOption("DoTheRitualsYourselves.UI.Edit".Translate(), () =>
                    {
                        Find.WindowStack.Add(new Dialog_EditRitualPolicy());
                    }));

                    Find.WindowStack.Add(new FloatMenu(options));
                }

                curY += lineHeight + spacing;
            }
        }
    }
}