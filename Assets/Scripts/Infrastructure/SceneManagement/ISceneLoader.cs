using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Infrastructure.SceneManagement
{
	public interface ISceneLoader
	{
		void ValidateScene(string sceneName);

		UniTask LoadAsync(string sceneName, Func<float, UniTask> onProgress, CancellationToken cancellationToken);
	}
}
