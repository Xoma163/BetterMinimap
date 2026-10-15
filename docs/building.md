# Building and tests

Use **.NET SDK 8** and reference DLLs from your installed Valheim client and BepInEx 5 profile. The mod targets `net48`; the helper tests target `net8.0`. Game/runtime DLLs are not included in this repository or the package.

Run from the repository root, using your own installation paths:

```powershell
dotnet build RotatingMinimap.csproj -c Release `
  '-p:ValheimDir=C:\Games\Valheim' `
  '-p:BepInExCore=C:\ModProfile\BepInEx\core'
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
dotnet run --project tests/GeometryTests.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
git -c core.autocrlf=false diff --check
if ($LASTEXITCODE -ne 0) { throw 'Diff check failed' }
powershell -NoProfile -ExecutionPolicy Bypass -File package.ps1
if ($LASTEXITCODE -ne 0) { throw 'Packaging failed' }
```

- DLL: `bin/Release/net48/BetterMinimap.dll`.
- ZIP: `artifacts/AndrewSha-BetterMinimap-<version>-r2modman.zip`.
- `package.ps1` does **not** compile the DLL. It checks the DLL/manifest version, packages the current README/CHANGELOG/LICENSE and generates the icon. An existing ZIP of the same version is overwritten.
- Packaging uses Windows PowerShell/System.Drawing. A private `local.props` can supply reference paths and is ignored by Git.
- GitHub Actions runs the helper tests, not the full DLL build: game libraries are only available locally.

Automated checks cover geometry, six-slot spacing and sizes, migration helpers, clock/weight formatting, transparency failure handling, zoom, presets, localization and zoom-memory file I/O. **They do not launch Unity or verify rendering, input or mod compatibility.** Use the [in-game test checklist](testing-hud-navigation.md).

Report bugs with game/mod versions, reproduction steps and screenshots. Remove private data from attached logs and configs.
