using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class WindowViewData
{
	public WindowViewType WindowViewType;

	public bool IsActive;

	public WindowViewData()
	{
		WindowViewType = WindowViewType.Fireworks;
		IsActive = false;
	}

	public WindowViewData(WindowViewType windowViewType)
	{
		WindowViewType = windowViewType;
		IsActive = false;
	}

	public void CopyFrom(WindowViewData other)
	{
		WindowViewType = other.WindowViewType;
		IsActive = other.IsActive;
	}

	public bool IsSame(WindowViewData other)
	{
		if (WindowViewType == other.WindowViewType)
		{
			return IsActive == other.IsActive;
		}
		return false;
	}
}
