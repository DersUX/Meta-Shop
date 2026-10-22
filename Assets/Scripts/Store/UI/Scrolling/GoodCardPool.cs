using System;
using System.Collections.Generic;
using Store.UI.Views;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Store.UI.Scrolling
{
	internal sealed class GoodCardPool : IDisposable
	{
		private readonly GoodView _prefab;
		private readonly RectTransform _parent;
		private readonly List<GoodCardSlot> _slots = new List<GoodCardSlot>();

		private bool _purchaseInteractable;

		public GoodCardSlot this[int index] => _slots[index];

		public event Action<int> BuyClicked;

		public GoodCardPool(GoodView prefab, RectTransform parent)
		{
			_prefab = prefab;
			_parent = parent;
		}

		public void Resize(int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			while (_slots.Count > count)
			{
				DestroySlot(_slots[_slots.Count - 1]);
				_slots.RemoveAt(_slots.Count - 1);
			}

			while (_slots.Count < count)
			{
				GoodView view = Object.Instantiate(_prefab, _parent, false);
				view.gameObject.SetActive(false);

				view.BuyClicked += OnBuyClicked;
				view.SetPurchaseInteractable(_purchaseInteractable);

				RectTransform rect = (RectTransform)view.transform;

				rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
				rect.localScale = Vector3.one;
				rect.localRotation = Quaternion.identity;

				_slots.Add(new GoodCardSlot(view, rect));
			}
		}

		public void Dispose()
		{
			foreach (GoodCardSlot slot in _slots)
				DestroySlot(slot);

			_slots.Clear();
		}

		public void SetPurchaseInteractable(bool interactable)
		{
			_purchaseInteractable = interactable;

			foreach (GoodCardSlot slot in _slots)
				slot.View.SetPurchaseInteractable(interactable);
		}

		private void OnBuyClicked(int goodId) => BuyClicked?.Invoke(goodId);

		private void DestroySlot(GoodCardSlot slot)
		{
			if (slot.View == null)
				return;

			slot.View.BuyClicked -= OnBuyClicked;

			slot.View.gameObject.SetActive(false);
			Object.Destroy(slot.View.gameObject);
		}
	}
}
