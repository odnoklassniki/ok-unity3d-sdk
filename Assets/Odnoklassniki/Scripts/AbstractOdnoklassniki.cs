using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Json;

namespace Odnoklassniki
{
	public abstract class AbstractOdnoklassniki : MonoBehaviour, IOdnoklassniki
	{
		protected string appKey;
		protected string appSecretKey;

		private bool forceOAuth;
		private bool fallbackToOAuth;
		protected HTTP.Format httpFormat;

		private bool clearTokenOnAuth = false; //Clear all data before attempting authorization

		protected OKAuthCallback authCallback;

		private string debugAccessToken;
		private string debugSessionKey;

		//Unity SDK Init Params
		private string apiServer;
		private string unitySessionKey;
		private string unitySecretSessionKey;
		private bool activatedProfile;

		private OKWebView webView;

		#region Constants

		private static string authURL = "https://www.odnoklassniki.ru/oauth/authorize?client_id={0}&scope={1}&response_type={2}&redirect_uri={3}&layout={4}";
		private static string refreshTokenURL = "{0}oauth/token.do?grant_type=refresh_token&refresh_token={1}&client_id={2}&client_secret={3}";
		private static string tokenByCodeURL = "{0}oauth/token.do?grant_type=authorization_code&code={1}&permissions={2}&redirect_uri={3}&client_id={4}&client_secret={5}";
		private static string apiURL = "{0}fb.do?access_token={1}&sig={2}&{3}";

		protected string scope;
		protected string responseType = "token";
		protected string layout = "m";

		private const int AccessTokenDuration = 1800; //access token expiration time in seconds
		private const int RefreshTokenDuration = 30; //access token expiration time in days

		#endregion

		protected OKAuthType AuthType { get; set; }

		protected string SessionSecretKey { get; set; }

		public string AppId { get; protected set; }

		public string AccessToken { get; protected set; }

		protected string RefreshToken { get; set; }

		public DateTime AccessTokenExpiresAt { get; protected set; }

		protected DateTime RefreshTokenExpiresAt { get; set; }

		public bool IsInitialized { get; set; }

		public bool IsAuthorized
		{
			get
			{
				return AccessTokenValid();
			}
		}

		public void Init(OKInitCallback callback)
		{
			DontDestroyOnLoad(gameObject);
			AppId = OKSettings.AppId;
			appKey = OKSettings.AppKey;
			appSecretKey = OKSettings.AppSecretKey;
			forceOAuth = OKSettings.ForceOAuth;
			fallbackToOAuth = OKSettings.FallbackToOAuth;
			HTTP.Request.LogAllRequests = OKSettings.LogAllRequests;
			httpFormat = OKSettings.UseXML ? HTTP.Format.XML : HTTP.Format.JSON;
			AuthType = OKAuthType.None;
			scope = OKSettings.Scope;
			debugAccessToken = OKSettings.DebugAccessToken;
			debugSessionKey = OKSettings.DebugSessionKey;
			SdkInit(OKSettings.SDK_VERSION, SystemInfo.deviceUniqueIdentifier, OKSettings.CLIENT_TYPE, OKSettings.CLIENT_VERSION, callback);
		}

		private void SdkInit(string sdkVersion, string deviceId, string clientType, string clientVersion, OKInitCallback callback)
		{
			Hashtable jsonData = new Hashtable {
				{"version", sdkVersion},
				{"device_id", deviceId},
				{"client_type", clientType},
				{"client_version", clientVersion}
			};
			Dictionary<string, string> args = new Dictionary<string, string> {
				{"method", OKMethod.SDK.init},
				{"application_key", appKey},
				{"session_data", JSON.Encode(jsonData)}
			};
			string url = string.Format("https://api.ok.ru/fb.do?sig={0}&{1}", CalculateSigNoSession(args), URLParams(args));
			new HTTP.Request(url).Send(request =>
			{
				try
				{
					if (request.response.Error != "")
					{
						IsInitialized = false;
						if (callback != null)
						{
							callback(false);
						}
						Debug.Log("Odnoklassniki API initialization failed. Reason: " + request.response.Error);
						return;
					}

					Hashtable response = request.response.Object;
					unitySessionKey = (string)response["session_key"];
					unitySecretSessionKey = (string)response["session_secret_key"];
					apiServer = (string)response["api_server"];
					activatedProfile = (bool)response["activated_profile"];

					IsInitialized = true;
					if (callback != null)
					{
						callback(true);
					}

					Debug.Log("Odnoklassniki API initialized successfully");
				}
				catch (Exception e)
				{
					IsInitialized = false;
					if (callback != null)
					{
						callback(false);
					}
					Debug.Log("Odnoklassniki API initialization failed. Exception: " + e.Message + ". Reason: " + request.response.Error);
				}
			});
		}

