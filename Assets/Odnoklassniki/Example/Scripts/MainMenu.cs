using UnityEngine;
using UnityEngine.UI;
using Odnoklassniki;

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
		OK.Init(initSuccess =>
		{
			if (!initSuccess)
			{
				Debug.Log("OK.Init() failed :(");
			}
		});

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
		OK.Auth(success => CheckLoginButton());
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
