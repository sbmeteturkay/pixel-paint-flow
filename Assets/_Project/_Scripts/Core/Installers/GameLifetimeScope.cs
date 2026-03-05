using SabanCoreTemplate.SceneManagement;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SabanCoreTemplate
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoadingScreen _loadingScreenPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            // LoadingScreen — DontDestroyOnLoad, instantiate edilir
            LoadingScreen loadingScreen = Instantiate(_loadingScreenPrefab);
            builder.RegisterInstance(loadingScreen);

            // SceneLoader — ISceneLoader üzerinden inject edilir
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);

            // Bootstrapper entry point — Start()'ta MainMenu'ya geçer
            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}