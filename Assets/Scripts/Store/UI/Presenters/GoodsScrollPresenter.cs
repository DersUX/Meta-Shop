using System;
using Store.Models;
using Store.UI.Scrolling;

namespace Store.UI.Presenters
{
	public sealed class GoodsScrollPresenter : IDisposable
	{
		private readonly GoodsListModel _model;
		private readonly IGoodsScrollView _view;
		private readonly GoodsLayoutCalculator _calculator;

		private GoodsLayoutInput _input;
		private GoodsGridLayout _anchorLayout;
		private float _anchorOffset;
		private int[] _indices = Array.Empty<int>();
		private int _first = -1;
		private bool _updating;
		private bool _disposed;

		public GoodsGridLayout Layout { get; private set; }

		public GoodsScrollPresenter(GoodsListModel model, IGoodsScrollView view, GoodsLayoutCalculator calculator)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			_view = view ?? throw new ArgumentNullException(nameof(view));
			_calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));

			_model.GoodsChanged += OnGoodsChanged;
			_view.GeometryChanged += OnGeometryChanged;
			_view.ScrollChanged += OnScrollChanged;

			try
			{
				OnGoodsChanged();
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		private void OnGoodsChanged()
		{
			if (_disposed)
				return;

			_updating = true;

			try
			{
				Layout = null;
				_anchorLayout = null;
				_indices = Array.Empty<int>();

				_view.StopMovement();
				_view.SetScrollOffset(0f);

				Rebuild();
			}
			finally
			{
				_updating = false;
			}
		}

		private void OnGeometryChanged()
		{
			if (_disposed || _updating)
				return;

			GoodsLayoutInput input = _view.Geometry;

			if (Layout != null && input.Equals(_input))
				return;

			_updating = true;

			try
			{
				Rebuild();
			}
			finally
			{
				_updating = false;
			}
		}

		private void Rebuild()
		{
			_input = _view.Geometry;
			GoodsGridLayout next = _calculator.Calculate(_model.Goods.Count, _input);

			if (next == null && Layout != null)
			{
				_anchorLayout = Layout;
				_anchorOffset = _view.ScrollOffset;
			}

			float offset = next?.RestoreOffset(Layout ?? _anchorLayout,
				Layout != null ? _view.ScrollOffset : _anchorOffset) ?? 0f;

			if (next != null)
				_anchorLayout = null;

			Layout = next;
			_first = -1;
			int count = next?.PoolSize ?? 0;

			if (_indices.Length != count)
			{
				_indices = new int[count];

				for (int i = 0; i < count; i++)
					_indices[i] = -1;
			}

			_view.SetContentHeight(next?.ContentHeight ?? 0f);
			_view.StopMovement();
			_view.SetScrollOffset(offset);
			_view.SetSlotCount(count);

			Show(offset, true);
		}

		private void OnScrollChanged(float offset)
		{
			if (!_disposed && !_updating)
				Show(offset, false);
		}

		private void Show(float offset, bool geometryChanged)
		{
			if (Layout == null || _indices.Length == 0)
				return;

			int first = Layout.GetFirstPooledIndex(offset);
			if (!geometryChanged && first == _first)
				return;

			for (int index = first; index < first + _indices.Length; index++)
			{
				int slot = index % _indices.Length;
				bool itemChanged = _indices[slot] != index;

				if (itemChanged || geometryChanged)
					_view.SetSlotGeometry(slot, Layout.CardSize, Layout.GetPosition(index));

				if (itemChanged)
				{
					_view.BindSlot(slot, _model.Goods[index]);
					_indices[slot] = index;
				}
			}

			_first = first;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			_model.GoodsChanged -= OnGoodsChanged;
			_view.GeometryChanged -= OnGeometryChanged;
			_view.ScrollChanged -= OnScrollChanged;
		}
	}
}
