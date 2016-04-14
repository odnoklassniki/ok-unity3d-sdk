using UnityEngine;
using UnityEngine.UI;
using Odnoklassniki;

public class TestFriendSlot : MonoBehaviour {

	public Image photo;
	public Text nameField;

	private Friend friend;

	public void Invite()
	{
		OK.OpenInviteDialog(response => {
			Debug.Log("INVITE => " + response.Text);
		}, 
		"Join me in this awesome game!", friend.uid);
	}

	public void SetFriend(Friend friend)
	{
		this.friend = friend;
		nameField.text = friend.name;
		
		photo.sprite = friend.GetPhoto();
		if (!friend.PhotoLoaded() && !friend.PhotoLoading())
		{
			StartCoroutine(friend.DownloadPhotoCoroutine(photo));
		}
	}
}