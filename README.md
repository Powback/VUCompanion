# VUCompanion

A companion for [Venice Unleashed](https://veniceunleashed.net/) (VU) mod development.

VUCompanion is a small Windows console application that compiles the WebUI of your
mods, then launches a local VU dedicated server and shows its output in a cleaner,
colour-coded console. It is meant to replace starting the server by hand while you
iterate on a mod.

## Features

- **WebUI compilation on start.** For every mod listed in `modlist.txt`, runs
  `vuicc.exe` to build the mod's WebUI folder into `Mods/<mod>/ui.vuic`, and reports
  success or the compiler's error line.
- **Launches the server.** Starts `vu.com` as a headless dedicated server with
  debugging on, and streams its output into the VUCompanion console.
- **Less noisy output.** Removes the date from log timestamps (only the time is kept)
  and folds repeated identical messages into a single line with a counter, such as
  `(3)`.
- **Colour coding.** Errors are red, success messages green and info lines dimmed.
  GUIDs and path-like strings are highlighted.
- **Per-mod colours.** For `[VeniceEXT]` messages, mod and module names (`[Mod] [Module]`)
  get a stable colour each, taken from an MD5 hash of the name.
- **Window title.** Once the server has started and registered, the console title
  shows the server version and the details reported by the Zeus authentication.

## Requirements

- Windows
- .NET Framework 4.6.1
- Venice Unleashed installed at `C:\Program Files (x86)\VeniceUnleashed` (this path is hard-coded)
- `vuicc.exe` (the VU WebUI compiler), needed only for WebUI compilation
- NuGet package [Colorful.Console](https://www.nuget.org/packages/Colorful.Console/) 1.2.9 (restored automatically)

## Building

Open `VUCompanion.sln` in Visual Studio (2017 or newer), restore NuGet packages, and
build. The output is `VUCompanion\bin\<Configuration>\VUCompanion.exe`.

You can also build from a Developer Command Prompt:

```
nuget restore VUCompanion.sln
msbuild VUCompanion.sln /p:Configuration=Release
```

## Usage

VUCompanion looks for its files in **the folder that contains `VUCompanion.exe`**,
not in the current working directory. That folder should contain:

```
VUCompanion.exe
vuicc.exe
modlist.txt        one mod name per line
Mods/
  <ModName>/
    www/  or  WebUI/   WebUI source; compiled to Mods/<ModName>/ui.vuic
```

Usually this means putting the executable in your VU server instance folder, next to
`modlist.txt` and `Mods/`.

Run `VUCompanion.exe`. It will:

1. Compile the WebUI for each mod in `modlist.txt`. This step is skipped, with a
   "Missing vuicc.exe" message, if `vuicc.exe` is not there.
2. Start the server with these arguments:
   `-server -dedicated -vudebug -high120 -highResTerrain -tracedc -headless`
3. Show the formatted server output until the server exits.

## Configuration

Nothing can be configured at runtime. The VU install path, server launch arguments,
file names and colours are constants in `Program.cs` and `WebUICompiler.cs`. To
change them, edit those constants and rebuild.

## Project structure

| File | Purpose |
|---|---|
| `VUCompanion/Program.cs` | Entry point; launches the server and formats/colours its output |
| `VUCompanion/WebUICompiler.cs` | Reads `modlist.txt` and runs `vuicc.exe` for each mod |
| `VUCompanion/ProcessUtils.cs` | Process launching/output helpers (`Shaman.Runtime.ProcessUtils`) |
| `VUCompanion/ConsoleUtils.cs` | Async stream reader helper (currently unused) |

## Status

This is an early, small personal tool. Known limitations:

- The VU install path and server arguments are hard-coded.
- If `modlist.txt` is missing, the program throws an exception instead of skipping
  the WebUI step.
- How the WebUI folder path is built in `WebUICompiler.cs` looks wrong (a leading `/`
  in `Path.Combine`, and missing separators for the `WebUI` case). WebUI folders may
  not be found as expected, so check this before relying on automatic compilation.
