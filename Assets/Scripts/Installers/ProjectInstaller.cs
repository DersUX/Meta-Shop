using Common;
using Infrastructure.Display;
using Infrastructure.Network;
using Player.Services;
using Session.Configs;
using Session.Services;
using UnityEngine;
using Zenject;

namespace Installers
{
	public class ProjectInstaller : MonoInstaller
	{
		[SerializeField] private BackendConfig _backendConfig;
		[SerializeField] private UserConfig _userConfig;

		private void Awake()
		{
			Utils.AssertIsNotNull(_backendConfig);
			Utils.AssertIsNotNull(_userConfig);
		}

		public override void InstallBindings()
		{
			Container.Bind<IBackendConfig>().FromInstance(_backendConfig).AsSingle();
			Container.Bind<UserConfig>().FromInstance(_userConfig).AsSingle();
			Container.Bind<ISessionService>().To<SessionService>().AsSingle();
			Container.BindInterfacesTo<NetworkService>().AsSingle();
			Container.Bind<PlayerService>().AsSingle();
			Container.BindInterfacesAndSelfTo<ScreenService>().AsSingle().NonLazy();
		}
	}
}
