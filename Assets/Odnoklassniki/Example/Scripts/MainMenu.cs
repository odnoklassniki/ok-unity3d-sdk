using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	public Button OKLoginButton;
	public Button OKLogoutButton;

	public Text LocalizationText;

	public void Play()
	{
		Application.LoadLevel("NewGame");
	}

	public void Awake()
	{
		CheckLoginButton();
		if (LocalizationText) LocalizationText.text = Application.systemLanguage.ToString();
	}

	private void CheckLoginButton()
	{
		if (OK.IsLoggedIn)
		{
			OKLoginButton.gameObject.SetActive(false);
			OKLogoutButton.gameObject.SetActive(true);
		}
		else
		{
			OKLoginButton.gameObject.SetActive(true);
			OKLogoutButton.gameObject.SetActive(false);
		}
	}

	public void LoginWithOK()
	{
		OK.Init(initSuccess =>
		{
			if (!initSuccess)
			{
				Debug.Log("OK.Init() failed :(");
				return;
			}

			OK.Auth(success => CheckLoginButton());
		});
	}

	public void Logout()
	{
		OK.Logout();
		CheckLoginButton();
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Quit();
		}
	}
}
