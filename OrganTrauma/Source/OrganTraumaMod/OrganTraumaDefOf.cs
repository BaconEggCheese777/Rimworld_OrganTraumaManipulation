using RimWorld;
using Verse;

namespace OrganTraumaMod
{
    [DefOf]
    public static class OrganTraumaDefOf
    {
        public static HediffDef OrganTrauma_ManipulationPenalty;
        public static HediffDef OrganTrauma_BrainConsciousnessPenalty;

        static OrganTraumaDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OrganTraumaDefOf));
        }
    }
}
