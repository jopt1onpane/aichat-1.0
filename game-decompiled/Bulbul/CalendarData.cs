using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class CalendarData
{
	public DateTime CurrentDataDateTime;

	private readonly List<CalenderMonthlyData> monthlyCache = new List<CalenderMonthlyData>();

	private readonly int maxCacheCount = 10;

	[Obsolete]
	public static readonly string CalendarFileName = "calendar".GetHashCode() + ".es3";

	private string GetWorkTimeDateKey(DateTime date)
	{
		return $"WorkTimeData{date:yyyyMMdd}";
	}

	private string GetTodoDateKey(DateTime date)
	{
		return $"Todo{date:yyyyMMdd}";
	}

	private string GetDiaryDateKey(DateTime date)
	{
		return $"DiaryData{date:yyyyMMdd}";
	}

	public CalendarDateData GetDailyDataAsync(int year, int month, int day)
	{
		CalenderMonthlyData monthlyData = GetMonthlyData(year, month);
		if (monthlyData.DiaryList.TryGetValue(day, out var value) && value != null)
		{
			return value;
		}
		monthlyData.DiaryList.TryAdd(day, value = new CalendarDateData());
		return value;
	}

	public CalendarDateData GetDailyData(DateTime dt)
	{
		return GetDailyData(dt.Year, dt.Month, dt.Day);
	}

	public CalenderMonthlyData GetMonthlyData(DateTime dt)
	{
		return GetMonthlyData(dt.Year, dt.Month);
	}

	public CalendarDateData GetDailyData(int year, int month, int day)
	{
		CalenderMonthlyData monthlyData = GetMonthlyData(year, month);
		if (monthlyData.DiaryList.TryGetValue(day, out var value) && value != null)
		{
			return value;
		}
		monthlyData.DiaryList.TryAdd(day, value = new CalendarDateData());
		return value;
	}

	public CalenderMonthlyData GetMonthlyData(int year, int month)
	{
		foreach (CalenderMonthlyData item in monthlyCache)
		{
			if (item.Month == month && item.Year == year)
			{
				return item;
			}
		}
		SaveDataManager.Instance.TryLoadCalenderData(year, month, out var data);
		if (data == null)
		{
			data = new CalenderMonthlyData();
		}
		monthlyCache.Add(data);
		data.Year = year;
		data.Month = month;
		if (monthlyCache.Count > maxCacheCount)
		{
			monthlyCache.RemoveAt(0);
		}
		return data;
	}

	public void SaveDateData(DateTime date)
	{
		CalenderMonthlyData monthlyData = GetMonthlyData(date);
		SaveDataManager.Instance.SaveCalenderMonth(monthlyData);
	}

	public void SaveDailyData(int year, int month, int day, CalendarDateData data)
	{
		CalenderMonthlyData monthlyData = GetMonthlyData(year, month);
		monthlyData.DiaryList[day] = data;
		SaveDataManager.Instance.SaveCalenderData(monthlyData);
	}
}
