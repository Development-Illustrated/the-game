/// <summary>
/// We don't need anything in here, other than inheriting from PersistentSingleton.
/// This will ensure that the Systems object will persist through scene loads, as well as it's child objects.
/// </summary>
public class Systems : PersistentSingleton<Systems>
{
}
