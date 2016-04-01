using UnityEngine;

public class Background : MonoBehaviour
{
	private Quaternion rotation;

	void Update()
	{
		float t = Time.time*20;
		rotation.eulerAngles = new Vector3(Mathf.Sin(t * Mathf.Deg2Rad)*2, Mathf.Cos(t * Mathf.Deg2Rad)*2, 0);
	}

	void LateUpdate()
	{
		transform.rotation = rotation;
	}
}
