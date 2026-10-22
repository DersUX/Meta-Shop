using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Display
{
	public interface IScreenService
	{
		event Action<Vector2Int> ScreenChanged;

		Vector2Int Size { get; }

		float AspectRatio { get; }

		Rect SafeArea { get; }

		IReadOnlyList<Rect> Cutouts { get; }
	}
}