		protected DateTime DefaultAccessTokenExpires()
		{
			return DateTime.Now.AddSeconds(AccessTokenDuration); //Since SSO Auth does not pass expiresIn, fill manually
		}

		protected DateTime DefaultRefreshTokenExpires()
		{
			return DateTime.Now.AddDays(RefreshTokenDuration);
		}

		#region Authorization

		public virtual void Auth(OKAuthCallback callback)
		{
			authCallback = callback;

			if (Application.isEditor)
			{
				AuthWithDebugToken();
				return;
			}

			if (forceOAuth)
			{
				Debug.Log("Auth with OAuth: " + GetAuthUrl());
				OAuth();
			}
			else
			{
				Debug.Log("Auth with SSO");
				SsoAuth();
			}
		}

		private void OAuth()
		{
			if (Application.isEditor)
			{
				Debug.Log("Authorization unavailable in Unity Editor");
				return;
			}

			if (clearTokenOnAuth)
			{
				ClearTokens();
			}
			OpenWebView(GetAuthUrl());
		}

		public void OAuthSuccess(string data)
		{
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
			//temp fix for permissions_granted change
			SessionSecretKey = args[1].Split('&')[0];
			AccessTokenExpiresAt = ParseExpiration(args[2]);
			AuthType = OKAuthType.OAuth;
			Debug.Log("Authorized via OAuth!");
			if (authCallback != null)
			{
				authCallback(true);
				authCallback = null;
			}
			HideWebView();
		}

		public void OAuthFailed(string details)
		{
			Debug.LogError("Received OAuth failed callback: " + details);
			if (authCallback != null)
			{
				authCallback(false);
				authCallback = null;
			}
			HideWebView();
		}

		public void SSOAuthFailed(string details)
		{
			Debug.Log("Received SSO Auth failed callback: " + details);
			bool cancelled;
			if (bool.TryParse(details.Replace("cancelled:", ""), out cancelled) && cancelled)
			{
				Debug.Log("Manual cancelled via Back");
				if (authCallback != null)
				{
					authCallback(false);
					authCallback = null;
				}
			}
			else
			{
				if (fallbackToOAuth)
				{
					Debug.Log("Fallback to OAuth");
					OAuth();
				}
				else
				{
					if (authCallback != null)
					{
						authCallback(false);
						authCallback = null;
					}
				}
			}
		}

		protected DateTime ParseExpiration(string s)
		{
			long value;
			if (long.TryParse(s, out value))
			{
				return DateTime.Now.AddSeconds(value);
			}
			else
			{
				Debug.Log("Could not parse expires_in: " + s);
				return DateTime.Now;
			}
		}

		protected virtual bool SsoAuth()
		{
			if (Application.isEditor)
			{
				Debug.Log("Authorization unavailable in Unity Editor");
				return false;
			}

			if (clearTokenOnAuth)
			{
				ClearTokens();
			}
			return true;
		}

		protected abstract string GetAppUrl();

		protected string GetAuthUrl()
		{
			return string.Format(authURL, AppId, scope, responseType, WWW.EscapeURL(GetAppUrl()), layout);
		}

		protected string RefreshTokenUrl()
		{
			return string.Format(refreshTokenURL, apiServer, RefreshToken, AppId, appSecretKey);
		}

		protected string TokenByCodeUrl(string code)
		{
			return string.Format(tokenByCodeURL, apiServer, code, scope, WWW.EscapeURL(GetAppUrl()), AppId, appSecretKey);
		}

		protected string GetApiUrl(Dictionary<string, string> args)
		{
			return string.Format(apiURL, apiServer, AccessToken, CalculateSig(args), URLParams(args));
		}

