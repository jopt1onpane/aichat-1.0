using System.Collections.Generic;
using UnityEngine;

namespace Bulbul;

public class CommonPrefabSupplier : ScriptableObject
{
	[SerializeField]
	private List<GameObject> prefabs = new List<GameObject>();

	private readonly Dictionary<string, GameObject> _prefabDic = new Dictionary<string, GameObject>();

	private void OnEnable()
	{
		_prefabDic.Clear();
		foreach (GameObject prefab in prefabs)
		{
			if (!_prefabDic.TryAdd(prefab.name, prefab))
			{
				Debug.LogWarning("CommonPrefabSupplier: 重複したプレファブは無視 " + prefab.name);
			}
		}
	}

	public GameObject Get(string prefabName)
	{
		if (_prefabDic.TryGetValue(prefabName, out var value))
		{
			return value;
		}
		Debug.LogError("CommonPrefabSupplier: プレファブが見つかりません " + prefabName);
		return null;
	}
}
