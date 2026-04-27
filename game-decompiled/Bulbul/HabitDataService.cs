using System;
using System.Collections.Generic;
using System.Linq;
using NestopiSystem;
using R3;

namespace Bulbul;

public class HabitDataService : IDisposable
{
	private readonly Subject<string> onAddHabit = new Subject<string>();

	private readonly Subject<string> onRemoveHabit = new Subject<string>();

	private readonly Subject<(string habitId, DateTime date, bool isCompleted)> onChangeHabitCompleted = new Subject<(string, DateTime, bool)>();

	private readonly Subject<(string habitId, int oldIndex, int newIndex)> onReorderHabit = new Subject<(string, int, int)>();

	private readonly Subject<string> onChangeHabitPeriod = new Subject<string>();

	private readonly Subject<string> onChangeHabitDayOfWeekEnabled = new Subject<string>();

	private readonly Subject<(string habitId, string title)> onChangeHabitTitle = new Subject<(string, string)>();

	public const DayOfWeek StartDayOfWeek = DayOfWeek.Sunday;

	private SaveDataManager SaveData => SaveDataManager.Instance;

	public Observable<string> OnAddHabit => onAddHabit;

	public Observable<string> OnRemoveHabit => onRemoveHabit;

	public Observable<(string habitId, DateTime date, bool isCompleted)> OnChangeHabitCompleted => onChangeHabitCompleted;

	public Observable<(string habitId, int oldIndex, int newIndex)> OnReorderHabit => onReorderHabit;

	public Observable<string> OnChangeHabitPeriod => onChangeHabitPeriod;

	public Observable<string> OnChangeHabitDayOfWeekEnabled => onChangeHabitDayOfWeekEnabled;

	public Observable<(string habitId, string title)> OnChangeHabitTitle => onChangeHabitTitle;

	public DateTime GetStartDateOfWeek(DateTime date)
	{
		int num = (int)date.DayOfWeek;
		if (num < 0)
		{
			num += 7;
		}
		return date.AddDays(-num);
	}

	public IEnumerable<string> GetAllHabitIds(bool includeDeleted)
	{
		IEnumerable<string> enumerable = SaveData.AllHabitHeaderData.Habits.Select((HabitHeaderData x) => x.HabitID);
		if (includeDeleted)
		{
			enumerable = enumerable.Concat(SaveData.AllHabitHeaderData.DeletedHabits.Select((DeletedHabitHeaderData x) => x.HabitID));
		}
		return enumerable;
	}

	public string CreateHabit()
	{
		HabitHeaderData habitHeaderData = HabitHeaderData.Create(DateTime.Today);
		SaveData.AllHabitHeaderData.Habits.Add(habitHeaderData);
		SaveData.SaveHabitHeadersThrottled();
		onAddHabit.OnNext(habitHeaderData.HabitID);
		return habitHeaderData.HabitID;
	}

	public void DeleteHabit(string habitId)
	{
		int num = SaveData.AllHabitHeaderData.Habits.FindIndex((HabitHeaderData x) => x.HabitID == habitId);
		if (num >= 0)
		{
			HabitHeaderData habitHeaderData = SaveData.AllHabitHeaderData.Habits[num];
			SaveData.AllHabitHeaderData.Habits.RemoveAt(num);
			SaveData.AllHabitHeaderData.DeletedHabits.Add(new DeletedHabitHeaderData
			{
				HabitID = habitHeaderData.HabitID,
				Title = habitHeaderData.Title
			});
			SaveData.SaveHabitHeadersThrottled();
			RemoveAllDeadPeriods(habitId);
			onRemoveHabit.OnNext(habitHeaderData.HabitID);
		}
	}

	public void ReorderHabit(string id, int newIndex)
	{
		int num = SaveData.AllHabitHeaderData.Habits.FindIndex((HabitHeaderData x) => x.HabitID == id);
		if (num >= 0 && num != newIndex)
		{
			SaveData.AllHabitHeaderData.Habits.Move(num, newIndex);
			SaveData.SaveHabitHeadersThrottled();
			onReorderHabit.OnNext((id, num, newIndex));
		}
	}

