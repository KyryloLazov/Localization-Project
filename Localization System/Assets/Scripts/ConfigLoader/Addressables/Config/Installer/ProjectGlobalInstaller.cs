using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "Installers/ProjectGlobalInstaller")]
public sealed class ProjectGlobalInstaller : ScriptableObjectInstaller<ProjectGlobalInstaller>
{
    [SerializeField] private string _localizationUrl = "https://localization-system-4a5c3.web.app/localization/localization.config.json";
    [SerializeField] private string _fontsUrl = "https://localization-system-4a5c3.web.app/configs/fonts.config.json";
    [SerializeField] private bool _useRemote = true;

    public override void InstallBindings()
    {
        Debug.Log("[ProjectGlobalInstaller] Bindings Installed Started");

        Container.Bind<IAddressablesLoader>().To<AddressablesLoader>().AsSingle();
        Container.Bind<IRemoteTextProvider>().To<UnityWebRequestTextProvider>().AsSingle().WithArguments(10);
        
        Container.BindInstance(_localizationUrl).WithId("LocalizationUrl");
        Container.BindInstance(_fontsUrl).WithId("FontsUrl");
        Container.BindInstance(_useRemote).WithId("UseRemote");

        Container.BindInterfacesAndSelfTo<LocalizationService>().AsSingle().NonLazy();
        Container.Bind<LocalizationFontService>().AsSingle().NonLazy();
    }
}