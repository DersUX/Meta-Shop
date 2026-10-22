using System;
using Common;
using Store.UI.Scrolling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Store.UI.Views
{
	public class SectionStoreView : MonoBehaviour, ISectionStoreView
	{
		private const string EmptyBalanceText = "—";

		[SerializeField] private GoodsScrollView _goodsScrollView;
		[SerializeField] private TMP_Text _balanceText;
		[SerializeField] private TMP_Text _purchaseStatusText;
		[SerializeField] private GameObject _purchaseStatusContainer;
		[SerializeField] private Button _closePurchaseStatusButton;

		private float _hideStatusAt = float.PositiveInfinity;

		public GoodsScrollView GoodsScroll => _goodsScrollView;

		public event Action<int> BuyClicked;

		private void Awake()
		{
			Utils.AssertIsNotNull(_goodsScrollView);
			Utils.AssertIsNotNull(_balanceText);
			Utils.AssertIsNotNull(_purchaseStatusText);
			Utils.AssertIsNotNull(_purchaseStatusContainer);
			Utils.AssertIsNotNull(_closePurchaseStatusButton);

			_goodsScrollView.BuyClicked += OnBuyClicked;
			_closePurchaseStatusButton.onClick.AddListener(HidePurchaseStatus);

			ClearBalance();
			SetPurchaseInteractable(false);
			ShowPurchaseStatus(null);
		}

		private void Update()
		{
			if (Time.unscaledTime >= _hideStatusAt)
				HidePurchaseStatus();
		}

		private void OnDestroy()
		{
			if (_closePurchaseStatusButton != null)
				_closePurchaseStatusButton.onClick.RemoveListener(HidePurchaseStatus);

			if (_goodsScrollView != null)
				_goodsScrollView.BuyClicked -= OnBuyClicked;
		}

		public void SetPurchaseInteractable(bool interactable) =>
			_goodsScrollView.SetPurchaseInteractable(interactable);

		public void ShowPurchaseStatus(string message, PurchaseStatusKind kind = PurchaseStatusKind.Progress)
		{
			_purchaseStatusText.text = message ?? string.Empty;
			_purchaseStatusContainer.SetActive(!string.IsNullOrEmpty(message));
			_closePurchaseStatusButton.gameObject.SetActive(kind == PurchaseStatusKind.Error);

			_purchaseStatusText.rectTransform.offsetMax =
				new Vector2(kind == PurchaseStatusKind.Error ? -60f : -16f, -12f);

			_hideStatusAt = kind == PurchaseStatusKind.Success ? Time.unscaledTime + 3f : float.PositiveInfinity;
		}

		public void SetBalance(int balance) => _balanceText.text = balance.ToString();

		public void ClearBalance() => _balanceText.text = EmptyBalanceText;

		private void OnBuyClicked(int goodId) => BuyClicked?.Invoke(goodId);

		private void HidePurchaseStatus()
		{
			_purchaseStatusContainer.SetActive(false);
			_hideStatusAt = float.PositiveInfinity;
		}
	}
}
