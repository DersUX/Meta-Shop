using Common;
using UnityEngine;

namespace Session.UI.Views
{
	public class SessionLoadingView : MonoBehaviour, ISessionLoadingView
	{
		[SerializeField] private GameObject _loadingContainer;

		private void Awake()
		{
			Utils.AssertIsNotNull(_loadingContainer);

			Hide();
		}

		public void Show() => _loadingContainer.SetActive(true);

		public void Hide()
		{
			if (_loadingContainer != null)
				_loadingContainer.SetActive(false);
		}
	}
}
