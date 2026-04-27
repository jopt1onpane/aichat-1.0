using System;

namespace Bulbul;

public class TaskES3
{
	[ES3Serializable]
	public string UniqueKey;

	[ES3Serializable]
	public string TaskName;

	[ES3Serializable]
	public long FocusTimeTicks;

	[ES3Serializable]
	public long DateTicks;

	[ES3NonSerializable]
	public DateTime Date => new DateTime(DateTicks);

	[ES3NonSerializable]
	public TimeSpan FocusTime => TimeSpan.FromTicks(FocusTimeTicks);

	public TaskES3()
	{
	}

	public TaskES3(string task, TimeSpan focus, DateTime date)
	{
		UniqueKey = Guid.NewGuid().ToString("N");
		TaskName = task;
		FocusTimeTicks = focus.Ticks;
		DateTicks = date.Ticks;
	}
}
