using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Player.Models;
using Player.Services;
using Store.Models;

namespace Store.Services
{
	public sealed class PurchaseService : IDisposable
	{
		private readonly IPurchaseApi _purchaseApi;
		private readonly IGoodsApi _goodsApi;
		private readonly PlayerService _player;
		private readonly GoodsListModel _goods;
		private readonly IPendingPurchaseStore _store;
		private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

		private PendingPurchase _pending;
		private int _playerId;
		private string _login;
		private bool _restored;
		private bool _disposed;
		private bool _hadUnknownResult;
		private bool _refreshPlayer;

		public PurchaseState State { get; private set; }

		public PurchaseError Error { get; private set; }

		public Exception LastException { get; private set; }

		public event Action StateChanged;

		public event Action<PurchaseResult> PurchaseSucceeded;

		public PurchaseService(
			IPurchaseApi purchaseApi,
			IGoodsApi goodsApi,
			PlayerService player,
			GoodsListModel goods,
			IPendingPurchaseStore store)
		{
			_purchaseApi = purchaseApi;
			_goodsApi = goodsApi;
			_player = player;
			_goods = goods;
			_store = store ?? throw new ArgumentNullException(nameof(store));

			_player.PlayerChanged += OnPlayerChanged;

			if (_player.CurrentPlayer != null)
				OnPlayerChanged(_player.CurrentPlayer);
		}

		public UniTask BuyAsync(int goodId, CancellationToken cancellationToken = default)
		{
			if (_disposed || State != PurchaseState.Idle || _player.CurrentPlayer == null)
				return UniTask.CompletedTask;

			cancellationToken.ThrowIfCancellationRequested();
			GoodModel selected = null;

			foreach (GoodModel good in _goods.Goods)
			{
				if (good.Id != goodId)
					continue;

				selected = good;
				break;
			}

			if (selected == null || goodId <= 0)
				return UniTask.CompletedTask;

			if (_player.CurrentPlayer.Balance < selected.Price)
			{
				SetState(PurchaseState.Idle, PurchaseError.InsufficientFunds);
				return UniTask.CompletedTask;
			}

			_pending = new PendingPurchase { GoodId = goodId, RequestId = Guid.NewGuid().ToString("N") };
			_hadUnknownResult = false;
			_refreshPlayer = false;

			if (!SavePending())
				return UniTask.CompletedTask;

			SetState(PurchaseState.Purchasing);

			return RunOperationAsync(PurchaseAsync, cancellationToken);
		}

		public UniTask RetryAsync(CancellationToken cancellationToken = default)
		{
			if (_disposed)
				return UniTask.CompletedTask;

			cancellationToken.ThrowIfCancellationRequested();

			if (!_restored)
			{
				RestorePending();
				return UniTask.CompletedTask;
			}

			if (_pending != null && _pending.Rejected)
			{
				CompleteRejected();
				return UniTask.CompletedTask;
			}

			if (State == PurchaseState.PurchaseUnknown)
			{
				if (_pending == null || !SavePending())
					return UniTask.CompletedTask;

				SetState(PurchaseState.Purchasing);

				return RunOperationAsync(PurchaseAsync, cancellationToken);
			}

			if (State == PurchaseState.RefreshFailed)
			{
				if (!SavePending())
					return UniTask.CompletedTask;

				SetState(PurchaseState.RefreshingGoods);

				return RunOperationAsync(RefreshAsync, cancellationToken);
			}

			return UniTask.CompletedTask;
		}

		private async UniTask RunOperationAsync(Func<CancellationToken, UniTask> operation,
			CancellationToken cancellationToken)
		{
			using CancellationTokenSource linked =
				CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token, cancellationToken);

			await operation(linked.Token);
		}

