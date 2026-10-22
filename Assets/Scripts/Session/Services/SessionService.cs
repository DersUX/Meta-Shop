using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Network;
using Session.Configs;
using Session.Dto;
using UnityEngine;
using UnityEngine.Networking;

namespace Session.Services
{
	public class SessionService : ISessionService
	{
		private const string InvalidSessionTokenText = "Authorization did not provide a valid session token.";
		private const string AuthorizationFailedFormat = "Authorization failed: {0}";
		private const string MissingSessionTokenText = "Authorization response does not contain a session token.";
		private const string MissingTokenTimingText = "serverTime and expiresAt must not be null or empty.";
		private const string ExpiredSessionTokenText = "Authorization returned an expired session token.";

		private readonly IBackendConfig _backendConfig;
		private readonly UserConfig _userConfig;

		private string _sessionToken;
		private readonly SemaphoreSlim _authorizationGate = new SemaphoreSlim(1, 1);

		private DateTime _serverTime;
		private DateTime _expiresAt;
		private double _receivedAt;

		public event Action<bool> AuthorizationStateChanged;

		public event Action<bool> IsAuthorizingChanged;

		public bool IsAuthorizing { get; private set; }

		public bool HasValidToken => !string.IsNullOrWhiteSpace(_sessionToken) && IsTokenValid();

		public SessionService(IBackendConfig backendConfig, UserConfig userConfig)
		{
			if (backendConfig == null)
				throw new ArgumentNullException(nameof(backendConfig));

			if (userConfig == null)
				throw new ArgumentNullException(nameof(userConfig));

			_backendConfig = backendConfig;
			_userConfig = userConfig;
		}

		public void InvalidateToken(string rejectedToken)
		{
			if (string.IsNullOrEmpty(rejectedToken) || _sessionToken != rejectedToken)
				return;

			_sessionToken = null;
			AuthorizationStateChanged?.Invoke(false);
		}

		public async UniTask<string> GetValidTokenAsync(CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (HasValidToken)
				return _sessionToken;

			await _authorizationGate.WaitAsync(cancellationToken);
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (!HasValidToken)
				{
					try
					{
						AuthorizationStateChanged?.Invoke(false);

						IsAuthorizing = true;
						IsAuthorizingChanged?.Invoke(true);

						await AuthorizeAsync(cancellationToken);

						if (!HasValidToken)
							throw new InvalidOperationException(InvalidSessionTokenText);

						AuthorizationStateChanged?.Invoke(true);
					}
					finally
					{
						IsAuthorizing = false;
						IsAuthorizingChanged?.Invoke(false);
					}
				}

				cancellationToken.ThrowIfCancellationRequested();

				if (!HasValidToken)
					throw new InvalidOperationException(InvalidSessionTokenText);

				return _sessionToken;
			}
			finally
			{
				_authorizationGate.Release();
			}
		}

		private async UniTask AuthorizeAsync(CancellationToken cancellationToken)
		{
			LoginRequest data = new LoginRequest
			{
				login = _userConfig.Login,
				password = _userConfig.Password
			};

			string json = JsonUtility.ToJson(data);

			using UnityWebRequest request =
				UnityWebRequest.Post(
					_backendConfig.LoginUrl,
					json,
					"application/json"
				);

			request.timeout = 15;

			await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

			cancellationToken.ThrowIfCancellationRequested();

			if (request.result != UnityWebRequest.Result.Success)
				throw new InvalidOperationException(string.Format(AuthorizationFailedFormat, request.error));

			AuthorizationResponse response =
				JsonUtility.FromJson<AuthorizationResponse>(request.downloadHandler.text);

			if (response == null || string.IsNullOrWhiteSpace(response.sessionToken))
				throw new InvalidOperationException(MissingSessionTokenText);

			SetTokenTiming(response.serverTime, response.expiresAt);
			_sessionToken = response.sessionToken;
		}

		private void SetTokenTiming(
			string serverTime,
			string expiresAt)
		{
			if (string.IsNullOrEmpty(serverTime) || string.IsNullOrEmpty(expiresAt))
				throw new ArgumentException(MissingTokenTimingText);

			DateTime parsedServerTime = DateTimeOffset
				.Parse(serverTime, CultureInfo.InvariantCulture)
				.UtcDateTime;

			DateTime parsedExpiresAt = DateTimeOffset
				.Parse(expiresAt, CultureInfo.InvariantCulture)
				.UtcDateTime;

			if (parsedExpiresAt <= parsedServerTime)
				throw new InvalidOperationException(ExpiredSessionTokenText);

			_serverTime = parsedServerTime;
			_expiresAt = parsedExpiresAt;
			_receivedAt = Time.realtimeSinceStartupAsDouble;
		}

		private bool IsTokenValid()
		{
			double elapsed = Time.realtimeSinceStartupAsDouble - _receivedAt;
			DateTime estimatedServerTime = _serverTime.AddSeconds(elapsed);

			return estimatedServerTime < _expiresAt;
		}
	}
}
