using System.Collections.Generic;
using UnityEngine;

namespace DistantLands;

public class GlobalFlock : MonoBehaviour
{
	public GameObject[] fishPrefabs;

	public GameObject fishSchool;

	public float wanderSize = 7f;

	public GameObject target;

	public int numFish = 30;

	[HideInInspector]
	public List<GameObject> allFish;

	public static Vector3 goalPos = Vector3.zero;

	private void Start()
	{
		for (int i = 0; i < numFish; i++)
		{
			GameObject gameObject = Object.Instantiate(fishPrefabs[Random.Range(0, fishPrefabs.Length)], base.transform.position + Random.insideUnitSphere * wanderSize, Quaternion.identity);
			gameObject.transform.parent = fishSchool.transform;
			gameObject.transform.localScale = Vector3.one * (Random.value * 0.2f + 0.9f);
			gameObject.GetComponent<Fish>().flock = this;
			allFish.Add(gameObject);
		}
	}

	private void Update()
	{
		HandleGoalPos();
	}

	private void HandleGoalPos()
	{
		if (Random.Range(1, 10000) < 50)
		{
			goalPos = new Vector3(Random.Range(0f - wanderSize, wanderSize), Random.Range(0f - wanderSize, wanderSize), Random.Range(0f - wanderSize, wanderSize));
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, wanderSize);
	}
}
