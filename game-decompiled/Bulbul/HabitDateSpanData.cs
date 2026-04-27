using System;
using System.Collections.Generic;
using System.Linq;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class HabitDateSpanData
{
	public Dictionary<int, HabitDateData> DateDataDic = new Dictionary<int, HabitDateData>();

	public void LoadSetup()
	{
	}

	public void SaveReady()
	{
		foreach (int item in (from kvp in DateDataDic
			where kvp.Value.IsDefaultData()
			select kvp.Key).ToList())
		{
			DateDataDic.Remove(item);
		}
	}

	public bool IsDefaultData()
	{
		if (DateDataDic.Count != 0)
		{
			return DateDataDic.Values.All((HabitDateData x) => x.IsDefaultData());
		}
		return true;
	}
}
