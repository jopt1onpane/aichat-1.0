using UnityEngine;

namespace Bulbul.Mobile;

public class SpecialSelectListWindowView : MonoBehaviour, ISpecialSelectListUI
{
	[SerializeField]
	private SpecialSelectListUIForMobile _specialSelectList;

	[SerializeField]
	private FacilityCommonActivateAnimationMobile _activator;

	void ISpecialSelectListUI.Activate()
	{
		_specialSelectList.ResetScroll();
		_activator.Activate();
	}

	void ISpecialSelectListUI.Deactivate()
	{
		_activator.Deactivate();
	}

	void ISpecialSelectListUI.Setup()
	{
		_activator.Setup();
	}
}
