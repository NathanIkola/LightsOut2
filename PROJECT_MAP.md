# Project Map

LightsOut 2 is a RimWorld mod. The repository root is the installable mod layout; the C# source and solution live under `Source/LightsOut2`.

## Repository layout

```text
.
├── About/                     RimWorld mod metadata, preview, Workshop ID
├── Assemblies/                Built mod DLLs used by RimWorld
├── Languages/English/Keyed/   English keyed translations
├── Patches/                   Active XML patches applied to RimWorld defs
├── Patches - Old/             Retired compatibility and experimental patches
├── Source/
│   ├── Build/                 RimWorld/Unity reference assemblies for local builds
│   ├── LightsOut2/            Visual Studio solution and C# projects
│   ├── post-build.cmd         Copies build outputs into root Assemblies/
│   └── post-build-custom.cmd  Optional local deployment script
├── loadFolders.xml            Declares supported RimWorld load folder version
├── README.md                  Short project description
└── TODO.md                    Code review follow-ups
```

## Solution projects

`Source/LightsOut2/LightsOut2.sln` contains four projects, targeting .NET Framework 4.7.2 and RimWorld 1.6 references:

- `LightsOut2/` is the game-facing mod assembly. `LightsOut2Mod` initializes settings, logging, and the global ticker. Its subfolders implement game components, standby effects/influencers, Harmony patches, helpers, and logging.
- `LightsOut2.Core/` contains the base types that define standby effects and standby influencers, separated from the game-specific implementation.
- `jl08lib/` provides reusable mod infrastructure: settings attributes/controllers/serialization, logging, translation, and `ModBase`.
- `jl08lib.Tests/` contains MSTest coverage for the shared library's settings and logging. The current tests do not exercise game-facing room/standby behavior (see `TODO.md`).

## Runtime flow

1. `Patches/LightsOut2DefaultPatches.xml` attaches `RoomTrackerComp` to pawns. It also adds `StandbyComp` to eligible powered lights and powered interaction-cell buildings, selecting their influencer and effect types.
2. Room tracker and game component classes observe pawn room occupancy. `RoomOccupancyTrackerGameComp` and `RoomTrackerComp` coordinate room changes and reevaluation; `TickerGameComp`/`TickingCompBase` provide scheduled ticking.
3. `StandbyComp` combines configured influencers (such as `EmptyRoomInfluencer` and `KeepOnInfluencer`) to decide when a building desires standby, with configured delay behavior.
4. Effects apply that state: `OnOffPowerEffect` and `DisableGlowerEffect` control lights; `ReducePowerEffect` adjusts power draw for other devices. `KeepOnGizmo` lets a player prevent a building from entering standby.
5. `LightsOut2Settings` exposes behavior and performance settings through `jl08lib` settings infrastructure. Changes that affect room decisions mark rooms dirty for reevaluation.

## Build and local development

- Open `Source/LightsOut2/LightsOut2.sln` in Visual Studio or build it with the .NET SDK. The projects reference `Krafs.Rimworld.Ref` and Harmony NuGet packages.
- Building the main `LightsOut2` project runs `Source/post-build.cmd`, which replaces the root `Assemblies/` directory with the compiled mod DLLs.
- `Source/post-build-custom.cmd` is an optional machine-specific deployment script. It targets a fixed Steam RimWorld Mods path; inspect/update that path before using it.
- Run the shared-library tests with `dotnet test Source/LightsOut2/LightsOut2.sln`. Tests target .NET Framework 4.7.2 and require the configured reference/package dependencies.

## Useful starting points

| Task | Start here |
| --- | --- |
| Mod metadata and supported game version | `About/About.xml`, `loadFolders.xml` |
| Which game defs get mod components | `Patches/LightsOut2DefaultPatches.xml` |
| Settings and defaults | `Source/LightsOut2/LightsOut2/LightsOut2Settings.cs` |
| Room occupancy and light decisions | `Source/LightsOut2/LightsOut2/Comps/` |
| Standby policy and effects | `Source/LightsOut2/LightsOut2/StandbyInfluencers/`, `StandbyEffects/`, and `LightsOut2.Core/` |
| Shared settings/logging implementation | `Source/LightsOut2/jl08lib/` |
| Existing tests and coverage gaps | `Source/LightsOut2/jl08lib.Tests/`, `TODO.md` |
| Player-facing translated strings | `Languages/English/Keyed/Keyed.xml` |
