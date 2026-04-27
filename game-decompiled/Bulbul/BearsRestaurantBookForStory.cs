using UnityEngine;

namespace Bulbul;

public class BearsRestaurantBookForStory : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	public void Play(string motionName)
	{
		if (!animator.gameObject.activeSelf)
		{
			animator.gameObject.SetActive(value: true);
		}
		animator.Play(motionName, 0, 0f);
	}

	public void Deactivate()
	{
		animator.gameObject.SetActive(value: false);
	}
}
