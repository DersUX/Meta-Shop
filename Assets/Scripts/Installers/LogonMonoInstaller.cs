using Common;
using Infrastructure.SceneManagement;
using Logon.Configs;
using Logon.UI.Presenters;
using Logon.UI.Views;
using UnityEngine;
using Zenject;

namespace Installers
{
	public class LogonMonoInstaller : MonoInstaller
	{
		[SerializeField] private LogonView _logonView;
		[SerializeField] private LogonConfig _config;
		[SerializeField] private LoadingAnimationView _loadingAnimationView;

		private void Awake()
		{
			Utils.AssertIsNotNull(_logonView);
			Utils.AssertIsNotNull(_config);
			Utils.AssertIsNotNull(_loadingAnimationView);
		}

		public override void InstallBindings()
		{
			Container.Bind<LogonConfig>().FromInstance(_config).AsSingle();
			Container.Bind<ILogonView>().FromInstance(_logonView).AsSingle();
			Container.Bind<LoadingAnimationView>().FromInstance(_loadingAnimationView).AsSingle();
			Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
			Container.BindInterfacesAndSelfTo<LogonPresenter>().AsSingle().NonLazy();
		}
	}
}
