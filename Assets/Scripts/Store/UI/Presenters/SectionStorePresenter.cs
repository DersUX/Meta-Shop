using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Player.Models;
using Player.Services;
using Store.Models;
using Store.Services;
using Store.UI.Views;
using UnityEngine;
using Zenject;

namespace Store.UI.Presenters
{
	public class SectionStorePresenter : IInitializable, IDisposable
	{
		private const string StoreLoadFailedText = "Не удалось загрузить магазин. Повторяем автоматически…";
		private const string PlayerLoadFailedFormat = "Failed to load player: {0}";
		private const string GoodsLoadFailedFormat = "Failed to load goods: {0}";
		private const string StoreLoadingText = "Загружаем магазин…";
		private const string PurchasingText = "Покупаем…";
		private const string RefreshingGoodsText = "Покупка выполнена. Обновляем товары…";
		private const string PurchaseSucceededText = "Покупка выполнена.";
		private const string InsufficientFundsText = "Недостаточно средств для покупки.";
		private const string PurchaseRejectedText = "Сервер отклонил покупку.";
		private const string PurchaseUnknownText = "Не удалось подтвердить покупку. Восстанавливаем автоматически…";
		private const string RefreshFailedText = "Покупка выполнена. Повторяем обновление данных автоматически…";
		private const string RecoveryRequiredText = "Восстанавливаем незавершённую покупку автоматически…";

		private const string PendingReadFailedText =
			"Не удалось прочитать сохранённую покупку. Повторяем автоматически…";

		private const string PendingWriteFailedText = "Не удалось сохранить покупку. Повторяем автоматически…";

		private const string PendingClearFailedText =
			"Не удалось сохранить результат покупки. Повторяем автоматически…";

		private readonly ISectionStoreView _view;
		private readonly IGoodsApi _goodsApi;
		private readonly PlayerService _playerService;
		private readonly GoodsListModel _goods;
		private readonly PurchaseService _purchases;
		private readonly Func<TimeSpan, CancellationToken, UniTask> _retryDelay;

		private CancellationTokenSource _cancellationTokenSource;
		private UniTaskCompletionSource _recoverySignal;
		private bool _initialized;
		private bool _disposed;
		private bool _loading;
		private bool _playerReady;
		private bool _goodsReady;
		private string _loadError;
		private bool _purchaseConfirmed;

		public SectionStorePresenter(
			ISectionStoreView view,
			IGoodsApi goodsApi,
			GoodsListModel goods,
			PlayerService playerService,
			PurchaseService purchases,
			Func<TimeSpan, CancellationToken, UniTask> retryDelay = null)
		{
			_view = view ?? throw new ArgumentNullException(nameof(view));
			_goodsApi = goodsApi;
			_playerService = playerService;
			_goods = goods ?? throw new ArgumentNullException(nameof(goods));
			_purchases = purchases;
			_retryDelay = retryDelay ?? DelayAsync;
		}

		public void Initialize()
		{
			if (_initialized || _disposed)
				return;

			_initialized = true;
			_cancellationTokenSource = new CancellationTokenSource();

			_view.BuyClicked += OnBuyClicked;
			_playerService.PlayerChanged += OnPlayerChanged;
			_purchases.StateChanged += UpdatePurchaseView;

			_view.ClearBalance();
			RunAsync(_cancellationTokenSource.Token).Forget();
		}

		private async UniTask RunAsync(CancellationToken cancellationToken)
		{
			try
			{
				await LoadAsync(cancellationToken);
				int retrySeconds = 2;

				while (true)
				{
					cancellationToken.ThrowIfCancellationRequested();

					if (!NeedsRecovery())
					{
						retrySeconds = 2;
						_recoverySignal = new UniTaskCompletionSource();

						await _recoverySignal.Task.AttachExternalCancellation(cancellationToken);

						_recoverySignal = null;
						continue;
					}

					await _retryDelay(TimeSpan.FromSeconds(retrySeconds), cancellationToken);

					cancellationToken.ThrowIfCancellationRequested();
					if (!NeedsRecovery())
						continue;

					if (!_playerReady || !_goodsReady)
						await LoadAsync(cancellationToken);
					else
						await _purchases.RetryAsync(cancellationToken);

					retrySeconds = Math.Min(30, retrySeconds * 2);
				}
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
			}
			finally
			{
				_recoverySignal = null;
			}
		}

