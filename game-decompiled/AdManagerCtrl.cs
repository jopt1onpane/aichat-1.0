using UnityEngine;

public class AdManagerCtrl : MonoBehaviour
{
	private void Awake()
	{
	}

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
	}
}
