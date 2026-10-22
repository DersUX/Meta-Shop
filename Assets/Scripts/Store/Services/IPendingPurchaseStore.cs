namespace Store.Services
{
	public interface IPendingPurchaseStore
	{
		PendingPurchase Load(int playerId, string login);

		void Save(int playerId, string login, PendingPurchase purchase);

		void Clear(int playerId, string login);
	}
}
