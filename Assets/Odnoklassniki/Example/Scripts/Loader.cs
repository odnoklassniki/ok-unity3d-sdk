using System.Collections;
using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class Loader : MonoBehaviour
{
	public Transform spinningCookie;
	public SpriteRenderer loading;
	public SpriteRenderer loadingSlow;

	internal bool levelsLoaded;

	private float timeOut = 2;
	internal bool timeOutCompleted;

	void Awake()
	{
		if (Application.isPlaying)
		{
			SetProgressSlow(0);
			SetProgress(0);
		}
	}

	void Start()
	{
		if (Application.isPlaying)
		{
			StartCoroutine(SetProgressSlow());
			StartCoroutine(WaitForTimeout());
			StartCoroutine(LoadMainMenu());
		}
	}

	void Update()
	{
		if (spinningCookie != null)
		{
			spinningCookie.Rotate(0, 2.0f, 0);
		}
	}

	IEnumerator SetProgressSlow()
	{
		while (!levelsLoaded)
		{
			float i = Time.realtimeSinceStartup / 2;
			float sin = (Mathf.Sin(i) + 1) / 2;
			SetProgressSlow(sin);
			yield return null;
		}
	}

	void SetProgress(float progress)
	{
		if (loading != null)
		{
			loading.transform.localScale = new Vector3(progress, 1, 1);
		}
	}

	void SetProgressSlow(float progress)
	{
		if (loadingSlow != null)
		{
			loadingSlow.transform.localScale = new Vector3(progress, 1, 1);
		}
	}

	IEnumerator WaitForTimeout()
	{
		yield return new WaitForSeconds(timeOut);
		timeOutCompleted = true;
	}

	IEnumerator LoadMainMenu()
	{
		Application.backgroundLoadingPriority = ThreadPriority.Low;

#if UNITY_5_3_OR_NEWER
		AsyncOperation async = SceneManager.LoadSceneAsync("MainMenu");
#else
		AsyncOperation async = Application.LoadLevelAsync("MainMenu");
#endif
		async.allowSceneActivation = false;

		while (async.progress < 0.9f)
		{
			SetProgress(async.progress);
			yield return null;
		}

		while (!timeOutCompleted)
		{
			yield return null;
		}

		if (!levelsLoaded)
		{
			levelsLoaded = true;
		}

		async.allowSceneActivation = true;
	}
}
