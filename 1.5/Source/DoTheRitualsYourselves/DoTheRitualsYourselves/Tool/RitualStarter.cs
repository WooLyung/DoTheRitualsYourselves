using DoTheRitualsYourselves.RitualPolicy;
using DoTheRitualsYourselves.WorldComponents;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace DoTheRitualsYourselves.Tool
{
    public static class RitualStarter
    {
        public delegate void StartRitualCallback();

        private static List<ThingDef> ritualBuildingDefs;

        public static void MakeRitualCache()
        {
            ritualBuildingDefs = new List<ThingDef>();
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
                if (def?.building?.buildingTags?.Contains("RitualFocus") ?? false)
                    ritualBuildingDefs.Add(def);
        }

        public static void TryStart()
        {
            foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
            {
                foreach (Precept_Ritual ritual in ideo.GetRituals())
                {
                    if (!WorldComponent_AutoRituals.Instance.IsAutoStart(ritual.Id))
                        continue;

                    RitualPolicyBase policy = WorldComponent_AutoRituals.Instance.GetRitualPolicy(ritual.Id);
                    foreach (Map map in Find.Maps)
                    {
                        // 정책 필요함
                        string reason = "";
                        if (CanStartNow(ritual, ref reason, true, map))
                            break;
                    }
                }
            }
        }

        public static float PredictedQuality(Precept_Ritual ritual, TargetInfo targetInfo, RitualObligation ritualObligation, RitualRoleAssignments ritualRoleAssignments)
        {
            var outcome = ritual.outcomeEffect.def;
            float num = outcome.startingQuality;
            float num2 = 0f;
            foreach (RitualOutcomeComp comp in outcome.comps)
            {
                QualityFactor qualityFactor = comp.GetQualityFactor(ritual, targetInfo, ritualObligation, ritualRoleAssignments, ritual?.outcomeEffect?.DataForComp(comp));
                if (qualityFactor != null)
                {
                    if (qualityFactor.uncertainOutcome)
                        num2 += qualityFactor.quality;
                    else
                        num += qualityFactor.quality;
                }
            }

            if (ritual != null && ritual.RepeatPenaltyActive)
                num += ritual.RepeatQualityPenalty;

            Tuple<ExpectationDef, float> expectationsOffset = RitualOutcomeEffectWorker_FromQuality.GetExpectationsOffset(targetInfo.Map, ritual?.def);
            if (expectationsOffset != null)
                num += expectationsOffset.Item2;

            num = Mathf.Clamp(num, outcome.minQuality, outcome.maxQuality);
            num2 += num;
            num2 = Mathf.Clamp(num2, outcome.minQuality, outcome.maxQuality);

            return num;
        }

        public static IEnumerable<Precept_Ritual> GetRituals(this Ideo ideo)
        {
            foreach (Precept_Ritual ritual in ideo.PreceptsListForReading.OfType<Precept_Ritual>())
                if (ritual.def.visible)
                    yield return ritual;
        }

        public static bool CanStartWithObligations(Precept_Ritual ritual, Thing thing, ref RitualObligation ritualObligation, ref string reason)
        {
            if (!ritual.activeObligations.NullOrEmpty())
            {
                foreach (RitualObligation activeObligation in ritual.activeObligations)
                {
                    RitualTargetUseReport ritualTargetUseReport2 = ritual.CanUseTarget(thing, activeObligation);
                    if (ritualTargetUseReport2.canUse)
                    {
                        ritualObligation = activeObligation;
                        return true;
                    }
                }
            }

            if (ritual.isAnytime)
            {
                RitualTargetUseReport ritualTargetUseReport = ritual.CanUseTarget(thing, null);
                if (ritualTargetUseReport.failReason.NullOrEmpty())
                    return true;
                else
                    reason = ritualTargetUseReport.failReason;
            }

            RitualObligationTrigger ritualObligationTrigger = ritual.obligationTriggers?.FirstOrDefault((RitualObligationTrigger o) => o is RitualObligationTrigger_Date);
            if (ritualObligationTrigger != null)
            {
                RitualObligationTrigger_Date ritualObligationTrigger_Date = (RitualObligationTrigger_Date)ritualObligationTrigger;
                int num = ritualObligationTrigger_Date.OccursOnTick();
                int num2 = ritualObligationTrigger_Date.CurrentTickRelative();
                if (num2 > num)
                    num += 3600000;
                reason = "DateRitualNoObligation".Translate(ritual.LabelCap, (num - num2).ToStringTicksToPeriod(), ritualObligationTrigger_Date.DateString).Resolve();
            }

            if (reason == "")
                reason = "DoTheRitualsYourselves.Message.RitualNoObligation".Translate();
            return false;
        }

        public static bool CanStartWithPawns(Precept_Ritual ritual, Thing thing, RitualObligation ritualObligation, ref string reason, bool start, ref StartRitualCallback callback, ref float quality)
        {
            TargetInfo targetInfo = new TargetInfo(thing);
            Dialog_BeginRitual.PawnFilter filter = delegate (Pawn pawn, bool voluntary, bool allowOtherIdeos)
            {
                if (pawn.GetLord() != null)
                    return false;
                if (pawn.RaceProps.Animal && !ritual.behavior.def.roles.Any((RitualRole r) => r.AppliesToPawn(pawn, out var _, targetInfo, null, null, null, skipReason: true)))
                    return false;
                if (pawn.IsMutant)
                    return false;
                return !ritual.ritualOnlyForIdeoMembers || ritual.def.allowSpectatorsFromOtherIdeos || pawn.Ideo == ritual.ideo || !voluntary || allowOtherIdeos || pawn.IsPrisonerOfColony || pawn.RaceProps.Animal;
            };

            RitualRoleAssignments ritualRoleAssignments = Dialog_BeginRitual.CreateRitualRoleAssignments(ritual, targetInfo, thing.Map, filter, null, null, null);
            ritualRoleAssignments.FillPawns(filter, targetInfo);

            if (!ritualRoleAssignments.Participants.Any())
            {
                reason = "MessageRitualNeedsAtLeastOnePerson".Translate();
                return false;
            }

            foreach (Pawn participant in ritualRoleAssignments.Participants)
            {
                if (!participant.IsPrisoner && !participant.SafeTemperatureRange().IncludesEpsilon(targetInfo.Cell.GetTemperature(targetInfo.Map)))
                {
                    reason = "CantJoinRitualInExtremeWeather".Translate();
                    return false;
                }
            }

            if (ritual.behavior.SpectatorsRequired() && ritualRoleAssignments.SpectatorsForReading.Count == 0)
            {
                reason = "MessageRitualNeedsAtLeastOneSpectator".Translate();
                return false;
            }

            if (ritual.outcomeEffect != null)
            {
                foreach (string item in ritual.outcomeEffect.BlockingIssues(ritual, targetInfo, ritualRoleAssignments))
                {
                    reason = item;
                    return false;
                }
            }

            if (ritual.obligationTargetFilter != null)
            {
                foreach (string blockingIssue in ritual.obligationTargetFilter.GetBlockingIssues(targetInfo, ritualRoleAssignments))
                {
                    reason = blockingIssue;
                    return false;
                }
            }

            if (!ritual.behavior.def.roles.NullOrEmpty())
            {
                bool stillAddToPawnList;
                foreach (IGrouping<string, RitualRole> item2 in from r in ritual.behavior.def.roles group r by r.mergeId ?? r.id)
                {
                    RitualRole firstRole = item2.First();
                    int requiredPawnCount = item2.Count((RitualRole r) => r.required);
                    if (requiredPawnCount <= 0)
                        continue;

                    IEnumerable<Pawn> selectedPawns = item2.SelectMany((RitualRole r) => ritualRoleAssignments.AssignedPawns(r));
                    foreach (Pawn item3 in selectedPawns)
                    {
                        string text = ritualRoleAssignments.PawnNotAssignableReason(item3, firstRole, out stillAddToPawnList);
                        if (text != null)
                        {
                            reason = text;
                            return false;
                        }
                    }

                    if (requiredPawnCount == 1 && !selectedPawns.Any())
                    {
                        reason = "MessageLordJobNeedsAtLeastOneRolePawn".Translate(firstRole.Label.Resolve());
                        return false;
                    }
                    else if (requiredPawnCount > 1 && selectedPawns.Count() < requiredPawnCount)
                    {
                        reason = "MessageLordJobNeedsAtLeastNumRolePawn".Translate(Find.ActiveLanguageWorker.Pluralize(firstRole.Label), requiredPawnCount);
                        return false;
                    }
                }

                if (!ritualRoleAssignments.ExtraRequiredPawnsForReading.NullOrEmpty())
                {
                    foreach (Pawn item4 in ritualRoleAssignments.ExtraRequiredPawnsForReading)
                    {
                        string text2 = ritualRoleAssignments.PawnNotAssignableReason(item4, ritualRoleAssignments.RoleForPawn(item4), out stillAddToPawnList);
                        if (text2 != null)
                        {
                            reason = text2;
                            return false;
                        }
                    }
                }
            }

            if (ritual.ritualOnlyForIdeoMembers && !ritualRoleAssignments.Participants.Any((Pawn p) => p.Ideo == ritual.ideo))
            {
                reason = "MessageNeedAtLeastOneParticipantOfIdeo".Translate(ritual.ideo.memberName);
                return false;
            }

            if (start)
            {
                quality = PredictedQuality(ritual, targetInfo, ritualObligation, ritualRoleAssignments);
                callback = () => ritual.behavior.TryExecuteOn(targetInfo, null, ritual, ritualObligation, ritualRoleAssignments, true);
            }

            return true;
        }

        public static IEnumerable<Thing> GetRitualBuildings(Map map) {
            var lister = map.listerThings;
            foreach (ThingDef def in ritualBuildingDefs)
                foreach (Thing thing in lister.ThingsOfDef(def))
                    if (thing.Faction?.IsPlayer ?? false)
                        yield return thing;
        }

        public static bool CanStartNow(this Precept_Ritual ritual, ref string reason, bool start = false, Map map = null)
        {
            if (map == null)
                map = Find.CurrentMap;

            if (!ritual.allowOtherInstances)
            {
                foreach (LordJob_Ritual activeRitual in Find.IdeoManager.GetActiveRituals(map))
                {
                    if (activeRitual.Ritual == ritual)
                    {
                        reason = "CantStartRitualAlreadyInProgress".Translate(ritual.LabelCap);
                        return false;
                    }
                }
            }

            string reason2 = "", reason3 = "";
            StartRitualCallback callback = null;
            float quality = 0;

            foreach (Thing thing in GetRitualBuildings(map))
            {
                RitualObligation ritualObligation = null;
                if (!CanStartWithObligations(ritual, thing, ref ritualObligation, ref reason2))
                    continue;

                StartRitualCallback callbackTmp = null;
                float qualityTmp = 0;
                if (CanStartWithPawns(ritual, thing, ritualObligation, ref reason3, start, ref callbackTmp, ref qualityTmp))
                {
                    if (!start)
                        return true;

                    if (callback == null || quality < qualityTmp)
                    {
                        callback = callbackTmp;
                        quality = qualityTmp;
                    }
                }
            }

            if (callback != null)
            {
                callback();
                return true;
            }

            reason = "DoTheRitualsYourselves.Message.RitualNoSpot".Translate();
            if (reason3 != "")
                reason = reason3;
            else if (reason2 != "")
                reason = reason2;

            return false;
        }

        public static void StartNow(this Precept_Ritual ritual)
        {
            string reason = "";
            CanStartNow(ritual, ref reason, true);
        }
    }
}
