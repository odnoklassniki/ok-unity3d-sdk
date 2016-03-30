using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TestFriendPanel : MonoBehaviour {

	public Scrollbar scrollbar;
	public GameObject suggestButton;
	public GameObject friendsContainer;
	public GameObject openButton;
	public List<Friend> friends = new List<Friend>();
	public TestFriendSlot friendPrefab;
	public Sprite defaultPhoto;
	public Text noFriendsLabel;
	public float friendGap = 150;

	private float scrollbarThreshold = 0.99f;

	public void SetFriends(OKUserInfo[] friends)
	{
		// Let's scale the friends container manually! (Explained below, why)
		RectTransform friendsContainerRect = friendsContainer.GetComponent(typeof(RectTransform)) as RectTransform;
		friendsContainerRect.sizeDelta = new Vector2(friends.Length*friendGap, friendsContainerRect.sizeDelta.y);
		
		int x = 0;
		foreach (OKUserInfo friend in friends)
		{
			Friend f = new Friend(friend.uid, friend.name, friend.pic128x128, defaultPhoto);

			TestFriendSlot newFriendSlot = (TestFriendSlot) Instantiate(friendPrefab);
			newFriendSlot.transform.SetParent(friendsContainer.transform, false);
			
			// Offset the friend slot - We could the "Horizontal layer group" component,
			// but it messes with the child objects too much, so that's why we scaled the friends container before!
			Vector3 newFriendSlotPos = newFriendSlot.transform.localPosition;
			newFriendSlot.transform.localPosition = new Vector3(x*friendGap, newFriendSlotPos.y, 0);
			
			// Give the slot its friend data
			newFriendSlot.SetFriend(f);
			this.friends.Add(f);
			
			x++;
		}
		
		if (this.friends.Count == 0)
		{
			NoFriends();
		}
		else
		{
			noFriendsLabel.gameObject.SetActive(false);
		}

		UpdateScrollbar();

		ScreenManager.AddOnScreenChanged(UpdateScrollbar);
	}

	public void UpdateScrollbar()
	{
		StartCoroutine(UpdateScrollbarCoroutine());
	}

	public IEnumerator UpdateScrollbarCoroutine()
	{
		if (friends.Count == 0)
		{
			scrollbar.gameObject.SetActive(false);
			yield break;
		}

		yield return new WaitForEndOfFrame();

		scrollbar.gameObject.SetActive(scrollbar.size < scrollbarThreshold);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
		if (OK.IsLoggedIn)
		{
			openButton.SetActive(true);
		}
	}
	public void Unhide()
	{
		gameObject.SetActive(true);
		openButton.SetActive(false);
	}
	
	public void NoFriends()
	{
		scrollbar.gameObject.SetActive(false);
		suggestButton.gameObject.SetActive(false);
		noFriendsLabel.gameObject.SetActive(true);
		noFriendsLabel.text = "No friends found";
	}
}