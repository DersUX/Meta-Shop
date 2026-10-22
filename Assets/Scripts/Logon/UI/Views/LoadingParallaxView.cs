using DG.Tweening;
using UnityEngine;

namespace Logon.UI.Views
{
	public class LoadingParallaxView : MonoBehaviour
	{
		[SerializeField] private RectTransform _background;

		private Vector2 _startPosition;
		private Vector3 _startScale;
		private Tween _tween;

		private void Awake()
		{
			_startPosition = _background.anchoredPosition;
			_startScale = _background.localScale;
		}

		private void OnEnable()
		{
			_tween = _background
				.DOScale(_startScale * 1.055f, 7f)
				.SetEase(Ease.InOutSine)
				.SetLoops(-1, LoopType.Yoyo)
				.SetUpdate(true);
		}

		private void OnDisable()
		{
			_tween?.Kill();
			_tween = null;

			_background.anchoredPosition = _startPosition;
			_background.localScale = _startScale;
		}
	}
}
