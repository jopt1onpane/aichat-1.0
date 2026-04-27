using System;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class AutoTimeWindowViewChangerForMobile : MonoBehaviour, IAutoTimeWindowViewChanger
{
	[Inject]
	private DateService _dateService;

	[Inject]
	private WindowViewService _windowViewService;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[SerializeField]
	private AutoTimeWindowView _autoTimeWindowView;

	private bool _isPossibleUseAutoChanger = true;

	public ReactiveProperty<bool> IsActive = new ReactiveProperty<bool>();

	public void Setup()
	{
		_autoTimeWindowView.Setup();
		IsActive.Value = false;
		AutoTimeWindowChangeData saveData = SaveDataManager.Instance.AutoTimeWindowChangeData;
		saveData.IsActiveAuto.Subscribe(delegate(bool active)
		{
			if (active && _isPossibleUseAutoChanger)
			{
				ApplyTimeOfDayFromCurrentTime();
			}
			else
			{
				RestoreWindowViewFromSaveData();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_autoTimeWindowView.OnOutsideClick, delegate
		{
			IsActive.Value = false;
			_autoTimeWindowView.DeactivateUI();
		}).AddTo(this);
		_autoTimeWindowView.OnSave.Subscribe(delegate(AutoTimeWindowSettings data)
		{
			SaveAutoTimeWindowChangeData(data);
		}).AddTo(this);
		_dateService.OnChangeTime.Subscribe(delegate(DateTime dateTime)
		{
			if (saveData.IsActiveAuto.Value && _isPossibleUseAutoChanger)
			{
				float timeHours = (float)dateTime.Hour + (float)dateTime.Minute / 60f + (float)dateTime.Second / 3600f;
				WindowViewType windowViewTypeFromTime = _autoTimeWindowView.Settings.GetWindowViewTypeFromTime(timeHours);
				if (!TryGetCurrentTimeOfDayType(out var viewType) || viewType != windowViewTypeFromTime)
				{
					_windowViewService.ChangeWeatherAndTime(windowViewTypeFromTime);
				}
			}
		}).AddTo(this);
		bool TryGetCurrentTimeOfDayType(out WindowViewType viewType)
		{
			if (_windowViewService.IsActiveWindow(WindowViewType.Day))
			{
				viewType = WindowViewType.Day;
				return true;
			}
			if (_windowViewService.IsActiveWindow(WindowViewType.Sunset))
			{
				viewType = WindowViewType.Sunset;
				return true;
			}
			if (_windowViewService.IsActiveWindow(WindowViewType.Night))
			{
				viewType = WindowViewType.Night;
				return true;
			}
			viewType = WindowViewType.Fireworks;
			return false;
		}
	}

	private void SaveAutoTimeWindowChangeData(AutoTimeWindowSettings settings)
	{
		AutoTimeWindowChangeData autoTimeWindowChangeData = SaveDataManager.Instance.AutoTimeWindowChangeData;
		autoTimeWindowChangeData.TimeDayStart = settings.TimeDayStart;
		autoTimeWindowChangeData.TimeSunsetStart = settings.TimeSunsetStart;
		autoTimeWindowChangeData.TimeNightStart = settings.TimeNightStart;
		SaveDataManager.Instance.SaveAutoTimeWindowChangeData();
	}

	public void ActivateSetting()
	{
		IsActive.Value = true;
		_autoTimeWindowView.ActivateUI();
	}

	public void ApplyTimeOfDayFromCurrentTime()
	{
		if (SaveDataManager.Instance.AutoTimeWindowChangeData.IsActiveAuto.Value)
		{
			_autoTimeWindowView.RefreshFromSaveData();
			DateTime now = DateTime.Now;
			float timeHours = (float)now.Hour + (float)now.Minute / 60f + (float)now.Second / 3600f;
			WindowViewType windowViewTypeFromTime = _autoTimeWindowView.Settings.GetWindowViewTypeFromTime(timeHours);
			_windowViewService.ChangeWeatherAndTime(windowViewTypeFromTime);
		}
	}

	void IAutoTimeWindowViewChanger.SetPossibleUseAutoChanger(bool possible)
	{
		_isPossibleUseAutoChanger = possible;
	}

	private void RestoreWindowViewFromSaveData()
	{
		WindowViewType[] array = new WindowViewType[4]
		{
			WindowViewType.Day,
			WindowViewType.Sunset,
			WindowViewType.Night,
			WindowViewType.Cloudy
		};
		foreach (WindowViewType windowViewType in array)
		{
			if (_environmentDataService.IsWindowActive(windowViewType))
			{
				_windowViewService.ChangeWeatherAndTime(windowViewType);
				return;
			}
		}
		_windowViewService.ChangeWeatherAndTime(WindowViewType.Day);
	}
}
