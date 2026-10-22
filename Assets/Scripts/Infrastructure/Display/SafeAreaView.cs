using UnityEngine;
using Zenject;

namespace Infrastructure.Display
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class SafeAreaView : MonoBehaviour
	{
		private IScreenService _screenService;

		[Inject]
		private void Construct(IScreenService screenService)
		{
			Unsubscribe();
			_screenService = screenService;

			if (isActiveAndEnabled)
				Subscribe();
		}

		private void OnEnable() => Subscribe();

		private void OnDisable() => Unsubscribe();

		private void Subscribe()
		{
			if (_screenService == null)
				return;

			_screenService.ScreenChanged -= Apply;
			_screenService.ScreenChanged += Apply;

			Apply(_screenService.Size);
		}

		private void Unsubscribe()
		{
			if (_screenService != null)
				_screenService.ScreenChanged -= Apply;
		}

		private void Apply(Vector2Int size)
		{
			if (size.x <= 0 || size.y <= 0)
				return;

			Rect area = _screenService.SafeArea;
			RectTransform rect = (RectTransform)transform;

			rect.anchorMin = new Vector2(area.xMin / size.x, area.yMin / size.y);
			rect.anchorMax = new Vector2(area.xMax / size.x, area.yMax / size.y);

			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
		}
	}
}