		private async UniTask PurchaseAsync(CancellationToken cancellationToken)
		{
			PendingPurchase pending = _pending;
			int playerId = _playerId;
			string login = _login;
			PurchaseResult response;

			try
			{
				response = await _purchaseApi.PurchaseAsync(pending.GoodId, pending.RequestId, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
			}
			catch (OperationCanceledException) when (_disposed)
			{
				return;
			}
			catch (PurchaseFailureException exception)
			{
				if (!IsCurrent(pending, playerId, login))
					return;

				bool unresolved = _hadUnknownResult && exception.Reason == PurchaseFailureReason.AuthorizationDenied;

				if (!unresolved)
				{
					pending.Rejected = true;
					CompleteRejected(exception);
					return;
				}

				SetState(PurchaseState.PurchaseUnknown, PurchaseError.ResultUnknown, exception);

				return;
			}
			catch (Exception exception)
			{
				if (!IsCurrent(pending, playerId, login))
					return;

				_hadUnknownResult = true;
				_refreshPlayer = true;
				SetState(PurchaseState.PurchaseUnknown, PurchaseError.ResultUnknown, exception);

				return;
			}

			if (!IsCurrent(pending, playerId, login))
				return;

			pending.Confirmed = true;
			if (!_refreshPlayer)
				_player.ApplyBalance(response.Balance);

			if (_disposed)
				return;

			if (!SavePending())
				return;

			SetState(PurchaseState.RefreshingGoods);
			PurchaseSucceeded?.Invoke(response);

			await RefreshAsync(cancellationToken);
		}

		private async UniTask RefreshAsync(CancellationToken cancellationToken)
		{
			PendingPurchase pending = _pending;
			int playerId = _playerId;
			string login = _login;

			try
			{
				if (_refreshPlayer)
				{
					await _player.LoadAsync(cancellationToken);

					if (!IsCurrent(pending, playerId, login))
						return;

					_refreshPlayer = false;
				}

				List<GoodModel> goods = await _goodsApi.GetGoodsAsync(cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();

				if (!IsCurrent(pending, playerId, login))
					return;

				_goods.ReplaceGoods(goods);
				_store.Clear(playerId, login);
				_pending = null;

				SetState(PurchaseState.Idle);
			}
			catch (OperationCanceledException) when (_disposed)
			{
			}
			catch (Exception exception)
			{
				if (!IsCurrent(pending, playerId, login))
					return;

				SetState(PurchaseState.RefreshFailed, PurchaseError.RefreshFailed, exception);
			}
		}

		private void OnPlayerChanged(PlayerModel player)
		{
			if (_disposed || (_playerId == player.Id && _login == player.Login))
				return;

			_playerId = player.Id;
			_login = player.Login;
			_pending = null;

			RestorePending();
		}

		private void RestorePending()
		{
			_restored = false;

			try
			{
				_pending = _store.Load(_playerId, _login);
				_restored = true;
				_hadUnknownResult = _pending != null;
				_refreshPlayer = _pending != null;

				SetState(
					_pending == null ? PurchaseState.Idle :
					_pending.Confirmed ? PurchaseState.RefreshFailed : PurchaseState.PurchaseUnknown,
					_pending == null ? PurchaseError.None : PurchaseError.RecoveryRequired);
			}
			catch (Exception exception)
			{
				SetState(PurchaseState.PurchaseUnknown, PurchaseError.PendingReadFailed, exception);
			}
		}

		private bool SavePending()
		{
			try
			{
				_store.Save(_playerId, _login, _pending);

				return true;
			}
			catch (Exception exception)
			{
				SetState(_pending.Confirmed ? PurchaseState.RefreshFailed : PurchaseState.PurchaseUnknown,
					PurchaseError.PendingWriteFailed, exception);

				return false;
			}
		}

		private void CompleteRejected(Exception rejection = null)
		{
			if (!SavePending())
				return;

			try
			{
				_store.Clear(_playerId, _login);
				_pending = null;

				SetState(PurchaseState.Idle, PurchaseError.Rejected, rejection);
			}
			catch (Exception exception)
			{
				SetState(PurchaseState.PurchaseUnknown, PurchaseError.PendingClearFailed, exception);
			}
		}

		private bool IsCurrent(PendingPurchase pending, int playerId, string login) =>
			!_disposed && ReferenceEquals(_pending, pending) && _playerId == playerId && _login == login;

		private void SetState(PurchaseState state, PurchaseError error = PurchaseError.None, Exception exception = null)
		{
			State = state;
			Error = error;
			LastException = exception;
			StateChanged?.Invoke();
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_player.PlayerChanged -= OnPlayerChanged;
			_lifetime.Cancel();
			_lifetime.Dispose();
		}
	}
}
