using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Infrastructure.SceneManagement;
using Logon.Configs;
using Logon.UI.Views;
using Session.Services;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Logon.UI.Presenters
{
	public class LogonPresenter : IInitializable, IDisposable
	{
		private const string StoreOpenFailedText = "Не удалось открыть магазин";
		private const string ConnectionFailedText = "Не удалось подключиться к серверу";
		private const string StoreLoadFailedText = "Не удалось загрузить магазин";

		private readonly ILogonView _view;
		private readonly LoadingAnimationView _loadingAnimationView;

		private readonly ISessionService _sessionService;
		private readonly ISceneLoader _sceneLoader;
		private readonly LogonConfig _config;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private bool _initialized;
		private bool _disposed;
		private bool _sceneLoaded;

		public bool IsLoading { get; private set; }

		public float Progress { get; private set; }

		public string ErrorMessage { get; private set; }

		public LogonPresenter(ILogonView view,
			LoadingAnimationView loadingAnimationView,
			ISessionService sessionService,
			ISceneLoader sceneLoader,
			LogonConfig config)
		{
			_view = view;
			_loadingAnimationView = loadingAnimationView;
			_sessionService = sessionService;
			_sceneLoader = sceneLoader;
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_config.Validate();
		}

		public void Initialize()
		{
			if (_initialized || _disposed)
				return;

			_initialized = true;
			_view.RetryRequested += Retry;
			LoadAsync(_cancellationTokenSource.Token, playIntro: true).Forget();
		}

		private void Retry()
		{
			if (_disposed || IsLoading || _sceneLoaded)
				return;

			LoadAsync(_cancellationTokenSource.Token, playIntro: false).Forget();
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_view.RetryRequested -= Retry;
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
		}

		private async UniTask LoadAsync(CancellationToken cancellationToken, bool playIntro)
		{
			IsLoading = true;
			ErrorMessage = null;
			string failureMessage = StoreOpenFailedText;

			try
			{
				SetProgress(0f);
				_view.ShowPreparing();

				if (playIntro)
					await PlayIntroAsync(cancellationToken);

				_sceneLoader.ValidateScene(_config.MainSceneName);

				await AdvanceProgressAsync(_config.PreparationProgress, cancellationToken);

				failureMessage = ConnectionFailedText;
				_view.ShowConnecting();

				await _sessionService.GetValidTokenAsync(cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();

				await AdvanceProgressAsync(_config.ConnectionProgress, cancellationToken);

				failureMessage = StoreLoadFailedText;
				_view.ShowSceneLoading();

				await _sceneLoader.LoadAsync(_config.MainSceneName, async progress =>
				{
					await AdvanceProgressAsync(Mathf.Lerp(_config.ConnectionProgress, _config.SceneLoadingProgress,
						Mathf.Clamp01(progress)), cancellationToken);

					if (progress >= 1f)
					{
						_view.ShowFinishing();

						await AdvanceProgressAsync(1f, cancellationToken);

						await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken);
					}
				}, cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();
				_sceneLoaded = true;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
			}
			catch (Exception exception)
			{
				if (!_disposed)
				{
					Debug.LogException(exception);
					ErrorMessage = failureMessage;
					_view.ShowError(ErrorMessage);
				}
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async UniTask PlayIntroAsync(CancellationToken cancellationToken)
		{
			await UniTask.WaitUntil(() => SplashScreen.isFinished, cancellationToken: cancellationToken);
			await UniTask.NextFrame(cancellationToken);
			Canvas.ForceUpdateCanvases();

			Sequence intro = _loadingAnimationView.PlayIntro();
			try
			{
				await UniTask.WaitUntil(() => !intro.IsActive() || intro.IsComplete(),
					cancellationToken: cancellationToken);
			}
			finally
			{
				intro.Kill();
			}

			cancellationToken.ThrowIfCancellationRequested();
		}

		private async UniTask AdvanceProgressAsync(float target, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (_config.MinimumLoadingSeconds <= 0f)
			{
				SetProgress(target);
				return;
			}

			float start = Progress;
			double startedAt = Time.realtimeSinceStartupAsDouble;

			while (Progress < target)
			{
				await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
				float elapsed = (float)(Time.realtimeSinceStartupAsDouble - startedAt);
				SetProgress(Mathf.Min(target, start + elapsed / _config.MinimumLoadingSeconds));
			}
		}

		private void SetProgress(float progress)
		{
			if (_disposed)
				return;

			Progress = progress;
			_view.SetProgress(progress);
		}
	}
}
