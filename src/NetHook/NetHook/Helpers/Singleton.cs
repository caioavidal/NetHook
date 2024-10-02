namespace NetHook.Helpers;

public abstract class Singleton<T>
{
    private static readonly object padlock = new();

    private static T _instance;

    public static T Instance
    {
        get
        {
            lock (padlock)
            {
                return Setup();
            }
        }
        set
        {
            lock (padlock)
            {
                _instance = value;
            }
        }
    }

    public static T Setup()
    {
        _instance ??= Activator.CreateInstance<T>();
        return _instance;
    }
}