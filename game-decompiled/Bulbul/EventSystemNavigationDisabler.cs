using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Bulbul;

public static class EventSystemNavigationDisabler
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Initialize()
	{
		DisableAllEventSystemNavigation();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		DisableAllEventSystemNavigation();
	}

	private static void DisableAllEventSystemNavigation()
	{
		EventSystem[] array = Object.FindObjectsOfType<EventSystem>(includeInactive: true);
		foreach (EventSystem eventSystem in array)
		{
			if (eventSystem != null)
			{
				eventSystem.sendNavigationEvents = false;
			}
		}
	}
}
