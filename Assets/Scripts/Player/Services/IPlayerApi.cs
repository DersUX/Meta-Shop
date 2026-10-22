using System.Threading;
using Cysharp.Threading.Tasks;
using Player.Models;

namespace Player.Services
{
	public interface IPlayerApi
	{
		UniTask<PlayerModel> GetPlayerAsync(CancellationToken cancellationToken = default);
	}
}
