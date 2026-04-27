using System;

namespace Bulbul.Mobile;

public class HabitTrackerListItemModel
{
	public class HabitTrackerListItemDayInfoModel
	{
		public DateTime date;

		public HabitDateEnableState enableState;

		public bool isChecked;

		public bool IsCellDisplay()
		{
			return date.DayOfWeek != DayOfWeek.Saturday;
		}
	}

	public string UniqueId;

	public string Title;

	public bool IsAlivePeriod;

	public HabitTrackerListItemDayInfoModel[] dayInfos;

	public bool IsCreateNew;
}
