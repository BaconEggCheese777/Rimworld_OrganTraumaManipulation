# Organ Trauma - Manipulation Penalties

Damage to internal organs now hits Manipulation directly, instead of having
no gameplay effect the way it does in vanilla (organs mostly feed things
like Blood Pumping/Filtration/Consciousness, not Manipulation, at all).

## The rules

**Tiers**
- Tier 1: Brain
- Tier 2: Heart, Liver
- Tier 3: Kidney, Stomach, Lung

**Severity**, based on % of that organ's HP lost:
- 0-20%: no penalty
- 21-40%: Minor
- 41-60%: Moderate
- 61-80%: Severe
- 81-100%: Critical (this also covers a fully destroyed/missing organ)

**Penalty by tier and severity** (subtracted from Manipulation, as a flat
offset - e.g. -0.10 means -10 percentage points of Manipulation):

| Tier | Minor | Moderate | Severe | Critical |
|------|------:|---------:|-------:|---------:|
| 1    |  -10% |     -20% |   -40% |     -80% |
| 2    |   -5% |     -10% |   -20% |     -40% |
| 3    |   -2% |      -5% |   -10% |     -20% |

**Stacking:** if multiple organs are damaged at once (e.g. a hurt liver
*and* a hurt kidney), their penalties add together.

Small Intestine and Large Intestine were dropped - vanilla RimWorld only has
a single "Stomach" organ for digestion, there's no separate intestine body
part to hook into.

## How it works technically

No Harmony needed. RimWorld's capacity system already sums up capacity
offsets ("capMods") from every hediff a pawn has - that's the same
mechanism vanilla uses for e.g. a punctured lung reducing Breathing. Rather
than using vanilla's fixed-severity-stage system, `Hediff_OrganTraumaManipulation`
overrides the `CapMods` property directly, so it recalculates the total
Manipulation penalty live, every time the game asks for it, straight from
each organ's current HP.

Every pawn quietly carries one of these tracking hediffs (added automatically
by a game component - see below). Its label updates itself to show the
current total penalty (e.g. "Organ trauma (Manipulation -25%)"), and it
only shows up in the pawn's health list at all once that penalty is above
zero - so healthy pawns won't see any clutter.

### Why there's a game component

Because this mod attaches its tracking hediff via a `GameComponent` that
scans all pawns (rather than adding it through a pawn-generation XML patch),
it applies immediately to pawns already in your current save - no need to
start a new game, same as the Show Move Speed mod.

## How to build - same GitHub Actions flow as before

1. Create a new **public** GitHub repo (e.g. "OrganTrauma").
2. Upload the contents of this folder, keeping `About/`, `Defs/`, `Source/`
   intact.
3. Manually add `.github/workflows/build.yml` via **Add file > Create new
   file** if drag-and-drop skips the hidden `.github` folder.
4. Check the **Actions** tab - once the "Build mod DLL" run goes green,
   download the `OrganTraumaMod-dll` artifact.
5. Unzip it and drop `OrganTraumaMod.dll` into this mod's `Assemblies/`
   folder.
6. Copy the whole `OrganTrauma` folder into your RimWorld `Mods` folder,
   enable it, restart.

If your repo ends up nested differently than `OrganTrauma/Source/...` at
the root, the `run:` and `path:` lines in `.github/workflows/build.yml`
need their paths adjusted to match - screenshot your repo's file listing if
the build errors with "file does not exist" and I'll fix the paths.

## Verifying it in-game

Turn on Dev Mode, select a pawn, open their Health tab, and use the dev
tools to damage one of the six organs above until it's lost more than 20%
of its HP. You should see a new "Organ trauma (Manipulation -X%)" entry
appear in their health list, and their Manipulation capacity (and the many
things downstream of it - work speed, combat, etc.) will drop accordingly.

## Folder layout

```
OrganTrauma/
  About/About.xml
  Defs/HediffDefs/OrganTrauma_Hediffs.xml   - the new HediffDef
  Source/OrganTraumaMod/                     - C# source + csproj
  Assemblies/                                - compiled DLL goes here
  .github/workflows/build.yml
```
