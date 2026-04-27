using System;
using System.Collections.Generic;
using System.Linq;
using FastEnumUtility;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class EnviromentData
{
	public Dictionary<WindowViewType, WindowViewData> WindowViewDic;

	public Dictionary<AmbientSoundType, AmbientSoundData> AmbientSoundDic;

	public EnviromentData()
	{
		WindowViewDic = new Dictionary<WindowViewType, WindowViewData>();
		foreach (WindowViewType value in FastEnum.GetValues<WindowViewType>())
		{
			WindowViewDic.Add(value, new WindowViewData(value));
		}
		AmbientSoundDic = new Dictionary<AmbientSoundType, AmbientSoundData>();
		foreach (AmbientSoundType value2 in FastEnum.GetValues<AmbientSoundType>())
		{
			AmbientSoundDic.Add(value2, new AmbientSoundData(value2));
		}
		WindowViewDic[WindowViewType.Sunset].IsActive = true;
	}

	public void CopyFrom(EnviromentData other)
	{
		foreach (WindowViewType value5 in FastEnum.GetValues<WindowViewType>())
		{
			if (other.WindowViewDic.TryGetValue(value5, out var value))
			{
				if (!WindowViewDic.TryGetValue(value5, out var value2))
				{
					value2 = new WindowViewData(value5);
					WindowViewDic.Add(value5, value2);
				}
				value2.CopyFrom(value);
			}
			else
			{
				WindowViewDic.Remove(value5);
			}
		}
		foreach (AmbientSoundType value6 in FastEnum.GetValues<AmbientSoundType>())
		{
			if (other.AmbientSoundDic.TryGetValue(value6, out var value3))
			{
				if (!AmbientSoundDic.TryGetValue(value6, out var value4))
				{
					value4 = new AmbientSoundData(value6);
					AmbientSoundDic.Add(value6, value4);
				}
				value4.CopyFrom(value3);
			}
			else
			{
				AmbientSoundDic.Remove(value6);
			}
		}
	}

	public bool IsSame(EnviromentData other)
	{
		if (WindowViewDic.Count == other.WindowViewDic.Count && WindowViewDic.All((KeyValuePair<WindowViewType, WindowViewData> x) => other.WindowViewDic.TryGetValue(x.Key, out var value) && value.IsSame(x.Value)) && AmbientSoundDic.Count == other.AmbientSoundDic.Count)
		{
			return AmbientSoundDic.All((KeyValuePair<AmbientSoundType, AmbientSoundData> x) => other.AmbientSoundDic.TryGetValue(x.Key, out var value) && value.IsSame(x.Value));
		}
		return false;
	}
}
