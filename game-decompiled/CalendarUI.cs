using System;
using System.Collections.Generic;
using System.Linq;
using Bulbul;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NestopiSystem;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class CalendarUI : MonoBehaviour
{
	private const int CalendarButtonMax = 42;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private ChangeOrderService _changeOrderService;

	[Inject]
	private HabitDataService _habitDataService;

	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[SerializeField]
	[Header("作業時間：時間")]
	private TMP_InputField _workTimeHour;

	[SerializeField]
	[Header("作業時間：分")]
	private TMP_InputField _workTimeMinutes;

	[SerializeField]
	[Header("作業時間：秒")]
	private TMP_InputField _workTimeSeconds;

	[SerializeField]
	[Header("完了Todoのプレハブ TodoUIを持つオブジェクトを設定")]
	private GameObject _completeTodoPrefab;

	[SerializeField]
	[Header("完了Todoの親オブジェクト")]
	private GameObject _completeTodoParent;

	[SerializeField]
	[Header("完了習慣のプレハブ")]
	private CalendarCompleteHabitUI _completeHabitPrefab;

	[SerializeField]
	[Header("完了習慣の親オブジェクト")]
	private GameObject _completeHabitParent;

	[SerializeField]
	[Header("日記")]
	private TMP_InputField _diaryInputField;

	[SerializeField]
	private CalenderCoreUI completeTimeEditCalender;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("閉じるボタン")]
	private Button _closeButton;

	private SelectCalendarDayUI _currentSelectDayUI;

	private List<TodoUI> _currentCompleteTodoUI = new List<TodoUI>();

	private TodoUI calenderTargetTodo;

	private readonly List<(string habitId, CalendarCompleteHabitUI ui)> _currentCompleteHabitUIs = new List<(string, CalendarCompleteHabitUI)>();

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _fromPosY;

	private float _toPosY;

	private bool _isActive;

	private Tween openTween;

	[field: SerializeField]
	[field: Header("日付\u3000年")]
	public CalenderCoreUI CalenderCoreUI { get; private set; }

	public TMP_InputField DiaryInputField => _diaryInputField;

	public SelectCalendarDayUI[] SelectCalendarDayUIs => CalenderCoreUI.SelectCalendarDayUIs;

	public Observable<Unit> OnClickCloseButton => _closeButton.OnClickAsObservable();

	private DateTime? CurrentSelectDay
	{
		get
		{
			if (!(_currentSelectDayUI != null))
			{
				return null;
			}
			return _currentSelectDayUI.DateTime;
		}
	}

	public void Setup(bool isValidWorkCheck, bool resetOnDisable)
	{
		CalenderCoreUI.Setup(isValidWorkCheck, resetOnDisable);
		completeTimeEditCalender.Setup(isValidWorkCheck: false, resetOnDisable: true);
		ObservableSubscribeExtensions.Subscribe(completeTimeEditCalender.OnClickOutside.ObserveOn(UnityFrameProvider.PreLateUpdate), delegate
		{
			if (!(calenderTargetTodo == null))
			{
				CloseCompleteTimeEditCalender();
			}
		}).AddTo(this);
		completeTimeEditCalender.OnSelectedDay.Subscribe(delegate(SelectCalendarDayUI ui)
		{
			if (!(calenderTargetTodo == null))
			{
				TodoData todoData = calenderTargetTodo.TodoData;
				DateTime? completed = todoData.Completed;
				calenderTargetTodo.SetComplete(ui.DateTime);
				CalenderMonthlyData calenderMonthlyData = null;
				if (completed.HasValue)
				{
					calenderMonthlyData = SaveDataManager.Instance.CalendarData.GetMonthlyData(completed.Value);
					if (calenderMonthlyData.DiaryList.TryGetValue(completed.Value.Day, out var value))
					{
						value.CompleteTodoListDic.Remove(todoData.UniqueID);
					}
					SaveDataManager.Instance.CalendarData.SaveDateData(completed.Value);
					CalenderCoreUI.UpdateWorkedUI(completed.Value);
				}
				CalenderMonthlyData monthlyData = SaveDataManager.Instance.CalendarData.GetMonthlyData(ui.DateTime);
				monthlyData.GetDateData(ui.DateTime.Day).CompleteTodoListDic.TryAdd(todoData.UniqueID, todoData);
				SaveDataManager.Instance.SaveCalenderData(monthlyData, calenderMonthlyData);
				TodoListData todoListData;
				TodoData todoData2 = SaveDataManager.Instance.TodoAllData.GetTodoData(todoData.UniqueID, out todoListData);
				if (todoData2 != null)
				{
					todoData2.SetCompleteTodoDatetime(ui.DateTime);
					SaveDataManager.Instance.SaveTodoList(todoListData);
					SaveDataManager.Instance.CalendarData.SaveDateData(ui.DateTime);
				}
				CalenderCoreUI.UpdateWorkedUI(ui.DateTime);
				if (completed != todoData.Completed)
				{
					_currentCompleteTodoUI.Remove(calenderTargetTodo);
					calenderTargetTodo.Destroy();
					calenderTargetTodo = null;
				}
				CloseCompleteTimeEditCalender();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pomodoroService.OnCompletePomodoro, delegate
		{
			CalenderCoreUI.UpdateWorkedUI(DateTime.Now);
			if (_currentSelectDayUI?.DateTime == DateTime.Today)
			{
				CalendarDateData dailyData = SaveDataManager.Instance.CalendarData.GetDailyData(DateTime.Now);
				UpdateWorkTimeView(dailyData.WorkTime);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(completeTimeEditCalender.OnDisableAsObservable(), delegate
		{
			calenderTargetTodo = null;
		}).AddTo(this);
		_workTimeHour.DisableIME();
		_workTimeMinutes.DisableIME();
		_workTimeSeconds.DisableIME();
		ObservableSubscribeExtensions.Subscribe(Observable.Merge<string>(_workTimeHour.OnEndEditAsObservable(), _workTimeMinutes.OnEndEditAsObservable(), _workTimeSeconds.OnEndEditAsObservable()), delegate
		{
			if (_currentSelectDayUI == null)
			{
				Debug.LogError("CalendarUI: 選択している日付がNullです。");
			}
			else
			{
				CalendarDateData dailyData = SaveDataManager.Instance.CalendarData.GetDailyData(_currentSelectDayUI.DateTime);
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				if (_workTimeHour.text == string.Empty)
				{
					num = dailyData.WorkTime.Hours;
				}
				else
				{
					num = int.Parse(_workTimeHour.text);
					if (num > 23)
					{
						num = 23;
					}
					else if (num < 0)
					{
						num = 0;
					}
				}
				_workTimeHour.text = num.ToString("00");
				if (_workTimeMinutes.text == string.Empty)
				{
					num2 = dailyData.WorkTime.Minutes;
				}
				else
				{
					num2 = int.Parse(_workTimeMinutes.text);
					if (num2 > 59)
					{
						num2 = 59;
					}
					else if (num2 < 0)
					{
						num2 = 0;
					}
				}
				_workTimeMinutes.text = num2.ToString("00");
				if (_workTimeSeconds.text == string.Empty)
				{
					num3 = dailyData.WorkTime.Seconds;
				}
				else
				{
					num3 = int.Parse(_workTimeSeconds.text);
					if (num3 > 59)
					{
						num3 = 59;
					}
					else if (num3 < 0)
					{
						num3 = 0;
					}
				}
				_workTimeSeconds.text = num3.ToString("00");
				dailyData.WorkTimeSeconds = new TimeSpan(num, num2, num3).TotalSeconds;
				SaveDataManager.Instance.CalendarData.SaveDateData(_currentSelectDayUI.DateTime);
				CalenderCoreUI.UpdateWorkedUI(_currentSelectDayUI.DateTime);
			}
		}).AddTo(this);
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		_habitDataService.OnRemoveHabit.Subscribe(delegate(string habitId)
		{
			ReorderHabit(habitId);
		}).AddTo(this);
		_habitDataService.OnChangeHabitCompleted.Subscribe(delegate((string habitId, DateTime date, bool isCompleted) x)
		{
			if (CurrentSelectDay.HasValue && CurrentSelectDay.Value.IsSameDay(x.date))
			{
				if (x.isCompleted && !_habitDataService.ShouldHideOnCalendar(x.habitId, x.date))
				{
					AddHabit(x.habitId, x.date);
				}
				else
				{
					RemoveHabit(x.habitId);
				}
			}
		}).AddTo(this);
		_habitDataService.OnReorderHabit.Subscribe(delegate((string habitId, int oldIndex, int newIndex) x)
		{
			ReorderHabit(x.habitId);
		}).AddTo(this);
		Deactivate();
	}

	public bool IsSelectedDay(DateTime dateTime)
	{
		if (_currentSelectDayUI.DateTime.Day == dateTime.Day && _currentSelectDayUI.DateTime.Month == dateTime.Month)
		{
			return _currentSelectDayUI.DateTime.Year == dateTime.Year;
		}
		return false;
	}

	public bool IsActive()
	{
		return _isActive;
	}

	public void Activate(DateTime selectDate)
	{
		_changeOrderService.BringToFront(ChangeOrderService.OrderItemType.Calendar);
		_isActive = true;
		_facilityOpenButton.ActivateUseUI();
		base.gameObject.SetActive(value: true);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
	}

	public void Deactivate()
	{
		_isActive = false;
		_facilityOpenButton.DeactivateUseUI();
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	public void ChangeShowMonth(int year, int month)
	{
		CalenderCoreUI.View(year, month, enableWorkCheck: true);
	}

	public void OnClickButtonSelectDay(ulong taskID, SelectCalendarDayUI selectDayUI, Action<TodoData, string> onChangeEndTodoText, Action<TodoData> onClickButtonDeleteTask, bool isPlayClickSound)
	{
		_currentSelectDayUI?.OnSelectOtherButton();
		selectDayUI.OnSelectDay();
		_currentSelectDayUI = selectDayUI;
		CalendarDateData dailyData = SaveDataManager.Instance.CalendarData.GetDailyData(selectDayUI.DateTime);
		UpdateWorkTimeView(dailyData.WorkTime);
		for (int num = _currentCompleteTodoUI.Count - 1; num >= 0; num--)
		{
			_currentCompleteTodoUI[num].Destroy();
		}
		_currentCompleteTodoUI.Clear();
		foreach (KeyValuePair<ulong, TodoData> item in dailyData.CompleteTodoListDic)
		{
			AddTodo(item.Value, onChangeEndTodoText, onClickButtonDeleteTask);
		}
		for (int num2 = _currentCompleteHabitUIs.Count - 1; num2 >= 0; num2--)
		{
			UnityEngine.Object.Destroy(_currentCompleteHabitUIs[num2].ui.gameObject);
		}
		_currentCompleteHabitUIs.Clear();
		foreach (string item2 in _habitDataService.GetCompletedHabitsForCalendar(selectDayUI.DateTime))
		{
			AddHabit(item2, selectDayUI.DateTime);
		}
		_diaryInputField.text = dailyData.DiaryText;
		if (isPlayClickSound)
		{
			_systemSeService.PlayClick();
		}
	}

	private void UpdateWorkTimeView(TimeSpan dayWorkTimeSpan)
	{
		if (dayWorkTimeSpan.TotalHours >= 24.0)
		{
			_workTimeHour.text = "23";
			_workTimeMinutes.text = "59";
			_workTimeSeconds.text = "59";
		}
		else
		{
			_workTimeHour.text = dayWorkTimeSpan.Hours.ToString("00");
			_workTimeMinutes.text = dayWorkTimeSpan.Minutes.ToString("00");
			_workTimeSeconds.text = dayWorkTimeSpan.Seconds.ToString("00");
		}
	}

	public void AddTodo(TodoData todo, Action<TodoData, string> onChangeEndTodoText, Action<TodoData> onClickButtonDeleteTask)
	{
		if (todo == null)
		{
			return;
		}
		TodoUI component = UnityEngine.Object.Instantiate(_completeTodoPrefab, _completeTodoParent.transform, worldPositionStays: false).GetComponent<TodoUI>();
		if (_currentCompleteHabitUIs.Count > 0)
		{
			component.Transform.SetSiblingIndex(_currentCompleteHabitUIs[0].ui.transform.GetSiblingIndex());
		}
		component.Setup(todo, onChangeEndTodoText, onClickButtonDeleteTask);
		_currentCompleteTodoUI.Add(component);
		component.OnExpireClick.Subscribe(delegate(TodoUI ui)
		{
			if (calenderTargetTodo != null)
			{
				TodoUI todoUI = calenderTargetTodo;
				CloseCompleteTimeEditCalender();
				if (todoUI == ui)
				{
					return;
				}
			}
			OpenCompleteTimeEditCalender(ui.TodoData.Completed);
			calenderTargetTodo = ui;
		}).AddTo(component.DestroyCancellationToken);
	}

	public void RemoveTodo(TodoData todoData)
	{
		TodoUI todoUI = _currentCompleteTodoUI.FirstOrDefault((TodoUI completeTodo) => completeTodo.TodoData.UniqueID == todoData.UniqueID);
		if (!(todoUI == null))
		{
			if (calenderTargetTodo == todoUI)
			{
				CloseCompleteTimeEditCalender();
			}
			todoUI.Destroy();
			_currentCompleteTodoUI.Remove(todoUI);
		}
	}

	public void OnClickButtonDeleteTask(TodoData todoData)
	{
		RemoveTodo(todoData);
	}

	public void OpenCompleteTimeEditCalender(DateTime? completed)
	{
		DateTime dateTime = completed ?? DateTime.Now;
		completeTimeEditCalender.View(dateTime.Year, dateTime.Month);
		completeTimeEditCalender.gameObject.SetActive(value: true);
		openTween?.Kill(complete: true);
		completeTimeEditCalender.transform.localScale = Vector3.one * 0.7f;
		openTween = completeTimeEditCalender.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
	}

	public void CloseCompleteTimeEditCalender()
	{
		completeTimeEditCalender.gameObject.SetActive(value: false);
	}

	private void AddHabit(string habitId, DateTime date)
	{
		if (_currentCompleteHabitUIs.Any(((string habitId, CalendarCompleteHabitUI ui) x) => x.habitId == habitId))
		{
			return;
		}
		CalendarCompleteHabitUI calendarCompleteHabitUI = UnityEngine.Object.Instantiate(_completeHabitPrefab, _completeHabitParent.transform, worldPositionStays: false);
		calendarCompleteHabitUI.transform.SetSiblingIndex(GetHabitTargetSiblingIndex(habitId));
		ReactiveProperty<string> title = new ReactiveProperty<string>(_habitDataService.GetHabitTitle(habitId));
		_habitDataService.OnChangeHabitTitle.Subscribe(delegate((string habitId, string title) x)
		{
			if (x.habitId == habitId)
			{
				title.Value = x.title;
			}
		}).AddTo(this);
		calendarCompleteHabitUI.Setup(title, date, OnClickDelete, OnTitleEndEdit);
		_currentCompleteHabitUIs.Add((habitId, calendarCompleteHabitUI));
		void OnClickDelete()
		{
			_systemSeService.PlayCancel();
			_habitDataService.SetHideOnCalendar(habitId, date, hide: true);
			RemoveHabit(habitId);
		}
		void OnTitleEndEdit(string val)
		{
			_habitDataService.SetHabitTitle(habitId, val);
			title.Value = val;
		}
	}

	private int GetHabitTargetSiblingIndex(string habitId)
	{
		int habitSortIndex = _habitDataService.GetHabitSortIndex(habitId);
		int num = 0;
		if (_currentCompleteTodoUI.Count > 0)
		{
			num = _currentCompleteTodoUI.Max((TodoUI x) => x.Transform.GetSiblingIndex() + 1);
		}
		int? num2 = null;
		foreach (var currentCompleteHabitUI in _currentCompleteHabitUIs)
		{
			int habitSortIndex2 = _habitDataService.GetHabitSortIndex(currentCompleteHabitUI.habitId);
			if (habitSortIndex < habitSortIndex2)
			{
				num = Mathf.Min(num, currentCompleteHabitUI.ui.transform.GetSiblingIndex());
			}
			else if (habitSortIndex > habitSortIndex2)
			{
				num = Mathf.Max(num, currentCompleteHabitUI.ui.transform.GetSiblingIndex() + 1);
			}
			else
			{
				num2 = currentCompleteHabitUI.ui.transform.GetSiblingIndex();
			}
		}
		if (num2.HasValue && num > num2.Value)
		{
			num--;
		}
		return num;
	}

	private void RemoveHabit(string habitId)
	{
		int num = _currentCompleteHabitUIs.FindIndex(((string habitId, CalendarCompleteHabitUI ui) x) => x.habitId == habitId);
		if (num >= 0)
		{
			UnityEngine.Object.Destroy(_currentCompleteHabitUIs[num].ui.gameObject);
			_currentCompleteHabitUIs.RemoveAt(num);
		}
	}

	private void ReorderHabit(string habitId)
	{
		CalendarCompleteHabitUI item = _currentCompleteHabitUIs.Find(((string habitId, CalendarCompleteHabitUI ui) x) => x.habitId == habitId).ui;
		if (!(item == null))
		{
			item.transform.SetSiblingIndex(GetHabitTargetSiblingIndex(habitId));
		}
	}
}
