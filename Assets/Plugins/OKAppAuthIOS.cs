using UnityEngine;
using System.Runtime.InteropServices;

namespace Odnoklassniki
{
	public static class OKAppAuthIOS
	{
		[DllImport("__Internal")]
		private static extern void _authorizeInApp(string appId, string scope);

		[DllImport("__Internal")]
		private static extern bool _isNativeAppInstalled(string appId, string scope);

		public static void AuthorizeInApp(string appId, string scope = "VALUABLE_ACCESS")
		{
			if (!Application.isEditor)
			{
				_authorizeInApp(appId, scope);
			}
		}

		public static bool IsNativeAppInstalled(string appId, string scope = "VALUABLE_ACCESS")
		{
			if (Application.isEditor)
			{
				return false;
			}

			return _isNativeAppInstalled(appId, scope);
		}
	}
}