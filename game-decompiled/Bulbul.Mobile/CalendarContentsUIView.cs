using System;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class CalendarContentsUIView : MonoBehaviour, ITabChangedSetuper
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private CalendarContentsListView _listView;

	[SerializeField]
	private CalendarDateTimeSettingView _calendarDateTimeSettingView;

	[SerializeField]
	private CalendarSimpleDateView _simpleDateView;

	[SerializeField]
	private WorkTimeEditorDialogView _workTimeEditorDialogView;

	private Subject<(ulong, DateTime)> _onChangedTodoCompletedDateTime = new Subject<(ulong, DateTime)>();

	private Subject<bool> _onPrepareContents = new Subject<bool>();

	private Subject<double> _onChangedWorkTime = new Subject<double>();

	private ulong _edittingCompletedDateTimeTodoUniqueID;

	public CalendarContentsListView ListView => _listView;

	public Observable<(ulong, DateTime)> OnChangedTodoCompletedDateTime => _onChangedTodoCompletedDateTime;

	public Observable<bool> OnPrepareContents => _onPrepareContents;

	public Observable<double> OnChangedWorkTime => _onChangedWorkTime;

	void ITabChangedSetuper.SetupBeforeTabChanged(bool isChangedFromTab)
	{
		_onPrepareContents.OnNext(isChangedFromTab);
	}

	private void OnDestroy()
	{
		_onPrepareContents.Dispose();
		_onPrepareContents = null;
	}

	public void Setup(int firstSelectedYear, int firstSelectedMonth, int firstSelectedDay)
	{
		_calendarDateTimeSettingView.Setup();
		_workTimeEditorDialogView.Setup();
		_simpleDateView.DeactivateImmidiate();
		_listView.OnChangedActiveCalendar.Subscribe(delegate(bool flag)
		{
			if (flag)
			{
				_simpleDateView.Deactivate();
			}
			else
			{
				_simpleDateView.Activate();
			}
		}).AddTo(this);
		_simpleDateView.SetDateText(firstSelectedYear, firstSelectedMonth, firstSelectedDay);
		ObservableSubscribeExtensions.Subscribe(_simpleDateView.OnClickbackButton, delegate
		{
			_systemSeService.PlayClick();
			_listView.MovePrevSelectedDay();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_simpleDateView.OnClickNextButton, delegate
		{
			_systemSeService.PlayClick();
			_listView.MoveNextSelectedDay();
		}).AddTo(this);
		_listView.OnSelectedDay.Subscribe(delegate(DateTime dateTime)
		{
			_simpleDateView.SetDateText(dateTime.Year, dateTime.Month, dateTime.Day);
		}).AddTo(this);
		_listView.OnSettingCompletedDateTime.Subscribe(delegate((ulong uuid, DateTime? datetime) settingData)
		{
			_systemSeService.PlaySelect();
			(_edittingCompletedDateTimeTodoUniqueID, _) = settingData;
			_calendarDateTimeSettingView.Open(settingData.datetime);
		}).AddTo(this);
		_calendarDateTimeSettingView.OnSelectedDay.Subscribe(delegate(SelectCalendarDayUI day)
		{
			_onChangedTodoCompletedDateTime.OnNext((_edittingCompletedDateTimeTodoUniqueID, day.DateTime));
		}).AddTo(this);
		_listView.OnSettingWorkTime.Subscribe(delegate(TimeSpan workTime)
		{
			if (workTime.TotalHours >= 24.0)
			{
				_workTimeEditorDialogView.Open(24, 0, 0);
			}
			else
			{
				_workTimeEditorDialogView.Open(workTime.Hours, workTime.Minutes, workTime.Seconds);
			}
		}).AddTo(this);
		_workTimeEditorDialogView.OnSubmit.Subscribe(delegate(double workTime)
		{
			_onChangedWorkTime.OnNext(workTime);
		}).AddTo(this);
	}

	public void RefreshCalendar(CalenderMonthlyData calenderMonthlyData)
	{
	}

	public void ResetScroll()
	{
		_listView.ResetScroll();
	}
}
