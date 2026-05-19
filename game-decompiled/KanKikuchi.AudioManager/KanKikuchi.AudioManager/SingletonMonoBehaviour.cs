using UnityEngine;

namespace KanKikuchi.AudioManager;

public class SingletonMonoBehaviour<T> : MonoBehaviourWithInit where T : SingletonMonoBehaviour<T>
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindAnyObjectByType<T>();
				if (_instance == null)
				{
					Debug.LogError(typeof(T)?.ToString() + " is nothing");
				}
				else
				{
					_instance.InitIfNeeded();
				}
			}
			return _instance;
		}
	}

	protected override void Awake()
	{
		if (!(this == Instance))
		{
			if (Instance == null)
			{
				_instance = (T)this;
				InitIfNeeded();
			}
			else
			{
				Debug.LogError(typeof(T)?.ToString() + " is duplicated");
			}
		}
	}
}