		private bool AccessTokenValid()
		{
			return AccessToken != null && AccessTokenExpiresAt > DateTime.Now;
		}

		public bool IsRefreshTokenValid
		{
			get
			{
				return RefreshToken != null && RefreshTokenExpiresAt > DateTime.Now;
			}
		}

		public void RefreshAccessToken(OKRefreshTokenCallback callback)
		{
			if (AuthType != OKAuthType.SSO)
			{
				Debug.Log("RefreshToken is only supported for SSO Auth type: Received " + AuthType);
				if (callback != null)
				{
					callback(false);
				}
				return;
			}

			if (!IsRefreshTokenValid)
			{
				Debug.Log("RefreshToken expired");
				if (callback != null)
				{
					callback(false);
				}
				return;
			}

			Debug.Log("Refreshing token!");
			new HTTP.Request(RefreshTokenUrl(), HTTP.Method.POST).Send(request =>
			{
				Debug.Log("RefreshToken response: " + request.response.Text);
				Hashtable json = request.response.Object;
				if (!json.ContainsKey("access_token"))
				{
					Debug.LogError("Refresh token response does not contain access_token");
					if (callback != null)
					{
						callback(false);
					}
					return;
				}
				string newToken = (string)json["access_token"];
				Debug.Log("Token refreshed: " + newToken);
				AccessToken = newToken;
				AccessTokenExpiresAt = DefaultAccessTokenExpires();
				if (callback != null)
				{
					callback(true);
				}
			});
		}

		private string CalculateSigNoSession(Dictionary<string, string> args)
		{
			return Encryption.Md5string(string.Format("{0}{1}", URLParams(args, "", false), appSecretKey)).ToLower();
		}

		private string CalculateSig(Dictionary<string, string> args)
		{
			if (AuthType == OKAuthType.OAuth)
			{
				//Debug.Log(string.Format("{0}{1}", URLParams(args, "", false), SessionSecretKey));
				return Encryption.Md5string(string.Format("{0}{1}", URLParams(args, "", false), SessionSecretKey)).ToLower();
			}

			if (AuthType == OKAuthType.SSO)
			{
				string md5Secret = Encryption.Md5string(AccessToken + appSecretKey);
				return Encryption.Md5string(URLParams(args, "", false) + md5Secret);
			}

			Debug.LogError("Cannot calculate sig - invalid auth type: " + AuthType);
			return "";
		}

		#endregion

		public void Api(string query, HTTP.Method method, Dictionary<string, string> args, OKRequestCallback callback)
		{
			Api(query, method, httpFormat, args, callback);
		}

		private void Api(string query, HTTP.Method method, HTTP.Format format, Dictionary<string, string> args, OKRequestCallback callback)
		{
			if (!IsAuthorized)
			{
				Debug.Log("Cannot call API:" + query + ": Not authorized");
				return;
			}

			args.Add("application_key", appKey);
			args.Add("method", query);
			args.Add("format", format.ToString());

			new HTTP.Request(GetApiUrl(args), method, format).Send(request =>
			{
				//check for error
				Hashtable obj = request.response.Object;
				if (obj != null)
				{
					if (obj.ContainsKey("error_code"))
					{
						Debug.Log(query + " failed -> " + request.response.Error);
						callback(request.response);
						return;
					}
				}

				if (callback != null)
				{
					callback(request.response);
				}
			});
		}

		private void Api(string okMethod, OKRequestCallback callback)
		{
			Api(okMethod, new Dictionary<string, string>(), callback);
		}

		private void Api(string okMethod, Dictionary<string, string> args, OKRequestCallback callback)
		{
			Api(okMethod, HTTP.Method.GET, args, callback);
		}

		private string URLParams(Dictionary<string, string> args, string delim = "&", bool encode = true)
		{
			List<string> urlParams = new List<string>();
			foreach (string key in args.Keys)
			{
				//Debug.Log("Encoding " + key + ": " + args[key] + " -> " + WWW.EscapeURL(args[key]));
				if (encode)
				{
					urlParams.Add(key + "=" + WWW.EscapeURL(args[key]));
				}
				else
				{
					urlParams.Add(key + "=" + args[key]);
				}
			}
			urlParams.Sort();
			return string.Join(delim, urlParams.ToArray());
		}

