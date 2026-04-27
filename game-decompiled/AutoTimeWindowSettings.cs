using Bulbul;
using UnityEngine;

public class AutoTimeWindowSettings
{
	private readonly int _pinStepMinutes;

	public float TimeDayStart { get; private set; }

	public float TimeSunsetStart { get; private set; }

	public float TimeNightStart { get; private set; }

	public AutoTimeWindowSettings(float initialNightEnd, float initialDayEnd, float initialNightStart, int pinStepMinutes = 10)
	{
		_pinStepMinutes = pinStepMinutes;
		TimeDayStart = RoundToStep(initialNightEnd);
		TimeSunsetStart = RoundToStep(initialDayEnd);
		TimeNightStart = RoundToStep(initialNightStart);
		ClampAndPushTimes(-1);
	}

	public void SetTime(int pinIndex, float hours)
	{
		hours = RoundToStep(hours);
		switch (pinIndex)
		{
		case 0:
			TimeDayStart = hours;
			break;
		case 1:
			TimeSunsetStart = hours;
			break;
		case 2:
			TimeNightStart = hours;
			break;
		}
		ClampAndPushTimes(pinIndex);
	}

	public WindowViewType GetWindowViewTypeFromTime(float timeHours)
	{
		if (timeHours >= TimeDayStart && timeHours < TimeSunsetStart && TimeSunsetStart - TimeDayStart > 0f)
		{
			return WindowViewType.Day;
		}
		if (timeHours >= TimeSunsetStart && timeHours < TimeNightStart && TimeNightStart - TimeSunsetStart > 0f)
		{
			return WindowViewType.Sunset;
		}
		return WindowViewType.Night;
	}

	public float RoundToStep(float hours)
	{
		if (_pinStepMinutes <= 0)
		{
			return hours;
		}
		float num = (float)_pinStepMinutes / 60f;
		float num2 = Mathf.Round(hours / num) * num;
		if (num2 >= 24f)
		{
			return 24f;
		}
		if (num2 <= 0f)
		{
			return 0f;
		}
		return num2;
	}

	private void ClampAndPushTimes(int draggedPinIndex)
	{
		switch (draggedPinIndex)
		{
		case 0:
			if (TimeDayStart > TimeSunsetStart)
			{
				TimeSunsetStart = TimeDayStart;
			}
			if (TimeDayStart > TimeNightStart)
			{
				TimeNightStart = TimeDayStart;
			}
			if (TimeSunsetStart > TimeNightStart)
			{
				TimeNightStart = TimeSunsetStart;
			}
			break;
		case 1:
			if (TimeSunsetStart < TimeDayStart)
			{
				TimeDayStart = TimeSunsetStart;
			}
			if (TimeSunsetStart > TimeNightStart)
			{
				TimeNightStart = TimeSunsetStart;
			}
			break;
		case 2:
			if (TimeNightStart < TimeSunsetStart)
			{
				TimeSunsetStart = TimeNightStart;
			}
			if (TimeNightStart < TimeDayStart)
			{
				TimeDayStart = TimeNightStart;
			}
			if (TimeDayStart > TimeSunsetStart)
			{
				TimeSunsetStart = TimeDayStart;
			}
			break;
		default:
			if (TimeDayStart > TimeSunsetStart)
			{
				TimeSunsetStart = TimeDayStart;
			}
			if (TimeSunsetStart > TimeNightStart)
			{
				TimeNightStart = TimeSunsetStart;
			}
			break;
		}
		if (TimeNightStart > 24f)
		{
			TimeNightStart = 24f;
			TimeSunsetStart = Mathf.Min(TimeSunsetStart, 24f);
			TimeDayStart = Mathf.Min(TimeDayStart, TimeSunsetStart);
		}
		if (TimeDayStart < 0f)
		{
			TimeDayStart = 0f;
			TimeSunsetStart = Mathf.Max(TimeSunsetStart, 0f);
			TimeNightStart = Mathf.Max(TimeNightStart, TimeSunsetStart);
		}
	}
}
