namespace Infrastructure.Network
{
	public interface IBackendConfig
	{
		string LoginUrl { get; }

		string PlayerUrl { get; }

		string GoodsUrl { get; }

		string PurchasesUrl { get; }
	}
}
