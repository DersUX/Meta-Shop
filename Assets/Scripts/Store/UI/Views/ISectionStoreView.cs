using System;

namespace Store.UI.Views
{
	public enum PurchaseStatusKind
	{
		Progress,
		Success,
		Error
	}

	public interface ISectionStoreView
	{
		event Action<int> BuyClicked;

		void SetPurchaseInteractable(bool interactable);

		void ShowPurchaseStatus(string message, PurchaseStatusKind kind = PurchaseStatusKind.Progress);

		void SetBalance(int balance);

		void ClearBalance();
	}
}
