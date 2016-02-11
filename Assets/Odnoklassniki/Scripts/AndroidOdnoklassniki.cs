using UnityEngine;

namespace Odnoklassniki
{
	public class AndroidOdnoklassniki : AbstractOdnoklassniki
	{
		private const string AndroidAppUrl = "okauth://ok{0}";

		protected override bool SsoAuth()
		{
			if (!base.SsoAuth()) return false;
		 
#if UNITY_ANDROID
			AndroidJavaClass unityOKAndroidPlugin = new AndroidJavaClass("ru.odnoklassniki.unity.OKAndroidPlugin");
			unityOKAndroidPlugin.CallStatic("SSOAuth", AppId, appSecretKey, scope);
#endif
			return true;
		}

		protected override string GetAppUrl()
		{
			return string.Format(AndroidAppUrl, AppId);
		}

		public void SSOAuthSuccessAndroid(string data)
		{
			Debug.Log("Received SSOAuth callback: " + data);
			string[] args = data.Split(';');
			if (args.Length != 2)
			{
				Debug.LogError("Auth failed. Bad argument count - " + args.Length);
				Debug.LogError("Should be 2: access_token, refresh_token");
				return;
			}
			AccessToken = args[0];
			RefreshToken = args[1];
			AccessTokenExpiresAt = DefaultAccessTokenExpires();
			RefreshTokenExpiresAt = DefaultRefreshTokenExpires();
			AuthType = OKAuthType.SSO;
			Debug.Log("Authorized via SSO");
			if (authCallback != null)
			{
				authCallback(true);
				authCallback = null;
			}
		}
	}
}
