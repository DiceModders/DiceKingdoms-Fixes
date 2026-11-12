using System;
using DiceModders.ModCore.Tools;
using Steamworks;
using FiveTime = Fivemid.FiveTime.FiveTime;

namespace DiceModders.Fixes;

/// <summary>
/// Joining with a wrong PC clock.
///
/// The bug: the game decides whether a lobby and its host are alive from time stamps (MainMenu.DecodeKeepAlive: alive when less than 10 seconds
/// apart from "now"). "Now" is FiveTime.UtcTime = the PC clock + a correction from an internet time server (NTP). When NTP cannot be reached
/// (UDP port 123 blocked by a router, school or company network), the correction stays 0, and a PC clock that is more than about 10 seconds off makes
/// every host look dead: lobbies show no Join button and joining waits for a host forever.
///
/// The fix: once Steam runs, and while FiveTime has no correction yet, it sets the correction from Steam's server time (one second resolution, well
/// inside the 10 second window). A later NTP answer overwrites it with the exact value, as before.
/// </summary>
internal static class ClockFix
{
    private static readonly TimeSpan Tolerance = TimeSpan.FromSeconds(2);

    internal static void Install(Patcher patcher)
    {
        patcher.Patch(typeof(SteamClient), nameof(SteamClient.Init), typeof(ClockFix), postfix: nameof(SteamInitPostfix));
    }

    internal static void SteamInitPostfix() => Apply();

    internal static void Apply()
    {
        try
        {
            if (!Plugin.On || !SteamClient.IsValid || FiveTime.deltaTimeTicks != 0) return;   // NTP already answered: nothing to fix
            long steam = SteamUtils.SteamServerTime.Ticks, local = Il2CppSystem.DateTime.UtcNow.Ticks;
            var delta = TimeSpan.FromTicks(steam - local);
            if (delta.Duration() < Tolerance) return;   // the PC clock is right
            FiveTime.deltaTimeTicks = delta.Ticks;
            Plugin.L.LogInfo($"[clock] no internet time yet; the PC clock is {delta.TotalSeconds:0} s off Steam's time, corrected");
        }
        catch (Exception e) { Plugin.L.LogWarning("[clock] " + e.Message); }
    }
}
