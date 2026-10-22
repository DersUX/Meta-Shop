using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Store.Models
{
	public class GoodsListModel
	{
		private const string NullGoodText = "Goods must not contain null items.";
		private const string DuplicateGoodIdText = "Goods must have unique IDs.";

		private ReadOnlyCollection<GoodModel> _goods = new List<GoodModel>().AsReadOnly();

		public IReadOnlyList<GoodModel> Goods => _goods;

		public event Action GoodsChanged;

		public void ReplaceGoods(IEnumerable<GoodModel> goods)
		{
			if (goods == null)
				throw new ArgumentNullException(nameof(goods));

			List<GoodModel> replacement = new List<GoodModel>();
			HashSet<int> ids = new HashSet<int>();

			foreach (GoodModel good in goods)
			{
				if (good == null)
					throw new ArgumentException(NullGoodText, nameof(goods));

				if (!ids.Add(good.Id))
					throw new ArgumentException(DuplicateGoodIdText, nameof(goods));

				replacement.Add(good);
			}

			_goods = replacement.AsReadOnly();
			GoodsChanged?.Invoke();
		}
	}
}
