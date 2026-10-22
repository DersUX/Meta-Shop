using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Session.Services
{
	public interface ISessionService
	{
		event Action<bool> AuthorizationStateChanged;

		event Action<bool> IsAuthorizingChanged;

		bool IsAuthorizing { get; }

		bool HasValidToken { get; }

		void InvalidateToken(string rejectedToken);

		UniTask<string> GetValidTokenAsync(CancellationToken cancellationToken = default);
	}
}
