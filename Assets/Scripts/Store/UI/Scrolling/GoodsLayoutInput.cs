namespace Store.UI.Scrolling
{
	public readonly struct GoodsSize
	{
		public readonly float Width;
		public readonly float Height;

		public GoodsSize(float width, float height)
		{
			Width = width;
			Height = height;
		}
	}

	public readonly struct GoodsPosition
	{
		public readonly float X;
		public readonly float Y;

		public GoodsPosition(float x, float y)
		{
			X = x;
			Y = y;
		}
	}

	public readonly struct GoodsLayoutInput
	{
		public readonly float Width;
		public readonly float Height;
		public readonly float MinWidth;
		public readonly float MaxWidth;
		public readonly float Spacing;
		public readonly float HorizontalSpacing;
		public readonly float Ratio;
		public readonly GoodsHorizontalAlignment Alignment;

		public GoodsLayoutInput(float width, float height, float minWidth, float maxWidth,
			float spacing, float horizontalSpacing, float ratio, GoodsHorizontalAlignment alignment)
		{
			Width = width;
			Height = height;
			MinWidth = minWidth;
			MaxWidth = maxWidth;
			Spacing = spacing;
			HorizontalSpacing = horizontalSpacing;
			Ratio = ratio;
			Alignment = alignment;
		}

		public bool Equals(GoodsLayoutInput other) =>
			Width == other.Width
			&& Height == other.Height
			&& MinWidth == other.MinWidth
			&& MaxWidth == other.MaxWidth
			&& Spacing == other.Spacing
			&& HorizontalSpacing == other.HorizontalSpacing
			&& Ratio == other.Ratio
			&& Alignment == other.Alignment;
	}
}
