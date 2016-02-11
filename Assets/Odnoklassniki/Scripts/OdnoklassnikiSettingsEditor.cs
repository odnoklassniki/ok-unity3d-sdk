#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.OdnoklassnikiEditor;

[CustomEditor(typeof(OKSettings))]
public class OdnoklassnikiSettingsEditor : Editor
{
	bool showInitParams = false;
	bool showPermissions = false;
	bool showDebugParams = false;
	bool showAndroidUtils = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android);
#if UNITY_5
	bool showIOSSettings = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS);
#else
	bool showIOSSettings = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iPhone);
#endif

	//Main app info
	GUIContent appNameLabel = new GUIContent("App Name [?]:", "Odnoklassniki Application Name");
	GUIContent appIdLabel = new GUIContent("App Id [?]:", "Odnoklassniki Application ID");
	GUIContent appKeyLabel = new GUIContent("App Key [?]:", "Odnoklassniki Application Key");
	GUIContent appSecretKeyLabel = new GUIContent("App Secret Key [?]:", "Odnoklassniki Application Secret Key");

	//Permissions
	GUIContent valuableAccessLabel = new GUIContent("Valuable Access [?]:", "Permission to access API methods");
	GUIContent setStatusLabel = new GUIContent("Set Status [?]:", "Permission to change user's status");
	GUIContent appInviteLabel = new GUIContent("App Invite [?]:", "Permission to invite friends");
	GUIContent messagingLabel = new GUIContent("Messaging [?]:", "Permission to send and receive messages on user's behalf");
	GUIContent photoContentLabel = new GUIContent("Photo Content [?]:", "Permission to upload photos and create/edit albums");
	GUIContent videoContentLabel = new GUIContent("Video Content [?]:", "Permission to upload videos");
	GUIContent groupContentLabel = new GUIContent("Group Content [?]:", "Permission to create/edit groups");

	//Init params
	GUIContent forceOAuthLabel = new GUIContent("Force OAUth [?]", "Force authorization via browser instead of authorization via native Android/iOS OK application");
	GUIContent fallbackToOAuthLabel = new GUIContent("Fallback to OAuth [?]", "Fallback to authorization via browser if authorization via native OK application fails");
	GUIContent useXmlLabel = new GUIContent("Use XML [?]", "Receive API responses in XML format if ENABLED, in JSON format if DISABLED");

	//Debug params
	GUIContent debugAccessTokenLabel = new GUIContent("Debug access token [?]", "Everlasting access token which can be used for debug purposes");
	GUIContent debugSessionKeyLabel = new GUIContent("Debug session key [?]", "Everlasting session key which can be used for debug purposes");
	GUIContent logAllRequestsLabel = new GUIContent("Log all requests [?]", "Explicitly log all the Odnoklassniki API requests");

	//Misc

	GUIContent packageNameLabel = new GUIContent("Package Name [?]", "aka: the bundle identifier");
	GUIContent classNameLabel = new GUIContent("Class Name [?]", "aka: the activity name");
	GUIContent debugAndroidKeyLabel = new GUIContent("Debug Android Key Hash [?]", "Copy this key to the Odnoklassniki Settings in order to test a Odnoklassniki Android app");

	GUIContent sdkVersion = new GUIContent("SDK Version [?]", "This Unity Odnoklassniki SDK version.  If you have problems or compliments please include this so we know exactly what version to look out for.");	

	public override void OnInspectorGUI()
	{
		AppIdGUI();
		ScopeGUI();
		OKParamsInitGUI();
		DebugGUI();
		//AndroidUtilGUI();
		//IOSUtilGUI();
		AboutGUI();
	}

	private void AppIdGUI()
	{
		EditorGUILayout.HelpBox("1) Add the Odnoklassniki App Id associated with this game", MessageType.None);
		if (OKSettings.AppId == "0")
		{
			EditorGUILayout.HelpBox("Invalid App Id", MessageType.Error);
		}

		OKSettings.AppName = EditorGUILayout.TextField(appNameLabel, OKSettings.AppName);
		OKSettings.AppId = EditorGUILayout.TextField(appIdLabel, OKSettings.AppId);
		OKSettings.AppKey = EditorGUILayout.TextField(appKeyLabel, OKSettings.AppKey);
		OKSettings.AppSecretKey = EditorGUILayout.TextField(appSecretKeyLabel, OKSettings.AppSecretKey);

		EditorGUILayout.Space();
	}

	private void ScopeGUI()
	{
		EditorGUILayout.HelpBox("2) Set required permissions for the game", MessageType.None);
		showPermissions = EditorGUILayout.Foldout(showPermissions, "OK Application Permissions");
		if (showPermissions)
		{		
			OKSettings.SetScope(OKScope.VALUABLE_ACCESS, EditorGUILayout.Toggle(valuableAccessLabel, OKSettings.HasScope(OKScope.VALUABLE_ACCESS)));
			OKSettings.SetScope(OKScope.SET_STATUS, EditorGUILayout.Toggle(setStatusLabel, OKSettings.HasScope(OKScope.SET_STATUS)));
			OKSettings.SetScope(OKScope.APP_INVITE, EditorGUILayout.Toggle(appInviteLabel, OKSettings.HasScope(OKScope.APP_INVITE)));
			OKSettings.SetScope(OKScope.MESSAGING, EditorGUILayout.Toggle(messagingLabel, OKSettings.HasScope(OKScope.MESSAGING)));
			OKSettings.SetScope(OKScope.PHOTO_CONTENT, EditorGUILayout.Toggle(photoContentLabel, OKSettings.HasScope(OKScope.PHOTO_CONTENT)));
			OKSettings.SetScope(OKScope.VIDEO_CONTENT, EditorGUILayout.Toggle(videoContentLabel, OKSettings.HasScope(OKScope.VIDEO_CONTENT)));
			OKSettings.SetScope(OKScope.GROUP_CONTENT, EditorGUILayout.Toggle(groupContentLabel, OKSettings.HasScope(OKScope.GROUP_CONTENT)));
		}

		EditorGUILayout.Space();
	}

	private void OKParamsInitGUI()
	{
		EditorGUILayout.HelpBox("(Optional) Edit the OK.Init() parameters", MessageType.None);
		showInitParams = EditorGUILayout.Foldout(showInitParams, "OK.Init() Parameters");
		if (showInitParams)
		{
			OKSettings.ForceOAuth = EditorGUILayout.Toggle(forceOAuthLabel, OKSettings.ForceOAuth);
			OKSettings.FallbackToOAuth = EditorGUILayout.Toggle(fallbackToOAuthLabel, OKSettings.FallbackToOAuth);
			OKSettings.UseXML = EditorGUILayout.Toggle(useXmlLabel, OKSettings.UseXML);
		}
		EditorGUILayout.Space();
	}

	private void DebugGUI()
	{
		EditorGUILayout.HelpBox("(Optional) Odnoklassniki debug parameters", MessageType.None);
		showDebugParams = EditorGUILayout.Foldout(showDebugParams, "Debug Parameters");
		if (showDebugParams)
		{
			OKSettings.DebugAccessToken = EditorGUILayout.TextField(debugAccessTokenLabel, OKSettings.DebugAccessToken);
			OKSettings.DebugSessionKey = EditorGUILayout.TextField(debugSessionKeyLabel, OKSettings.DebugSessionKey);
			OKSettings.LogAllRequests = EditorGUILayout.Toggle(logAllRequestsLabel, OKSettings.LogAllRequests);
		}
		EditorGUILayout.Space();
	}

	private void IOSUtilGUI()
	{
		showIOSSettings = EditorGUILayout.Foldout(showIOSSettings, "iOS Build Settings");
		if (showIOSSettings)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.Space();
	}

	private void AndroidUtilGUI()
	{
		showAndroidUtils = EditorGUILayout.Foldout(showAndroidUtils, "Android Build Odnoklassniki Settings");
		if (showAndroidUtils)
		{
			if (!OdnoklassnikiAndroidUtil.IsSetupProperly())
			{
				var msg = "Your Android setup is not right. Check the documentation.";
				switch (OdnoklassnikiAndroidUtil.SetupError)
				{
					case OdnoklassnikiAndroidUtil.ERROR_NO_SDK:
						msg = "You don't have the Android SDK setup!  Go to " + (Application.platform == RuntimePlatform.OSXEditor ? "Unity" : "Edit") + "->Preferences... and set your Android SDK Location under External Tools";
						break;
					case OdnoklassnikiAndroidUtil.ERROR_NO_KEYSTORE:
						msg = "Your android debug keystore file is missing! You can create new one by creating and building empty Android project in Ecplise.";
						break;
					case OdnoklassnikiAndroidUtil.ERROR_NO_KEYTOOL:
						msg = "Keytool not found. Make sure that Java is installed, and that Java tools are in your path.";
						break;
					case OdnoklassnikiAndroidUtil.ERROR_NO_OPENSSL:
						msg = "OpenSSL not found. Make sure that OpenSSL is installed, and that it is in your path.";
						break;
					case OdnoklassnikiAndroidUtil.ERROR_KEYTOOL_ERROR:
						msg = "Unkown error while getting Debug Android Key Hash.";
						break;
				}
				EditorGUILayout.HelpBox(msg, MessageType.Warning);
			}
			EditorGUILayout.HelpBox("Copy and Paste these into your \"Native Android App\" Settings on Developers OK", MessageType.None);
			SelectableLabelField(packageNameLabel, PlayerSettings.bundleIdentifier);
			SelectableLabelField(classNameLabel, ManifestMod.DeepLinkingActivityName);
			SelectableLabelField(debugAndroidKeyLabel, OdnoklassnikiAndroidUtil.DebugKeyHash);
			if (GUILayout.Button("Regenerate Android Manifest"))
			{
				ManifestMod.GenerateManifest();
			}
		}
		EditorGUILayout.Space();
	}

	private void AboutGUI()
	{
		EditorGUILayout.HelpBox("About the Odnoklassniki SDK", MessageType.None);
		SelectableLabelField(sdkVersion, OKSettings.CLIENT_VERSION);
		
		EditorGUILayout.Space();
	}

	private void SelectableLabelField(GUIContent label, string value)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(label, GUILayout.Width(180), GUILayout.Height(16));
		EditorGUILayout.SelectableLabel(value, GUILayout.Height(16));
		EditorGUILayout.EndHorizontal();
	}
}
#endif