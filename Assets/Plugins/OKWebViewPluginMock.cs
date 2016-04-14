#if !UNITY_ANDROID && !UNITY_IOS
using UnityEngine;
using System.Collections;

namespace Odnoklassniki.WebView {

	public class OKWebViewPlugin {

		public static void Init(string gameObject)
		{
			Warning();
		}

		public static void Load(string url)
		{
			Warning();
		}

		public static void Show()
		{
			Warning();
		}

		public static void Hide()
		{
			Warning();
		}

		public static void ClearCookies()
		{
			Warning();
		}

		public static void Destroy()
		{
			Warning();
		}

		private static void Warning()
		{
			Debug.LogWarning("Web view plugin not available for " + Application.platform);
		}

		public static void Resize() {
			Warning();
		}
	}
}
#endif