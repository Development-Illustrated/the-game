using Unity.Netcode;
using UnityEngine;

public class NetworkedCameraManager : NetworkBehaviour
{
    private Camera _camera;
    private Transform _player;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Subscribe to the client connected callback
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Check if this is the local client
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Get the player's transform and set the camera follow target
            _player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform;
            _camera.GetComponent<Unity.Cinemachine.CinemachineCamera>().Follow = _player;

            // Unsubscribe from the callback to avoid repeated calls
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
