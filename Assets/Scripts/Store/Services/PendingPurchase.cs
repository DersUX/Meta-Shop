using System;

namespace Store.Services
{
	[Serializable]
	public sealed class PendingPurchase
	{
		private const string InvalidPurchaseText = "Saved purchase is invalid; manual recovery is required.";

		public int GoodId;
		public string RequestId;
		public bool Confirmed;
		public bool Rejected;

		public void Validate()
		{
			if (GoodId <= 0 || !Guid.TryParseExact(RequestId, "N", out _) || (Confirmed && Rejected))
				throw new InvalidOperationException(InvalidPurchaseText);
		}
	}
}
