using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;
using System.IO;
using Unity.Mathematics;

namespace Sunflowered
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch("OnClientDisconnect")]
        [HarmonyPrefix]
        private static void getSunflowered(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer) { return; }
            if (StartOfRound.Instance.ClientPlayerList.TryGetValue(clientId, out var value))
            {
                FlowerManager[] managers = GameObject.FindObjectsOfType<FlowerManager>();
                foreach (var item in managers)
                {
                    if (item.Controller == StartOfRound.Instance.allPlayerScripts[value])
                    {
                        //Sunflowered.Logger.LogMessage($"{StartOfRound.Instance.allPlayerScripts[value].name} Disconected!");
                        item.SpawnFlowerClientRpc();
                        break;
                    }
                }
            }
        }
        [HarmonyPatch("OnClientConnect")]
        [HarmonyPostfix]
        private static void WeOnlyHaveOneSunflower(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer) { return; }
            if (StartOfRound.Instance.ClientPlayerList.TryGetValue(clientId, out var value))
            {
                FlowerManager[] managers = GameObject.FindObjectsOfType<FlowerManager>();
                foreach (var item in managers)
                {
                    if (item.Controller == StartOfRound.Instance.allPlayerScripts[value])
                    {
                        //Sunflowered.Logger.LogMessage($"{StartOfRound.Instance.allPlayerScripts[value].name} Resconected 4 rel!");
                        item.DeSpawnFlowerClientRpc();
                        break;
                    }
                }
            }
        }

    }
}