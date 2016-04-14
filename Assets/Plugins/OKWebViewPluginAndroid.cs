#if UNITY_ANDROID
using UnityEngine;

namespace Odnoklassniki.WebView
{
	public class OKWebViewPlugin
	{
		private static AndroidJavaClass java;

		public static void Init(string name)
		{
			java = new AndroidJavaClass("ru.odnoklassniki.unity.OKAndroidPlugin");
			java.CallStatic("OKWV_Init", name);
		}

		public static void Load(string url)
		{
			java.CallStatic("OKWV_Load", url);
		}

		public static void Show()
		{
			java.CallStatic("OKWV_Show");
		}

		public static void Hide()
		{
			java.CallStatic("OKWV_Hide");
		}

		public static void ClearCookies()
		{
			java.CallStatic("OKWV_ClearCookies");
		}

		public static void Destroy()
		{
			java.CallStatic("OKWV_Destroy");
		}

		public static void Resize()
		{
			java.CallStatic("OKWV_Resize");
		}
	}
}
#endif