using System;
using System.Linq;
using System.Threading;
using NestopiSystem;
using R3;
using TMPro;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class CalenderCoreUI : MonoBehaviour
{
	private const int CalendarButtonMax = 42;

	[Inject]
	protected SystemSeService _systemSeService;

	[Inject]
	private HabitDataService _habitDataService;

	[SerializeField]
	[Header("日付\u3000年")]
	private TextMeshProUGUI _yearText;

	[SerializeField]
	[Header("日付\u3000月")]
	private TextMeshProUGUI _monthText;

	[SerializeField]
	private ButtonEventObservable nextMonthButton;

	[SerializeField]
	private ButtonEventObservable prevMonthButton;

	private SelectCalendarDayUI[] _selectCalendarDayUIs;

	private SelectCalendarDayUI _currentSelectDayUI;

	private readonly Subject<SelectCalendarDayUI> onSelectedDay = new Subject<SelectCalendarDayUI>();

	private readonly Subject<(int, int)> _onChangedMonth = new Subject<(int, int)>();

	private readonly Subject<Unit> onClickOutside = new Subject<Unit>();

	private DateTime? _selectingDate;

	private RectTransform rectTransform;

	private CancellationTokenSource viewCancellation = new CancellationTokenSource();

	private bool isValidWorkCheck;

	public SelectCalendarDayUI[] SelectCalendarDayUIs => _selectCalendarDayUIs;

	public Observable<SelectCalendarDayUI> OnSelectedDay => onSelectedDay;

	public Observable<(int, int)> OnChangedMonth => _onChangedMonth;

	public Observable<Unit> OnClickOutside => onClickOutside;

	public int Year { get; private set; }

	public int Month { get; private set; }

	public RectTransform RectTransform
	{
		get
		{
			if (!rectTransform)
			{
				return rectTransform = GetComponent<RectTransform>();
			}
			return rectTransform;
		}
	}

	public void Setup(bool isValidWorkCheck, bool resetOnDisable)
	{
		if (_systemSeService == null)
		{
			_systemSeService = RoomLifetimeScope.Resolve<SystemSeService>();
		}
		if (_habitDataService == null)
		{
			_habitDataService = RoomLifetimeScope.Resolve<HabitDataService>();
		}
		this.isValidWorkCheck = isValidWorkCheck;
		_selectCalendarDayUIs = new SelectCalendarDayUI[42];
		_selectCalendarDayUIs = GetComponentsInChildren<SelectCalendarDayUI>(includeInactive: true);
		_selectCalendarDayUIs.Select((SelectCalendarDayUI x) => x.OnSelected).Merge().Subscribe(delegate(SelectCalendarDayUI dayUI)
		{
			_currentSelectDayUI = dayUI;
			_selectingDate = new DateTime(Year, Month, dayUI.Day);
			onSelectedDay.OnNext(dayUI);
		})
			.AddTo(this);
		SelectCalendarDayUI[] selectCalendarDayUIs = SelectCalendarDayUIs;
		for (int num = 0; num < selectCalendarDayUIs.Length; num++)
		{
			selectCalendarDayUIs[num].Setup(isValidWorkCheck, resetOnDisable);
		}
		nextMonthButton.OnClick.Subscribe(ViewNextMonth).AddTo(this);
		prevMonthButton.OnClick.Subscribe(ViewPrevMonth).AddTo(this);
	}

	public void ViewEditorMode(int year, int month, DateTime? selectingDate)
	{
		_selectingDate = selectingDate;
		View(year, month, enableWorkCheck: false);
	}

	public void View(int year, int month)
	{
		viewCancellation?.Cancel();
		viewCancellation = new CancellationTokenSource();
		ViewCore(year, month);
	}

	public void View(int year, int month, bool enableWorkCheck)
	{
		if (!enableWorkCheck)
		{
			View(year, month);
			return;
		}
		viewCancellation?.Cancel();
		viewCancellation = new CancellationTokenSource();
		CancellationToken token = viewCancellation.Token;
		CalenderMonthlyData monthlyData = SaveDataManager.Instance.CalendarData.GetMonthlyData(year, month);
		token.ThrowIfCancellationRequested();
		View(monthlyData);
	}

	public void View(CalenderMonthlyData monthlyData, DateTime selectedDate)
	{
		_selectingDate = selectedDate;
		ChangeOtherSelectAllDayButton();
		View(monthlyData);
	}

	public void View(CalenderMonthlyData monthlyData)
	{
		ViewCore(monthlyData.Year, monthlyData.Month, delegate(SelectCalendarDayUI dayUI, DateTime dateTime)
		{
			bool isWork = IsWorkedOnDate(dateTime);
			dayUI.Activate(dateTime, isWork);
		});
	}

	private void ViewCore(int year, int month, Action<SelectCalendarDayUI, DateTime> dayUISelection = null)
	{
		Year = year;
		Month = month;
		int dayOfWeek = (int)new DateTime(year, month, 1).DayOfWeek;
		int num = DateTime.DaysInMonth(year, month);
		int num2 = 1;
		for (int i = 0; i < _selectCalendarDayUIs.Length; i++)
		{
			if (i < dayOfWeek || num2 > num)
			{
				_selectCalendarDayUIs[i].Deactivate(isUseDoComplete: true);
				continue;
			}
			if (dayUISelection != null)
			{
				dayUISelection(_selectCalendarDayUIs[i], new DateTime(year, month, num2));
			}
			else
			{
				_selectCalendarDayUIs[i].Activate(new DateTime(year, month, num2), isWork: false);
			}
			num2++;
		}
		_yearText.text = year.ToString();
		string text = month.ToString();
		if (text.Length < 2)
		{
			text = "0" + text;
		}
		_monthText.text = text;
		AdjustCurrentSelectingDayButton();
	}

	public void UpdateWorkedUI(DateTime dateTime)
	{
		if (dateTime.Date.Year != Year || dateTime.Date.Month != Month)
		{
			return;
		}
		bool isWork = IsWorkedOnDate(dateTime);
		string text = dateTime.Day.ToString();
		SelectCalendarDayUI[] selectCalendarDayUIs = SelectCalendarDayUIs;
		foreach (SelectCalendarDayUI selectCalendarDayUI in selectCalendarDayUIs)
		{
			if (selectCalendarDayUI.DayText == text)
			{
				selectCalendarDayUI.UpdateWorkedUI(isWork);
				break;
			}
		}
	}

	private bool IsWorkedOnDate(DateTime dateTime)
	{
		CalendarDateData dailyData = SaveDataManager.Instance.CalendarData.GetDailyData(dateTime);
		if (dailyData.WorkTimeSeconds > 0.0 || dailyData.CompleteTodoListDic.Count > 0)
		{
			return true;
		}
		if (_habitDataService.GetCompletedHabitsForCalendar(dateTime).Any())
		{
			return true;
		}
		return false;
	}

	private void AdjustCurrentSelectingDayButton()
	{
		DateTime? selectingDate = _selectingDate;
		if (!selectingDate.HasValue)
		{
			return;
		}
		DateTime valueOrDefault = selectingDate.GetValueOrDefault();
		if (_currentSelectDayUI != null)
		{
			_currentSelectDayUI.OnSelectOtherButton(isUseDoComplete: true);
		}
		if (valueOrDefault.Year != Year || valueOrDefault.Month != Month)
		{
			return;
		}
		string text = valueOrDefault.Day.ToString();
		SelectCalendarDayUI[] selectCalendarDayUIs = _selectCalendarDayUIs;
		foreach (SelectCalendarDayUI selectCalendarDayUI in selectCalendarDayUIs)
		{
			if (selectCalendarDayUI.DayText == text)
			{
				_currentSelectDayUI = selectCalendarDayUI;
				selectCalendarDayUI.OnSelectDay();
			}
		}
	}

	private void ChangeOtherSelectAllDayButton()
	{
		SelectCalendarDayUI[] selectCalendarDayUIs = _selectCalendarDayUIs;
		for (int i = 0; i < selectCalendarDayUIs.Length; i++)
		{
			selectCalendarDayUIs[i].OnSelectOtherButton(isUseDoComplete: true);
		}
		_currentSelectDayUI = null;
	}

	public void ViewPrevMonth()
	{
		int month = Month;
		int num = Year;
		if (month <= 1)
		{
			if (num - 1 < 2025)
			{
				return;
			}
			num--;
			month = 12;
		}
		else
		{
			month--;
		}
		View(num, month, isValidWorkCheck);
		_onChangedMonth.OnNext((num, month));
		_systemSeService.PlayClick();
	}

	public void ViewNextMonth()
	{
		int month = Month;
		int num = Year;
		if (month == 12)
		{
			if (num + 1 >= 2999)
			{
				return;
			}
			num++;
			month = 1;
		}
		else
		{
			month++;
		}
		View(num, month, isValidWorkCheck);
		_onChangedMonth.OnNext((num, month));
		_systemSeService.PlayClick();
	}

	private void Update()
	{
		if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) && !(base.transform as RectTransform).ContainsScreenPoint(Input.mousePosition, 30f))
		{
			onClickOutside.OnNext(Unit.Default);
		}
	}

	private void OnDestroy()
	{
		if (viewCancellation != null && !viewCancellation.IsCancellationRequested)
		{
			viewCancellation?.Cancel();
		}
	}
}
