using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrganTraumaMod
{
    /// <summary>
    /// A synthetic, whole-body hediff that never changes severity on its own.
    /// Instead of using vanilla's stage/severity system, it overrides CapMods
    /// to compute a fresh Manipulation offset every time the game asks for
    /// it, based on live damage to a fixed set of tiered organs. This is the
    /// same extension point vanilla injuries use to reduce capacities - we're
    /// just supplying our own numbers instead of relying on HediffStage.
    /// </summary>
    public class Hediff_OrganTraumaManipulation : Hediff
    {
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

        private readonly List<PawnCapacityModifier> mods = new List<PawnCapacityModifier>();

        public override bool Visible => CalculateTotalPenalty() > 0f;

        public override string Label
        {
            get
            {
                float total = CalculateTotalPenalty();
                return "Organ trauma (Manipulation -" + (total * 100f).ToString("0") + "%)";
            }
        }

        public override List<PawnCapacityModifier> CapMods
        {
            get
            {
                mods.Clear();

                float totalPenalty = CalculateTotalPenalty();
                if (totalPenalty > 0f)
                {
                    mods.Add(new PawnCapacityModifier
                    {
                        capacity = PawnCapacityDefOf.Manipulation,
                        offset = -totalPenalty,
                    });
                }

                return mods;
            }
        }

        private float CalculateTotalPenalty()
        {
            float total = 0f;

            if (pawn?.health?.hediffSet == null || pawn.RaceProps?.body == null)
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
