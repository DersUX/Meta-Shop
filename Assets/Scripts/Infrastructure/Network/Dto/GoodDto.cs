using System;

namespace Infrastructure.Network.Dto
{
	[Serializable]
	public class GoodDto
	{
		public int id;
		public string header;
		public string description;
		public int price = -1;
		public sbyte typeId;
	}
}
