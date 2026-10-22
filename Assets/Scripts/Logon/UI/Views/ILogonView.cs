using System;

namespace Logon.UI.Views
{
	public interface ILogonView
	{
		event Action RetryRequested;

		void ShowPreparing();

		void ShowConnecting();

		void ShowSceneLoading();

		void ShowFinishing();

		void SetProgress(float progress);

		void ShowError(string message);
	}
}
