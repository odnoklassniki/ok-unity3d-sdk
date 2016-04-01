using UnityEngine;

public class Cookie : MonoBehaviour
{
	private Vector3 position;
	private Quaternion rotation;
	private Vector3 scale;

	public AudioClip CookieCrunchSound;

	public ParticleSystem ClickParticle;
	public Transform RigidCookie;

	public AnimationCurve curveClick;

	private bool clickAnim;
	private float animT;

	private Vector3 localTilt;
	public Vector3 localTiltTarget;

	void Awake()
	{
		position = transform.localPosition;
		rotation = transform.rotation;
		scale = Vector3.one;
	}

	float radius = 10;

	private void Update()
	{
		float t = Time.time*80;
		Vector3 localSpin = new Vector3(Mathf.Sin(t*Mathf.Deg2Rad),
			Mathf.Cos(t*Mathf.Deg2Rad), 0)*radius;
		rotation = Quaternion.Euler(localSpin + localTilt);


		if (!clickAnim) return;

		animT += 3*Time.deltaTime;

		Vector3 positionLerp = Vector3.Lerp(Vector3.zero, 30 * Vector3.back,
			curveClick.Evaluate(animT));
		position = Vector3.Lerp(position, positionLerp, 30 * Time.deltaTime);

		Vector3 scaleLerp = Vector3.Lerp(Vector3.one, 1.25f * Vector3.one,
			curveClick.Evaluate(animT));
		scale = Vector3.Lerp(scale, scaleLerp, 30*Time.deltaTime);

		Vector3 localTiltLerp = Vector3.Lerp(Vector3.zero, localTiltTarget, curveClick.Evaluate(animT));
		localTilt = Vector3.Lerp(localTilt, localTiltLerp, 30*Time.deltaTime);

		if (animT <= 1) return;

		clickAnim = false;
		animT = 0;

		localTilt = Vector3.zero;
		position = Vector3.zero;
		scale = Vector3.one;
	}

	void LateUpdate()
	{
		transform.localPosition = position;
		transform.rotation = rotation;
		transform.localScale = scale;
	}

	public void ClickResponse()
	{
		ClickParticle.Emit(Random.Range(5, 15));
		animT = 0;
		clickAnim = true;

		Vector2 rMousePos = new Vector2(Input.mousePosition.x - Screen.width * 0.5f,
										Screen.height * 0.5f - Input.mousePosition.y);

		if (Vector2.Distance(Vector2.zero, rMousePos) < 100) // We don't want it to glitch out when using keyboard
		{
			localTiltTarget = new Vector3(transform.position.y - rMousePos.y,
										  transform.position.x - rMousePos.x, 0) * 0.5f;  
		}
		else
		{
			localTiltTarget = Vector3.zero;
		}
		

		AudioSource audioObj = GameLogic.PlayClipAt(CookieCrunchSound, transform.position);
		audioObj.pitch = 0.8f + Random.value * 0.4f;
	}

	public void LevelUp()
	{
		for (int i = 0; i < 5; i++)
		{
			Transform cookie = Instantiate(RigidCookie, transform.position + 5 * i * Vector3.back, Quaternion.identity) as Transform;

			Rigidbody body = cookie.GetComponent("Rigidbody") as Rigidbody;
			body.velocity = new Vector3(-20 + Random.value * 40, 10 + Random.value * 20, -40 - Random.value * 40);
			body.angularVelocity = Random.insideUnitSphere * 4;
			
			Destroy(cookie.gameObject, 6);
		}
	}
}
