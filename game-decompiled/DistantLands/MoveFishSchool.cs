using UnityEngine;

namespace DistantLands;

public class MoveFishSchool : MonoBehaviour
{
	public float moveRange;

	public float moveSpeed;

	public Vector2 positionChangeSpeed;

	private float _time;

	private Vector3 _originalPos;

	private Vector3 _newPos;

	private Transform target;

	private void Awake()
	{
		_originalPos = base.transform.position;
		target = base.transform.GetChild(0);
	}

	private void Update()
	{
		if (_time >= 0f)
		{
			_time -= Time.deltaTime;
			target.position = Vector3.MoveTowards(target.position, _newPos, moveSpeed * Time.deltaTime);
		}
		else
		{
			_time = Random.Range(positionChangeSpeed.x, positionChangeSpeed.y);
			_newPos = (_originalPos += Random.insideUnitSphere * moveRange);
		}
	}
}
