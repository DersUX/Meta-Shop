using System;

namespace Store.UI.Scrolling
{
	public static class GoodsLayoutMath
	{
		public static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

		public static bool IsPositive(float value) => IsFinite(value) && value > 0f;

		public static float Clamp(float value, float min, float max) => Math.Min(max, Math.Max(min, value));
	}
}
