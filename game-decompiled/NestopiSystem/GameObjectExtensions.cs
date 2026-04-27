using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NestopiSystem;

public static class GameObjectExtensions
{
	public static bool TryGetComponent<T>(this Collision source, out T component) where T : Component
	{
		return source.gameObject.TryGetComponent<T>(out component);
	}

	public static bool TryGetComponent<T>(this Collider source, out T component) where T : Component
	{
		return source.gameObject.TryGetComponent<T>(out component);
	}

	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		if (!gameObject.TryGetComponent<T>(out var component))
		{
			return gameObject.AddComponent<T>();
		}
		return component;
	}

	public static T GetOrAddComponent<T>(this Component component) where T : Component
	{
		if (!component.TryGetComponent<T>(out var component2))
		{
			return component.gameObject.AddComponent<T>();
		}
		return component2;
	}

	public static bool HasComponent<T>(this GameObject gameObject) where T : Component
	{
		T component;
		return gameObject.TryGetComponent<T>(out component);
	}

	public static bool HasComponent<T>(this Component component) where T : Component
	{
		T component2;
		return component.gameObject.TryGetComponent<T>(out component2);
	}

	public static bool TryAddComponent<T>(this GameObject source, out T component) where T : Component
	{
		component = null;
		if (source.HasComponent<T>())
		{
			return false;
		}
		component = source.gameObject.AddComponent<T>();
		return true;
	}

	public static bool TryAddComponent<T>(this Component source, out T component) where T : Component
	{
		return source.gameObject.TryAddComponent<T>(out component);
	}

	public static void DestroyChildren(this Transform self)
	{
		while (self.childCount != 0)
		{
			Object.DestroyImmediate(self.GetChild(0).gameObject);
		}
	}

	public static IEnumerable<Transform> GetAllChildren(this Transform self, bool includeSelf = false)
	{
		List<Transform> list = new List<Transform>();
		if (includeSelf)
		{
			list.Add(self);
		}
		return GetAllChildrenCore(self, list);
	}

	public static IEnumerable<GameObject> GetAllChildren(this GameObject self, bool includeSelf = false)
	{
		List<GameObject> list = new List<GameObject>();
		if (includeSelf)
		{
			list.Add(self);
		}
		return GetAllChildrenCore(self, list);
	}

	private static List<Transform> GetAllChildrenCore(Transform self, List<Transform> list)
	{
		foreach (Transform item in self.transform)
		{
			list.Add(item);
			GetAllChildrenCore(item, list);
		}
		return list;
	}

	private static List<GameObject> GetAllChildrenCore(GameObject self, List<GameObject> list)
	{
		foreach (Transform item in self.transform)
		{
			list.Add(item.gameObject);
			GetAllChildrenCore(item.gameObject, list);
		}
		return list;
	}

	public static void SetLayer(this GameObject gameObject, int layer, bool needSetChildren = true, params int[] ignoreLayers)
	{
		if (gameObject == null)
		{
			return;
		}
		if (!ignoreLayers.Contains(gameObject.layer))
		{
			gameObject.layer = layer;
		}
		if (!needSetChildren)
		{
			return;
		}
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetLayer(layer, needSetChildren: true, ignoreLayers);
		}
	}

	public static void SetLayer(this Transform transform, int layer, bool needSetChildren = true, params int[] ignoreLayers)
	{
		transform.gameObject.SetLayer(layer, needSetChildren, ignoreLayers);
	}
}
