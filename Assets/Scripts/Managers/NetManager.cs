using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-800)] // Ensure this runs after GameManager
public class NetManager : MonoBehaviour
{
    private NetworkManager nm;

    private void Awake()
    {
        nm = GetComponent<NetworkManager>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSystem<NetManager>(this);

            GameManager.Instance.OnJoinGame += HandleJoinGame;
            GameManager.Instance.OnHostGame += HandleHostGame;

            if (!GameManager.Instance.IsInitialised)
            {
                GameManager.Instance.OnInitialisationComplete += HandleInitialisationComplete;
            }
            else
            {
                HandleInitialisationComplete();
            }
        }
        else
        {
            Debug.LogError("GameManager not found but required by NetManager");
        }
    }

    private void HandleInitialisationComplete()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if (arg == "--server")
            {
                HandleHostGame();
                break;
            }
        }
    }

    private void HandleJoinGame(string serverIp)
    {
        Debug.Log($"Connecting to server at {serverIp}");
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(serverIp);
        nm.StartClient();
    }

    private void HandleHostGame()
    {
        Debug.Log("Starting host");
        nm.StartHost();
    }
}
