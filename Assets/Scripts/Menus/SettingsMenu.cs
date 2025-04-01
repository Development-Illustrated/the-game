using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsMenu : MonoBehaviour
{
    private VisualElement _rootElement;
    private Button _backButton;
    
    // Store the callback delegate
    private EventCallback<ClickEvent> _backButtonClickCallback;

    // Events
    public event Action OnBackClicked;

    void Awake()
    {
        _rootElement = GetComponent<UIDocument>().rootVisualElement;
        
        // Create the delegate once
        _backButtonClickCallback = ev => ClickBackButton();
    }

    void Start()
    {
        RegisterButtonCallbacks();
    }

    public void RegisterButtonCallbacks()
    {
        // Make sure to unregister first to avoid duplicates
        UnregisterButtonCallbacks();
        
        // Re-query the button (important when UI is reactivated)
        _backButton = _rootElement.Q<Button>("BackBtn");
        
        // Register callback using the stored delegate
        if (_backButton != null)
        {
            _backButton.RegisterCallback<ClickEvent>(_backButtonClickCallback);
        }
        else
        {
            Debug.LogError("Back button not found in UI");
        }
    }

    public void UnregisterButtonCallbacks()
    {
        if (_backButton != null)
        {
            _backButton.UnregisterCallback<ClickEvent>(_backButtonClickCallback);
        }
    }

    private void ClickBackButton()
    {
        Debug.Log("Back button clicked!");
        OnBackClicked?.Invoke();
    }
}
