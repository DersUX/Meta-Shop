using UnityEngine;

namespace Store.UI.Views
{
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public sealed class StoreWindowLayout : MonoBehaviour
	{
		[SerializeField, Min(1f)] private float _maxWidth = 1100f;
		[SerializeField, Min(0f)] private float _horizontalMargin = 80f;

		private void OnEnable() => Refresh();

		private void OnValidate() => Refresh();

		private void OnRectTransformDimensionsChange() => Refresh();

		private void Refresh()
		{
			RectTransform rect = (RectTransform)transform;

			if (!(rect.parent is RectTransform parent))
				return;

			float width = Mathf.Max(0f, Mathf.Min(_maxWidth, parent.rect.width - 2f * _horizontalMargin));

			if (!Mathf.Approximately(rect.rect.width, width))
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		}
	}
}
