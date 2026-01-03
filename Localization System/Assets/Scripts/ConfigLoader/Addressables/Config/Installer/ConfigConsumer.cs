using System.Linq;
using UnityEngine;

public abstract class ConfigConsumer<T> : MonoBehaviour where T : LiveConfigSO
{
    protected T Config { get; private set; }

    private async void Awake()
    {
        await ConfigInitGate.WaitReady();
        Config = ConfigHub.All().Values.FirstOrDefault(x => x is T) as T;
        if (Config != null) OnConfigReady();
    }

    protected abstract void OnConfigReady();
}