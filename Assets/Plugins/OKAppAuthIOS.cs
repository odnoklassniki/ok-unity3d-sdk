using UnityEngine;
using System.Runtime.InteropServices;

public static class OKAppAuthIOS {

	[DllImport("__Internal")]
	private static extern void _authorizeInApp(string appId, string scope);

	public static void AuthorizeInApp(string appId, string scope = "VALUABLE_ACCESS")
	{
		Debug.Log("Calling iOS in app authorization");
		if (!Application.isEditor) {
			_authorizeInApp(appId, scope);
		}
	}
}
