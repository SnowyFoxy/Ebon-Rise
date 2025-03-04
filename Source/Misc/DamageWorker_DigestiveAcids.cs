using RimWorld;
using UnityEngine;
using Verse;

namespace EbonRiseV2.Misc
{
    public class DamageWorker_DigestiveAcids : DamageWorker_AddInjury
    {
        protected override void ApplySpecialEffectsToPart(
            Pawn pawn,
            float totalDamage,
            DamageInfo dinfo,
            DamageResult result)
        {
            FinalizeAndAddInjury(pawn, totalDamage, dinfo, result);
        }

        // These two are copied from DamageWorker_AddInjury, except one small diff.
        // If this needs to be updated, take it from there.
        private new static void FinalizeAndAddInjury(Pawn pawn,
            float totalDamage,
            DamageInfo dinfo,
            DamageResult result)
        {
            if (pawn.health.hediffSet.PartIsMissing(dinfo.HitPart)) return;
            Pawn instigator = dinfo.Instigator as Pawn;
            HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, dinfo.HitPart);
            Hediff_Injury hediffInjury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, pawn);
            hediffInjury.Part = dinfo.HitPart;
            hediffInjury.sourceDef = dinfo.Weapon;
            if (instigator != null && instigator.IsMutant && dinfo.Weapon == ThingDefOf.Human)
                hediffInjury.sourceLabel = instigator.mutant.Def.label;
            else
                hediffInjury.sourceLabel = dinfo.Weapon?.label ?? "";
            hediffInjury.sourceBodyPartGroup = dinfo.WeaponBodyPartGroup;
            hediffInjury.sourceHediffDef = dinfo.WeaponLinkedHediff;
            hediffInjury.sourceToolLabel = dinfo.Tool?.labelNoLocation ?? dinfo.Tool?.label;
            hediffInjury.Severity = totalDamage;
            if (instigator != null && instigator.CurJobDef == JobDefOf.SocialFight)
                hediffInjury.destroysBodyParts = false;
            if (dinfo.InstantPermanentInjury)
            {
                HediffComp_GetsPermanent comp = hediffInjury.TryGetComp<HediffComp_GetsPermanent>();
                if (comp != null)
                    comp.IsPermanent = true;
                else
                    Log.Error("Tried to create instant permanent injury on Hediff without a GetsPermanent comp: " +
                              hediffDefFromDamage + " on " + pawn);
            }

            FinalizeAndAddInjury(pawn, hediffInjury, dinfo, result);
        }

        private new static void FinalizeAndAddInjury(Pawn pawn,
            Hediff_Injury injury,
            DamageInfo dinfo,
            DamageResult result)
        {
            injury.TryGetComp<HediffComp_GetsPermanent>()?.PreFinalizeInjury();
            float partHealth = pawn.health.hediffSet.GetPartHealth(injury.Part);
            if (pawn.IsColonist && !dinfo.IgnoreInstantKillProtection && dinfo.Def.ExternalViolenceFor(pawn) &&
                !Rand.Chance(Find.Storyteller.difficulty.allowInstantKillChance))
            {
                float num1 = injury.IsLethal ? injury.def.lethalSeverity * 1.1f : 1f;
                float min = 1f;
                float max = Mathf.Min(injury.Severity, partHealth);
                for (int index = 0; index < 7 && pawn.health.WouldDieAfterAddingHediff(injury); ++index)
                {
                    float num2 = Mathf.Clamp(partHealth - num1, min, max);
                    if (DebugViewSettings.logCauseOfDeath)
                        Log.Message(string.Format(
                            "CauseOfDeath: attempt to prevent death for {0} on {1} attempt:{2} severity:{3}->{4} part health:{5}",
                            pawn.Name, injury.Part.Label, index + 1, injury.Severity, num2, partHealth));
                    injury.Severity = num2;
                    num1 *= 2f;
                    min = 0.0f;
                }
            }

            pawn.health.AddHediff(injury, dinfo: dinfo, result: result);
            float num = Mathf.Min(injury.Severity, partHealth);
            result.totalDamageDealt += num;
            // DIFFERENCE: Not setting wounded to true prevents the red spray
            result.AddPart(pawn, injury.Part);
            result.AddHediff(injury);
        }
    }
}