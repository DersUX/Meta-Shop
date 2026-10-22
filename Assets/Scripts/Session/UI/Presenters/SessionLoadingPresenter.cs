using System;
using Session.Services;
using Session.UI.Views;
using Zenject;

namespace Session.UI.Presenters
{
	public class SessionLoadingPresenter : IInitializable, IDisposable
	{
		private readonly ISessionLoadingView _view;
		private readonly ISessionService _sessionService;

		public SessionLoadingPresenter(ISessionLoadingView view, ISessionService sessionService)
		{
			_view = view;
			_sessionService = sessionService;
		}

		public void Initialize()
		{
			_sessionService.IsAuthorizingChanged += OnIsAuthorizingChanged;
			OnIsAuthorizingChanged(_sessionService.IsAuthorizing);
		}

		public void Dispose()
		{
			_sessionService.IsAuthorizingChanged -= OnIsAuthorizingChanged;
			_view.Hide();
		}

		private void OnIsAuthorizingChanged(bool isAuthorizing)
		{
			if (isAuthorizing)
				_view.Show();
			else
				_view.Hide();
		}
	}
}
