using System;

namespace Store.UI.Scrolling
{
	public class GoodsGridLayout
	{
		private const double ColumnFitTolerance = 0.0001;

		private const string InvalidCardDimensionsText = "Card dimensions must be finite and positive.";
		private const string InvalidDimensionText = "Dimension must be finite.";
		private const string InvalidSpacingText = "Spacing must be finite and non-negative.";
		private const string InvalidAlignmentText = "Unknown horizontal alignment.";
		private const string LayoutOverflowText = "Layout dimensions overflow.";
		private const string InvalidOffsetText = "Offset must be finite.";

		private readonly float _horizontalOffset;

		private float RowStep => CardSize.Height + VerticalSpacing;

		public int ItemCount { get; }

		public GoodsSize CardSize { get; }

		public float ContentWidth { get; }

		public float ViewportHeight { get; }

		public float VerticalSpacing { get; }

		public float HorizontalSpacing { get; }

		public GoodsHorizontalAlignment HorizontalAlignment { get; }

		public int Columns { get; }

		public float ContentHeight { get; }

		public float MaxOffset => Math.Max(0f, ContentHeight - ViewportHeight);

		public int PoolSize { get; }

		public GoodsGridLayout(
			int itemCount,
			GoodsSize cardSize,
			float contentWidth,
			float viewportHeight,
			float verticalSpacing,
			float horizontalSpacing,
			GoodsHorizontalAlignment horizontalAlignment = GoodsHorizontalAlignment.Left)
		{
			ValidateArguments(itemCount, cardSize, contentWidth, viewportHeight, verticalSpacing,
				horizontalSpacing, horizontalAlignment);

			ItemCount = itemCount;
			CardSize = cardSize;
			ContentWidth = Math.Max(0f, contentWidth);
			ViewportHeight = Math.Max(0f, viewportHeight);
			VerticalSpacing = verticalSpacing;
			HorizontalSpacing = horizontalSpacing;
			HorizontalAlignment = horizontalAlignment;

			Columns = CalculateColumns();
			_horizontalOffset = CalculateHorizontalOffset();
			int rows = itemCount / Columns + (itemCount % Columns == 0 ? 0 : 1);
			ContentHeight = CalculateContentHeight(rows);

			if (!GoodsLayoutMath.IsFinite(ContentHeight) || !GoodsLayoutMath.IsPositive(RowStep))
				throw new ArgumentException(LayoutOverflowText);

			PoolSize = CalculatePoolSize(rows);
		}

		public int GetFirstPooledIndex(float offset)
		{
			if (!GoodsLayoutMath.IsFinite(offset))
				throw new ArgumentException(InvalidOffsetText, nameof(offset));

			float clampedOffset = GoodsLayoutMath.Clamp(offset, 0f, MaxOffset);
			double firstVisibleRow = Math.Floor((clampedOffset + (double)VerticalSpacing) / RowStep);
			int firstVisibleIndex = (int)Math.Min(ItemCount, firstVisibleRow * Columns);
			int lastPoolStartIndex = ItemCount - PoolSize;

			return Math.Min(lastPoolStartIndex, Math.Max(0, firstVisibleIndex));
		}

		public float RestoreOffset(GoodsGridLayout previous, float offset)
		{
			if (!GoodsLayoutMath.IsFinite(offset))
				throw new ArgumentException(InvalidOffsetText, nameof(offset));

			if (previous == null || previous.ItemCount == 0 || ItemCount == 0)
				return 0f;

			float previousRowPosition = GoodsLayoutMath.Clamp(offset, 0f, previous.MaxOffset) / previous.RowStep;
			int previousRow = (int)Math.Floor(previousRowPosition);
			long previousRowFirstIndex = (long)previousRow * previous.Columns;
			int previousLastIndex = previous.ItemCount - 1;
			int anchorIndex = (int)Math.Min(previousLastIndex, previousRowFirstIndex);
			int newRow = anchorIndex / Columns;
			float restoredRowPosition = newRow + previousRowPosition - previousRow;
			float restoredOffset = restoredRowPosition * RowStep;

			return GoodsLayoutMath.Clamp(restoredOffset, 0f, MaxOffset);
		}

		public GoodsPosition GetPosition(int index)
		{
			int column = index % Columns;
			int row = -index / Columns;
			float x = _horizontalOffset + column * (CardSize.Width + HorizontalSpacing);
			float y = row * RowStep;

			return new GoodsPosition(x, y);
		}

		private int CalculateColumns()
		{
			double availableWidth = ContentWidth + (double)HorizontalSpacing + ColumnFitTolerance;
			double columnStep = CardSize.Width + (double)HorizontalSpacing;
			double columns = Math.Floor(availableWidth / columnStep);

			return (int)Math.Min(int.MaxValue, Math.Max(1d, columns));
		}

		private float CalculateHorizontalOffset()
		{
			int occupiedColumns = Math.Min(ItemCount, Columns);
			float gridWidth = occupiedColumns * CardSize.Width + Math.Max(0, occupiedColumns - 1) * HorizontalSpacing;
			float freeWidth = Math.Max(0f, ContentWidth - gridWidth);

			switch (HorizontalAlignment)
			{
				case GoodsHorizontalAlignment.Right:
					return freeWidth;
				case GoodsHorizontalAlignment.Middle:
					return freeWidth * 0.5f;
				default:
					return 0f;
			}
		}

		private float CalculateContentHeight(int rows) =>
			rows == 0 ? 0f : rows * CardSize.Height + (rows - 1) * VerticalSpacing;

		private int CalculatePoolSize(int rows)
		{
			if (ViewportHeight <= 0f)
				return 0;

			int maxVisibleRows =
				(int)Math.Min(rows, Math.Ceiling(((double)ViewportHeight + CardSize.Height) / RowStep));

			return (int)Math.Min(ItemCount, (long)maxVisibleRows * Columns);
		}

		private static void ValidateArguments(
			int itemCount,
			GoodsSize cardSize,
			float contentWidth,
			float viewportHeight,
			float verticalSpacing,
			float horizontalSpacing,
			GoodsHorizontalAlignment horizontalAlignment)
		{
			if (itemCount < 0)
				throw new ArgumentOutOfRangeException(nameof(itemCount));

			if (!GoodsLayoutMath.IsPositive(cardSize.Width) || !GoodsLayoutMath.IsPositive(cardSize.Height))
				throw new ArgumentException(InvalidCardDimensionsText, nameof(cardSize));

			if (!GoodsLayoutMath.IsFinite(contentWidth))
				throw new ArgumentException(InvalidDimensionText, nameof(contentWidth));

			if (!GoodsLayoutMath.IsFinite(viewportHeight))
				throw new ArgumentException(InvalidDimensionText, nameof(viewportHeight));

			if (!GoodsLayoutMath.IsFinite(verticalSpacing) || verticalSpacing < 0f)
				throw new ArgumentException(InvalidSpacingText, nameof(verticalSpacing));

			if (!GoodsLayoutMath.IsFinite(horizontalSpacing) || horizontalSpacing < 0f)
				throw new ArgumentException(InvalidSpacingText, nameof(horizontalSpacing));

			if (!Enum.IsDefined(typeof(GoodsHorizontalAlignment), horizontalAlignment))
				throw new ArgumentException(InvalidAlignmentText, nameof(horizontalAlignment));
		}
	}
}
