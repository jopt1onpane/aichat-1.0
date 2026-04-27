using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilityLocation : MonoBehaviour
{
	private enum MainState
	{
		Idle,
		ChangingLocation
	}

	private MainState _mainState;

	[Inject]
	private RoomHeroineMovementController _roomHeroineMovementController;

	[SerializeField]
	private LocationUI _locationUI;

	public void Setup()
	{
		_locationUI.Setup();
	}

	public void UpdateFacility()
	{
		if (_mainState != MainState.Idle)
		{
			_ = 1;
		}
	}

	public bool IsActive()
	{
		return _locationUI.IsActive();
	}

	public void Activate()
	{
		_locationUI.Activate();
	}

	public void Deactivate()
	{
		_locationUI.Deactivate();
	}

	public bool IsCanChangeLocation()
	{
		if (_mainState == MainState.Idle)
		{
			return true;
		}
		return false;
	}

	private void StartChangeLocation(LocationType locationType)
	{
		_mainState = MainState.ChangingLocation;
		RoomHeroineMovementParam parameter = new RoomHeroineMovementParam
		{
			MovementNo = (int)locationType
		};
		_roomHeroineMovementController.OnInput(parameter, delegate
		{
			_mainState = MainState.Idle;
		});
		_locationUI.OnChangeLocation(locationType);
	}

	public void OnClickButtonDesk()
	{
		if (IsCanChangeLocation())
		{
			StartChangeLocation(LocationType.Desk);
		}
	}

	public void OnClickButtonSofa()
	{
		if (IsCanChangeLocation())
		{
			StartChangeLocation(LocationType.Sofa);
		}
	}

	public void OnClickButtonBed()
	{
		if (IsCanChangeLocation())
		{
			StartChangeLocation(LocationType.Bed);
		}
	}
}
