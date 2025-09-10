using UnityEngine;

public static class GameInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        var addressablesLoader = new AddressablesLoader();
        Services.RegisterLoader(addressablesLoader);
    }
}