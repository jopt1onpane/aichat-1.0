using UnityEngine;

namespace Bulbul;

public class FacilityAmbientSound : FacilityBase
{
	[SerializeField]
	private GameObject uiParent;

	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[SerializeField]
	private AmbientSoundBehavior[] ambientSoundVolumeContorollers;

	public void Setup()
	{
		AmbientSoundBehavior[] array = ambientSoundVolumeContorollers;
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
		}
	}

	public void UpdateFacility()
	{
	}

	public bool IsAcrive()
	{
		return uiParent.activeSelf;
	}

	public void Activate()
	{
		_facilityOpenButton.ActivateUseUI();
		uiParent.SetActive(value: true);
	}

	public void Deactivate()
	{
		_facilityOpenButton.DeactivateUseUI();
		uiParent.SetActive(value: false);
	}
}
