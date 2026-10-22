using System;

namespace Store.Services
{
	public sealed class PurchaseFailureException : Exception
	{
		private const string PurchaseRejectedText = "Purchase was rejected: ";

		public PurchaseFailureReason Reason { get; }

		public PurchaseFailureException(PurchaseFailureReason reason, Exception innerException)
			: base(PurchaseRejectedText + reason, innerException)
		{
			Reason = reason;
		}
	}
}
