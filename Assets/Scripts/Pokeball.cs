using UnityEngine;
using System.Collections;

public class Pokeball : MonoBehaviour
{
	[SerializeField]
	private float throwSpeed = 35f;
	private float speed;
	private float lastMouseX, lastMouseY;

	[SerializeField]
	private bool thrown, holding, curve;

	private Rigidbody _rigidbody;
	private Vector3 newPosition;

	[SerializeField]
	private float curveAmount = 0f, curveSpeed = 2f, minCurveAmountToCurveBall = 1f, maxCurveAmount = 2.5f;
	private Rect circlingBox;

	void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();

		_rigidbody.maxAngularVelocity = curveAmount * 8f;

		circlingBox = new Rect(Screen.width / 2, Screen.height / 2, 0f, 0f);

		Reset();
	}

	void Update()
	{
		if (holding)

			OnTouch();

		curve = (Mathf.Abs(curveAmount) > minCurveAmountToCurveBall);

		if (curve && thrown)
		{
			Vector3 direction = transform.right.normalized;

			_rigidbody.AddForce(direction * curveAmount * Time.deltaTime, ForceMode.Impulse);
		}

		_rigidbody.maxAngularVelocity = curveAmount * 8f;

		_rigidbody.angularVelocity = transform.forward * curveAmount * 8f + _rigidbody.angularVelocity;

		if (thrown)
			return;

		if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 100f))
			{
				if (hit.transform == transform)
				{
					holding = true;

					transform.SetParent(null);
				}
			}
		}

		if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
		{
			if (lastMouseY < Input.GetTouch(0).position.y)
			{
				ThrowBall(Input.GetTouch(0).position);
			}
		}

		if (Input.touchCount == 1)
		{
			lastMouseX = Input.GetTouch(0).position.x;

			lastMouseY = Input.GetTouch(0).position.y;

			if (lastMouseX < circlingBox.x)
				circlingBox.x = lastMouseX;

			if (lastMouseX > circlingBox.xMax)
				circlingBox.xMax = lastMouseX;

			if (lastMouseY < circlingBox.y)
				circlingBox.y = lastMouseY;

			if (lastMouseY > circlingBox.yMax)
				circlingBox.yMax = lastMouseY;
		}
	}

	void Reset()
	{
		curveAmount = 0f;

		CancelInvoke();

		transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.1f, Camera.main.nearClipPlane * 7.5f));

		newPosition = transform.position;

		thrown = holding = false;

		_rigidbody.useGravity = false;

		_rigidbody.velocity = Vector3.zero;

		_rigidbody.angularVelocity = Vector3.zero;

		_rigidbody.Sleep();

		transform.SetParent(Camera.main.transform);

		transform.localRotation = Quaternion.Euler(0f, 200f, 0f);

		missed = false;
	}

	void OnTouch()
	{
		CalcCurveAmount();

		Vector3 mousePos = Input.GetTouch(0).position;

		mousePos.z = Camera.main.nearClipPlane * 7.5f;

		newPosition = Camera.main.ScreenToWorldPoint(mousePos);

		transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, 50f * Time.deltaTime);
	}

	void CalcCurveAmount()
	{
		Vector2 b = new Vector2(lastMouseX, lastMouseY);

		Vector2 c = Input.GetTouch(0).position;

		Vector2 a = circlingBox.center;

		if (b == c)

			return;

		bool isLeft = ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0;

		if (isLeft)

			curveAmount -= Time.deltaTime * curveSpeed;

		else

			curveAmount += Time.deltaTime * curveSpeed;

		curveAmount = Mathf.Clamp(curveAmount, -maxCurveAmount, maxCurveAmount);
	}

	void ThrowBall(Vector2 mousePos)
	{
		_rigidbody.useGravity = true;

		float differenceY = (mousePos.y - lastMouseY) / Screen.height * 100;

		speed = throwSpeed * differenceY;

		float x = (mousePos.x - lastMouseX) / Screen.width;

		Vector3 direction = Quaternion.AngleAxis(x * 180f, Vector3.up) * new Vector3(0f, 1f, 1f);

		direction = Camera.main.transform.TransformDirection(direction);

		_rigidbody.AddForce(direction * speed);

		holding = false;

		thrown = true;

		Invoke("reset", 5.0f);
	}


	bool missed = false;

	void OnCollisionEnter(Collision collision)
	{
		Debug.Log(collision.transform.tag);

		if (collision.transform.tag == "Pokemon" && !missed)
		{
			GameObject pokemon = collision.transform.gameObject;

			StartCoroutine(CatchingPhase(0.5f, pokemon));
		}
		else if (collision.transform.tag != "Pokemon")
		{
			missed = true;
		}
	}

	IEnumerator CatchingPhase(float chance, GameObject pokemon)
	{
		Debug.Log("gotta catchem");

		bool caught = (Random.Range(0, 1) < chance);

		pokemon.SetActive(false);

		_rigidbody.AddForce(Vector3.up * 2.0f);

		yield return new WaitForSeconds(0.25f);

		_rigidbody.isKinematic = true;

		_rigidbody.velocity = Vector3.zero;

		_rigidbody.angularVelocity = Vector3.zero;

		yield return new WaitForSeconds(.25f);

		_rigidbody.isKinematic = false;

		yield return new WaitForSeconds(2f);
		if (caught)
		{
			Debug.Log(pokemon + "sucked into ball");

			PokemonSpawner.Run();

			yield break;

		}
		else
		{
			Debug.Log(pokemon + "broke the ball");
		}
		pokemon.SetActive(true);

		yield break;
	}
}