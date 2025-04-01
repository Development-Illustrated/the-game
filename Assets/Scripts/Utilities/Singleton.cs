using UnityEngine;

/// <summary>
/// Similar to a singleton, but instead of destroying any new instances
/// it overrides the current instance. This is handy for restting the state
/// and saves you doing it manually.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set ;}
    protected virtual void Awake() => Instance = this as T;

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}

/// <summary>
/// This transforms the static instance into a basic singleton. This will destroy any new versions
/// created, leaving the original intact.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if(Instance != null) Destroy(gameObject);
        base.Awake();
    }
}

/// <summary>
/// This is a persistent singleton. This will survive through scene loads. 
/// This is ideal for system classes which require stateful, persistent data. Or audio sources
/// where music plays through loading screens, etc
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}