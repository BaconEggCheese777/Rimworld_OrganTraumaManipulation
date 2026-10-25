using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrganTraumaMod
{
    /// <summary>
    /// RimWorld auto-instantiates one instance of every non-abstract
    /// GameComponent subclass on new/loaded games - no XML registration
    /// needed. This one periodically scans every humanlike pawn, works out
    /// their current total Manipulation penalty from tiered organ damage,
    /// and adds/updates/removes a plain vanilla Hediff (OrganTrauma_ManipulationPenalty)
    /// to reflect it - purely via Severity and the def's own XML stages, no
    /// custom Hediff subclass or overrides required.
    /// </summary>
    public class GameComponent_OrganTraumaBackfill : GameComponent
    {
        private int ticksSinceLastScan = 0;
        private const int ScanIntervalTicks = 250; // roughly every 4 seconds

        // BodyPartDef.defName -> tier
        private static readonly Dictionary<string, int> OrganTiers = new Dictionary<string, int>
        {
            { "Brain", 1 },
            { "Heart", 2 },
            { "Liver", 2 },
            { "Kidney", 3 },
            { "Stomach", 3 },
            { "Lung", 3 },
        };

        // tier -> [Minor, Moderate, Severe, Critical] penalty fractions
        private static readonly Dictionary<int, float[]> TierPenalties = new Dictionary<int, float[]>
        {
            { 1, new float[] { 0.10f, 0.20f, 0.40f, 0.80f } },
            { 2, new float[] { 0.05f, 0.10f, 0.20f, 0.40f } },
            { 3, new float[] { 0.02f, 0.05f, 0.10f, 0.20f } },
        };

        public GameComponent_OrganTraumaBackfill(Game game)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            ScanAndUpdateAll();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            ticksSinceLastScan++;
            if (ticksSinceLastScan >= ScanIntervalTicks)
            {
                ticksSinceLastScan = 0;
                ScanAndUpdateAll();
            }
        }

        private static void ScanAndUpdateAll()
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsAndWorld_Alive)
            {
                UpdatePawn(pawn);
            }
        }

        private static void UpdatePawn(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
                return;

            if (pawn.RaceProps == null || !pawn.RaceProps.Humanlike)
                return;

            float totalPenalty = CalculateTotalPenalty(pawn);
            // Cap at 100% - Manipulation can't usefully go below 0 anyway.
            totalPenalty = Math.Min(totalPenalty, 1f);
            // Snap to the same 1% grid the HediffDef's stages are defined in.
            float rounded = (float)Math.Round(totalPenalty, 2);

            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(OrganTraumaDefOf.OrganTrauma_ManipulationPenalty);

            if (rounded <= 0f)
            {
                if (existing != null)
                    pawn.health.RemoveHediff(existing);
                return;
            }

            if (existing == null)
            {
                existing = HediffMaker.MakeHediff(OrganTraumaDefOf.OrganTrauma_ManipulationPenalty, pawn);
                existing.Severity = rounded;
                pawn.health.AddHediff(existing);
            }
            else if (Math.Abs(existing.Severity - rounded) > 0.001f)
            {
                existing.Severity = rounded;
            }
        }

        private static float CalculateTotalPenalty(Pawn pawn)
        {
            float total = 0f;

            if (pawn.RaceProps?.body == null)
                return 0f;

            foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
            {
                int tier;
                if (!OrganTiers.TryGetValue(part.def.defName, out tier))
                    continue;

                float maxHealth = part.def.GetMaxHealth(pawn);
                if (maxHealth <= 0f)
                    continue;

                float currentHealth = pawn.health.hediffSet.GetPartHealth(part);
                float fractionLost = 1f - (currentHealth / maxHealth);

                total += GetPenaltyForFractionLost(tier, fractionLost);
            }

            return total;
        }

        private static float GetPenaltyForFractionLost(int tier, float fractionLost)
        {
            float[] penalties;
            if (!TierPenalties.TryGetValue(tier, out penalties))
                return 0f;

            if (fractionLost >= 0.81f) return penalties[3]; // Critical: 81-100%
            if (fractionLost >= 0.61f) return penalties[2]; // Severe:   61-80%
            if (fractionLost >= 0.41f) return penalties[1]; // Moderate: 41-60%
            if (fractionLost >= 0.21f) return penalties[0]; // Minor:    21-40%
            return 0f;                                       // <21%: no penalty
        }
    }
}
