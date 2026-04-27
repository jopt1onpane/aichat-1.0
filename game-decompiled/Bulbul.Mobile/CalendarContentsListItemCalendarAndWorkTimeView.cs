using System;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class CalendarContentsListItemCalendarAndWorkTimeView : MonoBehaviour
{
	[SerializeField]
	private CalenderCoreUI _calendarCoreUI;

	[SerializeField]
	private CalendarContentWorkTimeView _workTimeView;

	[SerializeField]
	private RectTransform _calendarRectTransform;

	private bool isInitialized;

	public CalenderCoreUI CalendarCoreUI => _calendarCoreUI;

	public CalendarContentWorkTimeView WorkTimeView => _workTimeView;

	public void UpdateView(CalendarAndWorkViewModel model)
	{
		if (!isInitialized)
		{
			_calendarCoreUI.Setup(isValidWorkCheck: true, resetOnDisable: true);
			_calendarCoreUI.OnSelectedDay.Subscribe(delegate(SelectCalendarDayUI ui)
			{
				ui.OnSelectDay();
			}).AddTo(this);
			isInitialized = true;
		}
		if (model.CalendarMonthlyData != null)
		{
			_calendarCoreUI.View(model.CalendarMonthlyData, new DateTime(model.SelectedYear, model.SelectedMonth, model.SelectedDay));
			_workTimeView.SetWorkTimeText(model.WorkTime);
		}
	}

	public bool CheckCalendarInsideViewport(RectTransform viewport)
	{
		RectTransform calendarRectTransform = _calendarRectTransform;
		Vector3 position = calendarRectTransform.position;
		Vector3 position2 = viewport.position;
		Vector2 vector = new Vector2(calendarRectTransform.rect.width * calendarRectTransform.lossyScale.x, calendarRectTransform.rect.height * calendarRectTransform.lossyScale.y);
		Vector2 vector2 = new Vector2(viewport.rect.width * viewport.lossyScale.x, viewport.rect.height * viewport.lossyScale.y);
		float num = position2.y + vector2.y * (1f - viewport.pivot.y);
		float num2 = position2.y - vector2.y * viewport.pivot.y;
		float num3 = position.y + vector.y * (1f - calendarRectTransform.pivot.y);
		if (position.y - vector.y * calendarRectTransform.pivot.y > num || num3 < num2)
		{
			return false;
		}
		return true;
	}
}
