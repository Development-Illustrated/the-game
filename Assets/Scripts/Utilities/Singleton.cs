using UnityEngine;

/// <summary>
/// A static instance is similar to singleton, but instead of destroying any new instances
/// it overrides the current instance. This is habndy for resetting the state
/// and saves you doing it manually.
/// </summary>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    protected virtual void Awake() => Instance = this as T; 

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}

/// <summary>
/// This transforms the static instance into a singleton. This will destroy any new versions
/// created, leaving the original intact.
/// </summary>
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        base.Awake();
    }
}

/// <summary>
/// This is a persistent version of a singleton. This will survive through scene loads.
/// Ideal for system classes which require stateful, persistent data. Or audio sources
/// where music plays through loading screens, etc.
/// </summary>
public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}