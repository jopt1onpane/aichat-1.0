using System.Collections.Generic;
using TMPro;

namespace Bulbul.Extensions;

public static class TextMeshProUGUIExtensions
{
	public static bool IsSupportedString(this TextMeshProUGUI tmpugui, string target)
	{
		foreach (char key in target)
		{
			if (!tmpugui.font.characterLookupTable.ContainsKey(key))
			{
				return false;
			}
		}
		return true;
	}

	public static List<char> GetNotSupportedCharList(this TextMeshProUGUI tmpugui, string target)
	{
		List<char> list = new List<char>();
		foreach (char c in target)
		{
			if (!tmpugui.font.characterLookupTable.ContainsKey(c))
			{
				list.Add(c);
			}
		}
		return list;
	}
}
