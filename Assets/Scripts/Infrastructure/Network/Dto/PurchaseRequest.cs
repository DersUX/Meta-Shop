using System;

namespace Infrastructure.Network.Dto
{
	[Serializable]
	public class PurchaseRequest
	{
		public int goodId;
		public string requestId;
	}
}
