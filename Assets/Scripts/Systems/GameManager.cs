using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;

public enum GameState
{
    Initialising,
    MainMenu,
    Settings,
    Connecting,
    Loading,
    Playing,
}


[DefaultExecutionOrder(-999)] // Ensure this runs before any other scripts
public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameState initialState = GameState.MainMenu;

    private GameState _currentState;
    public GameState CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState == value) return;

            GameState previousState = _currentState;
            _currentState = value;

            Debug.Log($"Game state changed: {previousState} -> {_currentState}");
            OnGameStateChanged?.Invoke(previousState, _currentState);
        }
    }

    // State change events
    public event Action<GameState, GameState> OnGameStateChanged;

    // Game specific events

    // Networking Events
    public event Action<string> OnJoinGame;
    public event Action OnHostGame;
    public event Action OnGameStarted;
    public event Action<string> OnLoadLevel;

    // Player Events
    public event Action OnPlayerJoined;
    public event Action OnPlayerLeft;
    public event Action OnPlayerDeath;
    public event Action OnLocalPlayerDeath;
    
    private Dictionary<Type, object> _systems = new Dictionary<Type, object>();

    public bool IsInitialised { get; private set; }
    public event Action OnInitialisationComplete;

    protected override void Awake()
    {
        base.Awake();
        _currentState = GameState.Initialising;

        StartCoroutine(InitialiseSystems());
    }

    private IEnumerator InitialiseSystems()
    {
        yield return null;

        IsInitialised = true;
        OnInitialisationComplete?.Invoke();

        SetState(initialState);
    }

    public void SetState(GameState newState)
    {
        // We can do some custom validation here if needed
        CurrentState = newState;
    }

    public void StartGame()
    {
        SetState(GameState.Playing);
    }

    public void LoadLevel(string levelName)
    {
        SetState(GameState.Loading);
        OnLoadLevel?.Invoke(levelName);
    }

    public void JoinGame(string serverIp)
    {
        SetState(GameState.Connecting);
        OnJoinGame?.Invoke(serverIp);
    }

    public void HostGame()
    {
        SetState(GameState.Connecting);
        OnHostGame?.Invoke();
    }

    public void RegisterSystem<T>(T system) where T : class
    {
        Type systemType = typeof(T);

        if (_systems.ContainsKey(systemType))
        {
            Debug.LogWarning($"System of type {systemType} is already registered.");
            return;
        }

        _systems[systemType] = system;
        Debug.Log($"System of type {systemType} registered.");
    }

    public T GetSystem<T>() where T : class
    {
        Type systemType = typeof(T);

        if (_systems.TryGetValue(systemType, out object system))
        {
            return system as T;
        }

        Debug.LogWarning($"System not found: {systemType}");
        return null;
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
