using GameNetcodeStuff;
using Sunflowered;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class FlowerManager : NetworkBehaviour
{
    public PlayerControllerB? Controller;
    public GameObject? FlowerInstance;


    [ClientRpc]
    public void SpawnFlowerClientRpc()
    {
        if (FlowerInstance == null && Controller != null)
        {
            FlowerInstance = Instantiate(Sunflowered.Sunflowered.Instance.flowerprefab, Controller.transform.position, quaternion.identity);
            NetworkObject networkObject = FlowerInstance.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
            }
        }
    }
    [ClientRpc]
    public void DeSpawnFlowerClientRpc()
    {
        if (FlowerInstance != null)
        {
            if (FlowerInstance.GetComponent<PhysicsProp>().isHeld)
            {
                FlowerInstance.GetComponent<PhysicsProp>().playerHeldBy.DropAllHeldItemsAndSync();
            }
            if (IsServer)
            {
                NetworkObject networkObject = FlowerInstance.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Despawn();
                }
            }
            Destroy(FlowerInstance);
        }
    }
}