	public void ReorderHabit(string from, string to)
	{
		int num = SaveData.AllHabitHeaderData.Habits.FindIndex((HabitHeaderData x) => x.HabitID == from);
		int num2 = SaveData.AllHabitHeaderData.Habits.FindIndex((HabitHeaderData x) => x.HabitID == to);
		if (num >= 0)
		{
			if (to == null && num2 < 0)
			{
				num2 = 0;
			}
			if (to != null && num > num2)
			{
				num2++;
			}
			SaveData.AllHabitHeaderData.Habits.Move(num, num2);
			SaveData.SaveHabitHeadersThrottled();
			onReorderHabit.OnNext((from, num, num2));
		}
	}

	public string GetHabitTitle(string habitId)
	{
		HabitHeaderData habitHeaderData = SaveData.AllHabitHeaderData.Habits.Find((HabitHeaderData x) => x.HabitID == habitId);
		if (habitHeaderData != null)
		{
			return habitHeaderData.Title;
		}
		return SaveData.AllHabitHeaderData.DeletedHabits.Find((DeletedHabitHeaderData x) => x.HabitID == habitId)?.Title;
	}

	public void SetHabitTitle(string habitId, string title)
	{
		HabitHeaderData habitHeaderData = SaveData.AllHabitHeaderData.Habits.Find((HabitHeaderData x) => x.HabitID == habitId);
		if (habitHeaderData != null)
		{
			if (habitHeaderData.Title == title)
			{
				return;
			}
			habitHeaderData.Title = title;
		}
		else
		{
			DeletedHabitHeaderData deletedHabitHeaderData = SaveData.AllHabitHeaderData.DeletedHabits.Find((DeletedHabitHeaderData x) => x.HabitID == habitId);
			if (deletedHabitHeaderData == null || deletedHabitHeaderData.Title == title)
			{
				return;
			}
			deletedHabitHeaderData.Title = title;
		}
		SaveData.SaveHabitHeadersThrottled();
		onChangeHabitTitle.OnNext((habitId, title));
	}

	public IEnumerable<string> GetCompletedHabitsForCalendar(DateTime date)
	{
		return from habitId in GetAllHabitIds(includeDeleted: true)
			where !ShouldHideOnCalendar(habitId, date) && IsHabitDayCompleted(habitId, date)
			select habitId;
	}

	public int GetHabitSortIndex(string habitId)
	{
		int num = SaveData.AllHabitHeaderData.Habits.FindIndex((HabitHeaderData x) => x.HabitID == habitId);
		if (num >= 0)
		{
			return num;
		}
		int num2 = SaveData.AllHabitHeaderData.DeletedHabits.FindIndex((DeletedHabitHeaderData x) => x.HabitID == habitId);
		if (num2 >= 0)
		{
			return num2 + SaveData.AllHabitHeaderData.Habits.Count;
		}
		return int.MaxValue;
	}

	public bool IsHabitDayOfWeekEnabled(string habitId, DayOfWeek dayOfWeek)
	{
		return SaveData.AllHabitHeaderData.Habits.Find((HabitHeaderData x) => x.HabitID == habitId)?.IsDayOfWeekEnabled(dayOfWeek) ?? false;
	}

	public void SetHabitDayOfWeekEnabled(string habitId, DayOfWeek dayOfWeek, bool enabled)
	{
		HabitHeaderData habitHeaderData = SaveData.AllHabitHeaderData.Habits.Find((HabitHeaderData x) => x.HabitID == habitId);
		if (habitHeaderData != null && habitHeaderData.IsDayOfWeekEnabled(dayOfWeek) != enabled)
		{
			habitHeaderData.SetDayOfWeekEnabled(dayOfWeek, enabled);
			SaveData.SaveHabitHeadersThrottled();
			onChangeHabitDayOfWeekEnabled.OnNext(habitId);
		}
	}

