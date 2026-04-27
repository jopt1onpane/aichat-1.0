using System;
using System.Globalization;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class HabitHeaderData
{
	public string HabitID;

	[ES3NonSerializable]
	public DateTime CreatedDate;

	[ES3Serializable]
	private string _createdDate;

	public string Title;

	public byte DayOfWeekBitFlag = byte.MaxValue;

	private const string DateFormat = "yyyyMMdd";

	public static HabitHeaderData Create(DateTime createdDate)
	{
		return new HabitHeaderData
		{
			HabitID = Guid.NewGuid().ToString(),
			CreatedDate = createdDate
		};
	}

	public void LoadSetup()
	{
		CreatedDate = DateTime.ParseExact(_createdDate, "yyyyMMdd", CultureInfo.InvariantCulture);
	}

	public void SaveReady()
	{
		_createdDate = CreatedDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
	}

	public bool IsDayOfWeekEnabled(DayOfWeek dayOfWeek)
	{
		return (DayOfWeekBitFlag & (byte)(1 << (int)dayOfWeek)) != 0;
	}

	public void SetDayOfWeekEnabled(DayOfWeek dayOfWeek, bool enable)
	{
		if (enable)
		{
			DayOfWeekBitFlag |= (byte)(1 << (int)dayOfWeek);
		}
		else
		{
			DayOfWeekBitFlag &= (byte)(~(1 << (int)dayOfWeek));
		}
	}
}