		public void ClearTokens(bool clearCookies = true)
		{
			AccessToken = null;
			AccessTokenExpiresAt = DateTime.Now;
			RefreshToken = null;
			RefreshTokenExpiresAt = DateTime.Now;
			SessionSecretKey = null;
			AuthType = OKAuthType.None;

			if (webView != null && clearCookies)
			{
				webView.ClearCookies();
			}
		}

		#region Helper methods

		public void GetCallsLeft(string[] methods, OKGetCallsLeftCallback callback)
		{
			Api(OKMethod.Users.getCallsLeft,
				new Dictionary<string, string> {
					{"methods", string.Join(",", methods)}
				},
				delegate(HTTP.Response response)
				{
					List<OKMethodInfo> methodInfos = new List<OKMethodInfo>();
					foreach (Hashtable hashtable in response.Array)
					{
						methodInfos.Add(new OKMethodInfo(hashtable));
					}
					callback(methodInfos.ToArray());
				}
			);
		}

		public void GetCurrentUser(string[] fields, OKGetCurrentUserCallback callback)
		{
			Api(OKMethod.Users.getCurrentUser,
				new Dictionary<string, string> {
					{"fields", string.Join(",", fields)}
				},
				delegate(HTTP.Response response)
				{
					callback(new OKUserInfo(response.Object));
				}
			);
		}

		public void GetInfo(string[] uids, string[] fields, bool emptyPictures, OKGetInfoCallback callback)
		{
			Api(OKMethod.Users.getInfo,
				new Dictionary<string, string> {
					{"uids", string.Join(",", uids)},
					{"fields", string.Join(",", fields)},
					{"emptyPictures", emptyPictures.ToString()}
				},
				delegate(HTTP.Response response)
				{
					ArrayList users = response.Array;
					OKUserInfo[] userInfos = new OKUserInfo[users.Count];
					for (int i = 0; i < users.Count; i++)
					{
						userInfos[i] = new OKUserInfo((Hashtable)users[i]);
					}
					callback(userInfos);
				}
			);
		}

		public void GetAppUsers(OKGetAppUsersCallback callback)
		{
			Api(OKMethod.Friends.getAppUsers, delegate(HTTP.Response response)
			{
				ArrayList uids;
				if (response.IsJSON())
				{
					Hashtable appUsersResponse = response.Object;
					if (!appUsersResponse.ContainsKey("uids"))
					{
						Debug.Log("Bad response, no uids found");
						return;
					}

					uids = (ArrayList)appUsersResponse["uids"];
				}
				else
				{
					uids = response.Array;
				}

				callback(ToStringArray(uids));
			});
		}

		public abstract bool IsOdnoklassnikiNativeAppInstalled ();

		private void AppInvite(string[] uids, string[] devices, string text, OKRequestCallback callback)
		{
			Api(OKMethod.SDK.appInvite,
				new Dictionary<string, string> {
					{"uids", string.Join(",", uids)},
					{"devices", string.Join(",", devices)},
					{"text", text},
					{"sdkToken", unitySessionKey}
				},
				delegate(HTTP.Response response)
				{
					callback(response);
				}
			);
		}

		private void AppSuggest(string[] uids, string text, OKRequestCallback callback)
		{
			Api(OKMethod.SDK.appSuggest,
				new Dictionary<string, string>
				{
					{"uids", string.Join(",", uids)},
					{"text", text},
					{"sdkToken", unitySessionKey}
				},
				callback
			);
		}

		private void Publish(Hashtable attachment, OKRequestCallback callback)
		{
			Api(OKMethod.SDK.post,
				new Dictionary<string, string>
				{
					{"sdkToken", unitySessionKey},
					{"attachment", JSON.Encode(attachment)}
				},
				callback
			);
		}

