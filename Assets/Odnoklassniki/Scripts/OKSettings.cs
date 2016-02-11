using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
#endif
public class OKSettings : ScriptableObject
{

	public const string SDK_VERSION = "2";
	public const string CLIENT_TYPE = "SDK_UNITY3D";
	public const string CLIENT_VERSION = "1.0.0";

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

	[MenuItem("Odnoklassniki/Developers Page")]
	public static void OpenAppPage()
	{
		string url = string.Format("http://ok.ru/dk?st.cmd=appEdit&st.appId={0}&st._aid=App_Main_EditApp", AppId);
		Application.OpenURL(url);
	}

	[MenuItem("Odnoklassniki/SDK Documentation")]
	public static void OpenDocumentation()
	{
		string url = "http://apiok.ru";
		Application.OpenURL(url);
	}

	[MenuItem("Odnoklassniki/Report a SDK Bug")]
	public static void ReportABug()
	{
		string url = "mailto:api-support@odnoklassniki.ru";
		Application.OpenURL(url);
	}
#endif

	#region App Settings

	[SerializeField]
	private string appId = "";
	[SerializeField]
	private string appKey = "";
	[SerializeField]
	private string appSecretKey = "";
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

	public static void SetScope(OKScope scope, bool value)
	{
		if (Instance.scope.HasScope(scope) != value)
		{
			Instance.scope.SetScope(scope, value);
			DirtyEditor();
		}
	}

	public static bool HasScope(OKScope scope)
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

	public static string AppSecretKey
	{
		get { return Instance.appSecretKey; }
		set
		{
			if (Instance.appSecretKey != value)
			{
				Instance.appSecretKey = value;
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
			if (Instance.debugAccessToken != value) {
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
			if (Instance.debugSessionKey != value) {
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
	private List<OKScope> scopes = new List<OKScope>();

	public void SetScope(OKScope scope, bool enabled)
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

	public bool HasScope(OKScope scope)
	{
		return scopes.Contains(scope);
	}

	public string GetScope()
	{
		return string.Join(",", scopes.Select(i => i.ToString()).ToArray());
	}
}
