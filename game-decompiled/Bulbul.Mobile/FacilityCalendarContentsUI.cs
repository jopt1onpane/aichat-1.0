using System;
using System.Collections.Generic;
using System.Linq;
using NestopiSystem;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityCalendarContentsUI : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private HabitDataService _habitDataService;

	[SerializeField]
	private CalendarContentsUIView _calendarContentsUIView;

	private int _lastSelectedYear;

	private int _lastSelectedMonth;

	private int _lastSelectedDay;

	private int _lastOpenedYear;

	private int _lastOpenedMonth;

	private Subject<Unit> _onAchievementNoticeOnEndEditDiary = new Subject<Unit>();

	public Observable<Unit> OnAchievementNoticeOnEndEditDiary => _onAchievementNoticeOnEndEditDiary;

	public void Setup()
	{
		SetLastSelectedDateTime(DateTime.Now);
		_lastOpenedYear = _lastSelectedYear;
		_lastOpenedMonth = _lastSelectedMonth;
		_calendarContentsUIView.Setup(_lastSelectedYear, _lastSelectedMonth, _lastSelectedDay);
		_calendarContentsUIView.OnPrepareContents.Subscribe(delegate(bool fromTab)
		{
			if (fromTab)
			{
				EnterCalendarContents(isPullDownForceOpen: false);
			}
			else
			{
				EnterCalendarContents(isPullDownForceOpen: true);
			}
		}).AddTo(this);
		_calendarContentsUIView.ListView.OnEndEditDiary.Subscribe(delegate(string str)
		{
			SaveDataManager.Instance.CalendarData.GetMonthlyData(_lastSelectedYear, _lastSelectedMonth).GetDateData(_lastSelectedDay).DiaryText = str;
			SaveDataManager.Instance.CalendarData.SaveDateData(new DateTime(_lastOpenedYear, _lastSelectedMonth, _lastSelectedDay));
			_onAchievementNoticeOnEndEditDiary.OnNext(Unit.Default);
		}).AddTo(this);
		_calendarContentsUIView.OnChangedWorkTime.Subscribe(delegate(double work)
		{
			CalendarDateData dateData = SaveDataManager.Instance.CalendarData.GetMonthlyData(_lastSelectedYear, _lastSelectedMonth).GetDateData(_lastSelectedDay);
			dateData.WorkTimeSeconds = work;
			SaveDataManager.Instance.CalendarData.SaveDateData(new DateTime(_lastOpenedYear, _lastSelectedMonth, _lastSelectedDay));
			_calendarContentsUIView.ListView.UpdateWorkTime(dateData.WorkTime);
		}).AddTo(this);
		_calendarContentsUIView.ListView.OnSelectedDay.Subscribe(delegate(DateTime dateTime)
		{
			_systemSeService.PlayClick();
			SetLastSelectedDateTime(dateTime);
			CalendarDateData dateData = SaveDataManager.Instance.CalendarData.GetMonthlyData(_lastSelectedYear, _lastSelectedMonth).GetDateData(_lastSelectedDay);
			List<TodoData> list = dateData.CompleteTodoListDic.Select((KeyValuePair<ulong, TodoData> _) => _.Value).ToList();
			list.Sort(SortCompletedTodoTask);
			DateTime dateTime2 = new DateTime(_lastSelectedYear, _lastSelectedMonth, _lastSelectedDay);
			List<CalendarContentsListBaseModel> todos = _calendarContentsUIView.ListView.CreateModelList(list, _habitDataService.GetCompletedHabitsForCalendar(dateTime2), dateTime2, GetHabitTrackerTitle);
			_calendarContentsUIView.ListView.UpdateUIFromSelectedDay(_lastSelectedYear, _lastSelectedMonth, _lastSelectedDay, dateData.WorkTime, dateData.DiaryText, todos);
		}).AddTo(this);
		_calendarContentsUIView.ListView.OnValueChangedMonth.Subscribe(delegate((int, int) date)
		{
			int item = date.Item1;
			int item2 = date.Item2;
			_lastOpenedYear = item;
			_lastOpenedMonth = item2;
			CalenderMonthlyData monthlyData = SaveDataManager.Instance.CalendarData.GetMonthlyData(_lastOpenedYear, _lastOpenedMonth);
			_calendarContentsUIView.ListView.SetMonthlyDataOnlyWithoutUpdateUI(monthlyData);
		}).AddTo(this);
		_calendarContentsUIView.ListView.OnRemovedTodo.Subscribe(delegate(ulong uniqueID)
		{
			_systemSeService.PlayCancel();
			DateTime dateTime = new DateTime(_lastSelectedYear, _lastSelectedMonth, _lastSelectedDay);
			SaveDataManager.Instance.CalendarData.GetDailyData(dateTime).CompleteTodoListDic.Remove(uniqueID);
			SaveDataManager.Instance.CalendarData.SaveDateData(dateTime);
		}).AddTo(this);
		_calendarContentsUIView.ListView.OnChangedTodoTitle.Subscribe(delegate((ulong, string) data)
		{
			var (num, text) = data;
			if (SaveDataManager.Instance.CalendarData.GetDailyData(_lastSelectedYear, _lastSelectedMonth, _lastSelectedDay).CompleteTodoListDic.TryGetValue(num, out var value))
			{
				value.TodoText = text;
				SaveDataManager.Instance.CalendarData.SaveDateData(new DateTime(_lastSelectedYear, _lastSelectedMonth, _lastSelectedDay));
				SaveTodoTask(num, text, null);
			}
		}).AddTo(this);
		_calendarContentsUIView.OnChangedTodoCompletedDateTime.Subscribe(delegate((ulong, DateTime) data)
		{
			_systemSeService.PlayClick();
			ulong item = data.Item1;
			DateTime item2 = data.Item2;
			CalendarData calendarData = SaveDataManager.Instance.CalendarData;
			CalenderMonthlyData monthlyData = calendarData.GetMonthlyData(_lastOpenedYear, _lastOpenedMonth);
			CalendarDateData dateData = monthlyData.GetDateData(_lastSelectedDay);
			TodoData todoData = dateData.CompleteTodoListDic[item];
			if (!todoData.Completed.Value.IsSameDay(item2))
			{
				CalenderMonthlyData monthlyData2 = calendarData.GetMonthlyData(item2);
				CalendarDateData dateData2 = monthlyData2.GetDateData(item2.Day);
				dateData2.CompleteTodoListDic.TryGetValue(item, out var value);
				if (value == null)
				{
					value = todoData.DeepCopy();
				}
				value?.SetCompleteTodoDatetime(item2);
				dateData.CompleteTodoListDic.Remove(item);
				dateData2.CompleteTodoListDic.TryAdd(item, value);
				SaveDataManager.Instance.SaveCalenderData(monthlyData, monthlyData2);
				SaveTodoTask(item, null, item2);
				_calendarContentsUIView.ListView.RemoveCompletedTodoTaskWithAnimation(item);
				_calendarContentsUIView.ListView.RefleshCalendarView(monthlyData);
			}
		}).AddTo(this);
		_calendarContentsUIView.ListView.OnRemovedHabitTrackerTask.Subscribe(delegate(string habitID)
		{
			_systemSeService.PlayCancel();
			_habitDataService.SetHideOnCalendar(habitID, new DateTime(_lastSelectedYear, _lastSelectedMonth, _lastSelectedDay), hide: true);
		}).AddTo(this);
		_calendarContentsUIView.ListView.OnChangedHabitTrackerTaskTitle.Subscribe(delegate((string, string) data)
		{
			var (habitId, title) = data;
			_habitDataService.SetHabitTitle(habitId, title);
		}).AddTo(this);
	}

	private int SortCompletedTodoTask(TodoData a, TodoData b)
	{
		DateTime? completed = a.Completed;
		DateTime? completed2 = b.Completed;
		if (!completed.HasValue && !completed2.HasValue)
		{
			return 0;
		}
		if (!completed.HasValue)
		{
			return 1;
		}
		if (!completed2.HasValue)
		{
			return -1;
		}
		int num = completed.Value.CompareTo(completed2.Value);
		if (num < 0)
		{
			return 1;
		}
		if (num > 0)
		{
			return -1;
		}
		return 0;
	}

	public void OnClickCalendarButton()
	{
		_calendarContentsUIView.ResetScroll();
	}

	private void EnterCalendarContents(bool isPullDownForceOpen)
	{
		CalendarData calendarData = SaveDataManager.Instance.CalendarData;
		CalendarDateData dateData = calendarData.GetMonthlyData(_lastSelectedYear, _lastSelectedMonth).GetDateData(_lastSelectedDay);
		List<TodoData> list = dateData.CompleteTodoListDic.Select((KeyValuePair<ulong, TodoData> _) => _.Value).ToList();
		list.Sort(SortCompletedTodoTask);
		DateTime dateTime = new DateTime(_lastSelectedYear, _lastSelectedMonth, _lastSelectedDay);
		List<CalendarContentsListBaseModel> todos = _calendarContentsUIView.ListView.CreateModelList(list, _habitDataService.GetCompletedHabitsForCalendar(dateTime), dateTime, GetHabitTrackerTitle);
		CalenderMonthlyData monthlyData = calendarData.GetMonthlyData(_lastOpenedYear, _lastOpenedMonth);
		_calendarContentsUIView.ListView.EnterSettingCalendarContentsView(monthlyData, _lastSelectedYear, _lastSelectedMonth, _lastSelectedDay, dateData.WorkTime, dateData.DiaryText, todos, isPullDownForceOpen);
	}

	private void SetLastSelectedDateTime(DateTime datetime)
	{
		_lastSelectedYear = datetime.Year;
		_lastSelectedMonth = datetime.Month;
		_lastSelectedDay = datetime.Day;
	}

	private void SaveTodoTask(ulong uniqueId, string text, DateTime? dateTime)
	{
		TodoListData todoListData;
		TodoData todoData = SaveDataManager.Instance.TodoAllData.GetTodoData(uniqueId, out todoListData);
		bool flag = false;
		if (text != null)
		{
			todoData.TodoText = text;
			flag = true;
		}
		if (dateTime.HasValue)
		{
			todoData.SetCompleteTodoDatetime(dateTime);
			flag = true;
		}
		if (flag)
		{
			SaveDataManager.Instance.SaveTodoList(todoListData);
		}
	}

	private string GetHabitTrackerTitle(string habitID)
	{
		return _habitDataService.GetHabitTitle(habitID);
	}
}
