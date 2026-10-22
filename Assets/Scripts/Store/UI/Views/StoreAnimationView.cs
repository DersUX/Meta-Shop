using DG.Tweening;
using UnityEngine;

namespace Store.UI.Views
{
	public sealed class StoreAnimationView : MonoBehaviour
	{
		private const string WindowHeader = "Window";
		private const string IconHeader = "Icon";
		private const string TitleHeader = "Title";
		private const string BalanceHeader = "Balance";
		private const string ContentHeader = "Content";

		private const float InitialWindowScale = 0.94f;
		private const float InitialIconScale = 0.8f;
		private const float HeaderOffsetY = 10f;

		private const float WindowFadeDuration = 0.3f;
		private const float WindowScaleDuration = 0.35f;
		private const float IconStartTime = 0.13f;
		private const float IconFadeDuration = 0.2f;
		private const float IconScaleDuration = 0.32f;
		private const float HeaderStartTime = 0.2f;
		private const float HeaderAnimationDuration = 0.25f;
		private const float ContentStartTime = 0.36f;
		private const float ContentFadeDuration = 0.24f;
		private const float MaxAnimationDeltaTime = 1f / 33f;

		[Header(WindowHeader)]
		[SerializeField] private RectTransform _window;
		[SerializeField] private CanvasGroup _windowGroup;

		[Header(IconHeader)]
		[SerializeField] private RectTransform _icon;
		[SerializeField] private CanvasGroup _iconGroup;

		[Header(TitleHeader)]
		[SerializeField] private RectTransform _heading;
		[SerializeField] private CanvasGroup _titleGroup;

		[Header(BalanceHeader)]
		[SerializeField] private RectTransform _balance;
		[SerializeField] private CanvasGroup _balanceGroup;

		[Header(ContentHeader)]
		[SerializeField] private CanvasGroup _contentGroup;

		private Sequence _introSequence;

		private Vector3 _windowScale;
		private Vector3 _iconScale;
		private Vector2 _headingPosition;
		private Vector2 _balancePosition;
		private bool _skipFirstFrame;

		private void Awake()
		{
			_windowScale = _window.localScale;
			_iconScale = _icon.localScale;
			_headingPosition = _heading.anchoredPosition;
			_balancePosition = _balance.anchoredPosition;
		}

		private void OnEnable()
		{
			ResetVisualState();

			_skipFirstFrame = true;

			PlayIntro();
		}

		private void Update()
		{
			if (_skipFirstFrame)
			{
				_skipFirstFrame = false;
				return;
			}

			if (_introSequence == null || !_introSequence.IsActive())
				return;

			float deltaTime = Mathf.Min(Time.unscaledDeltaTime, MaxAnimationDeltaTime);
			_introSequence.ManualUpdate(deltaTime, deltaTime);
		}

		private void OnDisable()
		{
			_introSequence?.Kill();
			_introSequence = null;
			_windowGroup.alpha = 1f;
			_iconGroup.alpha = 1f;
			_titleGroup.alpha = 1f;
			_balanceGroup.alpha = 1f;
			_contentGroup.alpha = 1f;
			_windowGroup.interactable = true;
			_window.localScale = _windowScale;
			_icon.localScale = _iconScale;
			_heading.anchoredPosition = _headingPosition;
			_balance.anchoredPosition = _balancePosition;
		}

		private void PlayIntro()
		{
			_introSequence = DOTween.Sequence()
				.SetUpdate(UpdateType.Manual, true);

			// 1. Окно
			_introSequence.Insert(0f, _windowGroup.DOFade(1f, WindowFadeDuration));

			_introSequence.Insert(0f, _window.DOScale(_windowScale, WindowScaleDuration)
				.SetEase(Ease.OutCubic));

			// 2. Иконка
			_introSequence.Insert(IconStartTime, _iconGroup.DOFade(1f, IconFadeDuration));

			_introSequence.Insert(IconStartTime, _icon.DOScale(_iconScale, IconScaleDuration)
				.SetEase(Ease.OutBack));

			// 3. Заголовок
			_introSequence.Insert(HeaderStartTime, _titleGroup.DOFade(1f, HeaderAnimationDuration));

			_introSequence.Insert(HeaderStartTime, _heading.DOAnchorPos(_headingPosition, HeaderAnimationDuration)
				.SetEase(Ease.OutCubic));

			// 4. Баланс
			_introSequence.Insert(HeaderStartTime, _balanceGroup.DOFade(1f, HeaderAnimationDuration));

			_introSequence.Insert(HeaderStartTime, _balance.DOAnchorPos(_balancePosition, HeaderAnimationDuration)
				.SetEase(Ease.OutCubic));

			// 5. Контент
			_introSequence.Insert(ContentStartTime, _contentGroup.DOFade(1f, ContentFadeDuration));

			_introSequence.OnComplete(() => _windowGroup.interactable = true);
		}

		private void ResetVisualState()
		{
			_windowGroup.alpha = 0f;
			_windowGroup.interactable = false;
			_window.localScale = _windowScale * InitialWindowScale;

			_iconGroup.alpha = 0f;
			_icon.localScale = _iconScale * InitialIconScale;

			_titleGroup.alpha = 0f;
			_heading.anchoredPosition = _headingPosition + Vector2.down * HeaderOffsetY;

			_balanceGroup.alpha = 0f;
			_balance.anchoredPosition = _balancePosition + Vector2.down * HeaderOffsetY;

			_contentGroup.alpha = 0f;
		}
	}
}
