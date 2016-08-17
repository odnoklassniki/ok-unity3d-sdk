using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Odnoklassniki;
using Odnoklassniki.Util;
using UnityEngine.UI;

public class TestFriendPanel : MonoBehaviour
{

	public Scrollbar scrollbar;
	public GameObject suggestButton;
	public GameObject friendsContainer;
	public GameObject openButton;
	public List<Friend> friendObjectList = new List<Friend>();
	public TestFriendSlot friendPrefab;
	public Sprite defaultPhoto;
	public Text noFriendsLabel;
	public float friendGap = 150;

	private float scrollbarThreshold = 0.99f;

	private List<TestFriendSlot> friendPrefabList = new List<TestFriendSlot>();
	private int currentPositionX;
	private int childCount;

	public void SetFriends(OKUserInfo[] friends)
	{
		// Store all friend objects
		foreach (OKUserInfo friend in friends)
		{
			Friend f = new Friend(friend.uid, friend.name, friend.pic128x128, defaultPhoto);
			friendObjectList.Add(f);
		}
		// Let's scale the friends container manually! (Explained below, why)
		RectTransform friendsContainerRect = friendsContainer.GetComponent(typeof(RectTransform)) as RectTransform;
		friendsContainerRect.sizeDelta = new Vector2(friendObjectList.Count * friendGap, friendsContainerRect.sizeDelta.y);

		int optimalObjectCount = (int)((Screen.width / friendGap) + 8);
		childCount = Math.Min(optimalObjectCount, friendObjectList.Count);

		// Fill initial children.
		int x = 0;
		for (int i = 0; i < childCount; i++)
		{
			TestFriendSlot newFriendSlot = (TestFriendSlot)Instantiate(friendPrefab);
			newFriendSlot.transform.SetParent(friendsContainer.transform, false);
			newFriendSlot.transform.localPosition = new Vector3(x * friendGap, newFriendSlot.transform.localPosition.y, 0);
			friendPrefabList.Add(newFriendSlot);
			newFriendSlot.SetFriend(friendObjectList[i]);
			x++;
		}

		if (this.friendObjectList.Count == 0)
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

	public void Update()
	{
		Vector3 positionVector = friendsContainer.transform.localPosition;
		int position = (int)(Math.Abs(positionVector.x) / friendGap);
		UpdatePositions(position);
	}

	private void UpdatePositions(int position)
	{
		if (position != currentPositionX)
		{
			currentPositionX = position;
			int processedItems = 0;
			for (int i = position - 5; i < position + childCount; i++)
			{
				// End when all prefabs had their position set once, starting from left side.
				if (processedItems >= childCount) break;
				if (i >= 0 && i < friendObjectList.Count)
				{
					TestFriendSlot item = friendPrefabList[i % childCount];
					// Each item has a specific position already reserved for it during initialization.
					item.transform.localPosition = new Vector3(i * friendGap, item.transform.localPosition.y, 0);
					item.SetFriend(friendObjectList[i]);
					processedItems++;
				}
			}
		}
	}

	public void UpdateScrollbar()
	{
		StartCoroutine(UpdateScrollbarCoroutine());
	}

	public IEnumerator UpdateScrollbarCoroutine()
	{
		if (friendObjectList.Count == 0)
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