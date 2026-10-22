using UnityEngine;

namespace Session.Configs
{
	[CreateAssetMenu(fileName = "UserConfig", menuName = "Session/User Config")]
	public sealed class UserConfig : ScriptableObject
	{
		[SerializeField] private string _login;
		[SerializeField] private string _password;

		public string Login => _login;

		public string Password => _password;
	}
}
