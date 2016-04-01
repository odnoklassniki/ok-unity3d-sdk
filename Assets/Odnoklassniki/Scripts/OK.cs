using Odnoklassniki;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public static DateTime AccessTokenExpiresAt
	{
		get
		{
			return (odnoklassniki != null) ? odnoklassniki.AccessTokenExpiresAt : DateTime.MinValue;
		}
	}

	public static bool IsRefreshTokenValid
	{
		get
		{
			return (odnoklassniki != null) ? odnoklassniki.IsRefreshTokenValid : false;
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
#elif UNITY_WEBGL
		odnoklassniki = new GameObject("Odnoklassniki").AddComponent<WebGLOdnoklassniki>();
		odnoklassniki.Init(callback);
#else
		Debug.LogError("Odnoklassniki Unity SDK Unavailable");
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

	public static void RefreshAccessToken(OKRefreshTokenCallback callback = null)
	{
		if (!IsRefreshTokenValid)
		{
			Debug.Log("Cannot refresh access token: refresh token not valid/present");
			return;
		}

		OdnoklassnikiImpl.RefreshAccessToken(callback);
	}

	public static void API(string query, HTTP.Method method, Dictionary<string, string> args, OKRequestCallback callback)
	{
		OdnoklassnikiImpl.Api(query, method, args, callback);
	}

	public static void API(string query, HTTP.Method method, OKRequestCallback callback)
	{
		API(query, method, new Dictionary<string, string>(), callback);
	}

	public static void API(string query, Dictionary<string, string> args, OKRequestCallback callback)
	{
		API(query, HTTP.Method.GET, args, callback);
	}

	public static void API(string query, OKRequestCallback callback)
	{
		API(query, HTTP.Method.GET, new Dictionary<string, string>(), callback);
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

	public static void GetInfo(OKGetInfoCallback callback, string[] uids, string[] fields, bool emptyPictures = false)
	{
		OdnoklassnikiImpl.GetInfo(uids, fields, emptyPictures, callback);
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
		return OdnoklassnikiImpl.OpenPublishDialog(callback, media);
	}

	public static bool OpenPublishDialog(OKRequestCallback callback, Action onClosed, OKMedia media)
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
