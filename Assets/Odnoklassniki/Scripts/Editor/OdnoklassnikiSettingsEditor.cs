using UnityEngine;
using UnityEditor;

namespace Odnoklassniki
{
	[CustomEditor(typeof(OKSettings))]
	public class OdnoklassnikiSettingsEditor : Editor
	{
		bool showInitParams = false;
		bool showPermissions = false;
		bool showDebugParams = false;

		//Main app info
		GUIContent appNameLabel = new GUIContent("App Name [?]:", "Odnoklassniki Application Name");
		GUIContent appIdLabel = new GUIContent("App Id [?]:", "Odnoklassniki Application ID");
		GUIContent appKeyLabel = new GUIContent("App Key [?]:", "Odnoklassniki Application Key");

		//Permissions
		GUIContent valuableAccessLabel = new GUIContent("Valuable Access [?]:", "Permission to access API methods");
		GUIContent setStatusLabel = new GUIContent("Set Status [?]:", "Permission to change user's status");
		GUIContent appInviteLabel = new GUIContent("App Invite [?]:", "Permission to invite friends");
		GUIContent messagingLabel = new GUIContent("Messaging [?]:", "Permission to send and receive messages on user's behalf");
		GUIContent photoContentLabel = new GUIContent("Photo Content [?]:", "Permission to upload photos and create/edit albums");
		GUIContent videoContentLabel = new GUIContent("Video Content [?]:", "Permission to upload videos");
		GUIContent groupContentLabel = new GUIContent("Group Content [?]:", "Permission to create/edit groups");
		GUIContent longAccessTokenLabel = new GUIContent("Long Access Token [?]:", "Permission to have extended access token");
		GUIContent customScopesLabel = new GUIContent("Custom Scopes [?]:", "Specify other permissions separated by comma");

		//Init params
		GUIContent forceOAuthLabel = new GUIContent("Force OAUth [?]", "Force authorization via browser instead of authorization via native Android/iOS OK application");
		GUIContent fallbackToOAuthLabel = new GUIContent("Fallback to OAuth [?]", "Fallback to authorization via browser if authorization via native OK application fails");
		GUIContent useXmlLabel = new GUIContent("Use XML [?]", "Receive API responses in XML format if ENABLED, in JSON format if DISABLED");

		//Debug params
		GUIContent debugAccessTokenLabel = new GUIContent("Debug access token [?]", "Everlasting access token which can be used for debug purposes");
		GUIContent debugSessionKeyLabel = new GUIContent("Debug session key [?]", "Everlasting session key which can be used for debug purposes");
		GUIContent logAllRequestsLabel = new GUIContent("Log all requests [?]", "Explicitly log all the Odnoklassniki API requests");

		//Misc
		GUIContent sdkVersion = new GUIContent("SDK Version [?]", "This Unity Odnoklassniki SDK version.  If you have problems or compliments please include this so we know exactly what version to look out for.");

		public override void OnInspectorGUI()
		{
			AppIdGUI();
			ScopeGUI();
			OKParamsInitGUI();
			DebugGUI();
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
				OKSettings.SetScope(OKScope.LONG_ACCESS_TOKEN, EditorGUILayout.Toggle(longAccessTokenLabel, OKSettings.HasScope(OKScope.LONG_ACCESS_TOKEN)));

				OKSettings.SetCustomScopes(EditorGUILayout.TextField(customScopesLabel, OKSettings.GetCustomScopes()));
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
}