using System;

namespace Bulbul.Mobile;

public class CalendarAndWorkViewModel : CalendarContentsListBaseModel
{
	public CalenderMonthlyData CalendarMonthlyData;

	public int SelectedYear;

	public int SelectedMonth;

	public int SelectedDay;

	public TimeSpan WorkTime;
}
