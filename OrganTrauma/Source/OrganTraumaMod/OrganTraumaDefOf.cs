using RimWorld;
using Verse;

namespace OrganTraumaMod
{
    [DefOf]
    public static class OrganTraumaDefOf
    {
        public static HediffDef OrganTrauma_ManipulationPenalty;

        static OrganTraumaDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OrganTraumaDefOf));
        }
    }
}