	private HabitDateSpanData GetOrCreateHabitMonthlyData(string habitId, int year, int month)
	{
		HabitAllMonthlyData orCreateHabitsMonthlyData = GetOrCreateHabitsMonthlyData(year, month);
		if (!orCreateHabitsMonthlyData.HabitDic.TryGetValue(habitId, out var value))
		{
			value = new HabitDateSpanData();
			orCreateHabitsMonthlyData.HabitDic.Add(habitId, value);
		}
		return value;
	}

	private HabitDateSpanData GetHabitsMonthlyData(string habitId, int year, int month)
	{
		return GetHabitsMonthlyData(year, month)?.HabitDic.GetValueOrDefault(habitId);
	}

	private HabitAllMonthlyData GetOrCreateHabitsMonthlyData(int year, int month)
	{
		if (!SaveData.TryGetOrLoadHabitsMonthlyData(year, month, out var data))
		{
			data = new HabitAllMonthlyData();
			SaveData.AddHabitsMonthlyData(year, month, data);
		}
		return data;
	}

	private HabitAllMonthlyData GetHabitsMonthlyData(int year, int month)
	{
		if (SaveData.TryGetOrLoadHabitsMonthlyData(year, month, out var data))
		{
			return data;
		}
		return null;
	}

	private HabitDateData GetOrCreateHabitDateData(string habitId, DateTime date)
	{
		HabitDateSpanData orCreateHabitMonthlyData = GetOrCreateHabitMonthlyData(habitId, date.Year, date.Month);
		if (!orCreateHabitMonthlyData.DateDataDic.TryGetValue(date.Day, out var value))
		{
			value = new HabitDateData();
			orCreateHabitMonthlyData.DateDataDic.Add(date.Day, value);
		}
		return value;
	}

	private HabitDateData GetHabitDateData(string habitId, DateTime date)
	{
		return GetHabitsMonthlyData(habitId, date.Year, date.Month)?.DateDataDic.GetValueOrDefault(date.Day);
	}

	private void DeleteHabitDateData(string habitId, DateTime date, bool save)
	{
		HabitDateSpanData habitsMonthlyData = GetHabitsMonthlyData(habitId, date.Year, date.Month);
		if (habitsMonthlyData != null)
		{
			habitsMonthlyData.DateDataDic.Remove(date.Day);
			if (save)
			{
				SaveData.SaveHabitsMonthlyData(date.Year, date.Month);
			}
		}
	}

	public bool IsHabitDayCompleted(string habitId, DateTime date)
	{
		return GetHabitDateData(habitId, date)?.Completed ?? false;
	}

	public void SetHabitDayCompleted(string habitId, DateTime date, bool completed)
	{
		HabitDateData orCreateHabitDateData = GetOrCreateHabitDateData(habitId, date);
		if (orCreateHabitDateData.Completed != completed)
		{
			orCreateHabitDateData.Completed = completed;
			SaveData.SaveHabitsMonthlyData(date.Year, date.Month);
			onChangeHabitCompleted.OnNext((habitId, date, completed));
		}
	}

	public HabitDateEnableState GetHabitDayEnableState(string habitId, DateTime date)
	{
		if (!IsDateInAlivePeriod(habitId, date))
		{
			return HabitDateEnableState.DeadPeriod;
		}
		if (!IsHabitDayOfWeekEnabled(habitId, date.DayOfWeek))
		{
			return HabitDateEnableState.Disabled;
		}
		return HabitDateEnableState.Enabled;
	}

	public void SetHabitPeriodAlive(string habitId, bool enable)
	{
		bool flag = false;
		if (!SaveData.AllHabitDeadPeriodData.DeadPeriodDic.TryGetValue(habitId, out var value))
		{
			value = new List<DatePeriodData>();
			SaveData.AllHabitDeadPeriodData.DeadPeriodDic.Add(habitId, value);
		}
		DateTime today = DateTime.Today;
		if (enable)
		{
			int num = value.RemoveAll((DatePeriodData period) => period.StartDate.Date >= today);
			flag = flag || num > 0;
			DatePeriodData item = GetDeadPeriodAt(habitId, today).period;
			if (item != null)
			{
				item.EndDate = today.AddDays(-1.0);
				flag = true;
			}
		}
		else
		{
			int num2 = value.RemoveAll((DatePeriodData period) => period.StartDate.Date > today);
			flag = flag || num2 > 0;
			DatePeriodData item2 = GetDeadPeriodAt(habitId, today).period;
			if (item2 != null)
			{
				flag = item2.EndDate.HasValue;
				item2.EndDate = null;
			}
			else
			{
				DatePeriodData item3 = GetDeadPeriodAt(habitId, today.AddDays(-1.0)).period;
				if (item3 != null)
				{
					item3.EndDate = null;
					flag = true;
				}
				else
				{
					value.Add(new DatePeriodData(today, null));
					flag = true;
				}
			}
		}
		if (flag)
		{
			SaveData.SaveHabitDeadPeriodsThrottled();
			onChangeHabitPeriod.OnNext(habitId);
		}
	}

