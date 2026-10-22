using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Store.Models;

namespace Store.Services
{
	public interface IGoodsApi
	{
		UniTask<List<GoodModel>> GetGoodsAsync(CancellationToken cancellationToken = default);
	}
}
