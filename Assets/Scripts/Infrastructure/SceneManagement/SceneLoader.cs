using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infrastructure.SceneManagement
{
	public class SceneLoader : ISceneLoader
	{
		private const string MissingSceneFormat = "Scene '{0}' is missing from the build scene list.";
		private const string SceneLoadFailedFormat = "Failed to load scene '{0}'.";

		private const float SceneActivationProgress = 0.9f;

		public void ValidateScene(string sceneName)
		{
			if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
				throw new InvalidOperationException(string.Format(MissingSceneFormat, sceneName));
		}

		public async UniTask LoadAsync(string sceneName, Func<float, UniTask> onProgress,
			CancellationToken cancellationToken)
		{
			ValidateScene(sceneName);
			cancellationToken.ThrowIfCancellationRequested();

			AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
			if (operation == null)
				throw new InvalidOperationException(string.Format(SceneLoadFailedFormat, sceneName));

			operation.allowSceneActivation = false;
			try
			{
				while (operation.progress < SceneActivationProgress)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (onProgress != null)
						await onProgress(Mathf.Clamp01(operation.progress / SceneActivationProgress));

					await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
				}

				cancellationToken.ThrowIfCancellationRequested();
				if (onProgress != null)
					await onProgress(1f);
				cancellationToken.ThrowIfCancellationRequested();
			}
			finally
			{
				operation.allowSceneActivation = true;
			}

			while (!operation.isDone)
				await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

			cancellationToken.ThrowIfCancellationRequested();
		}
	}
}
