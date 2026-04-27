using UnityEngine;

namespace Bulbul;

public class AuthButton : MonoBehaviour
{
	[SerializeField]
	private RectTransform unlinkParent;

	[field: SerializeField]
	public ButtonEventObservable LinkButton { get; private set; }

	[field: SerializeField]
	public ButtonEventObservable UnlinkButton { get; private set; }

	public void SetState(bool isLinked)
	{
		unlinkParent.gameObject.SetActive(isLinked);
		LinkButton.gameObject.SetActive(!isLinked);
	}
}
