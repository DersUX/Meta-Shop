using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Network.Dto;
using Newtonsoft.Json.Linq;
using Player.Models;
using Player.Services;
using Session.Services;
using Store.Models;
using Store.Services;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Network
{
	public class NetworkService : IPlayerApi, IGoodsApi, IPurchaseApi
	{
		private const string PlayerLoadFailedFormat = "Failed to load player: {0}";
		private const string EmptyPlayerResponseText = "Player response is empty.";
		private const string StoreLoadFailedFormat = "Failed to load store config: {0}";
		private const string MissingGoodsArrayText = "Store config does not contain a goods array.";
		private const string InvalidGoodsFieldsText = "Goods must contain an integer ID, integer price and string header.";
		private const string InvalidGoodsIdsText = "Goods must contain non-null items with unique IDs.";
		private const string RequestIdRequiredText = "Request ID is required.";
		private const string PurchaseUnknownFormat = "Purchase result is unknown: {0}";
		private const string InvalidPurchaseResponseText = "Purchase response is invalid; result is unknown.";
		private const string AuthorizationRetryLimitText = "Authorization retry limit reached.";

		private readonly ISessionService _sessionService;
		private readonly IBackendConfig _backendConfig;

		public NetworkService(ISessionService sessionService, IBackendConfig backendConfig)
		{
			_sessionService = sessionService;
			_backendConfig = backendConfig ?? throw new ArgumentNullException(nameof(backendConfig));
		}

		public async UniTask<PlayerModel> GetPlayerAsync(CancellationToken cancellationToken = default)
		{
			using UnityWebRequest request = await SendAuthorizedAsync(
				() => UnityWebRequest.Get(_backendConfig.PlayerUrl), cancellationToken);

			if (request.result != UnityWebRequest.Result.Success)
				throw new InvalidOperationException(string.Format(PlayerLoadFailedFormat, request.error));

			if (string.IsNullOrWhiteSpace(request.downloadHandler.text))
				throw new InvalidOperationException(EmptyPlayerResponseText);

			PlayerDto player = new PlayerDto();
			JsonUtility.FromJsonOverwrite(request.downloadHandler.text, player);

			return new PlayerModel(player.id, player.login, player.balance);
		}

		public async UniTask<List<GoodModel>> GetGoodsAsync(CancellationToken cancellationToken = default)
		{
			using UnityWebRequest request = await SendAuthorizedAsync(
				() => UnityWebRequest.Get(_backendConfig.GoodsUrl), cancellationToken);

			if (request.result != UnityWebRequest.Result.Success)
				throw new InvalidOperationException(string.Format(StoreLoadFailedFormat, request.error));

			JArray goods = JObject.Parse(request.downloadHandler.text)["goods"] as JArray;
			if (goods == null)
				throw new InvalidOperationException(MissingGoodsArrayText);

			List<GoodModel> goodModels = new List<GoodModel>(goods.Count);
			HashSet<int> ids = new HashSet<int>();

			foreach (JToken item in goods)
			{
				if (item.Type != JTokenType.Object
					|| item["id"]?.Type != JTokenType.Integer
					|| item["price"]?.Type != JTokenType.Integer
					|| item["header"]?.Type != JTokenType.String)
				{
					throw new InvalidOperationException(
						InvalidGoodsFieldsText);
				}

				GoodDto goodDto = item.ToObject<GoodDto>();
				if (!ids.Add(goodDto.id))
					throw new InvalidOperationException(InvalidGoodsIdsText);

				goodModels.Add(new GoodModel(goodDto.id, goodDto.header, goodDto.description, goodDto.price,
					(GoodType)goodDto.typeId));
			}

			return goodModels;
		}

		public async UniTask<PurchaseResult> PurchaseAsync(int goodId, string requestId,
			CancellationToken cancellationToken = default)
		{
			if (goodId <= 0)
				throw new ArgumentOutOfRangeException(nameof(goodId));

			if (string.IsNullOrWhiteSpace(requestId))
				throw new ArgumentException(RequestIdRequiredText, nameof(requestId));

			string json = JsonUtility.ToJson(new PurchaseRequest { goodId = goodId, requestId = requestId });
			using UnityWebRequest request = await SendAuthorizedAsync(
				() => UnityWebRequest.Post(_backendConfig.PurchasesUrl, json, "application/json"), cancellationToken);

			ThrowIfPurchaseRejected(request);
			if (request.result != UnityWebRequest.Result.Success)
				throw new InvalidOperationException(string.Format(PurchaseUnknownFormat, request.error));

			PurchaseResponse response = new PurchaseResponse();
			JsonUtility.FromJsonOverwrite(request.downloadHandler.text, response);

			if (response.requestId != requestId
				|| response.goodId != goodId
				|| response.price < 0
				|| response.balance < 0
				|| response.quantity <= 0)
			{
				throw new InvalidOperationException(InvalidPurchaseResponseText);
			}

			return new PurchaseResult(response.requestId, response.goodId, response.price, response.balance,
				response.quantity);
		}

		private async UniTask<UnityWebRequest> SendAuthorizedAsync(Func<UnityWebRequest> createRequest,
			CancellationToken cancellationToken)
		{
			for (int attempt = 0; attempt < 2; attempt++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				string token = await _sessionService.GetValidTokenAsync(cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();
				UnityWebRequest request = createRequest();
				try
				{
					request.timeout = 15;
					request.SetRequestHeader("Authorization", $"Bearer {token}");
					try
					{
						await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
					}
					catch (UnityWebRequestException) when (request.result == UnityWebRequest.Result.ProtocolError)
					{
					}

					cancellationToken.ThrowIfCancellationRequested();
					if (request.responseCode != 401)
						return request;

					_sessionService.InvalidateToken(token);
					if (attempt == 0)
					{
						request.Dispose();
						continue;
					}

					return request;
				}
				catch
				{
					request.Dispose();
					throw;
				}
			}

			throw new InvalidOperationException(AuthorizationRetryLimitText);
		}

		private static void ThrowIfPurchaseRejected(UnityWebRequest request)
		{
			switch (request.responseCode)
			{
				case < 400 or >= 500 or 408:
					return;
				case 401:
				case 403:
					throw new PurchaseFailureException(PurchaseFailureReason.AuthorizationDenied,
						new PurchaseRejectedException(request.responseCode, request.downloadHandler.text));
				default:
					throw new PurchaseFailureException(PurchaseFailureReason.Rejected,
						new PurchaseRejectedException(request.responseCode, request.downloadHandler.text));
			}
		}
	}
}
