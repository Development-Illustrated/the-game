using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

[DefaultExecutionOrder(-900)] 
public class MenuManager : MonoBehaviour
{
    [SerializeField] private UIDocument _mainMenuDocument;
    [SerializeField] private UIDocument _settingsMenuDocument;

    private VisualElement _mainMenuRootElement;
    private VisualElement _settingsMenuRootElement;

    private MainMenu _mainMenu;
    private SettingsMenu _settingsMenu;

    void Awake()
    {
        InitializeMenuComponents();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSystem<MenuManager>(this);

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
        else
        {
            Debug.LogError("GameManager not found but required by MenuManager");
        }
    }

    void Start()
    {
        StartCoroutine(InitializeUIAfterDelay());
    }

    private void HandleGameStateChanged(GameState previousState, GameState currentState)
    {
        switch(currentState)
        {
            case GameState.MainMenu:
                ShowMainMenu();
                break;

            case GameState.Settings:
                ShowSettingsMenu();
                break;

            case GameState.Playing:
            case GameState.Connecting:
            case GameState.Loading:
                HideMenu();
                break;
        }
    }

    private void InitializeMenuComponents()
    {
        if (_mainMenuDocument != null)
        {
            _mainMenu = _mainMenuDocument.GetComponent<MainMenu>();
        }
        else
        {
            Debug.LogError("Main Menu UI Document not assigned!");
        }

        if (_settingsMenuDocument != null)
        {
            _settingsMenu = _settingsMenuDocument.GetComponent<SettingsMenu>();
        }
        else
        {
            Debug.LogError("Settings Menu UI Document not assigned!");
        }
    }

    private IEnumerator InitializeUIAfterDelay()
    {
        yield return new WaitForEndOfFrame();

        ActivateUIDocuments();
        GetUIReferences();
        ConfigureInitialUIState();
        RegisterEvents();
    }

    private void ActivateUIDocuments()
    {
        _mainMenuDocument.gameObject.SetActive(true);
        _settingsMenuDocument.gameObject.SetActive(true);
    }

    private void GetUIReferences()
    {
        _mainMenuRootElement = _mainMenuDocument.rootVisualElement;
        _settingsMenuRootElement = _settingsMenuDocument.rootVisualElement;
    }

    private void ConfigureInitialUIState()
    {
        _mainMenuRootElement.style.display = DisplayStyle.Flex;
        _settingsMenuRootElement.style.display = DisplayStyle.None;
    }

    private void RegisterEvents()
    {
        _mainMenu.OnJoinGameClicked += OnMMJoinGameClicked;
        _mainMenu.OnSettingsClicked += OnMMSettingsClicked;
        _mainMenu.OnQuitClicked += OnMMQuitClicked;
        _mainMenu.OnHostClicked += OnMMHostClicked;
        _settingsMenu.OnBackClicked += OnSettingsBackClicked;
    }

    private void OnSettingsBackClicked()
    {
        GameManager.Instance.SetState(GameState.MainMenu);
    }

    private void OnMMJoinGameClicked(string serverIp)
    {
        GameManager.Instance.JoinGame(serverIp);
    }

    private void OnMMSettingsClicked()
    {
        GameManager.Instance.SetState(GameState.Settings);
    }

    private void OnMMQuitClicked()
    {
        GameManager.Instance.QuitGame();
    }

    private void OnMMHostClicked()
    {
        GameManager.Instance.HostGame();
    }

    private void OnDestroy()
    {
        DeregisterEvents();
    }

    private void DeregisterEvents()
    {
        if (_mainMenu != null)
        {
            _mainMenu.OnJoinGameClicked -= OnMMJoinGameClicked;
            _mainMenu.OnSettingsClicked -= OnMMSettingsClicked;
            _mainMenu.OnQuitClicked -= OnMMQuitClicked;
            _mainMenu.OnHostClicked -= OnMMHostClicked;
        }

        if (_settingsMenu != null)
        {
            _settingsMenu.OnBackClicked -= OnSettingsBackClicked;
        }
    }

    private void ShowMainMenu()
    {
        _settingsMenuRootElement.style.display = DisplayStyle.None;
        _mainMenuRootElement.style.display = DisplayStyle.Flex;
    }

    private void ShowSettingsMenu()
    {
        _mainMenuRootElement.style.display = DisplayStyle.None;
        _settingsMenuRootElement.style.display = DisplayStyle.Flex;
    }

    private void HideMenu()
    {
        _mainMenuRootElement.style.display = DisplayStyle.None;
        _settingsMenuRootElement.style.display = DisplayStyle.None;

        _mainMenuDocument.gameObject.SetActive(false);
        _settingsMenuDocument.gameObject.SetActive(false);
    }
}
