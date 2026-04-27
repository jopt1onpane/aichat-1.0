using System;

namespace Bulbul;

public class TimeOfDayProvider
{
	public enum TimeOfDayType
	{
		Morning,
		Noon,
		Evening,
		Night
	}

	public TimeOfDayType GetCurrentTimeOfDayType()
	{
		DateTime now = DateTime.Now;
		if (now.Hour > 5 && now.Hour < 11)
		{
			return TimeOfDayType.Morning;
		}
		if (now.Hour >= 11 && now.Hour < 17)
		{
			return TimeOfDayType.Noon;
		}
		if (now.Hour >= 17 && now.Hour < 20)
		{
			return TimeOfDayType.Evening;
		}
		return TimeOfDayType.Night;
	}
}
