using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Odnoklassniki;
using Odnoklassniki.HTTP;
using Odnoklassniki.Util;

public class LevelUpPanel : MonoBehaviour {

	public RawImage screenshot;
	public Text buttonText;
	public Text congratulations;
	private Texture2D texture;
	public AspectRatioFitter textureAspectRatio;
	private int level;

	private float aspectRatio;

	public void Start()
	{
		ScreenManager.AddOnScreenChanged(OnResize);

		buttonText.text = !OK.IsLoggedIn ? "OK" : "SHARE";
	}

	public void Open(int newLevel, Texture2D image)
	{
		level = newLevel;
		congratulations.text = string.Format("You reached {0} level!", newLevel);
		texture = image;
		screenshot.texture = image;
		aspectRatio = image.width * 1f / image.height;

		AdjustScreenshotSize();

		gameObject.SetActive(true);
	}

	private void OnResize()
	{
		AdjustScreenshotSize();
	}

	private void AdjustScreenshotSize()
	{
		textureAspectRatio.gameObject.SetActive(false);
		textureAspectRatio.aspectRatio = aspectRatio;
		textureAspectRatio.gameObject.SetActive(true);
	}

	public void Share()
	{
		string description = string.Format("WOW! I reached level {0}!", level);
		if (OK.IsLoggedIn)
		{
			OK.OpenPhotoDialog(uploadResponse => {
				Debug.Log("Photo uploaded!");
				OK.OpenPublishDialog(PublishCallback, new List<OKMedia>()
				{
					OKMedia.Photo(texture),
					OKMedia.Text(description)
				});
			}, () => {
				OK.OpenPublishDialog(PublishCallback, new List<OKMedia>()
				{
					OKMedia.Photo(texture),
					OKMedia.Text(description)
				});
			},
			texture, description);
		}
		Close();
	}

	private void PublishCallback(Response response)
	{
		if (response.Object != null && response.Object.ContainsKey("error_code"))
		{
			ErrorDialog.Show(response.Object["error_code"].ToString(), response.Object["error_msg"].ToString());
		}
		else
		{
			Debug.Log("Photo published");
		}
	}

	public void Close()
	{ 
		gameObject.SetActive(false);
	}
}
