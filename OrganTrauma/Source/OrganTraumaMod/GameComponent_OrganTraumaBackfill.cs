using RimWorld;
using Verse;

namespace OrganTraumaMod
{
    /// <summary>
    /// RimWorld auto-instantiates one instance of every non-abstract
    /// GameComponent subclass on new/loaded games - no XML registration
    /// needed. We use that to make sure every humanlike pawn - both ones
    /// already in your save and any created afterward - is carrying our
    /// tracking hediff.
    /// </summary>
    public class GameComponent_OrganTraumaBackfill : GameComponent
    {
        private int ticksSinceLastScan = 0;
        private const int ScanIntervalTicks = 250; // roughly every 4 seconds

        public GameComponent_OrganTraumaBackfill(Game game)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            ScanAndAddHediff();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            ticksSinceLastScan++;
            if (ticksSinceLastScan >= ScanIntervalTicks)
            {
                ticksSinceLastScan = 0;
                ScanAndAddHediff();
            }
        }

        private static void ScanAndAddHediff()
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsAndWorld_Alive)
            {
                AddHediffIfMissing(pawn);
            }
        }

        private static void AddHediffIfMissing(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
                return;

            if (pawn.RaceProps == null || !pawn.RaceProps.Humanlike)
                return;

            if (pawn.health.hediffSet.HasHediff(OrganTraumaDefOf.OrganTrauma_ManipulationPenalty))
                return;

            Hediff hediff = HediffMaker.MakeHediff(OrganTraumaDefOf.OrganTrauma_ManipulationPenalty, pawn);
            pawn.health.AddHediff(hediff);
        }
    }
}
