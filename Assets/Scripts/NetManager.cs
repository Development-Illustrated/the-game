using Unity.Netcode;
using UnityEngine;

public class NetManager : MonoBehaviour
{
    [SerializeField] private bool disableCommandLineArgs = false;
    private NetworkManager nm;

    private string serverAddress = "127.0.0.1";

    private bool startServer = false;

    private void Awake()
    {
        nm = GetComponent<NetworkManager>();
    }

    private void Start()
    {
        if (!nm)
        {
            Debug.LogError("NetworkManager not found on object");
            return;
        }

        if (!disableCommandLineArgs)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                if (arg == "--server")
                {
                    startServer = true;
                }
            }
            if (startServer)
            {
                nm.StartServer();
            }
            else
            {
                NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(serverAddress);
                nm.StartClient();
            }
        }

    }

    private void OnGUI()
    {
        if (disableCommandLineArgs)
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 200));
            if (!nm.IsClient && !nm.IsServer && !nm.IsHost)
            {
                ServerButtons();
            }
            else
            {
                StatusLabels();
            }
            GUILayout.EndArea();
        }
    }

    private void ServerButtons()
    {
        GUILayout.Label("Server Address");
        serverAddress = GUILayout.TextField(serverAddress);

        // Ensure default value is set if the text field is empty
        if (string.IsNullOrEmpty(serverAddress))
        {
            serverAddress = "127.0.0.1";
        }

        if (GUILayout.Button("Connect"))
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(serverAddress);
            nm.StartClient();
        }
        if (GUILayout.Button("Host"))
        {
            nm.StartHost();
        }

    }

    private void StatusLabels()
    {
        var mode = nm.IsHost ? "Host" : nm.IsServer ? "Server" : nm.IsClient ? "Client" : "Offline";

        GUILayout.Label("Mode: " + mode);
        GUILayout.Label("Address: " + serverAddress);
    }
}
