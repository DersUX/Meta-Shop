using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Infrastructure.Display
{
	public class ScreenService : IScreenService, IInitializable, ITickable
	{
		public Vector2Int Size { get; private set; }

		public float AspectRatio => Size.y > 0 ? (float)Size.x / Size.y : 0f;

		public Rect SafeArea { get; private set; }

		public IReadOnlyList<Rect> Cutouts { get; private set; } = Array.Empty<Rect>();

		public event Action<Vector2Int> ScreenChanged;

		public ScreenService()
		{
			Refresh();
		}

		public void Initialize() => Refresh();

		public void Tick() => Refresh();

		private void Refresh() =>
			UpdateScreen(new Vector2Int(Screen.width, Screen.height), Screen.safeArea, Screen.cutouts);

		private void UpdateScreen(Vector2Int size, Rect safeArea, Rect[] cutouts)
		{
			if (size.x <= 0 || size.y <= 0)
				return;

			safeArea = ApplyCutouts(size, safeArea, cutouts);
			bool cutoutsChanged = Cutouts.Count != cutouts.Length;

			for (int i = 0; !cutoutsChanged && i < cutouts.Length; i++)
				cutoutsChanged = Cutouts[i] != cutouts[i];

			if (size == Size && safeArea == SafeArea && !cutoutsChanged)
				return;

			Size = size;
			SafeArea = safeArea;

			if (cutoutsChanged)
				Cutouts = Array.AsReadOnly(cutouts);

			ScreenChanged?.Invoke(size);
		}

		private static Rect ApplyCutouts(Vector2Int size, Rect safeArea, Rect[] cutouts)
		{
			float xMin = Mathf.Clamp(safeArea.xMin, 0f, size.x);
			float xMax = Mathf.Clamp(safeArea.xMax, xMin, size.x);
			float yMin = Mathf.Clamp(safeArea.yMin, 0f, size.y);
			float yMax = Mathf.Clamp(safeArea.yMax, yMin, size.y);

			foreach (Rect cutout in cutouts)
			{
				if (cutout.width <= 0f || cutout.height <= 0f ||
					!Rect.MinMaxRect(xMin, yMin, xMax, yMax).Overlaps(cutout))
					continue;

				float left = cutout.xMax / size.x;
				float right = (size.x - cutout.xMin) / size.x;
				float bottom = cutout.yMax / size.y;
				float top = (size.y - cutout.yMin) / size.y;
				float nearest = Mathf.Min(Mathf.Min(left, right), Mathf.Min(bottom, top));

				if (Mathf.Approximately(nearest, left))
					xMin = Mathf.Clamp(cutout.xMax, xMin, xMax);
				else if (nearest == right)
					xMax = Mathf.Clamp(cutout.xMin, xMin, xMax);
				else if (nearest == bottom)
					yMin = Mathf.Clamp(cutout.yMax, yMin, yMax);
				else
					yMax = Mathf.Clamp(cutout.yMin, yMin, yMax);
			}

			return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
		}
	}
}
