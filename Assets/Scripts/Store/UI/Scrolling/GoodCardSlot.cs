using Store.UI.Views;
using UnityEngine;

namespace Store.UI.Scrolling
{
	public class GoodCardSlot
	{
		public readonly GoodView View;
		public readonly RectTransform Rect;

		public GoodCardSlot(GoodView view, RectTransform rect)
		{
			View = view;
			Rect = rect;
		}
	}
}
