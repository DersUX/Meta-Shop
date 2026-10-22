using System;

namespace Infrastructure.Network
{
	public sealed class PurchaseRejectedException : Exception
	{
		private const string PurchaseRejectedFormat = "Purchase HTTP {0}: {1}";

		public long StatusCode { get; }

		public string ResponseBody { get; }

		public PurchaseRejectedException(long statusCode, string responseBody)
			: base(string.Format(PurchaseRejectedFormat, statusCode, responseBody))
		{
			StatusCode = statusCode;
			ResponseBody = responseBody;
		}
	}
}
