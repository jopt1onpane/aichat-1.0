using UnityEngine;

namespace Bulbul;

public class UICanvasRaycastBlockerProvider : MonoBehaviour, IRaycastBlocker
{
	[SerializeField]
	private GameObject _blocker;

	public void Block()
	{
		_blocker.SetActive(value: true);
	}

	public void Release()
	{
		_blocker.SetActive(value: false);
	}
}
