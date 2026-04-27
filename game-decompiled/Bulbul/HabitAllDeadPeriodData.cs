using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class HabitAllDeadPeriodData
{
	public Dictionary<string, List<DatePeriodData>> DeadPeriodDic = new Dictionary<string, List<DatePeriodData>>();

	public void LoadSetup()
	{
		foreach (List<DatePeriodData> value in DeadPeriodDic.Values)
		{
			foreach (DatePeriodData item in value)
			{
				item.LoadSetup();
			}
		}
	}

	public void SaveReady()
	{
		List<string> list = new List<string>();
		foreach (var (item, list3) in DeadPeriodDic)
		{
			if (list3.Count == 0)
			{
				list.Add(item);
				continue;
			}
			foreach (DatePeriodData item2 in list3)
			{
				item2.SaveReady();
			}
		}
		foreach (string item3 in list)
		{
			DeadPeriodDic.Remove(item3);
		}
	}
}
