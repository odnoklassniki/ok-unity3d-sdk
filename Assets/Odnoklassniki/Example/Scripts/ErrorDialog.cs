using UnityEngine;
using UnityEngine.UI;

public class ErrorDialog : MonoBehaviour {

	private static Transform canvas;

	public Text errorCode;
	public Text errorMessage;
	public Button button;

	public void Awake()
	{
		button.onClick.AddListener(Close);
	}

	public void Close()
	{
		Destroy(gameObject);
	}

	public static void Show(string code, string message)
	{
		ErrorDialog dialog = (Instantiate(Resources.Load("Error Dialog")) as GameObject).GetComponent<ErrorDialog>();
		dialog.transform.SetParent(Canvas(), false);
		dialog.errorCode.text = string.Format("Error {0}", code);
		dialog.errorMessage.text = message;
	}

	private static Transform Canvas()
	{
		if (canvas == null)
		{
			canvas = FindObjectOfType<Canvas>().transform;
		}
		return canvas;
	}
}
