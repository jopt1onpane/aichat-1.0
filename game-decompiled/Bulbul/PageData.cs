using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class PageData
{
	public ulong UniqueID;

	public string TitleText;

	public string MainText;

	public PageData()
	{
		UniqueID = UniqueIDGenerator.GetNewID();
		TitleText = string.Empty;
		MainText = null;
	}

	public PageDataV2 ToV2()
	{
		return new PageDataV2
		{
			UniqueID = UniqueID,
			MainText = MainText
		};
	}
}
