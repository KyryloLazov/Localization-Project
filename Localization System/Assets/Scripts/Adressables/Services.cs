public static class Services
{
    public static IAddressablesLoader Loader { get; private set; }
    public static void RegisterLoader(IAddressablesLoader loader)
    {
        Loader = loader;
    }
}