using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;

namespace Sunflowered
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        // Reference to the FlowerManager prefab

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner) return; // Only proceed for the local player

            // Request the server to spawn the FlowerManager
            __instance.StartCoroutine(SpawnFlowerManagerWhenReady(__instance));
        }
        private static System.Collections.IEnumerator SpawnFlowerManagerWhenReady(PlayerControllerB player)
        {
            // Wait until the player's NetworkObject is spawned
            while (player.NetworkObject == null || !player.NetworkObject.IsSpawned)
            {
                yield return null;
            }

            // Request the server to spawn the FlowerManager
            if (NetworkManager.Singleton.IsServer)
            {
                GameObject FlowerManagerInstance = UnityEngine.Object.Instantiate(Sunflowered.Instance.FlowerManagerPre, player.transform.position, Quaternion.identity);
                NetworkObject networkObject = FlowerManagerInstance.GetComponent<NetworkObject>();

                if (networkObject != null)
                {
                    try
                    {
                        // Spawn the NetworkObject with ownership set to the player
                        networkObject.SpawnWithOwnership(player.NetworkObject.OwnerClientId);

                        // Set up the FlowerManager component
                        FlowerManager FlowerManager = FlowerManagerInstance.GetComponent<FlowerManager>();
                        if (FlowerManager != null)
                        {
                            FlowerManager.Controller = player;
                        }

                        // Parent the FlowerManager to the player
                        if (NetworkManager.Singleton.IsServer)
                            FlowerManagerInstance.transform.SetParent(player.transform);
                        Sunflowered.Logger.LogInfo($"Spawned and parented FlowerManager for player: {player.playerUsername}");
                    }
                    catch (Exception ex)
                    {
                        Sunflowered.Logger.LogInfo($"Failed to spawn FlowerManager for player: {player.playerUsername} Due to : {ex}");
                    }
                }
                else
                {
                    Sunflowered.Logger.LogError("FlowerManager prefab is missing NetworkObject component!");
                    UnityEngine.Object.Destroy(FlowerManagerInstance);
                }
            }
            else
            {
                Sunflowered.Logger.LogInfo($"Requested FlowerManager spawn for player: {player.playerUsername}");
                SpawnFlowerManagerServerRpc(player.NetworkObject.OwnerClientId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private static void SpawnFlowerManagerServerRpc(ulong ownerClientId)
        {
            PlayerControllerB player = NetworkManager.Singleton.ConnectedClients[ownerClientId].PlayerObject.GetComponent<PlayerControllerB>();
            if (player == null)
            {
                Sunflowered.Logger.LogError($"Failed to find PlayerControllerB for client: {ownerClientId}");
                return;
            }

            GameObject FlowerManagerInstance = UnityEngine.Object.Instantiate(Sunflowered.Instance.FlowerManagerPre, player.transform.position, Quaternion.identity);
            NetworkObject networkObject = FlowerManagerInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                try
                {
                    // Parent the FlowerManager to the player
                    if (NetworkManager.Singleton.IsServer)
                        FlowerManagerInstance.transform.SetParent(player.transform);

                    // Spawn the NetworkObject with ownership set to the player
                    networkObject.SpawnWithOwnership(ownerClientId);

                    // Set up the FlowerManager component
                    FlowerManager FlowerManager = FlowerManagerInstance.GetComponent<FlowerManager>();
                    if (FlowerManager != null)
                    {
                        FlowerManager.Controller = player;
                    }

                    Sunflowered.Logger.LogInfo($"Spawned and parented FlowerManager for client: {ownerClientId}");
                }
                catch (Exception ex)
                {
                    Sunflowered.Logger.LogInfo($"Failed to spawn FlowerManager for client({ownerClientId}) Due to : {ex}");
                }
            }
            else
            {
                Sunflowered.Logger.LogError("FlowerManager prefab is missing NetworkObject component!");
                UnityEngine.Object.Destroy(FlowerManagerInstance);
            }
        }
    }
}