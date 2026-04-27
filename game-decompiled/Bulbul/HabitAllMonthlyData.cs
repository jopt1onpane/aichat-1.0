using System;
using System.Collections.Generic;
using System.Linq;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class HabitAllMonthlyData
{
	public Dictionary<string, HabitDateSpanData> HabitDic = new Dictionary<string, HabitDateSpanData>();

	public void LoadSetup()
	{
		foreach (HabitDateSpanData value in HabitDic.Values)
		{
			value.LoadSetup();
		}
	}

	public void SaveReady()
	{
		foreach (string item in (from kvp in HabitDic
			where kvp.Value.IsDefaultData()
			select kvp.Key).ToList())
		{
			HabitDic.Remove(item);
		}
		foreach (HabitDateSpanData value in HabitDic.Values)
		{
			value.SaveReady();
		}
	}

	public bool IsDefaultData()
	{
		if (HabitDic.Count != 0)
		{
			return HabitDic.All((KeyValuePair<string, HabitDateSpanData> kvp) => kvp.Value.IsDefaultData());
		}
		return true;
	}
}
