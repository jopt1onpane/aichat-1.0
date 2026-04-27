using UnityEngine;

namespace Bulbul;

public class LocationUI : MonoBehaviour
{
	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[SerializeField]
	[Header("デスク")]
	private InteractableUI _changeLocationDeskButton;

	[SerializeField]
	[Header("ソファ")]
	private InteractableUI _changeLocationSofaButton;

	[SerializeField]
	[Header("ベッド")]
	private InteractableUI _changeLocationBedButton;

	public void Setup()
	{
		_changeLocationDeskButton.ActivateUseUI();
	}

	public void OnChangeLocation(LocationType locationType)
	{
		_changeLocationDeskButton.DeactivateUseUI();
		_changeLocationSofaButton.DeactivateUseUI();
		_changeLocationBedButton.DeactivateUseUI();
		switch (locationType)
		{
		case LocationType.Desk:
			_changeLocationDeskButton.ActivateUseUI();
			break;
		case LocationType.Sofa:
			_changeLocationSofaButton.ActivateUseUI();
			break;
		case LocationType.Bed:
			_changeLocationBedButton.ActivateUseUI();
			break;
		}
	}

	public bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	public void Activate()
	{
		_facilityOpenButton.ActivateUseUI();
		base.gameObject.SetActive(value: true);
	}

	public void Deactivate()
	{
		_facilityOpenButton.DeactivateUseUI();
		base.gameObject.SetActive(value: false);
	}
}
