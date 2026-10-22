using System;
using Common;
using Infrastructure.Display;
using Store.Models;
using Store.UI.Configs;
using Store.UI.Views;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Store.UI.Scrolling
{
	[RequireComponent(typeof(ScrollRect))]
	[DisallowMultipleComponent]
	public class GoodsScrollView : MonoBehaviour, IGoodsScrollView
	{
		private const string MissingRectTransformText = "Good prefab must have a RectTransform.";
		private const string InvalidPrefabDimensionsText = "Good prefab must have finite positive dimensions.";
		private const string ContentLayoutConflictText = "Disable LayoutGroup and ContentSizeFitter on Content: GoodsScrollView controls its layout.";
		private const string MissingIconConfigText = "GoodsScrollView requires IGoodIconConfig injection.";

		[SerializeField] private GoodView _goodPrefab;
		[SerializeField] private ScrollRect _scrollRect;
		[SerializeField, Min(0.01f)] private float _minCardWidth = 200f;
		[SerializeField, Min(0.01f)] private float _maxCardWidth = 280f;
		[SerializeField, Min(0f)] private float _spacing = 10f;
		[SerializeField, Min(0f)] private float _horizontalSpacing = 10f;
		[SerializeField] private GoodsHorizontalAlignment _horizontalAlignment = GoodsHorizontalAlignment.Left;

		private RectTransform _content;
		private RectTransform _viewport;
		private float _cardRatio;
		private GoodCardPool _pool;
		private IScreenService _screenService;
		private IGoodIconConfig _iconConfig;
		private bool _screenSubscribed;
		private bool _screenDirty = true;
		private bool _geometryDirty = true;
		private bool _measuring;
		private GoodsLayoutInput _lastGeometry;
		private bool _purchaseInteractable;

		public event Action GeometryChanged;

		public event Action<float> ScrollChanged;

		public event Action<int> BuyClicked;

		public GoodsLayoutInput Geometry
		{
			get
			{
				Initialize();
				return new GoodsLayoutInput(_content.rect.width, _viewport.rect.height, _minCardWidth,
					_maxCardWidth, _spacing, _horizontalSpacing, _cardRatio, _horizontalAlignment);
			}
		}

		public float ScrollOffset
		{
			get
			{
				Initialize();
				return _content.anchoredPosition.y;
			}
		}

		private void Awake() => Initialize();

		[Inject]
		private void Construct(IScreenService screenService, IGoodIconConfig iconConfig)
		{
			UnsubscribeScreen();
			_screenService = screenService;
			_iconConfig = iconConfig ?? throw new ArgumentNullException(nameof(iconConfig));
			if (isActiveAndEnabled)
				SubscribeScreen();
		}

		private void SubscribeScreen()
		{
			if (_screenService == null || _screenSubscribed)
				return;

			_screenService.ScreenChanged += OnScreenChanged;
			_screenSubscribed = true;
			_screenDirty = true;
		}

		private void UnsubscribeScreen()
		{
			if (_screenSubscribed)
				_screenService.ScreenChanged -= OnScreenChanged;

			_screenSubscribed = false;
		}

		private void OnScreenChanged(Vector2Int size) => _screenDirty = true;

		private void OnValidate() => _geometryDirty = true;

		private void OnEnable()
		{
			Initialize();
			_scrollRect.onValueChanged.AddListener(OnScrollChanged);
			SubscribeScreen();
			_geometryDirty = true;
		}

		private void OnDisable()
		{
			UnsubscribeScreen();
			if (_scrollRect != null)
				_scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
		}

		private void OnDestroy()
		{
			UnsubscribeScreen();
			if (_pool != null)
				_pool.BuyClicked -= OnBuyClicked;
			_pool?.Dispose();
		}

		private void LateUpdate() => PublishGeometry();

		private void Initialize()
		{
			if (_pool != null)
				return;
			Utils.AssertIsNotNull(_goodPrefab);
			Utils.AssertIsNotNull(_scrollRect);
			_content = _scrollRect.content;
			Utils.AssertIsNotNull(_content);
			_viewport = _scrollRect.viewport != null ? _scrollRect.viewport : (RectTransform)_scrollRect.transform;
			if (!(_goodPrefab.transform is RectTransform prefabRect))
				throw new InvalidOperationException(MissingRectTransformText);
			Vector2 size = prefabRect.rect.size;
			if (!GoodsLayoutMath.IsPositive(size.x) || !GoodsLayoutMath.IsPositive(size.y))
				throw new InvalidOperationException(InvalidPrefabDimensionsText);
			_cardRatio = size.y / size.x;
			ValidateContent();
			_pool = new GoodCardPool(_goodPrefab, _content);
			_pool.BuyClicked += OnBuyClicked;
			_pool.SetPurchaseInteractable(_purchaseInteractable);
		}

		private void OnBuyClicked(int goodId) => BuyClicked?.Invoke(goodId);

		public void SetPurchaseInteractable(bool interactable)
		{
			_purchaseInteractable = interactable;
			_pool?.SetPurchaseInteractable(interactable);
		}

		private void ValidateContent()
		{
			LayoutGroup layout = _content.GetComponent<LayoutGroup>();
			ContentSizeFitter fitter = _content.GetComponent<ContentSizeFitter>();
			if ((layout != null && layout.enabled) || (fitter != null && fitter.enabled))
				throw new InvalidOperationException(
					ContentLayoutConflictText);
		}

		private void PublishGeometry()
		{
			if (_measuring)
				return;
			_measuring = true;
			try
			{
				bool screenChanged = _screenDirty;
				if (_screenDirty)
				{
					_screenDirty = false;
					Canvas.ForceUpdateCanvases();
				}

				GoodsLayoutInput input = Geometry;
				if (!screenChanged && !_geometryDirty && input.Equals(_lastGeometry))
					return;
				ValidateContent();
				_lastGeometry = input;
				_geometryDirty = false;
				GeometryChanged?.Invoke();
			}
			finally
			{
				_measuring = false;
			}
		}

		private void OnScrollChanged(Vector2 position)
		{
			if (_measuring || _screenDirty)
				return;
			PublishGeometry();
			ScrollChanged?.Invoke(ScrollOffset);
		}

		public void SetContentHeight(float height)
		{
			Initialize();
			_content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		}

		public void SetScrollOffset(float offset)
		{
			Initialize();
			_content.anchoredPosition = new Vector2(0f, offset);
		}

		public void StopMovement()
		{
			Initialize();
			_scrollRect.StopMovement();
		}

		public void SetSlotCount(int count)
		{
			Initialize();
			_pool.Resize(count);
		}

		public void SetSlotGeometry(int slot, GoodsSize size, GoodsPosition position)
		{
			GoodCardSlot card = _pool[slot];
			card.Rect.sizeDelta = new Vector2(size.Width, size.Height);
			card.Rect.anchoredPosition = new Vector2(position.X, position.Y);
		}

		public void BindSlot(int slot, GoodModel good)
		{
			if (_iconConfig == null)
				throw new InvalidOperationException(MissingIconConfigText);
			GoodView card = _pool[slot].View;
			card.SetData(good.Id, good.Header, good.Description, good.Price.ToString(), _iconConfig.GetIcon(good.Type));
			card.SetPurchaseInteractable(_purchaseInteractable);
			card.gameObject.SetActive(true);
		}
	}
}
