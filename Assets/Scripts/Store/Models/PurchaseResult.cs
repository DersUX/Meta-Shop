using System;

namespace Store.Models
{
	public sealed class PurchaseResult
	{
		private const string RequestIdRequiredText = "Request ID is required.";
		private const string InvalidResultText = "Purchase result values are invalid.";

		public string RequestId { get; }

		public int GoodId { get; }

		public int Price { get; }

		public int Balance { get; }

		public int Quantity { get; }

		public PurchaseResult(string requestId, int goodId, int price, int balance, int quantity)
		{
			if (string.IsNullOrWhiteSpace(requestId))
				throw new ArgumentException(RequestIdRequiredText, nameof(requestId));

			if (goodId <= 0 || price < 0 || balance < 0 || quantity <= 0)
				throw new ArgumentException(InvalidResultText);

			RequestId = requestId;
			GoodId = goodId;
			Price = price;
			Balance = balance;
			Quantity = quantity;
		}
	}
}
