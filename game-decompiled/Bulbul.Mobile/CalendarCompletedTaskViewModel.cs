using System;

namespace Bulbul.Mobile;

public class CalendarCompletedTaskViewModel : CalendarContentsListBaseModel
{
	public ulong UniqueID;

	public string HabitTrackerID;

	public string Title;

	public DateTime CompletedDateTime;

	public bool IsHabitTrackerTask;
}
