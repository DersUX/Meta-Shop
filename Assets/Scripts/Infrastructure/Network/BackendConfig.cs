using System;
using UnityEngine;

namespace Infrastructure.Network
{
	[CreateAssetMenu(fileName = "BackendConfig", menuName = "Network/Backend Config")]
	public class BackendConfig : ScriptableObject, IBackendConfig
	{
		private const string InvalidBaseUrlText = "Backend base URL must be an absolute HTTP(S) URL without a query or fragment.";
		private const string InvalidEndpointText = "Backend endpoint must be a non-empty relative path.";

		[SerializeField] private string _baseUrl;
		[SerializeField] private string _loginPath = "/auth/login";
		[SerializeField] private string _playerPath = "/player";
		[SerializeField] private string _goodsPath = "/items";
		[SerializeField] private string _purchasesPath = "/purchases";

		public string LoginUrl => BuildUrl(_loginPath);

		public string PlayerUrl => BuildUrl(_playerPath);

		public string GoodsUrl => BuildUrl(_goodsPath);

		public string PurchasesUrl => BuildUrl(_purchasesPath);

		private string BuildUrl(string path)
		{
			if (!Uri.TryCreate(_baseUrl, UriKind.Absolute, out Uri baseUri)
				|| (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps)
				|| !string.IsNullOrEmpty(baseUri.Query) || !string.IsNullOrEmpty(baseUri.Fragment))
				throw new InvalidOperationException(
					InvalidBaseUrlText);

			string relativePath = path?.TrimStart('/');
			
			if (string.IsNullOrWhiteSpace(relativePath) || !Uri.TryCreate(relativePath, UriKind.Relative, out _))
				throw new InvalidOperationException(InvalidEndpointText);

			return _baseUrl.TrimEnd('/') + "/" + relativePath;
		}
	}
}
