using System;
using UnityEngine;
using System.Collections;

namespace Odnoklassniki.WebView
{
	public class OKWebView : MonoBehaviour
	{

		int lastHeight;
		Action onLoadComplete;
		bool loaded;

		public void Awake()
		{
			OKWebViewPlugin.Init(gameObject.name);
		}

		public void Load(string url)
		{
			loaded = false;
			OKWebViewPlugin.Load(url.Trim());
		}

		public void Show(float delay = 0)
		{
			if (delay == 0)
			{
				OKWebViewPlugin.Show();
				onLoadComplete = null;
				return;
			}

			onLoadComplete = () =>
			{
				StartCoroutine(ShowCoroutine(delay));
			};
		}

		IEnumerator ShowCoroutine(float delay)
		{
			yield return new WaitForSeconds(delay);
			//onLoadComplete may be reset by Hide();
			if (onLoadComplete == null) yield break;
			Show();
		}

		public void Hide()
		{
			OKWebViewPlugin.Hide();
			onLoadComplete = null;
		}

		public void ClearCookies()
		{
			OKWebViewPlugin.ClearCookies();
		}

		private void OnDestroy()
		{
			OKWebViewPlugin.Destroy();
		}

		private void Update()
		{
			if (OrientationChanged())
			{
				Resize();
			}
		}

		private void Resize()
		{
			Debug.Log("Webview RESIZE");
			OKWebViewPlugin.Resize();
		}

		private void LoadComplete()
		{
			if (loaded)
			{
				Debug.Log("LOAD COMPLETE - ALREADY LOADED");
				return;
			}
			Debug.Log("LOAD COMPLETE");
			if (onLoadComplete != null)
			{
				onLoadComplete();
			}
		}

		private bool OrientationChanged()
		{
			if (lastHeight != Height())
			{
				lastHeight = Height();
				return true;
			}
			else
			{
				return false;
			}
		}

		private int Height()
		{
			return Screen.height;
		}
	}
}