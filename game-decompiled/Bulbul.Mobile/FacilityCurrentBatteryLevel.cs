using UnityEngine;

namespace Bulbul.Mobile;

public class FacilityCurrentBatteryLevel : MonoBehaviour
{
	[SerializeField]
	private CurrentBatteryLevelView[] _currentBatteryLevelViews;

	private int _currentBatteryLevel;

	private BatteryStatus _currentBatteryStatus;

	public void Setup()
	{
		_currentBatteryLevel = (int)(SystemInfo.batteryLevel * 100f);
		UpdateUIs();
	}

	public void UpdateFacility()
	{
		int num = (int)(SystemInfo.batteryLevel * 100f);
		BatteryStatus batteryStatus = SystemInfo.batteryStatus;
		if (_currentBatteryLevel != num || _currentBatteryStatus != batteryStatus)
		{
			_currentBatteryLevel = num;
			_currentBatteryStatus = batteryStatus;
			UpdateUIs();
		}
	}

	private void UpdateUIs()
	{
		CurrentBatteryLevelView[] currentBatteryLevelViews = _currentBatteryLevelViews;
		for (int i = 0; i < currentBatteryLevelViews.Length; i++)
		{
			currentBatteryLevelViews[i].UpdateUI(_currentBatteryLevel, CheckCharging(_currentBatteryStatus));
		}
	}

	private bool CheckCharging(BatteryStatus status)
	{
		if (status == BatteryStatus.Charging)
		{
			return true;
		}
		return false;
	}
}
