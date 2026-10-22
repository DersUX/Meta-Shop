using System;
using UnityEngine;

namespace Logon.Configs
{
	[CreateAssetMenu(fileName = "LogonConfig", menuName = "Logon/Config")]
	public class LogonConfig : ScriptableObject
	{
		private const string MinimumLoadingTimeTooltip = "Минимальное время заполнения прогресса в секундах. 0 — без задержки.";
		private const string StageWeightsHeader = "Веса этапов";
		private const string InvalidMinimumLoadingTimeText = "Logon minimum loading time must be finite and non-negative.";
		private const string InvalidStageWeightsText = "Logon stage weights must be finite and non-negative, with a positive total.";

		[SerializeField] private string _mainSceneName = "MainScene";

		[Tooltip(MinimumLoadingTimeTooltip)]
		[SerializeField, Min(0f)] private float _minimumLoadingSeconds = 3f;

		[Header(StageWeightsHeader)]
		[SerializeField, Min(0f)] private float _preparationWeight = 1f;

		[SerializeField, Min(0f)] private float _connectionWeight = 2f;
		[SerializeField, Min(0f)] private float _sceneLoadingWeight = 6f;
		[SerializeField, Min(0f)] private float _finishingWeight = 1f;

		public string MainSceneName => _mainSceneName;

		public float MinimumLoadingSeconds => _minimumLoadingSeconds;

		public float PreparationProgress => _preparationWeight / TotalWeight;

		public float ConnectionProgress => (_preparationWeight + _connectionWeight) / TotalWeight;

		public float SceneLoadingProgress =>
			(_preparationWeight + _connectionWeight + _sceneLoadingWeight) / TotalWeight;

		private float TotalWeight => _preparationWeight + _connectionWeight + _sceneLoadingWeight + _finishingWeight;

		public void Validate()
		{
			if (_minimumLoadingSeconds < 0f
				|| float.IsNaN(_minimumLoadingSeconds)
				|| float.IsInfinity(_minimumLoadingSeconds))
				throw new InvalidOperationException(InvalidMinimumLoadingTimeText);

			if (_preparationWeight < 0f
				|| _connectionWeight < 0f
				|| _sceneLoadingWeight < 0f
				|| _finishingWeight < 0f
				|| TotalWeight <= 0f
				|| float.IsNaN(TotalWeight)
				|| float.IsInfinity(TotalWeight))
				throw new InvalidOperationException(
					InvalidStageWeightsText);
		}
	}
}
