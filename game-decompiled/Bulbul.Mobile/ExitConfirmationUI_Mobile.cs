using UnityEngine;

namespace Bulbul.Mobile;

public class ExitConfirmationUI_Mobile : ExitConfirmationUI
{
	public override void Setup()
	{
		base.Setup();
		base.gameObject.SetActive(value: true);
	}

	public override void Activate()
	{
		base.Activate();
	}

	public override void Deactivate()
	{
		base.Deactivate();
	}

	public override void AddDontCloseOnClick(RectTransform trans)
	{
	}
}
