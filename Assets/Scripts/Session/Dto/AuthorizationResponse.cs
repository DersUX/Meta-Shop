using System;

namespace Session.Dto
{
	[Serializable]
	public class AuthorizationResponse
	{
		public string sessionToken;
		public string serverTime;
		public string expiresAt;
	}
}
