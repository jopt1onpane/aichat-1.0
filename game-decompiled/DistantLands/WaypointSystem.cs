using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DistantLands;

public class WaypointSystem : MonoBehaviour
{
	public Transform objectToMove;

	public float moveSpeed;

	public float turnSpeed;

	private Transform target;

	private int progress;

	private List<Transform> waypoints;

	private void Start()
	{
		waypoints = GetComponentsInChildren<Transform>().ToList();
		waypoints.Remove(base.transform);
		target = waypoints[progress];
	}

	private void Update()
	{
		objectToMove.position += objectToMove.forward * moveSpeed * Time.deltaTime;
		objectToMove.rotation = Quaternion.RotateTowards(objectToMove.rotation, Quaternion.LookRotation(target.position - objectToMove.position, Vector3.up), turnSpeed * Time.deltaTime);
		if (Vector3.Distance(objectToMove.position, target.position) < moveSpeed)
		{
			NextPoint();
		}
	}

	public void NextPoint()
	{
		progress++;
		if (progress >= waypoints.Count)
		{
			progress = 0;
		}
		target = waypoints[progress];
	}

	private void OnDrawGizmos()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			if ((bool)base.transform.GetChild(i))
			{
				Transform child = base.transform.GetChild(i);
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(child.position, 0.5f);
				if (i < base.transform.childCount - 1)
				{
					Gizmos.DrawLine(child.position, base.transform.GetChild(i + 1).position);
				}
				else
				{
					Gizmos.DrawLine(child.position, base.transform.GetChild(0).position);
				}
			}
		}
	}
}
