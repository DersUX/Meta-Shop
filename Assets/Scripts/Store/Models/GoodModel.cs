using System;

namespace Store.Models
{
	public class GoodModel
	{
		private const string EmptyHeaderText = "Good header must not be empty.";

		public readonly int Id;
		public readonly string Header;
		public readonly string Description;
		public readonly int Price;
		public readonly GoodType Type;

		public GoodModel(int id, string header, string description, int price, GoodType type)
		{
			if (id <= 0)
				throw new ArgumentOutOfRangeException(nameof(id));

			if (string.IsNullOrWhiteSpace(header))
				throw new ArgumentException(EmptyHeaderText, nameof(header));

			if (price < 0)
				throw new ArgumentOutOfRangeException(nameof(price));

			Id = id;
			Header = header;
			Description = description;
			Price = price;
			Type = type;
		}
	}
}
