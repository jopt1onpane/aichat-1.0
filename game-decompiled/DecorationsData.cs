using System;
using System.Collections.Generic;
using System.Linq;
using FastEnumUtility;
using GUPS.Obfuscator.Attribute;

[Serializable]
[DoNotObfuscateClass]
public class DecorationsData
{
	public Dictionary<DecorationService.DecorationSkinType, DecorationData> DecorationDic = new Dictionary<DecorationService.DecorationSkinType, DecorationData>();

	public void LoadSetup()
	{
		foreach (KeyValuePair<DecorationService.DecorationSkinType, DecorationData> item in DecorationDic)
		{
			item.Value.LoadSetup();
		}
	}

	public void SaveReady()
	{
		foreach (KeyValuePair<DecorationService.DecorationSkinType, DecorationData> item in DecorationDic)
		{
			item.Value.SaveReady();
		}
	}

	public void CopyFrom(DecorationsData other)
	{
		foreach (DecorationService.DecorationSkinType value3 in FastEnum.GetValues<DecorationService.DecorationSkinType>())
		{
			if (other.DecorationDic.TryGetValue(value3, out var value))
			{
				if (!DecorationDic.TryGetValue(value3, out var value2))
				{
					value2 = new DecorationData(value3);
					DecorationDic.Add(value3, value2);
				}
				value2.CopyFrom(value);
			}
			else
			{
				DecorationDic.Remove(value3);
			}
		}
	}

	public bool IsSame(DecorationsData other)
	{
		if (DecorationDic.Count == other.DecorationDic.Count)
		{
			return DecorationDic.All((KeyValuePair<DecorationService.DecorationSkinType, DecorationData> x) => other.DecorationDic.TryGetValue(x.Key, out var value) && value.IsSame(x.Value));
		}
		return false;
	}
}
