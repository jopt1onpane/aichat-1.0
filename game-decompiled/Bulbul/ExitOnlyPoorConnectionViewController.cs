using UnityEngine;

namespace Bulbul;

public class ExitOnlyPoorConnectionViewController : MonoBehaviour
{
	[SerializeField]
	private GameObject view;

	public void Start()
	{
		view.SetActive(value: false);
	}

	public void Activate()
	{
		view.SetActive(value: true);
	}
}
