using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class CalenderMonthlyData
{
	public int Year;

	public int Month;

	public Dictionary<int, CalendarDateData> DiaryList = new Dictionary<int, CalendarDateData>();

	public CalenderMonthlyData()
	{
	}

	public CalenderMonthlyData(int year, int month)
	{
		Year = year;
		Month = month;
	}

	public CalendarDateData GetDateData(int day)
	{
		if (DiaryList.TryGetValue(day, out var value) && value != null)
		{
			return value;
		}
		DiaryList.TryAdd(day, value ?? (value = new CalendarDateData()));
		return value;
	}
}
