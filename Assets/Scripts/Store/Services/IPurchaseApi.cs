using System.Threading;
using Cysharp.Threading.Tasks;
using Store.Models;

namespace Store.Services
{
	public interface IPurchaseApi
	{
		UniTask<PurchaseResult> PurchaseAsync(int goodId, string requestId, CancellationToken cancellationToken = default);
	}
}
