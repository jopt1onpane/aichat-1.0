using System.Collections.Generic;
using UnityEngine;

public static class FindGameObjectUtility
{
	public static void FindObjectsFromClass<T>(List<T> result) where T : class
	{
		Component[] array = Object.FindObjectsOfType<Component>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is T item)
			{
				result.Add(item);
			}
		}
	}
}
