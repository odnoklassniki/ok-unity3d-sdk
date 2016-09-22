﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using Odnoklassniki.HTTP;
using Odnoklassniki.Util;
using Odnoklassniki.WebView;

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
		private static string apiNoSessionURL = "{0}fb.do?{1}";

		protected string scope;
		protected string responseType = "token";
		protected string layout = "m";

		private const int AccessTokenDuration = 1800; //access token expiration time in seconds
		private const int RefreshTokenDuration = 30; //access token expiration time in days

		private const string PrefsSessionSecretKey = "unityok_session_secret_key";
		private const string PrefsAccessToken = "unityok_access_token";
		private const string PrefsRefreshToken= "unityok_refresh_token";
		private const string PrefsAccessTokenExpiration = "unityok_access_token_expiration";
		private const string PrefsRefreshTokenExpiration = "unityok_refresh_token_expiration";
		private const string PrefsAuthType = "unityok_auth_type";
		private const string PrefsReportPayments = "unityok_report_payments";

		private const string ParamApplicationKey = "application_key";
		private const string ParamMethod = "method";
		private const string ParamFormat= "format";
		private const string ParamPlatform = "platform";
		private const string ParamSdkToken = "sdkToken";

		#endregion

		protected OKAuthType authRequested = OKAuthType.None;

		LinkedList<OKTransaction> paymentTransactionQueue = new LinkedList<OKTransaction>();

		bool paymentTransactionInProgress = false;

		protected OKAuthType AuthType
		{
			get
			{
				string value = PlayerPrefs.GetString(PrefsAuthType);
				if (string.IsNullOrEmpty(value))
				{
					return OKAuthType.None;
				}

				try
				{
					return (OKAuthType) Enum.Parse(typeof(OKAuthType), value);
				} catch (Exception e)
				{
					Debug.LogError("Error parsing Auth Type \"" + value + "\" : " + e.Message);
					return OKAuthType.None;
				}
			}
			set { PlayerPrefs.SetString(PrefsAuthType, value.ToString()); }
		}

		protected string SessionSecretKey {
			get { return PlayerPrefs.GetString(PrefsSessionSecretKey); }
			set { PlayerPrefs.SetString(PrefsSessionSecretKey, value); }
		}

		public string AppId { get; protected set; }

		public string AccessToken
		{
			get { return PlayerPrefs.GetString(PrefsAccessToken); }
			set { PlayerPrefs.SetString(PrefsAccessToken, value); }
		}

		protected string RefreshToken
		{
			get { return PlayerPrefs.GetString(PrefsRefreshToken); }
			set { PlayerPrefs.SetString(PrefsRefreshToken, value); }
		}

		public DateTime AccessTokenExpiresAt
		{
			get
			{
				DateTime dateTime;
				if (DateTime.TryParse(PlayerPrefs.GetString(PrefsAccessTokenExpiration), out dateTime))
				{
					return dateTime;
				}
				else {
					return DateTime.Now;
				}

			}
			set { PlayerPrefs.SetString(PrefsAccessTokenExpiration, value.ToString()); }
		}

		protected DateTime RefreshTokenExpiresAt
		{
			get
			{
				DateTime dateTime;
				if (DateTime.TryParse(PlayerPrefs.GetString(PrefsRefreshTokenExpiration), out dateTime))
				{
					return dateTime;
				} else {
					return DateTime.Now;
				}

			}
			set { PlayerPrefs.SetString(PrefsRefreshTokenExpiration, value.ToString()); }
		}

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
			scope = OKSettings.Scope;
			debugAccessToken = OKSettings.DebugAccessToken;
			debugSessionKey = OKSettings.DebugSessionKey;
			ReportPaymentInit();
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
					unitySessionKey = (string) response["session_key"];
					unitySecretSessionKey = (string) response["session_secret_key"];
					apiServer = (string) response["api_server"];
					activatedProfile = (bool) response["activated_profile"];

					IsInitialized = true;
					if (callback != null)
					{
						callback(true);
					}

					Debug.Log("Odnoklassniki API initialized successfully");
					// Send any unsent payment reports.
					ReportPaymentInternal();
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
			authRequested = OKAuthType.OAuth;
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
			authRequested = OKAuthType.None;
			AuthType = OKAuthType.OAuth;
			Debug.Log("Authorized via OAuth!");
			if (authCallback != null)
			{
				authCallback(true);
				authCallback = null;
			}
			HideWebView();
		}

		public void AuthFailed(string error)
		{
			switch (authRequested)
			{
				case OKAuthType.None:
					Debug.LogError("Auth failed while no auth was requested");
					break;
				case OKAuthType.SSO:
					SSOAuthFailed(error);
					break;
				case OKAuthType.OAuth:
					OAuthFailed(error);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void OAuthFailed(string details)
		{
			authRequested = OKAuthType.None;
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
			authRequested = OKAuthType.None;
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

		public abstract string GetAdvertisingId();

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

		protected string GetApiNoSessionUrl(Dictionary<string, string> args)
		{
			return string.Format(apiNoSessionURL, apiServer, URLParams(args));
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
				return Encryption.Md5string(string.Format("{0}{1}", URLParams(args, "", false), SessionSecretKey)).ToLower();
			}

			if (AuthType == OKAuthType.SSO)
			{
				string md5Secret = Encryption.Md5string(AccessToken + appSecretKey);
				return Encryption.Md5string(URLParams(args, "", false) + md5Secret);
			}
			return "";
		}

		#endregion

		protected abstract string GetPlatform();

		public void Api(string query, HTTP.Method method, Dictionary<string, string> args, OKRequestCallback callback, bool useSession = true)
		{
			Api(query, method, httpFormat, args, callback, useSession);
		}

		private void Api(string query, HTTP.Method method, HTTP.Format format, Dictionary<string, string> args, OKRequestCallback callback, bool useSession = true)
		{
			args.Add(ParamApplicationKey, appKey);
			args.Add(ParamMethod, query);
			args.Add(ParamFormat, format.ToString());
			args.Add(ParamPlatform, GetPlatform().ToUpper());

			if (OKMethod.RequiresSdkToken(query))
			{
				args.Add(ParamSdkToken, unitySessionKey);
			}

			string url = useSession ? GetApiUrl(args) : GetApiNoSessionUrl(args);
			
			new HTTP.Request(url, method, format).Send(request =>
			{
				//check for error
				Hashtable obj = request.response.Object;
				if (obj != null)
				{
					if (obj.ContainsKey("error_code"))
					{
						string errorCode = obj["error_code"].ToString();
						string errorMsg = obj["error_msg"].ToString();
						switch (errorCode)
						{
							case "100":
								if (errorMsg == "PARAM : Missed required parameter: access_token")
								{
									Debug.Log("Missing access token - trying to auto refresh session");
									RefreshAuth(refreshed => {
										Debug.Log("REFRESHED: " + refreshed);
									});
								}
								break;
							case "102":
								Debug.Log("Session expired - trying to auto refresh session");
								RefreshAuth(refreshed => {
									Debug.Log("REFRESHED: " + refreshed);
								});
								break;
							case "103":
								Debug.Log("Invalid session key - trying to auto refresh session");
								RefreshAuth(refreshed => {
									Debug.Log("REFRESHED: " + refreshed);
								});
								break;
							default:
								Debug.LogWarning(query + " failed -> " + request.response.Error);
								callback(request.response);
								break;

						}
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

		private void Api(string okMethod, Dictionary<string, string> args, OKRequestCallback callback, bool useSession = true)
		{
			Api(okMethod, HTTP.Method.GET, args, callback, useSession);
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
			// Number of requests needed to obtain data for all required users, API limited by 100/request.
			int requestCounter = (uids.Length / 100) + ((uids.Length % 100 > 0) ? 1 : 0);
			HTTP.Response[] responseList = new Response[requestCounter];
			int responseCounter = 0;
			int responseUserCount = 0;
			for (int j = 0; j < requestCounter; j++)
			{
				int requestIndex = j;
				// Find the remaining number of uids to send, maximum 100, and put these uids in list.
				int maxCount = Math.Min(100, uids.Length - j * 100);
				string[] requestUids = uids.Skip(j * 100).Take(maxCount).ToArray();
				Api(OKMethod.Users.getInfo,
					new Dictionary<string, string> {
						{"uids", string.Join(",", requestUids)},
						{"fields", string.Join(",", fields)},
						{"emptyPictures", emptyPictures.ToString()}
					},
					delegate (HTTP.Response response)
					{
						string error = response.Error;
						if (!string.IsNullOrEmpty(error))
						{
							// In case of error, return a 0-element list
							callback(new OKUserInfo[0]);
						}
						else
						{
							// Store response
							responseList[requestIndex] = response;
							responseCounter++;
							// Count user response Array objects.
							responseUserCount += response.Array.Count;
							// Process data on last response received
							if (responseCounter == requestCounter)
							{
								OKUserInfo[] userInfos = new OKUserInfo[responseUserCount];
								int responseIndex = 0;
								// Iterate all response objects and save all user response Array objects.
								for (int k = 0; k < requestCounter; k++)
								{
									ArrayList users = responseList[k].Array;
									for (int i = 0; i < users.Count; i++)
									{
										userInfos[responseIndex] = new OKUserInfo((Hashtable) users[i]);
										responseIndex++;
									}
								}
								callback(userInfos);
							}
						}
					}
					);
			}
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

		public void ReportPayment(string trxId, string amount, string currency)
		{
			OKTransaction transaction = new OKTransaction(trxId, amount, currency);
			paymentTransactionQueue.AddFirst(transaction);
			UpdateUnprocessedPaymentList();
			ReportPaymentInternal();
		}

		public abstract bool IsOdnoklassnikiNativeAppInstalled ();

		private void AppInvite(string[] uids, string[] devices, string text, OKRequestCallback callback)
		{
			Api(OKMethod.SDK.appInvite,
				new Dictionary<string, string> {
					{"uids", string.Join(",", uids)},
					{"devices", string.Join(",", devices)},
					{"text", text}
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
					{"text", text}
				},
				callback
			);
		}

		private void Publish(Hashtable attachment, OKRequestCallback callback)
		{
			Api(OKMethod.SDK.post,
				new Dictionary<string, string>
				{
					{"attachment", JSON.Encode(attachment)}
				},
				callback
			);
		}

		private void UploadToAlbum(Texture2D texture, string albumId, Action<Texture2D, string> callback)
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

					callback(texture, token);
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

		private void UploadPhotoForPublish(Texture2D texture, string albumName, Action<Texture2D, string> callback)
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

					if (uids.Count > 0)
					{
						GetInfo(ToStringArray(uids), fields, false, users =>
						{
							OKWidgets.OpenInviteDialog(callback, onClosed, users, defaultMessage, selected, AppInvite);
						});
					} else
					{
						OKWidgets.OpenInviteDialog(callback, onClosed, new OKUserInfo[0], defaultMessage, new string[0], AppInvite);
					}
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
			Api(OKMethod.Friends.get, response =>
			{
				ArrayList uids = response.Array;
				string[] fields = { OKUserInfo.Field.pic128x128, OKUserInfo.Field.name };

				if (uids.Count > 0)
				{
					GetInfo(ToStringArray(uids), fields, false, users =>
					{
						OKWidgets.OpenSuggestDialog(callback, onClosed, users, defaultMessage, selected, AppSuggest);
					});
				}
				else
				{
					OKWidgets.OpenSuggestDialog(callback, onClosed, new OKUserInfo[0], defaultMessage, new string[0], AppSuggest);
				}
				
			});
			return true;
		}

		public bool OpenPublishDialog(OKRequestCallback callback, Action onClosed, List<OKMedia> media)
		{
			OKWidgets.OpenPublishDialog(callback, onClosed, media, Publish, UploadPhotoForPublish);
			return true;
		}

		public bool OpenPhotoDialog(OKRequestCallback callback, Texture2D image, string defaultComment)
		{
			return OpenPhotoDialog(callback, null, image, defaultComment);
		}

		public bool OpenPhotoDialog(OKRequestCallback callback, Action onClosed, Texture2D image, string defaultComment)
		{
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

		private void RefreshAuth(OKRefreshTokenCallback callback)
		{
			if (IsRefreshTokenValid)
			{
				RefreshAccessToken(callback);
			} else
			{
				RefreshOAuth(success => {
					callback(success);
				});
			}
		}

		public void RefreshOAuth(OKAuthCallback action)
		{
			authCallback = action;
			if (Application.isEditor)
			{
				Debug.Log("Authorization unavailable in Unity Editor");
				if (authCallback != null)
				{
					authCallback(false);
					authCallback = null;
				}
				return;
			}

			if (clearTokenOnAuth)
			{
				ClearTokens(false);
			}
			authRequested = OKAuthType.OAuth;
			OpenWebView(GetAuthUrl());
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

		private void ReportPaymentInit()
		{
			string unsentPayments = PlayerPrefs.GetString(PrefsReportPayments);
			if (!string.IsNullOrEmpty(unsentPayments))
			{
				ArrayList decodedList = (ArrayList) JSON.Decode(unsentPayments);
				foreach (string tableItem in decodedList)
				{
					paymentTransactionQueue.AddLast(new OKTransaction((Hashtable) JSON.Decode(tableItem)));
				}
			}
		}

		#region Payment transaction report methods

		private void ReportPaymentInternal()
		{
			// Check if we have any unsent transactions.
			if (paymentTransactionQueue.Count == 0) return;
			if (paymentTransactionInProgress) return;

			paymentTransactionInProgress = true;
			OKTransaction transaction = paymentTransactionQueue.First.Value;
			Api(OKMethod.SDK.reportPayment,
				transaction.ToDictionary(),
				delegate(HTTP.Response response)
				{
					try
					{
						Hashtable json = (Hashtable) JSON.Decode(response.Text);
						bool result = json.Contains("result") && (bool) json["result"];
						if (result)
						{
							// Remove sent transaction from list, if successful
							paymentTransactionQueue.Remove(transaction);
							paymentTransactionInProgress = false;
							// Try sending next transaction, if we have any remaining in queue.
							ReportPaymentInternal();
						}
					}
					catch (Exception e)
					{
						paymentTransactionInProgress = false;
						Debug.LogError("Error response on sdk.reportPayment, transaction id: " + transaction.GetId() + " error: " + e.Message);
					}
				});
		}

		private void UpdateUnprocessedPaymentList()
		{
			ArrayList transactionJsonList = new ArrayList();
			foreach (OKTransaction transaction in paymentTransactionQueue)
			{
				transactionJsonList.Add(transaction.Encode());
			}
			string transactionJson = JSON.Encode(transactionJsonList);
			PlayerPrefs.SetString(PrefsReportPayments, transactionJson);
			PlayerPrefs.Save();
		}

		#endregion
	}
}