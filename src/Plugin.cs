using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using DiceModders.ModCore.Tools;

namespace DiceModders.Fixes;

using Core = DiceModders.ModCore.ModCore;   // inside DiceModders.* the bare name would be the namespace

/// <summary>
/// DiceKingdoms Fixes: fixes for bugs of the game itself. Nothing here changes the simulation, so other players do not need the mod.
/// </summary>
[BepInPlugin(Guid, Name, Version)]
[BepInDependency("DiceModders.ModCore")]
[BepInProcess("Dice-Kingdoms.exe")]
public sealed class Plugin : BasePlugin
{
    public const string Guid = "org.dicemodders.dicekingdoms.fixes";
    public const string Name = "DiceKingdoms Fixes";
    public const string Version = "1.0.0";

    internal static ManualLogSource L;
    internal static bool On;

    public override void Load()
    {
        L = Log;
        var patcher = new Patcher(Guid, Log);
        LobbyOwner.Install();
        ClockFix.Install(patcher);
        patcher.Summary();
        Core.Mods.Switch(Config, affectsSimulation: false, description: "Turns every fix of this mod on or off.")
            .WhenOn(() => { On = true; LobbyOwner.Check("switched on"); ClockFix.Apply(); }, () => On = false);
    }
}
