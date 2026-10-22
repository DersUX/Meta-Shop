using Common;
using Session.UI.Presenters;
using Session.UI.Views;
using Store.Models;
using Store.Services;
using Store.UI.Configs;
using Store.UI.Presenters;
using Store.UI.Scrolling;
using Store.UI.Views;
using UnityEngine;
using Zenject;

namespace Installers
{
	public class MainMonoInstaller : MonoInstaller
	{
		[SerializeField] private SectionStoreView _sectionStoreView;
		[SerializeField] private GoodIconConfig _iconConfig;
		[SerializeField] private SessionLoadingView _sessionLoadingView;

		private void Awake()
		{
			Utils.AssertIsNotNull(_sectionStoreView);
			Utils.AssertIsNotNull(_iconConfig);
			Utils.AssertIsNotNull(_sessionLoadingView);
		}

		public override void InstallBindings()
		{
			BindSectionStore();
			Container.Bind<ISessionLoadingView>().FromInstance(_sessionLoadingView).AsSingle();
			Container.BindInterfacesAndSelfTo<SessionLoadingPresenter>().AsSingle().NonLazy();
		}

		private void BindSectionStore()
		{
			Container.Bind<ISectionStoreView>().FromInstance(_sectionStoreView).AsSingle();
			Container.Bind<IGoodIconConfig>().FromInstance(_iconConfig).AsSingle();
			Container.Bind<GoodsListModel>().AsSingle();
			Container.Bind<IPendingPurchaseStore>().To<FilePendingPurchaseStore>().AsSingle();
			Container.BindInterfacesAndSelfTo<PurchaseService>().AsSingle();
			Container.Bind<GoodsLayoutCalculator>().AsSingle();
			Container.Bind<IGoodsScrollView>().FromInstance(_sectionStoreView.GoodsScroll).AsSingle();
			Container.BindInterfacesAndSelfTo<GoodsScrollPresenter>().AsSingle().NonLazy();
			Container.BindInterfacesAndSelfTo<SectionStorePresenter>().AsSingle().NonLazy();
		}
	}
}
