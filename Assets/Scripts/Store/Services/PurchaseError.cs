namespace Store.Services
{
	public enum PurchaseError
	{
		None,
		InsufficientFunds,
		Rejected,
		ResultUnknown,
		RefreshFailed,
		RecoveryRequired,
		PendingReadFailed,
		PendingWriteFailed,
		PendingClearFailed
	}
}