		private bool NeedsRecovery() =>
			!_loading && (!_playerReady
				|| !_goodsReady
				|| _purchases.State == PurchaseState.PurchaseUnknown
				|| _purchases.State == PurchaseState.RefreshFailed);

		private static UniTask DelayAsync(TimeSpan delay, CancellationToken cancellationToken) =>
			UniTask.Delay(delay, DelayType.Realtime, cancellationToken: cancellationToken, cancelImmediately: true);

		private async UniTask LoadAsync(CancellationToken cancellationToken)
		{
			if (_loading)
				return;

			_loading = true;
			_loadError = null;
			UpdatePurchaseView();

			try
			{
				await UniTask.WhenAll(LoadPlayerAsync(cancellationToken), LoadGoodsAsync(cancellationToken));
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
			}
			finally
			{
				_loading = false;

				if (_initialized)
					UpdatePurchaseView();
			}
		}

		private async UniTask LoadPlayerAsync(CancellationToken cancellationToken)
		{
			if (_playerReady)
				return;
			try
			{
				await _playerService.LoadAsync(cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();
				_playerReady = true;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception exception)
			{
				_loadError = StoreLoadFailedText;
				Debug.LogError(string.Format(PlayerLoadFailedFormat, exception));
			}
		}

		private async UniTask LoadGoodsAsync(CancellationToken cancellationToken)
		{
			if (_goodsReady)
				return;

			try
			{
				List<GoodModel> goods = await _goodsApi.GetGoodsAsync(cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();

				_goods.ReplaceGoods(goods);
				_goodsReady = true;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception exception)
			{
				_loadError = StoreLoadFailedText;
				Debug.LogError(string.Format(GoodsLoadFailedFormat, exception));
			}
		}

		private void OnBuyClicked(int goodId)
		{
			if (_loading || !_playerReady || !_goodsReady)
				return;

			_purchases.BuyAsync(goodId, _cancellationTokenSource.Token).Forget();
		}

		private void OnPlayerChanged(PlayerModel player) => _view.SetBalance(player.Balance);

		private void UpdatePurchaseView()
		{
			if (!_initialized)
				return;

			PurchaseState state = _purchases.State;
			_view.SetPurchaseInteractable(!_loading && _playerReady && _goodsReady && state == PurchaseState.Idle);
			string message = GetPurchaseErrorMessage(_purchases.Error);

			if (_loading)
			{
				message = StoreLoadingText;
			}
			else if (_loadError != null)
			{
				message = _loadError;
			}
			else if (state == PurchaseState.Purchasing)
			{
				message = PurchasingText;
			}
			else if (state == PurchaseState.RefreshingGoods)
			{
				_purchaseConfirmed = true;
				message = RefreshingGoodsText;
			}

			bool succeeded = !_loading
				&& _playerReady
				&& _goodsReady
				&& state == PurchaseState.Idle
				&& _purchases.Error == PurchaseError.None
				&& _purchaseConfirmed;

			if (succeeded)
			{
				message = PurchaseSucceededText;
				_purchaseConfirmed = false;
			}

			bool dismissible = state == PurchaseState.Idle && _purchases.Error != PurchaseError.None;
			_view.ShowPurchaseStatus(message, succeeded
				? PurchaseStatusKind.Success
				: dismissible
					? PurchaseStatusKind.Error
					: PurchaseStatusKind.Progress);

			_recoverySignal?.TrySetResult();
		}

		private static string GetPurchaseErrorMessage(PurchaseError error)
		{
			switch (error)
			{
				case PurchaseError.None:
					return null;
				case PurchaseError.InsufficientFunds:
					return InsufficientFundsText;
				case PurchaseError.Rejected:
					return PurchaseRejectedText;
				case PurchaseError.ResultUnknown:
					return PurchaseUnknownText;
				case PurchaseError.RefreshFailed:
					return RefreshFailedText;
				case PurchaseError.RecoveryRequired:
					return RecoveryRequiredText;
				case PurchaseError.PendingReadFailed:
					return PendingReadFailedText;
				case PurchaseError.PendingWriteFailed:
					return PendingWriteFailedText;
				case PurchaseError.PendingClearFailed:
					return PendingClearFailedText;
				default:
					throw new ArgumentOutOfRangeException(nameof(error), error, null);
			}
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			if (!_initialized)
				return;

			_initialized = false;

			_view.BuyClicked -= OnBuyClicked;
			_playerService.PlayerChanged -= OnPlayerChanged;
			_purchases.StateChanged -= UpdatePurchaseView;

			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = null;
		}
	}
}
