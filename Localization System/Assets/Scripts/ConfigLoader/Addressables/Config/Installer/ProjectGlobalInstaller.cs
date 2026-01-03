using System.Collections.Generic;
using Zenject;
using UnityEngine;

[CreateAssetMenu(menuName = "Installers/ProjectGlobalInstaller")]
public sealed class ProjectGlobalInstaller : ScriptableObjectInstaller<ProjectGlobalInstaller>
{
    [SerializeField] private string _localizationUrl = "https://localization-system-4a5c3.web.app/localization/localization.config.json";
    [SerializeField] private bool _updateLocalizationOnStart = true;

    public override void InstallBindings()
    {
        Container.Bind<IAddressablesLoader>().To<AddressablesLoader>().AsSingle();
        Container.Bind<IRemoteTextProvider>().To<UnityWebRequestTextProvider>().AsSingle().WithArguments(10);
        
        Container.Bind<ConfigService>().AsSingle();
        Container.BindInterfacesTo<ConfigAppEntryPoint>().AsSingle().NonLazy();

        Container.BindInstance(_localizationUrl).WithId("LocalizationUrl");
        Container.BindInstance(_updateLocalizationOnStart).WithId("UseRemoteLocalization");
        Container.BindInterfacesAndSelfTo<LocalizationService>().AsSingle().NonLazy();
        Container.Bind<LocalizationFontService>().AsSingle().NonLazy();
    }

    private FontsConfigSO ResolveFontsConfig()
    {
        foreach (KeyValuePair<string, LiveConfigSO> kv in ConfigHub.All())
        {
            FontsConfigSO f = kv.Value as FontsConfigSO;
            if (f != null) return f;
        }
        return null;
    }
}

public sealed class ConfigAppEntryPoint : IInitializable
{
    private readonly ConfigService _cfg;
    private readonly DiContainer _container;

    public ConfigAppEntryPoint(ConfigService cfg, DiContainer container)
    {
        _cfg = cfg;
        _container = container;
    }

    public async void Initialize()
    {
        await _cfg.Initialize();

        foreach (KeyValuePair<string, LiveConfigSO> kv in _cfg.All())
        {
            System.Type t = kv.Value.GetType();
            if (!_container.HasBinding(t))
            {
                _container.Bind(t).FromInstance(kv.Value).AsSingle();
            }
        }
    }
}
