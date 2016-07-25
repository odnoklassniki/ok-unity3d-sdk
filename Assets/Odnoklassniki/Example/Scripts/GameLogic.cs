using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Odnoklassniki;
using Odnoklassniki.HTTP;

public class GameLogic : MonoBehaviour {

	private const string PrefsExp = "ok_clicker_exp";
	private const string PrefsLvl = "ok_clicker_lvl";

	public Text clicksText;
	public Text levelText;
	public Text expText;
	public Text nameText;
	public LevelUpPanel levelUpPanel;
	public TestFriendPanel friends;

	public Cookie cookie;
	public AudioClip LevelUpSound;

	private int timesClicked;
	private int level;
	private int exp;

	private bool coroutineRunning;

	private bool tokenRenewRequested;

	void Awake()
	{
		LoadProgress();
	}

	void Start()
	{
		UpdateClicks();
		UpdateLevel();
		UpdateExp();

		InitOk();
	}

	private void InitOk()
	{
		if (!OK.IsLoggedIn)
		{
			Debug.Log("Odnoklassniki not authorized -> Hiding friends and name panel");
			friends.Hide();
			nameText.transform.parent.gameObject.SetActive(false);
			return;
		}

		friends.Unhide();
		nameText.transform.parent.gameObject.SetActive(true);
		OK.GetCurrentUser(userInfo =>
		{
			nameText.text = userInfo.name;
		}, OKUserInfo.Field.name);
		
		InitFriends();
	}

	private void InitFriends()
	{
		OK.API(OKMethod.Friends.get, Method.GET, response =>
		{
			ArrayList uidList = response.Array;
			OK.GetAppUsers(appUsers =>
			{
				foreach (string appUser in appUsers)
				{
					uidList.Remove(appUser);
				}
				string[] uids = new string[uidList.Count];
				for (int i = 0; i < uidList.Count; i++)
				{
					uids[i] = uidList[i].ToString();
				}
				string[] fields = { OKUserInfo.Field.pic128x128, OKUserInfo.Field.name };
				OK.GetInfo(users =>
				{
					friends.SetFriends(users);
				}, uids, fields);
			});
		});
	}

	public void OnCookieClicked()
	{
		if (!coroutineRunning)
		{
			timesClicked++;
			UpdateClicks();
			AddExp();
			CookieClickResponse();
		}
	}

	private void AddExp()
	{
		exp++;
		if (exp >= ExpToNextLevel())
		{
			LevelUp();
		}
		SaveProgress();
		UpdateExp();
	}
	private void LevelUp()
	{
		exp = 0;
		level++;
		UpdateLevel();
		cookie.LevelUp();
		PlayClipAt(LevelUpSound, Vector3.zero);

		StartCoroutine(StartLvlUp());
	}

	private void SaveProgress()
	{
		PlayerPrefs.SetInt(PrefsExp, exp);
		PlayerPrefs.SetInt(PrefsLvl, level);
	}

	private void LoadProgress()
	{
		exp = PlayerPrefs.GetInt(PrefsExp, 0);
		level = PlayerPrefs.GetInt(PrefsLvl, 1);
	}
		
	private void CookieClickResponse()
	{
		cookie.ClickResponse();
	}

	private void UpdateExp()
	{
		expText.text = "EXP" + "\n" + exp + "/" + ExpToNextLevel();
	}

	private void UpdateClicks()
	{
		clicksText.text = "Clicks" + "\n" + timesClicked;
	}

	private void UpdateLevel()
	{
		levelText.text = "Level" + "\n" + level;
	}

	private int ExpToNextLevel()
	{
		return 5;
	}

	IEnumerator StartLvlUp()
	{
		coroutineRunning = true;
		yield return new WaitForSeconds(1);

		coroutineRunning = false;

		friends.Hide();

		StartCoroutine(ShowLvlUp());
	}
	IEnumerator ShowLvlUp()
	{
		yield return new WaitForEndOfFrame();

		Texture2D screenshot = new Texture2D(Screen.width, Screen.height);
		screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		screenshot.Apply();

		if (OK.IsLoggedIn)
		{
			friends.Unhide();
		}

		levelUpPanel.Open(level, screenshot);
	}

	public void Suggest()
	{
		OK.OpenSuggestDialog(response => {
			Debug.Log("SUGGEST => " + response.Text);
		},
		"This game is CRAZY");
	}

	public void PauseMenu()
	{
		Application.LoadLevel("MainMenu");
	}

	public static AudioSource PlayClipAt(AudioClip clip, Vector3 pos)
	{
		GameObject tempAudio = new GameObject("TempAudio");
		tempAudio.transform.position = pos;
		AudioSource aSource = tempAudio.AddComponent<AudioSource>();
		aSource.clip = clip;

		aSource.Play();
		Destroy(tempAudio, clip.length);

		return aSource;
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (!OKWidgets.HasActiveWidget())
			{
				PauseMenu();
			}
		}

		if (Input.GetKeyDown(KeyCode.N))
		{
			LevelUp();
		}
	}
}