		private void UploadToAlbum(Texture2D texture, string albumId, Action<string> callback)
		{
			Api("photosV2.getUploadUrl", 
				new Dictionary<string, string>
				{
					{"aid", albumId}
				},
				uploadUrlResponse =>
			{
				Hashtable obj = uploadUrlResponse.Object;
				if (obj == null)
				{
					Debug.LogError("Bad response: bad format");
					return;
				}
				if (!obj.ContainsKey("upload_url"))
				{
					Debug.LogError("Bad response: no upload_url specified");
					return;
				}
				if (!obj.ContainsKey("photo_ids"))
				{
					Debug.LogError("Bad response: no photo_ids specified");
					return;
				}
				string uploadUrl = (string)obj["upload_url"];
				ArrayList photoIds = (ArrayList)obj["photo_ids"];

				WWWForm form = new WWWForm();
				form.AddBinaryData("pic1", texture.EncodeToJPG(), "pic2.jpg", "multipart/form-data");
				new HTTP.Request(uploadUrl, form).Send(request =>
				{
					if (photoIds.Count != 1)
					{
						Debug.LogError("We have been uploading 1 photo, but received response for " + photoIds.Count);
						return;
					}

					string photoId = (string)photoIds[0];
					Hashtable uploadResponse = request.response.Object;
					Hashtable photos = (Hashtable)uploadResponse["photos"];
					Hashtable photoObject = (Hashtable)photos[photoId];
					string token = (string)photoObject["token"];

					callback(token);
				});
			});
		}

		private void CreateAlbum(string albumName, Action<string> callback)
		{
			Api("photos.createAlbum",
				new Dictionary<string, string>
				{
					{"title", albumName},
					{"type", "public"}
				},
				createAlbumResponse =>
				{
					callback(JSON.Decode(createAlbumResponse.Text).ToString());
				}
			);
		}

		private void UploadPhotoForPublish(Texture2D texture, string albumName, Action<string> callback)
		{
			Api("photos.getAlbums", albumsResponse =>
			{
				ArrayList albums = (ArrayList)albumsResponse.Object["albums"];
				string targetAlbumId = null;
				foreach (Hashtable album in albums)
				{
					if (albumName == ((string)album["title"]))
					{
						targetAlbumId = (string)album["aid"];
						break;
					}
				}

				if (targetAlbumId == null)
				{
					CreateAlbum(albumName, albumId =>
					{
						UploadToAlbum(texture, albumId, callback);
					});
				}
				else
				{
					UploadToAlbum(texture, targetAlbumId, callback);
				}
			});
		}

		private void UploadPhoto(Texture2D texture, string comment, OKRequestCallback callback)
		{
			Api("photosV2.getUploadUrl", uploadUrlResponse =>
			{
				Hashtable obj = uploadUrlResponse.Object;
				if (obj == null)
				{
					Debug.LogError("Bad response: bad format");
					return;
				}
				if (!obj.ContainsKey("upload_url"))
				{
					Debug.LogError("Bad response: no upload_url specified");
					return;
				}
				if (!obj.ContainsKey("photo_ids"))
				{
					Debug.LogError("Bad response: no photo_ids specified");
					return;
				}
				string uploadUrl = (string)obj["upload_url"];
				ArrayList photoIds = (ArrayList)obj["photo_ids"];

				WWWForm form = new WWWForm();
				form.AddBinaryData("pic1", texture.EncodeToJPG(), "pic2.jpg", "multipart/form-data");
				new HTTP.Request(uploadUrl, form).Send(request =>
				{
					if (photoIds.Count != 1)
					{
						Debug.LogError("We have been uploading 1 photo, but received response for " + photoIds.Count);
						return;
					}

					string photoId = (string)photoIds[0];
					Hashtable uploadResponse = request.response.Object;
					Hashtable photos = (Hashtable)uploadResponse["photos"];
					Hashtable photoObject = (Hashtable)photos[photoId];
					string token = (string)photoObject["token"];

					Api("photosV2.commit",
						new Dictionary<string, string>
						{
							{"photo_id", photoId},
							{"comment", comment},
							{"token", token}
						},
						callback
					);
				});
			});
		}

		public bool OpenInviteDialog(OKRequestCallback callback, string defaultMessage, string[] selected)
		{
			return OpenInviteDialog(callback, null, defaultMessage, selected);
		}

		public bool OpenInviteDialog(OKRequestCallback callback, Action onClosed, string defaultMessage, string[] selected)
		{
			if (!IsAuthorized)
			{
				Debug.Log("Cannot open Invite dialog: Not authorized");
				return false;
			}

			Api(OKMethod.Friends.get, response =>
			{
				ArrayList uids = response.Array;
				OK.GetAppUsers(appUsers =>
				{
					foreach (string appUser in appUsers)
					{
						uids.Remove(appUser);
					}
					string[] fields = { OKUserInfo.Field.pic128x128, OKUserInfo.Field.name };
					GetInfo(ToStringArray(uids), fields, false, users =>
					{
						OKWidgets.OpenInviteDialog(callback, onClosed, users, defaultMessage, selected, AppInvite);
					});
				});
			});
			return true;
		}

