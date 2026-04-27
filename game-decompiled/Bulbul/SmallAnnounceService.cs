using UnityEngine;

namespace Bulbul;

public class SmallAnnounceService : MonoBehaviour
{
	[SerializeField]
	private SmallAnnounceUI _smallAnnounceUI;

	public void Setup()
	{
		_smallAnnounceUI.Setup();
	}

	public void Activate(float moveAmountY, string localizeID)
	{
		_smallAnnounceUI.SetPosition(InputController.Instance.GetInputPos());
		_smallAnnounceUI.Activate(moveAmountY, localizeID);
	}

	public void Deactivate()
	{
		_smallAnnounceUI.Deactivate();
	}
}
