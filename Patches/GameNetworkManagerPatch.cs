using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using Unity.Netcode;

namespace Sunflowered
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        public static bool HasInitalized;
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Init()
        {
            if (HasInitalized == true) { Sunflowered.Logger.LogWarning("Already initalized Sunflowermod!!"); return; }

            if (Sunflowered.Instance.flowerprefab != null)
            {
                NetworkManager.Singleton.AddNetworkPrefab(Sunflowered.Instance.flowerprefab);
                Sunflowered.Logger.LogInfo("Added flower.prefab to network prefabs!");
            }
            else
            {
                Sunflowered.Logger.LogError("Failed to load flower.prefab!");
            }
            
            if (Sunflowered.Instance.FlowerManagerPre != null)
            {
                NetworkManager.Singleton.AddNetworkPrefab(Sunflowered.Instance.FlowerManagerPre);
                Sunflowered.Logger.LogInfo("Added flowerMan.prefab to network prefabs!");
            }
            else
            {
                Sunflowered.Logger.LogError("Failed to load flowerMan.prefab!");
            }

            HasInitalized = true;
        }
    }
}
