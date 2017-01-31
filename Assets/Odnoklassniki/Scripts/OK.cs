using System;
using System.Collections.Generic;
using Odnoklassniki.HTTP;
using UnityEngine;

namespace Odnoklassniki
{
	public sealed class OK : ScriptableObject
	{
		private static IOdnoklassniki odnoklassniki;

		static IOdnoklassniki OdnoklassnikiImpl
		{
			get
			{
				if (odnoklassniki == null)
				{
					Debug.Log("OK API not initialized yet");
					throw new NullReferenceException("Odnoklassniki API is not initialized yet.  Did you call OK.Init()?");
				}
				return odnoklassniki;
			}
		}

		public static string AppId
		{
			get
			{
				return (odnoklassniki != null) ? odnoklassniki.AppId : "";
			}
		}

		public static string AccessToken
		{
			get
			{
				return (odnoklassniki != null) ? odnoklassniki.AccessToken : "";
			}
		}

		public static bool IsLoggedIn
		{
			get
			{
				return (odnoklassniki != null) && odnoklassniki.IsAuthorized;
			}
		}

		public static bool IsInitialized
		{
			get
			{
				return (odnoklassniki != null) && odnoklassniki.IsInitialized;
			}
		}

		public static string AdvertisingId
		{
			get
			{
				if (odnoklassniki == null)
				{
					return "";
				}

				return odnoklassniki.GetAdvertisingId();
			}
		}

		#region Init

		public static void Init(OKInitCallback callback = null)
		{
			if (IsInitialized)
			{
				Debug.Log("Odnoklassniki API already initialized");
				callback(true);
				return;
			}
#if UNITY_ANDROID
			odnoklassniki = new GameObject("Odnoklassniki").AddComponent<AndroidOdnoklassniki>();
			odnoklassniki.Init(callback);
#elif UNITY_IOS
			odnoklassniki = new GameObject("Odnoklassniki").AddComponent<IOSOdnoklassniki>();
			odnoklassniki.Init(callback);
#else
			Debug.LogError("Odnoklassniki Unity SDK Unavailable: Set platform to either Android or iOS");
			odnoklassniki = null;
			callback(false);
#endif
		}

		#endregion

		public static void Auth(OKAuthCallback callback = null)
		{
			OdnoklassnikiImpl.Auth(callback);
		}

		public static void Logout()
		{
			OdnoklassnikiImpl.ClearTokens();
		}

		public static void API(string query, Method method, Dictionary<string, string> args, OKRequestCallback callback, bool useSession = true)
		{
			OdnoklassnikiImpl.Api(query, method, args, callback, useSession);
		}

		public static void API(string query, Method method, OKRequestCallback callback, bool useSession = true)
		{
			API(query, method, new Dictionary<string, string>(), callback, useSession);
		}

		public static void API(string query, Dictionary<string, string> args, OKRequestCallback callback, bool useSession = true)
		{
			API(query, Method.GET, args, callback, useSession);
		}

		public static void API(string query, OKRequestCallback callback)
		{
			API(query, Method.GET, new Dictionary<string, string>(), callback);
		}

		public static bool NativeAppInstalled()
		{
			return OdnoklassnikiImpl.IsOdnoklassnikiNativeAppInstalled();
		}

		#region Android-Only Implemented Methods
		#endregion

		#region Helper methods
		public static void GetCallsLeft(OKGetCallsLeftCallback callback, params string[] methods)
		{
			OdnoklassnikiImpl.GetCallsLeft(methods, callback);
		}

		public static void GetCurrentUser(OKGetCurrentUserCallback callback, params string[] fields)
		{
			OdnoklassnikiImpl.GetCurrentUser(fields, callback);
		}

		/**
		 * This is an API request wrapper that automatically splits requests into 100 uid chunks, based on API limitations at the time of implementation.
		 * This API only works when there is a session, can return an empty OKUserInfo[] list in case of an error.
		 */
		public static void GetInfo(OKGetInfoCallback callback, string[] uids, string[] fields, bool emptyPictures = false)
		{
			OdnoklassnikiImpl.GetInfo(uids, fields, emptyPictures, callback);
		}

		/**
		 * Returns sdk.getInstallSource response, or null, if it failed to get AdvertisingId.
		 */
		public static void GetInstallSource(OKGetInstallSource callback)
		{
			OdnoklassnikiImpl.GetInstallSource(callback);
		}

		public static void GetAppUsers(OKGetAppUsersCallback callback)
		{
			OdnoklassnikiImpl.GetAppUsers(callback);
		}

		public static bool OpenInviteDialog(OKRequestCallback callback, string defaultMessage, params string[] selected)
		{
			return OdnoklassnikiImpl.OpenInviteDialog(callback, defaultMessage, selected);
		}

		public static bool OpenInviteDialog(OKRequestCallback callback, Action onClosed, string defaultMessage, params string[] selected)
		{
			return OdnoklassnikiImpl.OpenInviteDialog(callback, onClosed, defaultMessage, selected);
		}

		public static bool OpenSuggestDialog(OKRequestCallback callback, string defaultMessage, params string[] selected)
		{
			return OdnoklassnikiImpl.OpenSuggestDialog(callback, defaultMessage, selected);
		}

		public static bool OpenSuggestDialog(OKRequestCallback callback, Action onClosed, string defaultMessage, params string[] selected)
		{
			return OdnoklassnikiImpl.OpenSuggestDialog(callback, onClosed, defaultMessage, selected);
		}

		public static bool OpenPublishDialog(OKRequestCallback callback, OKMedia media)
		{
			return OpenPublishDialog(callback, null, new List<OKMedia>() { media });
		}

		public static bool OpenPublishDialog(OKRequestCallback callback, List<OKMedia> media)
		{
			return OpenPublishDialog(callback, null, media);
		}

		public static bool OpenPublishDialog(OKRequestCallback callback, Action onClosed, OKMedia media)
		{
			return OpenPublishDialog(callback, onClosed, new List<OKMedia>() { media });
		}

		public static bool OpenPublishDialog(OKRequestCallback callback, Action onClosed, List<OKMedia> media)
		{
			return OdnoklassnikiImpl.OpenPublishDialog(callback, onClosed, media);
		}

		public static bool OpenPhotoDialog(OKRequestCallback callback, Texture2D image, string defaultComment)
		{
			return OdnoklassnikiImpl.OpenPhotoDialog(callback, image, defaultComment);
		}

		public static bool OpenPhotoDialog(OKRequestCallback callback, Action onClosed, Texture2D image, string defaultComment)
		{
			return OdnoklassnikiImpl.OpenPhotoDialog(callback, onClosed, image, defaultComment);
		}

		public static void GetFriendsByDevices(OKRequestCallback callback, string[] devices)
		{
			OdnoklassnikiImpl.GetFriendsByDevices(callback, devices);
		}

		/**
		 * Wrapper that retries payment reports later in case of connectivity loss.
		 */
		public static void ReportPayment(string trxId, string amount, string currency)
		{
			OdnoklassnikiImpl.ReportPayment(trxId, amount, currency);
		}

		#endregion

		#region Debug tools

		public static void SetBakedToken()
		{
			OdnoklassnikiImpl.AuthWithDebugToken();
		}

		#endregion

		public static void RefreshOAuth(OKAuthCallback action)
		{
			OdnoklassnikiImpl.RefreshOAuth(action);
		}
	}
}