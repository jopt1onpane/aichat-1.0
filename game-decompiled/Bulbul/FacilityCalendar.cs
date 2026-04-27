using System;
using R3;
using TMPro;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilityCalendar : CalendarBase
{
	private enum MainState
	{
		Idle
	}

	private MainState _mainState;

	private const string CalendarFileName = "calendar.es3";

	[SerializeField]
	[Header("日記のInputField")]
	private TMP_InputField _diaryInputField;

	private bool _isFinishedSetup;

	[Inject]
	private FacilityTodo facilityTodo;

	private Subject<Unit> _onEndEditDiary = new Subject<Unit>();

	public Observable<Unit> OnEndEditDiaryEvent => _onEndEditDiary;

	public void Setup()
	{
		BaseSetup();
		_calendarUI.CalenderCoreUI.OnSelectedDay.Subscribe(OnClickButtonSelectDay).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_calendarUI.OnClickCloseButton, delegate
		{
			_systemSeService.PlayCancel();
			Deactivate();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_diaryInputField.onEndEdit.AsObservable(), delegate
		{
			OnEndEditDiary();
		}).AddTo(this);
		int day = SelectingDate.Day;
		SelectCalendarDayUI[] selectCalendarDayUIs = _calendarUI.SelectCalendarDayUIs;
		foreach (SelectCalendarDayUI selectCalendarDayUI in selectCalendarDayUIs)
		{
			if (selectCalendarDayUI.Day == day)
			{
				selectCalendarDayUI.OnClickButtonSelectDay();
			}
		}
		_isFinishedSetup = true;
		Deactivate();
	}

	private void Start()
	{
		facilityTodo.OnCompleteTodo.Where(delegate(TodoData x)
		{
			DateTime? completed = x.Completed;
			if (completed.HasValue)
			{
				DateTime valueOrDefault = completed.GetValueOrDefault();
				if (valueOrDefault.Day == SelectingDate.Day && valueOrDefault.Month == SelectingDate.Month)
				{
					return valueOrDefault.Year == SelectingDate.Year;
				}
			}
			return false;
		}).Subscribe(delegate(TodoData t)
		{
			_calendarUI.AddTodo(t, OnChangeEndCompleteTodoText, OnClickButtonDeleteTask);
		}).AddTo(this);
		facilityTodo.OnUncompleteTodo.Subscribe(delegate(TodoData t)
		{
			_calendarUI.RemoveTodo(t);
		}).AddTo(this);
		facilityTodo.OnChangeCompleteDayTodo.Subscribe(delegate((TodoData, DateTime) todoChangeData)
		{
			TodoData item = todoChangeData.Item1;
			DateTime item2 = todoChangeData.Item2;
			ulong uniqueID = item.UniqueID;
			CalendarData calendarData = SaveDataManager.Instance.CalendarData;
			DateTime value = item.Completed.Value;
			CalenderMonthlyData monthlyData = calendarData.GetMonthlyData(value);
			CalenderMonthlyData monthlyData2 = calendarData.GetMonthlyData(item2);
			CalendarDateData dateData = monthlyData.GetDateData(value.Day);
			dateData.CompleteTodoListDic.TryGetValue(uniqueID, out var value2);
			if (value2 == null)
			{
				value2 = item.DeepCopy();
			}
			CalendarDateData dateData2 = monthlyData2.GetDateData(item2.Day);
			value2?.SetCompleteTodoDatetime(item2);
			dateData.CompleteTodoListDic.Remove(uniqueID);
			dateData2.CompleteTodoListDic.TryAdd(uniqueID, value2);
			SaveDataManager.Instance.SaveCalenderData(monthlyData, monthlyData2);
			_calendarUI.RemoveTodo(item);
			if (_calendarUI.IsSelectedDay(item2))
			{
				_calendarUI.AddTodo(value2, OnChangeEndCompleteTodoText, OnClickButtonDeleteTask);
			}
			_calendarUI.CalenderCoreUI.UpdateWorkedUI(value);
			_calendarUI.CalenderCoreUI.UpdateWorkedUI(item2);
		}).AddTo(this);
	}

	public void UpdateFacility()
	{
		_ = _mainState;
	}

	public void OnClickButtonSelectDay(SelectCalendarDayUI selectCalendarDayUI)
	{
		SelectingDate = new DateTime(base.CurrentYear, base.CurrentMonth, selectCalendarDayUI.Day);
		_ = _isFinishedSetup;
		_calendarUI.OnClickButtonSelectDay(0uL, selectCalendarDayUI, OnChangeEndCompleteTodoText, OnClickButtonDeleteTask, _isFinishedSetup);
	}

	private void OnChangeEndCompleteTodoText(TodoData todoData, string inputText)
	{
		DateTime? dateTime = todoData?.Completed;
		if (dateTime.HasValue)
		{
			DateTime valueOrDefault = dateTime.GetValueOrDefault();
			if (SaveDataManager.Instance.CalendarData.GetDailyData(valueOrDefault).CompleteTodoListDic.TryGetValue(todoData.UniqueID, out var value))
			{
				value.TodoText = inputText;
				SaveDataManager.Instance.CalendarData.SaveDateData(SelectingDate);
			}
		}
	}

	public void OnClickButtonDeleteTask(TodoData todoData)
	{
		DateTime? dateTime = todoData?.Completed;
		if (dateTime.HasValue)
		{
			DateTime valueOrDefault = dateTime.GetValueOrDefault();
			_calendarUI.OnClickButtonDeleteTask(todoData);
			SaveDataManager.Instance.CalendarData.GetDailyData(valueOrDefault).CompleteTodoListDic.Remove(todoData.UniqueID);
			SaveDataManager.Instance.CalendarData.SaveDateData(SelectingDate);
			_systemSeService.PlayCancel();
		}
	}

	public void OnEndEditDiary()
	{
		_onEndEditDiary.OnNext(Unit.Default);
		SaveDataManager.Instance.CalendarData.GetDailyData(SelectingDate).DiaryText = _calendarUI.DiaryInputField.text;
		SaveDataManager.Instance.CalendarData.SaveDateData(SelectingDate);
	}
}
