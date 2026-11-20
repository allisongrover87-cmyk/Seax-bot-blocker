using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace AntiSeaxBot;

[BepInPlugin("com.emily.antiseaxbot", "Anti-Seax Bot Guard", "1.0.0")]
[BepInProcess("Among Us.exe")]
public class AntiSeaxPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new("com.emily.antiseaxbot");

    public override void Load()
    {
        Harmony.PatchAll();
        Log.LogInfo("Anti-Seax Bot protection loaded.");
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public static class JoinGuardPatch
{
    private static readonly string[] BlockedKeywords = { "seax", "seaxdev", "nuke" };

    public static void Postfix(AmongUsClient __instance, ClientData data)
    {
        if (!__instance.AmHost || data == null) return;

        string playerName = data.PlayerName?.ToLowerInvariant() ?? "";
        string friendCode = data.FriendCode?.ToLowerInvariant() ?? "";

        foreach (string keyword in BlockedKeywords)
        {
            if (playerName.Contains(keyword) || friendCode.Contains(keyword))
            {
                __instance.KickPlayer(data.Id, true);
                break;
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class RpcRateLimiterPatch
{
    private static readonly Dictionary<int, Queue<DateTime>> RpcTimestamps = new();
    private const int MaxRpcsPerSecond = 20;

    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return true;
        if (__instance == null) return true;

        int clientId = __instance.OwnerId;
        if (clientId == AmongUsClient.Instance.HostId) return true;

        DateTime now = DateTime.UtcNow;

        if (!RpcTimestamps.TryGetValue(clientId, out var timestamps))
        {
            timestamps = new Queue<DateTime>();
            RpcTimestamps[clientId] = timestamps;
        }

        while (timestamps.Count > 0 && (now - timestamps.Peek()).TotalSeconds > 1.0)
        {
            timestamps.Dequeue();
        }

        timestamps.Enqueue(now);

        if (timestamps.Count > MaxRpcsPerSecond)
        {
            AmongUsClient.Instance.KickPlayer(clientId, true);
            RpcTimestamps.Remove(clientId);
            return false;
        }

        return true;
    }
}
