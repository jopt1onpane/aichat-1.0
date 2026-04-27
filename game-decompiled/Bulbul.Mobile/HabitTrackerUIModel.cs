using System;
using System.Collections.Generic;
using R3;

namespace Bulbul.Mobile;

public class HabitTrackerUIModel : IDisposable
{
	private HabitDataService _habitDataService;

	private Subject<bool> onChangedWeek = new Subject<bool>();

	private Subject<bool> onChangeRemoveMode = new Subject<bool>();

	private Subject<bool> onChangeSettingMode = new Subject<bool>();

	private Subject<(string uuid, DayOfWeek date, bool isEnable)> onChangeDirtyHabitDayEnable = new Subject<(string, DayOfWeek, bool)>();

	private Subject<bool> onChangeHabitDayOfWeekEnableComplete = new Subject<bool>();

	private DateTime _currentStartDate;

	private bool _isRemoveMode;

	private bool _isSettingMode;

	private Dictionary<(string uuid, DayOfWeek date), bool> _dirtyHabitDayEnable = new Dictionary<(string, DayOfWeek), bool>();

	public Observable<bool> OnChangedWeek => onChangedWeek;

	public Observable<bool> OnChangeRemoveMode => onChangeRemoveMode;

	public Observable<bool> OnChangeSettingMode => onChangeSettingMode;

	public Observable<string> OnAddHabit => _habitDataService.OnAddHabit;

	public Observable<string> OnRemoveHabit => _habitDataService.OnRemoveHabit;

	public Observable<string> OnChangeHabitPeriod => _habitDataService.OnChangeHabitPeriod;

	public Observable<(string uuid, string title)> OnChangeHabitTitle => _habitDataService.OnChangeHabitTitle;

	public Observable<(string uuid, DateTime date, bool isCompleted)> OnChangeHabitComplete => _habitDataService.OnChangeHabitCompleted;

	public Observable<(string uuid, DayOfWeek date, bool isEnable)> OnChangeDirtyHabitDayEnable => onChangeDirtyHabitDayEnable;

	public Observable<bool> OnChangeHabitDayOfWeekEnableComplete => onChangeHabitDayOfWeekEnableComplete;

	private DateTime StartDate => _habitDataService.GetStartDateOfWeek(DateTime.Today);

	public DateTime CurrentStartDate => _currentStartDate;

	public bool IsRemoveMode => _isRemoveMode;

	public bool IsSettingMode => _isSettingMode;

	public IReadOnlyDictionary<(string uuid, DayOfWeek date), bool> DirtyHabitDayEnable => _dirtyHabitDayEnable;

	public void EnterSetting()
	{
		if (_habitDataService == null)
		{
			_habitDataService = RoomLifetimeScope.Resolve<HabitDataService>();
		}
		_currentStartDate = StartDate;
		_isRemoveMode = false;
		_isSettingMode = false;
		_dirtyHabitDayEnable.Clear();
	}

	public void ChangeWeek(bool isNext)
	{
		_currentStartDate = (isNext ? _currentStartDate.AddDays(7.0) : _currentStartDate.AddDays(-7.0));
		onChangedWeek?.OnNext(isNext);
	}

	public void ChangeRemoveMode(bool isRemoveMode)
	{
		if (_isRemoveMode != isRemoveMode)
		{
			_isRemoveMode = isRemoveMode;
			onChangeRemoveMode?.OnNext(_isRemoveMode);
		}
	}

	public void ChangeSettingMode(bool isSettingMode)
	{
		_isSettingMode = isSettingMode;
		onChangeSettingMode?.OnNext(_isSettingMode);
	}

	public void HabitReorder(string from, string to)
	{
		_habitDataService.ReorderHabit(from, to);
	}

	public void AddHabit()
	{
		_habitDataService.CreateHabit();
	}

	public void RemoveHabit(string uuid)
	{
		_habitDataService.DeleteHabit(uuid);
	}

	public void ChangeHabitPeriod(string uuid, bool enable)
	{
		_habitDataService.SetHabitPeriodAlive(uuid, enable);
	}

	public void SetHabitTitle(string uuid, string title)
	{
		_habitDataService.SetHabitTitle(uuid, title);
	}

	public void SetHabitDayCompleted(string uuid, DateTime date, bool isCompleted)
	{
		_habitDataService.SetHabitDayCompleted(uuid, date, isCompleted);
	}

	public void SetDirtyHabitDayEnabled(string uuid, DayOfWeek date, bool enabled)
	{
		if (!_dirtyHabitDayEnable.ContainsKey((uuid, date)))
		{
			_dirtyHabitDayEnable.Add((uuid, date), enabled);
		}
		else
		{
			_dirtyHabitDayEnable[(uuid, date)] = enabled;
		}
		onChangeDirtyHabitDayEnable?.OnNext((uuid, date, enabled));
	}

	public void FinishHabitDayEnabledSetting(bool isApply)
	{
		if (isApply && _dirtyHabitDayEnable.Count > 0)
		{
			ApplyHabitDayEnabled();
		}
		else
		{
			onChangeHabitDayOfWeekEnableComplete?.OnNext(value: false);
		}
		_dirtyHabitDayEnable.Clear();
	}

	private void ApplyHabitDayEnabled()
	{
		foreach (KeyValuePair<(string, DayOfWeek), bool> item in _dirtyHabitDayEnable)
		{
			if (!string.IsNullOrEmpty(item.Key.Item1))
			{
				SetHabitDayOfWeekEnabled(item.Key.Item1, item.Key.Item2, item.Value);
			}
		}
		onChangeHabitDayOfWeekEnableComplete?.OnNext(value: true);
	}

	private void SetHabitDayOfWeekEnabled(string uuid, DayOfWeek date, bool enabled)
	{
		_habitDataService.SetHabitDayOfWeekEnabled(uuid, date, enabled);
	}

	public IEnumerable<string> GetAllHabitIds(bool includeDeleted)
	{
		return _habitDataService.GetAllHabitIds(includeDeleted);
	}

	public string GetHabitTitle(string uuid)
	{
		return _habitDataService.GetHabitTitle(uuid);
	}

	public HabitDateEnableState GetHabitDayEnableState(string uuid, DateTime date, bool includeDirty = false)
	{
		if (includeDirty && _dirtyHabitDayEnable.ContainsKey((uuid, date.DayOfWeek)))
		{
			if (!_dirtyHabitDayEnable[(uuid, date.DayOfWeek)])
			{
				return HabitDateEnableState.Disabled;
			}
			return HabitDateEnableState.Enabled;
		}
		return _habitDataService.GetHabitDayEnableState(uuid, date);
	}

	public bool IsAllCompletedForDate(DateTime date)
	{
		return _habitDataService.IsAllCompletedForDate(date);
	}

	public bool IsDateInAlivePeriod(string uuid, DateTime date)
	{
		return _habitDataService.IsDateInAlivePeriod(uuid, date);
	}

	public bool IsHabitDayCompleted(string uuid, DateTime date)
	{
		return _habitDataService.IsHabitDayCompleted(uuid, date);
	}

	public bool IsHabitDayOfWeekEnabled(string uuid, DayOfWeek week)
	{
		return _habitDataService.IsHabitDayOfWeekEnabled(uuid, week);
	}

	public void Dispose()
	{
		onChangedWeek?.Dispose();
		onChangeRemoveMode?.Dispose();
		onChangeSettingMode?.Dispose();
	}
}
