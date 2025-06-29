using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using DoTheRitualsYourselves.Core;
using RimWorld.Planet;
using DoTheRitualsYourselves.WorldComponents;
using DoTheRitualsYourselves.Extra;

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
                return new Vector2(1000f, height);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            if (WorldRendererUtility.WorldRendered && Find.CurrentMap != null)
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
            TooltipHandler.TipRegion(new Rect(inRect.x + nameWidth + 10f, curY, 100f, lineHeight), "DoTheRitualsYourselves.UI.StartNow.Tip".Translate());
            Widgets.Label(new Rect(inRect.x + nameWidth + 120f, curY, 100f, lineHeight), "DoTheRitualsYourselves.UI.AutoStart".Translate());
            TooltipHandler.TipRegion(new Rect(inRect.x + nameWidth + 120f, curY, 100f, lineHeight), "DoTheRitualsYourselves.UI.AutoStart.Tip".Translate());
            Widgets.Label(new Rect(inRect.x + nameWidth + 240f, curY, 150f, lineHeight), "DoTheRitualsYourselves.UI.RitualPolicy".Translate());
            TooltipHandler.TipRegion(new Rect(inRect.x + nameWidth + 240f, curY, 100f, lineHeight), "DoTheRitualsYourselves.UI.RitualPolicy.Tip".Translate());
            Widgets.Label(new Rect(inRect.x + nameWidth + 400f, curY, 200f, lineHeight), "DoTheRitualsYourselves.UI.RitualSpot".Translate());
            TooltipHandler.TipRegion(new Rect(inRect.x + nameWidth + 400f, curY, 200f, lineHeight), "DoTheRitualsYourselves.UI.RitualSpot.Tip".Translate());
            Text.Anchor = prevAnchor;

            curY += lineHeight + spacing * 2f;
            windowRect.y += windowRect.height - InitialSize.y;
            windowRect.height = InitialSize.y;
            foreach (Precept_Ritual ritual in selectedIdeo.GetRituals())
            {
                Widgets.Label(new Rect(inRect.x + 20f, curY, nameWidth, lineHeight), new GUIContent($"{ritual.LabelCap} ({ritual.def.label})", ritual.Icon));

                RitualExtraData extra = WorldComponent_AutoRituals.Instance.GetRitualExtraData(ritual.Id);

                // start now
                string reason = "";
                Rect rect = new Rect(inRect.x + nameWidth + 10f, curY, 100f, lineHeight);
                if (!ritual.TryStart(ref reason, true, true))
                {
                    if (Widgets.ButtonText(rect, "DoTheRitualsYourselves.UI.CannotStart".Translate()))
                        Messages.Message("DoTheRitualsYourselves.Message.CantStart".Translate(), MessageTypeDefOf.RejectInput);
                    TooltipHandler.TipRegion(rect, reason != "" ? reason : "DoTheRitualsYourselves.Reason.CantUnknown".Translate().RawText);
                }
                else
                {
                    if (Widgets.ButtonText(rect, "DoTheRitualsYourselves.UI.Start".Translate()))
                    {
                        string reason2 = "";
                        ritual.TryStart(ref reason2, true, false);

                        if (!reason2.NullOrEmpty())
                            Messages.Message(reason2, MessageTypeDefOf.RejectInput);
                    }

                    if (ritual.RepeatPenaltyActive)
                        TooltipHandler.TipRegion(rect, "DoTheRitualsYourselves.UI.RepeatPenaltyStartTooltip".Translate());
                    else
                        TooltipHandler.TipRegion(rect, "DoTheRitualsYourselves.UI.StartTooltip".Translate());
                }

                // auto start
                bool auto = extra.autoStart;
                Widgets.Checkbox(new Vector2(inRect.x + nameWidth + 160f, curY + 5f), ref auto);
                extra.autoStart = auto;

                // policy
                Rect policyRect = new Rect(inRect.x + nameWidth + 240f, curY, 150f, lineHeight);
                var currentPolicy = WorldComponent_AutoRituals.Instance.GetRitualPolicy(ritual.Id);
                if (Widgets.ButtonText(policyRect, currentPolicy.Label))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (var policyId in WorldComponent_RitualPolicy.Instance.AllPolicyIds)
                    {
                        var policy = WorldComponent_RitualPolicy.Instance.GetPolicy(policyId);
                        options.Add(new FloatMenuOption(policy.Label, () =>
                        {
                            extra.policyID = policyId;
                        }));
                    }
                    options.Add(new FloatMenuOption("DoTheRitualsYourselves.UI.Edit".Translate(), () =>
                    {
                        Find.WindowStack.Add(new Dialog_EditRitualPolicy());
                    }));

                    Find.WindowStack.Add(new FloatMenu(options));
                }

                // spot
                Rect splotRect = new Rect(inRect.x + nameWidth + 400f, curY, 200f, lineHeight);
                Thing ritualSpot = extra.ritualSpot;
                if (ritualSpot != null && !ritualSpot.Spawned)
                {
                    ritualSpot = null;
                    extra.ritualSpot = null;
                }

                if (ritualSpot == null)
                {
                    if (Widgets.ButtonText(splotRect, "DoTheRitualsYourselves.UI.RitualSpot.None".Translate()))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>
                        {
                            new FloatMenuOption("DoTheRitualsYourselves.UI.RitualSpot.Assign".Translate(), () => AssignRitualSpot(ritual, extra)),
                        };
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
                else
                {
                    TooltipHandler.TipRegion(splotRect, ritualSpot.LabelNoParenthesis);
                    if (Widgets.ButtonText(splotRect, ritualSpot.LabelNoParenthesis))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>
                        {
                            new FloatMenuOption("DoTheRitualsYourselves.UI.RitualSpot.Reassign".Translate(), () => AssignRitualSpot(ritual, extra)),
                            new FloatMenuOption("DoTheRitualsYourselves.UI.RitualSpot.Clear".Translate(), () => ClearRitualSpot(extra)),
                            new FloatMenuOption("DoTheRitualsYourselves.UI.RitualSpot.JumpTo".Translate(), () => JumpToRitualSpot(extra))
                        };
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }

                curY += lineHeight + spacing;
            }
        }

        public void AssignRitualSpot(Precept_Ritual ritual, RitualExtraData extra)
        {
            var targetingParams = new TargetingParameters
            {
                canTargetBuildings = true,
                validator = t => t.Thing is Building 
                && t.Thing.def.building.buildingTags.Contains("RitualFocus")
                && t.Thing.Faction == Faction.OfPlayer
                && ritual.CanUseTarget(t, null).canUse
            };

            Find.Targeter.BeginTargeting(targetingParams, 
                target => {
                    extra.ritualSpot = target.Thing;
                },
                target => { });
        }

        public void ClearRitualSpot(RitualExtraData extra)
        {
            extra.ritualSpot = null;
        }

        public void JumpToRitualSpot(RitualExtraData extra)
        {
            Thing thing = extra.ritualSpot;
            if (thing != null)
                CameraJumper.TryJumpAndSelect(thing);
        }
    }
}