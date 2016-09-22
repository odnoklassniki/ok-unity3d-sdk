using System;
using System.Collections.Generic;
using UnityEngine;
using Odnoklassniki.HTTP;

namespace Odnoklassniki
{
	public interface IOdnoklassniki
	{
		string AccessToken { get; }
		string AppId { get; }
		DateTime AccessTokenExpiresAt { get; }
		bool IsRefreshTokenValid { get; }
		bool IsInitialized { get; }
		bool IsAuthorized { get; }

		void Init(OKInitCallback callback);

		void Auth(OKAuthCallback callback);

		void RefreshAccessToken(OKRefreshTokenCallback callback);

		void Api(string query, Method method, Dictionary<string, string> args, OKRequestCallback callback, bool useSession = true);

		void ClearTokens(bool clearCookies = true);

		bool IsOdnoklassnikiNativeAppInstalled();

		#region Helper methods

		bool OpenInviteDialog(OKRequestCallback callback, string defaultMessage, string[] selected);

		bool OpenInviteDialog(OKRequestCallback callback, Action onClosed, string defaultMessage, string[] selected);

		bool OpenSuggestDialog(OKRequestCallback callback, string defaultMessage, string[] selected);

		bool OpenSuggestDialog(OKRequestCallback callback, Action onClosed, string defaultMessage, string[] selected);

		bool OpenPublishDialog(OKRequestCallback callback, Action onClosed, List<OKMedia> media);

		bool OpenPhotoDialog(OKRequestCallback callback, Texture2D image, string defaultComment);

		bool OpenPhotoDialog(OKRequestCallback callback, Action onClosed, Texture2D image, string defaultComment);

		void GetFriendsByDevices(OKRequestCallback callback, string[] devices);

		void GetCallsLeft(string[] methods, OKGetCallsLeftCallback callback);

		void GetCurrentUser(string[] fields, OKGetCurrentUserCallback callback);

		void GetInfo(string[] uids, string[] fields, bool emptyPictures, OKGetInfoCallback callback);

		void GetAppUsers(OKGetAppUsersCallback callback);

		void ReportPayment(string trxId, string amount, string currency);

		#endregion

		#region Debug tools

		void AuthWithDebugToken();

		#endregion

		void RefreshOAuth(OKAuthCallback action);

		string GetAdvertisingId();
	}
}
