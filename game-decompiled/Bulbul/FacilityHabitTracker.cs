using UnityEngine;

namespace Bulbul;

public class FacilityHabitTracker : MonoBehaviour
{
	[SerializeField]
	private HabitTrackerUI _habitTrackerUI;

	public void Setup()
	{
		_habitTrackerUI.Setup();
	}

	public void UpdateFacility()
	{
		_habitTrackerUI.UpdateUI();
	}

	public bool IsActive()
	{
		if (_habitTrackerUI.IsActive())
		{
			return true;
		}
		return false;
	}

	public void Activate()
	{
		_habitTrackerUI.Activate();
	}

	public void Deactivate()
	{
		_habitTrackerUI.Deactivate();
	}
}
