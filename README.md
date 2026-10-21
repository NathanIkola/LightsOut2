# LightsOut2
If LightsOut was so great, how come there's no LightsOut 2?

## VS Code test setup

This solution targets .NET Framework 4.7.2. C# Dev Kit does not support .NET Framework projects, so use DotRush to run the MSTest project in VS Code's Test Explorer. The repository recommends DotRush through `.vscode/extensions.json`.

## Run RimWorld from VS Code

`Ctrl+Shift+B` builds the mod only. Select **Debug RimWorld** in Run and Debug, then press `F5` to build, deploy, and start the game under the C# CoreCLR debugger. The launch configuration's pre-launch task runs `Source/prepare-rimworld.ps1` to build and copy the mod into the game's `Mods/LightsOut2` folder before launching RimWorld. It assumes the default Steam install under `C:\Program Files (x86)\Steam\steamapps\common\RimWorld`; edit `.vscode/launch.json` if your game is elsewhere. The deploy step replaces the existing `Mods/LightsOut2` folder.

## Versioning

Each project build uses the local build time as its version, in `yyyy.MM.dd.HHmm` format (for example, `2026.10.21.1437`). The same value is written to the assembly, file, and informational versions.
