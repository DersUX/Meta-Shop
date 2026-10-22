using System;

namespace Infrastructure.Network.Dto
{
	[Serializable]
	public class PurchaseResponse
	{
		public string requestId;
		public int goodId = -1;
		public int price = -1;
		public int balance = -1;
		public int quantity = -1;
	}
}
