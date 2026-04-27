using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class HabitDateData
{
	public bool Completed;

	public bool HideOnCalendar;

	public bool IsDefaultData()
	{
		return !Completed;
	}
}
