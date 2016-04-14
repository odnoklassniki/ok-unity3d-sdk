#if UNITY_IOS
using UnityEngine;
using System.Runtime.InteropServices;

namespace Odnoklassniki.WebView {

	public class OKWebViewPlugin {

		private static string webViewName;

		[DllImport("__Internal")]
		private static extern void _Init(string name);

		[DllImport("__Internal")]
		private static extern void _Load(string name, string url);

		[DllImport("__Internal")]
		private static extern void _Show(string name);

		[DllImport("__Internal")]
		private static extern void _Hide(string name);

		[DllImport("__Internal")]
		private static extern void _Destroy(string name);

		[DllImport("__Internal")]
		private static extern void _Resize(string name);

		[DllImport("__Internal")]
		private static extern void _ClearCookies(string name);


		public static void Init(string name)
		{
			webViewName = name;
			_Init(webViewName);
		}

		public static void Load(string url)
		{
			_Load(webViewName, url);
		}

		public static void Show()
		{
			_Show(webViewName);
		}

		public static void Hide()
		{
			_Hide(webViewName);
		}

		public static void ClearCookies() {
			_ClearCookies(webViewName);
		}

		public static void Destroy()
		{
			_Destroy(webViewName);
		}

		public static void Resize() {
			_Resize(webViewName);
		}
	}
}
#endif