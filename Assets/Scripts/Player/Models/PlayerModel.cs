using System;

namespace Player.Models
{
	public class PlayerModel
	{
		private const string EmptyLoginText = "Player login must not be empty.";

		public readonly int Id;
		public readonly string Login;
		public readonly int Balance;

		public PlayerModel(int id, string login, int balance)
		{
			if (id <= 0)
				throw new ArgumentOutOfRangeException(nameof(id));

			if (string.IsNullOrWhiteSpace(login))
				throw new ArgumentException(EmptyLoginText, nameof(login));

			if (balance < 0)
				throw new ArgumentOutOfRangeException(nameof(balance));

			Id = id;
			Login = login;
			Balance = balance;
		}
	}
}
