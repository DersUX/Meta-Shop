using UnityEngine;

namespace Store.UI.Views
{
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class GoodCardScale : MonoBehaviour
	{
		[SerializeField] private RectTransform _visual;

		private void OnEnable() => Refresh();

		private void OnRectTransformDimensionsChange() => Refresh();

		private void OnValidate() => Refresh();

		private void Refresh()
		{
			if (_visual == null || _visual == transform)
				return;

			Vector2 size = ((RectTransform)transform).rect.size;
			if (size.x <= 0f || size.y <= 0f)
				return;

			float scale = Mathf.Min(size.x / 200f, size.y / 300f);
			_visual.localScale = new Vector3(scale, scale, 1f);
			_visual.sizeDelta = size / scale;
		}
	}
}
