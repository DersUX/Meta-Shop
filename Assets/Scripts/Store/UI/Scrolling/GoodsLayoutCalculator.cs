using System;

namespace Store.UI.Scrolling
{
	public class GoodsLayoutCalculator
	{
		private const string InvalidSpacingOrAlignmentText = "Invalid spacing or horizontal alignment.";
		private const string InvalidDimensionsText = "Adaptive layout dimensions must be finite and limits valid.";

		public GoodsGridLayout Calculate(int count, GoodsLayoutInput input)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			if (!GoodsLayoutMath.IsFinite(input.Spacing) || input.Spacing < 0f ||
				!Enum.IsDefined(typeof(GoodsHorizontalAlignment), input.Alignment))
				throw new ArgumentException(InvalidSpacingOrAlignmentText, nameof(input));

			GoodsSize size = CalculateCardSize(input.Width, input.Height, input.MinWidth, input.MaxWidth,
				input.HorizontalSpacing, input.Ratio);

			return size.Width == 0f
				? null
				: new GoodsGridLayout(count, size, input.Width,
					input.Height, input.Spacing, input.HorizontalSpacing, input.Alignment);
		}

		public static GoodsSize CalculateCardSize(float width, float height, float minWidth,
			float maxWidth, float gap, float ratio)
		{
			if (!GoodsLayoutMath.IsFinite(width)
				|| !GoodsLayoutMath.IsFinite(height)
				|| !GoodsLayoutMath.IsPositive(minWidth)
				|| !GoodsLayoutMath.IsPositive(maxWidth)
				|| maxWidth < minWidth
				|| !GoodsLayoutMath.IsFinite(gap)
				|| gap < 0f
				|| !GoodsLayoutMath.IsPositive(ratio))
				throw new ArgumentException(InvalidDimensionsText);

			if (width <= 0f || height <= 0f)
				return new GoodsSize(0f, 0f);

			double columns = Math.Max(1d, Math.Floor(((double)width + gap) / (minWidth + (double)gap)));
			float cardWidth = (float)Math.Min((width - (columns - 1d) * gap) / columns, maxWidth);

			if (height >= minWidth * ratio)
				cardWidth = Math.Min(cardWidth, height / ratio);

			cardWidth = Math.Min(width, Math.Max(minWidth, cardWidth));

			return new GoodsSize(cardWidth, cardWidth * ratio);
		}
	}
}
