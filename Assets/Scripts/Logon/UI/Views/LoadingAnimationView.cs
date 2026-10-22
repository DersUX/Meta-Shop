using DG.Tweening;
using UnityEngine;

namespace Logon.UI.Views
{
	public class LoadingAnimationView : MonoBehaviour
	{
		private const string WindowHeader = "Window";
		private const string IconHeader = "Icon";
		private const string TitleHeader = "Title";
		private const string DividerHeader = "Divider";
		private const string ContentHeader = "Content";

		[Header(WindowHeader)]
		[SerializeField] private RectTransform _window;
		[SerializeField] private CanvasGroup _windowGroup;

		[Header(IconHeader)]
		[SerializeField] private RectTransform _icon;
		[SerializeField] private CanvasGroup _iconGroup;

		[Header(TitleHeader)]
		[SerializeField] private RectTransform _title;
		[SerializeField] private CanvasGroup _titleGroup;

		[Header(DividerHeader)]
		[SerializeField] private RectTransform _divider;
		[SerializeField] private CanvasGroup _dividerGroup;

		[Header(ContentHeader)]
		[SerializeField] private CanvasGroup _contentGroup;

		private Sequence _introSequence;

		private Vector3 _windowScale;
		private Vector3 _iconScale;
		private Vector3 _dividerScale;
		private Vector2 _titlePosition;

		private void Awake()
		{
			_windowScale = _window.localScale;
			_iconScale = _icon.localScale;
			_dividerScale = _divider.localScale;
			_titlePosition = _title.anchoredPosition;

			ResetVisualState();
		}

		private void Update()
		{
			if (_introSequence == null || !_introSequence.IsActive())
				return;

			float deltaTime = Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
			_introSequence.ManualUpdate(deltaTime, deltaTime);
		}

		private void OnDisable()
		{
			_introSequence?.Kill();
			_introSequence = null;
		}

		public Sequence PlayIntro()
		{
			_introSequence?.Kill();

			_introSequence = DOTween.Sequence()
				.SetAutoKill(false)
				.SetUpdate(UpdateType.Manual, true);

			// 1. Окно
			_introSequence.Insert(0f, _windowGroup.DOFade(1f, 0.3f));

			_introSequence.Insert(0f, _window.DOScale(_windowScale, 0.35f)
				.SetEase(Ease.OutCubic));

			// 2. Иконка
			_introSequence.Insert(0.13f, _iconGroup.DOFade(1f, 0.2f));

			_introSequence.Insert(0.13f, _icon.DOScale(_iconScale, 0.32f)
				.SetEase(Ease.OutBack));

			// 3. Заголовок
			_introSequence.Insert(0.2f, _titleGroup.DOFade(1f, 0.25f));

			_introSequence.Insert(0.2f, _title.DOAnchorPos(_titlePosition, 0.25f)
				.SetEase(Ease.OutCubic));

			// 4. Разделитель
			_introSequence.Insert(0.28f, _dividerGroup.DOFade(1f, 0.2f));

			_introSequence.Insert(0.28f, _divider.DOScaleX(_dividerScale.x, 0.2f)
				.SetEase(Ease.OutCubic));

			// 5. Контент
			_introSequence.Insert(0.36f, _contentGroup.DOFade(1f, 0.24f));

			return _introSequence;
		}

		private void ResetVisualState()
		{
			_windowGroup.alpha = 0f;
			_window.localScale = _windowScale * 0.94f;

			_iconGroup.alpha = 0f;
			_icon.localScale = _iconScale * 0.8f;

			_titleGroup.alpha = 0f;
			_title.anchoredPosition = _titlePosition + Vector2.down * 10f;

			_dividerGroup.alpha = 0f;
			_divider.localScale = new Vector3(0f, _dividerScale.y, _dividerScale.z);

			_contentGroup.alpha = 0f;
		}
	}
}
