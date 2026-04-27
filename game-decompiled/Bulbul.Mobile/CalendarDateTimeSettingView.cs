using System;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class CalendarDateTimeSettingView : MonoBehaviour
{
	[SerializeField]
	private CalenderCoreUI _calendar;

	[SerializeField]
	private GameObject _raycastBlocker;

	public Observable<SelectCalendarDayUI> OnSelectedDay => _calendar.OnSelectedDay;

	public void Setup()
	{
		_calendar.Setup(isValidWorkCheck: false, resetOnDisable: true);
		base.gameObject.SetActive(value: false);
		ObservableSubscribeExtensions.Subscribe(_calendar.OnClickOutside, delegate
		{
			Deactivate();
		}).AddTo(this);
		_calendar.OnSelectedDay.Subscribe(delegate(SelectCalendarDayUI _)
		{
			Deactivate();
			_.OnSelectOtherButton();
		}).AddTo(this);
	}

	public void Open(DateTime? date = null)
	{
		DateTime? selectingDate = date;
		if (!date.HasValue)
		{
			selectingDate = null;
			date = DateTime.Now;
		}
		_calendar.ViewEditorMode(date.Value.Year, date.Value.Month, selectingDate);
		Activate();
	}

	private void Activate()
	{
		base.gameObject.SetActive(value: true);
		_raycastBlocker.SetActive(value: false);
	}

	private void Deactivate()
	{
		_raycastBlocker.SetActive(value: true);
		base.gameObject.SetActive(value: false);
	}
}
