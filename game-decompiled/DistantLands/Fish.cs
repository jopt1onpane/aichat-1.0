using UnityEngine;

namespace DistantLands;

public class Fish : MonoBehaviour
{
	private float speed;

	public float averageSpeed;

	private Vector3 averageHeading;

	private Vector3 averagePosition;

	private float neighborDistance = 3f;

	public int performance;

	[HideInInspector]
	public GlobalFlock flock;

	private bool turning;

	private void Start()
	{
		speed = Random.Range(0.5f, 1.5f) * averageSpeed;
	}

	private void Update()
	{
		ApplyTankBoundary();
		if (turning)
		{
			Vector3 forward = flock.target.transform.position + Vector3.up * Random.Range(-2, 2) - base.transform.position;
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(forward), TurnSpeed() * Time.deltaTime);
		}
		else if (Random.Range(0, performance + 1) < 1)
		{
			ApplyRules();
		}
		base.transform.Translate(0f, 0f, Time.deltaTime * speed);
	}

	private void ApplyTankBoundary()
	{
		if ((bool)flock && (bool)flock.target)
		{
			if (Vector3.Distance(base.transform.position, flock.target.transform.position) >= flock.wanderSize)
			{
				turning = true;
			}
			else
			{
				turning = false;
			}
		}
	}

	private void ApplyRules()
	{
		GameObject[] array = flock.allFish.ToArray();
		speed = Random.Range(0.5f, 1.5f) * averageSpeed;
		Vector3 position = flock.target.transform.position;
		Vector3 zero = Vector3.zero;
		float num = 0f;
		Vector3 position2 = flock.target.transform.position;
		int num2 = 0;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (!(gameObject != base.gameObject))
			{
				continue;
			}
			float num3 = Vector3.Distance(gameObject.transform.position, base.transform.position);
			if (num3 <= neighborDistance)
			{
				position += gameObject.transform.position;
				num2++;
				if (num3 < 0.75f)
				{
					zero += base.transform.position - gameObject.transform.position;
				}
				Fish component = gameObject.GetComponent<Fish>();
				num += component.speed;
			}
		}
		if (num2 > 0)
		{
			position = position / num2 + (position2 - base.transform.position);
			speed = num / (float)num2;
			Vector3 vector = position + zero - base.transform.position;
			if (vector != Vector3.zero)
			{
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(vector), TurnSpeed() * Time.deltaTime);
			}
		}
	}

	private float TurnSpeed()
	{
		return Random.Range(0.2f, 0.4f) * speed;
	}
}
