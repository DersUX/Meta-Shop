using System;
using Store.Models;

namespace Store.UI.Scrolling
{
	public interface IGoodsScrollView
	{
		GoodsLayoutInput Geometry { get; }

		float ScrollOffset { get; }

		event Action GeometryChanged;

		event Action<float> ScrollChanged;

		void SetContentHeight(float height);

		void SetScrollOffset(float offset);

		void StopMovement();

		void SetSlotCount(int count);

		void SetSlotGeometry(int slot, GoodsSize size, GoodsPosition position);

		void BindSlot(int slot, GoodModel good);
	}
}
