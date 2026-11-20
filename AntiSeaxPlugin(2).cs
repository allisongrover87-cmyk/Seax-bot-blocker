using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace AntiSeaxBot;

[BepInPlugin("com.yourname.antiseax", "Anti-Seax Bot Guard", "1.0.1")]
[BepInProcess("Among Us.exe")]
public class AntiSeaxPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new("com.yourname.antiseax");

    public override void Load()
    {
        Harmony.PatchAll();
        Log.LogInfo("Anti-Seax Bot protection loaded.");
    }
}

// Required by Innersloth Mod Policy: displays the "Modded" stamp in-game
[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
public static class ModStampPatch
{
    public static void Prefix(ModManager __instance)
    {
        __instance.ShowModStamp();
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
                __instance.KickPlayer(data.Id, ban: true);
                break;
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class RpcRateLimiterPatch
{
    private static readonly Dictionary<int, (DateTime WindowStart, int Count)> RpcTracker = new();
    private const int MaxRpcsPerSecond = 20;

    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            return true;

        int clientId = __instance.OwnerId;
        if (clientId == AmongUsClient.Instance.HostId)
            return true;

        DateTime now = DateTime.UtcNow;

        if (!RpcTracker.TryGetValue(clientId, out var entry) || (now - entry.WindowStart).TotalSeconds >= 1.0)
        {
            RpcTracker[clientId] = (now, 1);
            return true;
        }

        int newCount = entry.Count + 1;
        RpcTracker[clientId] = (entry.WindowStart, newCount);

        if (newCount > MaxRpcsPerSecond)
        {
            AmongUsClient.Instance.KickPlayer(clientId, ban: true);
            RpcTracker.Remove(clientId);
            return false;
        }

        return true;
    }
}
