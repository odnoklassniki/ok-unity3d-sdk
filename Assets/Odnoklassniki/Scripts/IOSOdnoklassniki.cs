using UnityEngine;
using System.Collections;
using Odnoklassniki.HTTP;

namespace Odnoklassniki
{
	public class IOSOdnoklassniki : AbstractOdnoklassniki
	{
		private const string IOSAppUrl = "ok{0}://authorize";

		protected override bool SsoAuth()
		{
			if (!base.SsoAuth()) return false;

			OKAppAuthIOS.AuthorizeInApp(AppId, scope);
			return true;
		}

		public override bool IsOdnoklassnikiNativeAppInstalled()
		{
			return OKAppAuthIOS.IsNativeAppInstalled(AppId, scope);
		}

		protected override string GetAppUrl()
		{
			return string.Format(IOSAppUrl, AppId);
		}

		public void SSOAuthSuccessIOS(string data)
		{
			Debug.Log("Received IOS App Auth callback: " + data);
			Debug.Log("Requesting access token by code");
			new Request(TokenByCodeUrl(data), Method.POST).Send(request =>
			{
				string response = request.response.Text;
				Debug.Log("Got response from tokenByCode: " + response);
				Hashtable json = (Hashtable) JSON.Decode(response);
				if (!json.ContainsKey("access_token") || (!json.ContainsKey("refresh_token")))
				{
					Debug.LogError("Bad response: access_token or refresh_token is not present");
					return;
				}

				AccessToken = json["access_token"].ToString();
				AccessTokenExpiresAt = json.ContainsKey("expires_in") ? ParseExpiration(json["expires_in"].ToString()) : DefaultAccessTokenExpires();
				RefreshToken = json["refresh_token"].ToString();
				RefreshTokenExpiresAt = DefaultRefreshTokenExpires();
				AuthType = OKAuthType.SSO;
				Debug.Log("Authorized via SSO");
				if (authCallback != null)
				{
					authCallback(true);
					authCallback = null;
				}
			});
		}
	}
}
