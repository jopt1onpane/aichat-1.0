using System;
using UnityEngine;

namespace Bulbul;

public static class ScreenSleepModeExtensions
{
	public static int GetSleepTimeout(this ScreenSleepMode self)
	{
		return self switch
		{
			ScreenSleepMode.SystemSetting => -2, 
			ScreenSleepMode.Disable => -1, 
			_ => throw new ArgumentOutOfRangeException("self", self, null), 
		};
	}

	public static void SetSleepTimeout(this ScreenSleepMode self)
	{
		Screen.sleepTimeout = self.GetSleepTimeout();
	}
}
