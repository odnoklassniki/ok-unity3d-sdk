using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Odnoklassniki
{

#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class OKSettings : ScriptableObject
	{

		public const string SDK_VERSION = "2";
		public const string CLIENT_TYPE = "SDK_UNITY3D";
		public const string CLIENT_VERSION = "1.0.25";

		const string OdnoklassnikiSettingsAssetName = "OdnoklassnikiSettings";
		const string OdnoklassnikiSettingsPath = "Odnoklassniki/Resources";
		const string OdnoklassnikiSettingsAssetExtension = ".asset";

		private static OKSettings instance;

		public static OKSettings Instance
		{
			get
			{
				if (instance == null)
				{
					instance = Resources.Load(OdnoklassnikiSettingsAssetName) as OKSettings;
					if (instance == null)
					{
						// If not found, autocreate the asset object.
						instance = CreateInstance<OKSettings>();
#if UNITY_EDITOR
						string properPath = Path.Combine(Application.dataPath, OdnoklassnikiSettingsPath);
						if (!Directory.Exists(properPath))
						{
							AssetDatabase.CreateFolder("Assets/Odnoklassniki", "Resources");
						}

						string fullPath = Path.Combine(Path.Combine("Assets", OdnoklassnikiSettingsPath),
													   OdnoklassnikiSettingsAssetName + OdnoklassnikiSettingsAssetExtension
													  );
						AssetDatabase.CreateAsset(instance, fullPath);
#endif
					}
				}
				return instance;
			}
		}

#if UNITY_EDITOR
		[MenuItem("Odnoklassniki/Edit Settings")]
		public static void Edit()
		{
			Selection.activeObject = Instance;
		}

		[MenuItem("Odnoklassniki/Open Application Page")]
		public static void OpenAppPage()
		{
			string url = string.Format("http://ok.ru/game/{0}", AppId);
			Application.OpenURL(url);
		}

		[MenuItem("Odnoklassniki/SDK Documentation")]
		public static void OpenDocumentation()
		{
			string url = "https://github.com/odnoklassniki/ok-unity3d-sdk/blob/master/README.md";
			Application.OpenURL(url);
		}

		[MenuItem("Odnoklassniki/Report a SDK Bug")]
		public static void ReportABug()
		{
			string version = "\n\nSDK Version: " + CLIENT_VERSION;
			string url = "https://github.com/odnoklassniki/ok-unity3d-sdk/issues/new?body=" + WWW.EscapeURL(version);
			Application.OpenURL(url);
		}
#endif

		#region App Settings

		[SerializeField]
		private string appId = "";
		[SerializeField]
		private string appKey = "";
		[SerializeField]
		private string appName = "App Name";
		[SerializeField]
		private bool forceOAuth = false;
		[SerializeField]
		private bool fallbackToOAuth = true;
		[SerializeField]
		private bool useXML = false;
		[SerializeField]
		private bool logAllRequests = false;
		[SerializeField]
		private OKScopeSettings scope = new OKScopeSettings();
		[SerializeField]
		private string debugAccessToken = "";
		[SerializeField]
		private string debugSessionKey = "";

		public static string AppId
		{
			get { return Instance.appId; }
			set
			{
				if (Instance.appId != value)
				{
					Instance.appId = value;
					DirtyEditor();
				}
			}
		}

		public static string Scope
		{
			get { return Instance.scope.GetScope(); }
		}

		public static void SetScope(string scope, bool value)
		{
			if (Instance.scope.HasScope(scope) != value)
			{
				Instance.scope.SetScope(scope, value);
				DirtyEditor();
			}
		}

		public static void SetCustomScopes(string scopes)
		{
			if (!Instance.scope.HasCustomScopes(scopes))
			{
				Instance.scope.SetCustomScopes(scopes);
				DirtyEditor();
			}
		}

		public static string GetCustomScopes()
		{
			return Instance.scope.GetCustomScopes();
		}

		public static bool HasScope(string scope)
		{
			return Instance.scope.HasScope(scope);
		}

		public static string AppKey
		{
			get { return Instance.appKey; }
			set
			{
				if (Instance.appKey != value)
				{
					Instance.appKey = value;
					DirtyEditor();
				}
			}
		}

		public static string AppName
		{
			get { return Instance.appName; }
			set
			{
				if (Instance.appName != value)
				{
					Instance.appName = value;
					DirtyEditor();
				}
			}
		}

		public static bool ForceOAuth
		{
			get { return Instance.forceOAuth; }
			set
			{
				if (Instance.forceOAuth != value)
				{
					Instance.forceOAuth = value;
					DirtyEditor();
				}
			}
		}

		public static bool LogAllRequests
		{
			get { return Instance.logAllRequests; }
			set
			{
				if (Instance.logAllRequests != value)
				{
					Instance.logAllRequests = value;
					DirtyEditor();
				}
			}
		}

		public static bool FallbackToOAuth
		{
			get { return Instance.fallbackToOAuth; }
			set
			{
				if (Instance.fallbackToOAuth != value)
				{
					Instance.fallbackToOAuth = value;
					DirtyEditor();
				}
			}
		}

		public static bool UseXML
		{
			get { return Instance.useXML; }
			set
			{
				if (Instance.useXML != value)
				{
					Instance.useXML = value;
					DirtyEditor();
				}
			}
		}

		public static string DebugAccessToken
		{
			get { return Instance.debugAccessToken; }
			set
			{
				if (Instance.debugAccessToken != value)
				{
					Instance.debugAccessToken = value;
					DirtyEditor();
				}
			}
		}

		public static string DebugSessionKey
		{
			get { return Instance.debugSessionKey; }
			set
			{
				if (Instance.debugSessionKey != value)
				{
					Instance.debugSessionKey = value;
					DirtyEditor();
				}
			}
		}

		public static bool IsValidAppId
		{
			get
			{
				return !string.IsNullOrEmpty(AppId) && !AppId.Equals("0");
			}
		}

		private static void DirtyEditor()
		{
#if UNITY_EDITOR
			EditorUtility.SetDirty(Instance);
#endif
		}

		#endregion
	}

	[Serializable]
	public class OKScopeSettings
	{
		[SerializeField]
		private List<string> scopes = new List<string>();

		[SerializeField]
		string[] customScopes = new string[0];

		public void SetScope(string scope, bool enabled)
		{
			if (enabled)
			{
				if (!scopes.Contains(scope))
				{
					scopes.Add(scope);
				}
			}
			else
			{
				scopes.Remove(scope);
			}
		}

		public bool HasScope(string scope)
		{
			return scopes.Contains(scope);
		}

		public void SetCustomScopes(string scope)
		{
			if (string.IsNullOrEmpty(scope))
			{
				customScopes = new string[0];
			}
			else
			{
				customScopes = scope.Split(',');
			}
		}

		public bool HasCustomScopes(string scope)
		{
			return customScopes.Equals(scope.Split(','));
		}

		public string GetCustomScopes()
		{
			return string.Join(",", customScopes);
		}

		public string GetScope()
		{
			return string.Join(",", scopes.ToArray().Concat(customScopes).ToArray()).Replace(" ", "");
		}
	}
}