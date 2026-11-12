# DiceKingdoms Fixes (`DiceModders.Fixes.dll`)

Fixes for bugs of **Dice Kingdoms** itself (BepInEx 6 IL2CPP). Nothing here changes the game rules, so the other players do not need the mod.

## What it fixes

| Fix | Bug | What the mod does |
|---|---|---|
| **Joinable after the host left** | When the host leaves a match, the others pick a new host and play on, but Steam picks the new lobby owner by itself. Only the owner can update the lobby's "still alive" stamp, and the friends list shows the Join button only while that stamp is fresh, so the match looks dead and players who dropped out cannot get back in. | The lobby owner hands the Steam lobby to the player who hosts the game (when it connects to that host, and whenever the lobby changes). Works when the lobby owner has the mod. |
| **Joining with a wrong PC clock** | Every "is this host alive" test allows 10 seconds of difference. The game corrects the PC clock with an internet time server; when that server cannot be reached (blocked by a router, school or company network), a PC clock more than about 10 seconds off makes every host look dead: no Join buttons, and joining waits forever. | While the game has no internet time yet, the clock is corrected with Steam's server time. A later answer of the time server replaces it, as before. |

The mod can be switched off in the mod list.

## Code map

| File | Job |
|---|---|
| `Plugin.cs` | the plugin and the mod switch |
| `LobbyOwner.cs` | hands the Steam lobby to the game's host |
| `ClockFix.cs` | Steam's time when the internet time server cannot be reached |

## Requirements

* Dice Kingdoms with **BepInEx 6 IL2CPP (x64)**: see the wiki's [Mods Installation](https://dicemodders.github.io/DiceKingdoms-Modding-Wiki/guides/mod-installation/) guide. Start the game once so BepInEx creates its folders.
* [ModCore](https://github.com/DiceModders/DiceKingdoms-ModCore) (`DiceModders.ModCore.dll`) in `BepInEx\plugins`. This mod does not contain a copy of it.

## Install (players)

Copy `DiceModders.Fixes.dll` into `BepInEx\plugins`, next to ModCore.

## Build (developers)

Needs the .NET SDK (6 or newer) and PowerShell. Game files are never committed: the script links `BepInEx\interop` from your game folder and copies the libraries from `BepInEx\plugins` into `libs`.

```powershell
.\scripts\build.ps1 -GameDir "<your Dice-Kingdoms folder>"   # first time: remembers the folder
.\scripts\build.ps1                                           # build + install (Release, console and log OFF)
.\scripts\build.ps1 -Dev -Run                                 # Debug build, console and log ON, then start the game
```

Close the game before building: it locks the DLLs. If PowerShell refuses to run scripts: `powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1`.

## License

MIT, see [LICENSE](LICENSE).
