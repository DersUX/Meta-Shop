using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Player.Models;

namespace Player.Services
{
	public class PlayerService
	{
		private const string PlayerNotLoadedText = "Player must be loaded before purchasing.";

		private readonly IPlayerApi _playerApi;

		public PlayerModel CurrentPlayer { get; private set; }

		public event Action<PlayerModel> PlayerChanged;

		public PlayerService(IPlayerApi playerApi)
		{
			_playerApi = playerApi;
		}

		public async UniTask<PlayerModel> LoadAsync(CancellationToken cancellationToken = default)
		{
			PlayerModel player = await _playerApi.GetPlayerAsync(cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();

			CurrentPlayer = player;
			PlayerChanged?.Invoke(player);
			return player;
		}

		public void ApplyBalance(int balance)
		{
			if (CurrentPlayer == null)
				throw new InvalidOperationException(PlayerNotLoadedText);

			CurrentPlayer = new PlayerModel(CurrentPlayer.Id, CurrentPlayer.Login, balance);
			PlayerChanged?.Invoke(CurrentPlayer);
		}
	}
}
