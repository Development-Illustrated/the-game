using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private UIDocument _document;
    private VisualElement _root;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        _root = _document.rootVisualElement;
    }

    private void OnEnable()
    {
        Debug.Log("MainMenu enabled");
        AssignEventHandlers();
    }

    private void OnDisable()
    {
        Debug.Log("MainMenu disabled");
        RemoveEventHandlers();
    }

    private void AssignEventHandlers()
    {
        Debug.Log("Assigning event handlers");

        var findGameButton = _root.Q<Button>("FindGameBtn");
        if (findGameButton != null)
        {
            findGameButton.clicked += OnFindGameButtonClicked;
            Debug.Log("FindGameBtn handler assigned");
        }
        else
        {
            Debug.LogWarning("FindGameBtn not found");
        }

        var settingsButton = _root.Q<Button>("SettingsBtn");
        if (settingsButton != null)
        {
            settingsButton.clicked += OnSettingsButtonClicked;
            Debug.Log("SettingsBtn handler assigned");
        }
        else
        {
            Debug.LogWarning("SettingsBtn not found");
        }

        var hostButton = _root.Q<Button>("HostBtn");
        if (hostButton != null)
        {
            hostButton.clicked += OnHostButtonClicked;
            Debug.Log("HostBtn handler assigned");
        }
        else
        {
            Debug.LogWarning("HostBtn not found");
        }

        var quitButton = _root.Q<Button>("QuitBtn");
        if (quitButton != null)
        {
            quitButton.clicked += OnQuitButtonClicked;
            Debug.Log("QuitBtn handler assigned");
        }
        else
        {
            Debug.LogWarning("QuitBtn not found");
        }
    }

    private void RemoveEventHandlers()
    {
        Debug.Log("Removing event handlers");

        var findGameButton = _root.Q<Button>("FindGameBtn");
        if (findGameButton != null)
        {
            findGameButton.clicked -= OnFindGameButtonClicked;
        }

        var settingsButton = _root.Q<Button>("SettingsBtn");
        if (settingsButton != null)
        {
            settingsButton.clicked -= OnSettingsButtonClicked;
        }

        var hostButton = _root.Q<Button>("HostBtn");
        if (hostButton != null)
        {
            hostButton.clicked -= OnHostButtonClicked;
        }

        var quitButton = _root.Q<Button>("QuitBtn");
        if (quitButton != null)
        {
            quitButton.clicked -= OnQuitButtonClicked;
        }
    }

    private void OnHostButtonClicked()
    {
        Debug.Log("Host Button Clicked");
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("Settings Button Clicked");
    }

    private void OnFindGameButtonClicked()
    {
        Debug.Log("Find Game Button Clicked");
    }

    private void OnQuitButtonClicked()
    {
        Debug.Log("Quit Button Clicked");
        Application.Quit();
    }
}
