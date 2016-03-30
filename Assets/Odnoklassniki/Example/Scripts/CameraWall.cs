using UnityEngine;

public class CameraWall : MonoBehaviour
{
	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.GetComponent<CookieRigid>())
		{
			CookieRigid cScr = other.gameObject.GetComponent<CookieRigid>();

			AudioSource audioObj = GameLogic.PlayClipAt(cScr.impactSound[Random.Range(0, cScr.impactSound.Length)], transform.position);
			audioObj.minDistance = 200;
			audioObj.pitch = 0.8f + Random.value * 0.4f;
		}
	}
}
