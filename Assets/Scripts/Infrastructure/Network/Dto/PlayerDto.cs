using System;

namespace Infrastructure.Network.Dto
{
	[Serializable]
	public class PlayerDto
	{
		public int id;
		public string login;
		public int balance = -1;
	}
}
