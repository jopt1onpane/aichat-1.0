using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class HabitAllHeaderData
{
	public List<HabitHeaderData> Habits = new List<HabitHeaderData>();

	public List<DeletedHabitHeaderData> DeletedHabits = new List<DeletedHabitHeaderData>();

	public void LoadSetup()
	{
		foreach (HabitHeaderData habit in Habits)
		{
			habit.LoadSetup();
		}
	}

	public void SaveReady()
	{
		foreach (HabitHeaderData habit in Habits)
		{
			habit.SaveReady();
		}
	}
}
