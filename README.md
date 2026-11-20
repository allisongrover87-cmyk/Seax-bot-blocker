# AntiSeaxBot

A lightweight **BepInEx 6 (IL2CPP)** host-side anti-cheat plugin for *Among Us* that automatically protects public lobbies from automated lobby-nuking bots and RPC flooders.

## Features
* **Instant Join Guard:** Automatically kicks and bans any connecting player whose display name or friend code contains blocked bot keywords (`seax`, `seaxdev`, `nuke`).
* **RPC Flood Blocker:** Monitors incoming RPC packets per player and immediately bans clients sending more than 20 RPCs per second, dropping the malicious packets before they can crash or flood the lobby.
* **Host-Only Protection:** Only runs when you are the room host so it never interferes with normal gameplay when joining other people's lobbies.

## Installation (For Players)
1. Install **BepInEx 6 (Unity.IL2CPP win-x86)** into your *Among Us* folder and launch the game once to generate the `BepInEx` folders.
2. Download the latest **`AntiSeaxBot.dll`** from the **Releases** tab on the right side of this GitHub page.
3. Move `AntiSeaxBot.dll` into `Among Us/BepInEx/plugins/`.
4. Launch *Among Us* and host a lobby!

## Building from Source (For Developers)
1. Install the **.NET SDK** (.NET 6.0 or newer).
2. Open `AntiSeaxBot.csproj` and make sure the `<AmongUs>` path points to your local *Among Us* directory.
3. Run `dotnet build -c Release` inside the project folder.
4. Copy the compiled `AntiSeaxBot.dll` into your `Among Us/BepInEx/plugins/` folder.

---

## Disclaimer
This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.


