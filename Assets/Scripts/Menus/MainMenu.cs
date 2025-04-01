using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugMenu = true;

    [Header("Server Connection")]
    [SerializeField, TextArea(2,3)]
    private string serverIpInfo = "The IP address to connect to when using \"Join Game\".\nIf the debug menu is enabled, this is ignored and \"Join Game\" will always try to connect to your locally running hosted instance."; 
    [SerializeField] private string serverIp = "127.0.0.1";
    
    [Header("UI References")]
    [SerializeField] private UIDocument settingsMenuDocument;

    private VisualElement _rootElement;

    private Button _joinButton;
    private Button _hostButton;
    private Button _settingsButton;
    private Button _quitButton;

    private EventCallback<ClickEvent> _joinButtonClickCallback;
    private EventCallback<ClickEvent> _hostButtonClickCallback;
    private EventCallback<ClickEvent> _settingsButtonClickCallback;
    private EventCallback<ClickEvent> _quitButtonClickCallback;

    // Events
    public event Action<string> OnJoinGameClicked;
    public event Action OnSettingsClicked;
    public event Action OnQuitClicked;
    public event Action OnHostClicked;

    void Awake()
    {
        _rootElement = GetComponent<UIDocument>().rootVisualElement;
        
        // Create the delegates once
        _joinButtonClickCallback = ev => ClickJoinButton();
        _hostButtonClickCallback = ev => ClickHostButton();
        _settingsButtonClickCallback = ev => ClickSettingsButton();
        _quitButtonClickCallback = ev => ClickQuitButton();
    }

    void Start()
    {
        _joinButton = _rootElement.Q<Button>("JoinGameBtn");
        _hostButton = _rootElement.Q<Button>("HostBtn");
        _settingsButton = _rootElement.Q<Button>("SettingsBtn");
        _quitButton = _rootElement.Q<Button>("QuitBtn");

        _hostButton.style.display = enableDebugMenu ? DisplayStyle.Flex : DisplayStyle.None;

        RegisterButtonCallbacks();
    }

    public void RegisterButtonCallbacks()
    {
        UnregisterButtonCallbacks();
        
        _joinButton = _rootElement.Q<Button>("JoinGameBtn");
        _hostButton = _rootElement.Q<Button>("HostBtn");
        _settingsButton = _rootElement.Q<Button>("SettingsBtn");
        _quitButton = _rootElement.Q<Button>("QuitBtn");
        
        if (enableDebugMenu)
        {
            _hostButton.style.display = DisplayStyle.Flex;
            _hostButton.RegisterCallback<ClickEvent>(_hostButtonClickCallback);
        }
        else
        {
            _hostButton.style.display = DisplayStyle.None;
        }

        _joinButton.RegisterCallback<ClickEvent>(_joinButtonClickCallback);
        _settingsButton.RegisterCallback<ClickEvent>(_settingsButtonClickCallback);
        _quitButton.RegisterCallback<ClickEvent>(_quitButtonClickCallback);
    }

    public void UnregisterButtonCallbacks()
    {
        if (_joinButton != null) _joinButton.UnregisterCallback<ClickEvent>(_joinButtonClickCallback);
        if (_hostButton != null) _hostButton.UnregisterCallback<ClickEvent>(_hostButtonClickCallback);
        if (_settingsButton != null) _settingsButton.UnregisterCallback<ClickEvent>(_settingsButtonClickCallback);
        if (_quitButton != null) _quitButton.UnregisterCallback<ClickEvent>(_quitButtonClickCallback);
    }

    private void ClickJoinButton()
    {
        string connIp = enableDebugMenu ? "127.0.0.1" : serverIp;
        OnJoinGameClicked?.Invoke(connIp);
    }

    private void ClickSettingsButton()
    {
        OnSettingsClicked?.Invoke();
    }

    private void ClickQuitButton()
    {
        OnQuitClicked?.Invoke();
    }
    
    private void ClickHostButton()
    {
        OnHostClicked?.Invoke();
    }
}
