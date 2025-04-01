using UnityEngine;
using Unity.Netcode;

public class PlayerNetworkSetup : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            playerCamera.enabled = true;

            if (playerCamera.GetComponent<AudioListener>())
                playerCamera.GetComponent<AudioListener>().enabled = true;
        }
        else
        {
            playerCamera.enabled = false;

            if (playerCamera.GetComponent<AudioListener>())
                playerCamera.GetComponent<AudioListener>().enabled = false;
        }
    }
}