		public bool OpenSuggestDialog(OKRequestCallback callback, string defaultMessage, string[] selected)
		{
			return OpenSuggestDialog(callback, null, defaultMessage, selected);
		}

		public bool OpenSuggestDialog(OKRequestCallback callback, Action onClosed, string defaultMessage, string[] selected)
		{
			if (!IsAuthorized)
			{
				Debug.Log("Cannot open Suggest dialog: Not authorized");
				return false;
			}

			Api(OKMethod.Friends.get, response =>
			{
				ArrayList uids = response.Array;
				string[] fields = { OKUserInfo.Field.pic128x128, OKUserInfo.Field.name };

				GetInfo(ToStringArray(uids), fields, false, users =>
				{
					OKWidgets.OpenSuggestDialog(callback, onClosed, users, defaultMessage, selected, AppSuggest);
				});
			});
			return true;
		}

		public bool OpenPublishDialog(OKRequestCallback callback, OKMedia media)
		{
			return OpenPublishDialog(callback, null, media);
		}

		public bool OpenPublishDialog(OKRequestCallback callback, Action onClosed, OKMedia media)
		{
			if (!IsAuthorized)
			{
				Debug.Log("Cannot open Post dialog: Not authorized");
				return false;
			}

			OKWidgets.OpenPublishDialog(callback, onClosed, media, Publish, UploadPhotoForPublish);
			return true;
		}

		public bool OpenPhotoDialog(OKRequestCallback callback, Texture2D image, string defaultComment)
		{
			return OpenPhotoDialog(callback, null, image, defaultComment);
		}

		public bool OpenPhotoDialog(OKRequestCallback callback, Action onClosed, Texture2D image, string defaultComment)
		{
			if (!IsAuthorized)
			{
				Debug.Log("Cannot open Photo dialog: Not authorized");
				return false;
			}

			OKWidgets.OpenPhotoDialog(callback, onClosed, image, defaultComment, UploadPhoto);
			return true;
		}

		public void GetFriendsByDevices(OKRequestCallback callback, string[] devices)
		{
			Api(OKMethod.Friends.getByDevices,
				new Dictionary<string, string> {
					{"devices", string.Join(",", devices)}
				},
				callback
			);
		}

		#endregion

		private string[] ToStringArray(ArrayList arrayList)
		{
			string[] array = new string[arrayList.Count];
			for (int i = 0; i < arrayList.Count; i++)
			{
				array[i] = arrayList[i].ToString();
			}
			return array;
		}

		#region Debug tools



		public void AuthWithDebugToken()
		{
			if (string.IsNullOrEmpty(debugAccessToken))
			{
				OAuthFailed("Debug Access Token not specified");
				return;
			}

			if (string.IsNullOrEmpty(debugSessionKey))
			{
				OAuthFailed("Debug Session Key not specified");
				return;
			}

			OAuthSuccess(debugAccessToken + ";" + debugSessionKey + ";" + AccessTokenDuration);
		}

		public void RefreshOAuth(OKAuthCallback action)
		{
			authCallback = action;
			if (Application.isEditor)
			{
				Debug.Log("Authorization unavailable in Unity Editor");
				authCallback = null;
				return;
			}

			if (clearTokenOnAuth)
			{
				ClearTokens(false);
			}
			OpenWebView(GetAuthUrl(), 1);
		}

		private void HideWebView()
		{
			if (webView == null) return;
			webView.Hide();
		}

		private void OpenWebView(string url, float showDelay = 0)
		{
			if (Application.isEditor)
			{
				Debug.LogWarning("Webview unavailable in Unity Editor");
				return;
			}

			webView = gameObject.GetComponent<OKWebView>();
			if (webView == null)
			{
				webView = gameObject.AddComponent<OKWebView>();
			}
			webView.Load(url);
			webView.Show(showDelay);
		}

		#endregion
	}
}