	private void RemoveAllDeadPeriods(string habitId)
	{
		if (SaveData.AllHabitDeadPeriodData.DeadPeriodDic.Remove(habitId))
		{
			SaveData.SaveHabitDeadPeriodsThrottled();
		}
	}

	private (DatePeriodData period, int index) GetDeadPeriodAt(string habitId, DateTime date)
	{
		date = date.Date;
		List<DatePeriodData> valueOrDefault = SaveData.AllHabitDeadPeriodData.DeadPeriodDic.GetValueOrDefault(habitId);
		if (valueOrDefault == null)
		{
			return (period: null, index: -1);
		}
		for (int i = 0; i < valueOrDefault.Count; i++)
		{
			DatePeriodData datePeriodData = valueOrDefault[i];
			if (datePeriodData.StartDate.Date <= date && (!datePeriodData.EndDate.HasValue || datePeriodData.EndDate.Value.Date >= date))
			{
				return (period: datePeriodData, index: i);
			}
		}
		return (period: null, index: -1);
	}

	public bool IsDateInAlivePeriod(string habitId, DateTime date)
	{
		HabitHeaderData habitHeaderData = SaveData.AllHabitHeaderData.Habits.Find((HabitHeaderData x) => x.HabitID == habitId);
		if (habitHeaderData == null)
		{
			return false;
		}
		if (date.Date < habitHeaderData.CreatedDate.Date)
		{
			return false;
		}
		List<DatePeriodData> valueOrDefault = SaveData.AllHabitDeadPeriodData.DeadPeriodDic.GetValueOrDefault(habitId);
		if (valueOrDefault == null)
		{
			return true;
		}
		foreach (DatePeriodData item in valueOrDefault)
		{
			if (item.IsDateInPeriod(date))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsAllCompletedForDate(DateTime date)
	{
		bool result = false;
		foreach (string allHabitId in GetAllHabitIds(includeDeleted: false))
		{
			HabitDateData habitDateData = GetHabitDateData(allHabitId, date);
			if (habitDateData != null && habitDateData.Completed)
			{
				result = true;
			}
			else if (GetHabitDayEnableState(allHabitId, date) switch
			{
				HabitDateEnableState.Enabled => true, 
				HabitDateEnableState.Disabled => false, 
				_ => IsDateInAlivePeriod(allHabitId, date), 
			})
			{
				return false;
			}
		}
		return result;
	}

	public bool ShouldHideOnCalendar(string habitId, DateTime date)
	{
		return GetHabitDateData(habitId, date)?.HideOnCalendar ?? false;
	}

	public void SetHideOnCalendar(string habitId, DateTime date, bool hide)
	{
		HabitDateData orCreateHabitDateData = GetOrCreateHabitDateData(habitId, date);
		if (hide && SaveData.AllHabitHeaderData.Habits.Find((HabitHeaderData x) => x.HabitID == habitId) == null)
		{
			GetOrCreateHabitMonthlyData(habitId, date.Year, date.Month).DateDataDic.Remove(date.Day);
			SaveData.SaveHabitsMonthlyData(date.Year, date.Month);
		}
		else
		{
			orCreateHabitDateData.HideOnCalendar = hide;
			SaveData.SaveHabitsMonthlyData(date.Year, date.Month);
		}
	}

	public void Dispose()
	{
	}
}
