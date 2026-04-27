using UnityEngine;

namespace MagicLightProbes;

public class PingPong : MonoBehaviour
{
	public enum Direction
	{
		TopDown,
		LeftRight,
		ForwarBackward
	}

	public Direction direction;

	public float distance;

	public float speed;

	private void Start()
	{
	}

	private void Update()
	{
		switch (direction)
		{
		case Direction.ForwarBackward:
			base.transform.localPosition = new Vector3(0f, 0f, Mathf.PingPong(Time.time, distance));
			break;
		case Direction.LeftRight:
			base.transform.localPosition = new Vector3(Mathf.PingPong(Time.time, distance), 0f, 0f);
			break;
		case Direction.TopDown:
			base.transform.localPosition = new Vector3(0f, Mathf.PingPong(Time.time, distance), 0f);
			break;
		}
	}
}
