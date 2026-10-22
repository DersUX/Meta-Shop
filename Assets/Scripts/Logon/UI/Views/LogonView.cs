using System;
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Logon.UI.Views
{
	public class LogonView : MonoBehaviour, ILogonView
	{
		private const string PreparingText = "Подготовка…";
		private const string ConnectingText = "Подключение…";
		private const string SceneLoadingText = "Загрузка магазина…";
		private const string FinishingText = "Завершение…";
		private const string RetryText = "Попробуйте ещё раз";

		[SerializeField] private GameObject _statusContainer;
		[SerializeField] private TMP_Text _statusText;
		[SerializeField] private TMP_Text _statusInfoText;

		[Space]
		[SerializeField] private GameObject _loadingCircle;
		[SerializeField] private GameObject _warning;
		[SerializeField] private GameObject _progressContainer;

		[Space]
		[SerializeField] private Button _retryButton;
		[SerializeField] private LoadingBarView _loadingBarView;

		public event Action RetryRequested;

		private void Awake()
		{
			Utils.AssertIsNotNull(_statusContainer);
			Utils.AssertIsNotNull(_statusText);
			Utils.AssertIsNotNull(_statusInfoText);
			Utils.AssertIsNotNull(_loadingCircle);
			Utils.AssertIsNotNull(_warning);
			Utils.AssertIsNotNull(_progressContainer);
			Utils.AssertIsNotNull(_retryButton);
			Utils.AssertIsNotNull(_loadingBarView);

			_retryButton.onClick.AddListener(RequestRetry);
		}

		private void OnDestroy()
		{
			if (_retryButton != null)
				_retryButton.onClick.RemoveListener(RequestRetry);
		}

		public void ShowPreparing() => ShowState(PreparingText, string.Empty, false, false, true);

		public void ShowConnecting() => ShowState(ConnectingText, string.Empty, true, false, true);

		public void ShowSceneLoading() => ShowState(SceneLoadingText, string.Empty, false, false, true);

		public void SetProgress(float progress) => _loadingBarView.SetProgress(progress);

		public void ShowFinishing() => ShowState(FinishingText, string.Empty, false, false, true);

		public void ShowError(string message)
		{
			ShowState(message, RetryText, false, true, false);
			_retryButton.Select();
		}

		private void ShowState(string status, string info, bool connecting, bool error, bool progress)
		{
			if (!error && EventSystem.current != null
				&& EventSystem.current.currentSelectedGameObject == _retryButton.gameObject)
				EventSystem.current.SetSelectedGameObject(null);

			_statusContainer.SetActive(true);
			_statusText.text = status;
			_statusInfoText.text = info;
			_statusInfoText.gameObject.SetActive(!string.IsNullOrEmpty(info));
			_loadingCircle.SetActive(connecting);
			_warning.SetActive(error);
			_progressContainer.SetActive(progress);
			_retryButton.gameObject.SetActive(error);
		}

		private void RequestRetry() => RetryRequested?.Invoke();
	}
}
