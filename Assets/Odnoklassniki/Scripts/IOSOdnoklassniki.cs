using System;
using UnityEngine;
using System.Collections;
using Odnoklassniki.HTTP;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
namespace Odnoklassniki
{
	public class IOSOdnoklassniki : AbstractOdnoklassniki
	{
		private const string IOSAppUrl = "ok{0}://authorize";

		protected override bool SsoAuth()
		{
			if (!base.SsoAuth()) return false;

			authRequested = OKAuthType.SSO;
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

		protected override string GetPlatform()
		{
			return OKPlatform.iOS;
		}

		public override string GetAdvertisingId()
		{
#if UNITY_IOS
			return Device.advertisingIdentifier;
#else
			throw new NotImplementedException("iOSOdnoklasniki.GetAdvertisingId() only works on iOS platform");
#endif
		}

		public void AuthSuccessIOS(string data)
		{
			// Since we can no longer tell which auth type was actually used based on response from iOS, look at authRequested type to determine that.
			if (authRequested == OKAuthType.SSO)
			{
				SSOAuthSuccessIOS(data);
			}
			else
			{
				OAuthSuccess(data);
			}
		}

		public void SSOAuthSuccessIOS(string data)
		{
			Debug.Log("Received SSOAuth callback: " + data);
			string[] args = data.Split(';');
			if (args.Length != 3)
			{
				Debug.LogError("Auth failed. Bad argument count - " + args.Length);
				Debug.LogError("Should be 3: access_token, session_secret_key, expires_in");
				if (authCallback != null)
				{
					authCallback(false);
					authCallback = null;
				}
				return;
			}
			AccessToken = args[0];
			RefreshToken = args[1];
			authRequested = OKAuthType.None;
			AuthType = OKAuthType.SSO;

			// Send any unsent payment reports.
			ReportPaymentSendInternal();
			if (authCallback != null)
			{
				authCallback(true);
				authCallback = null;
			}
		}
	}
}
