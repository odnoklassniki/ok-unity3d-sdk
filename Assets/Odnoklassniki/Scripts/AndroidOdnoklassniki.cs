using System;
using UnityEngine;

namespace Odnoklassniki
{
	public class AndroidOdnoklassniki : AbstractOdnoklassniki
	{
		private const string AndroidAppUrl = "okauth://ok{0}";

#if UNITY_ANDROID && !UNITY_EDITOR
		private AndroidJavaClass android;

		private AndroidJavaClass Android
		{
			get
			{
				if (android == null)
				{
					android = new AndroidJavaClass("ru.odnoklassniki.unity.OKAndroidPlugin");
				}
				return android;
			}
		}
#endif

		protected override bool SsoAuth()
		{
			if (!base.SsoAuth()) return false;
#if UNITY_ANDROID && !UNITY_EDITOR
			authRequested = OKAuthType.SSO;
			Android.CallStatic("SSOAuth", AppId, scope);
			return true;
#else
			return false;
#endif
		}

		protected override string GetPlatform()
		{
			return OKPlatform.Android;
		}

		protected override string GetAppUrl()
		{
			return string.Format(AndroidAppUrl, AppId);
		}

		public override bool IsOdnoklassnikiNativeAppInstalled()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			return Android.CallStatic<bool>("IsNativeAppInstalled");
#else
			return false;
#endif
		}

		public override string GetAdvertisingId()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			return Android.CallStatic<string>("getAdvertisingId");
#else
			throw new NotImplementedException("AdvertisingId only available for Android platform, not in Editor.");
#endif
		}

		public void SSOAuthSuccessAndroid(string data)
		{
			Debug.Log("Received SSOAuth callback: " + data);
			string[] args = data.Split(';');
			if (args.Length != 2)
			{
				Debug.LogError("Auth failed. Bad argument count - " + args.Length);
				Debug.LogError("Should be 2: access_token, refresh_token(secret_session_key)");
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
			Debug.Log("Authorized via SSO");
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
