using System;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class CalendarBase : MonoBehaviour
{
	[Inject]
	protected SystemSeService _systemSeService;

	[Inject]
	protected CalendarUI _calendarUI;

	protected DateTime SelectingDate;

	protected int CurrentYear => _calendarUI.CalenderCoreUI.Year;

	protected int CurrentMonth => _calendarUI.CalenderCoreUI.Month;

	public void BaseSetup()
	{
		_calendarUI.Setup(isValidWorkCheck: true, resetOnDisable: false);
		SelectingDate = DateTime.Now;
		_calendarUI.ChangeShowMonth(SelectingDate.Year, SelectingDate.Month);
	}

	public bool IsActive()
	{
		if (_calendarUI.IsActive())
		{
			return true;
		}
		return false;
	}

	public void Activate()
	{
		_calendarUI.ChangeShowMonth(CurrentYear, CurrentMonth);
		_calendarUI.Activate(SelectingDate);
	}

	public void Deactivate()
	{
		_calendarUI.Deactivate();
	}
}
