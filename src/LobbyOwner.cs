using System;
using Steamworks;
using NetworkingManager = MLAPI.NetworkingManager;
using SteamworksTransport = Utility.Steamworks.SteamworksTransport;

namespace DiceModders.Fixes;

using Core = DiceModders.ModCore.ModCore;

/// <summary>
/// Keeps the Steam lobby owner and the game's host the same player.
///
/// The bug: when the host leaves, the others agree on a new host (MainMenu HostConsensus: the first player of the lobby's "players" list whose
/// keep-alive is fresh) and that player hosts again from the saved state. Steam, however, picks the new lobby OWNER on its own, and only the owner can
/// write lobby data. The host's keep-alive coroutine writes the lobby "keep-alive" value only while it owns the lobby (otherwise it logs "Missing lobby
/// ownership to update keep alive."), and the friends list shows a lobby's Join button only while that value is fresh. So after the host changed, the
/// match looks dead and players who dropped out cannot get back in.
///
/// The fix runs on the lobby owner: when it is connected as a client to another player's game, it hands the Steam lobby to that player (Steam's own
/// SetLobbyOwner). Checked when the match starts or is joined again on this computer (it is connected to the host by then) and when the lobby
/// changes (members, owner).
///
/// Do not hook NetworkingManager.InvokeOnClientConnectedCallback for this: the method starts with a compare and a short conditional jump, and the
/// detour library moves that jump to a wrong address. The game leaves the callback empty, so the jump is taken on every join and the game dies
/// without a message.
/// </summary>
internal static class LobbyOwner
{
    internal static void Install()
    {
        Core.Match.Started += () => Check("match started");
        Core.Match.Rejoined += () => Check("match joined again");
        Core.Lobby.Changed += () => Check("lobby changed");
    }

    internal static void Check(string why)
    {
        try
        {
            if (!Plugin.On) return;
            var lobby = Core.Lobby.Current;
            if (!lobby.HasValue || !SteamClient.IsValid) return;
            var nm = NetworkingManager.Singleton;
            if (nm == null || nm.IsServer || !nm.IsConnectedClient) return;   // only a client knows the host is somebody else
            var transport = nm.NetworkConfig?.NetworkTransport?.TryCast<SteamworksTransport>();
            if (transport == null) return;
            ulong host = transport.connectToSteamId, me = SteamClient.SteamId.Value;
            if (host == 0 || host == me) return;
            var l = lobby.Value;
            if (!l.IsOwnedBy(SteamClient.SteamId)) return;
            bool member = false;
            foreach (var m in Core.Lobby.Members) if (m.SteamId == host) { member = true; break; }
            if (!member) return;
            l.Owner = new Friend(host);
            Plugin.L.LogInfo($"[lobby] handed the lobby to the host {host} ({why}), so its keep-alive keeps the match joinable");
        }
        catch (Exception e) { Plugin.L.LogWarning("[lobby] owner check: " + e.Message); }
    }
}
