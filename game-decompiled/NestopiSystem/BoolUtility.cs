using System;
using System.Threading;

namespace NestopiSystem;

public static class BoolUtility
{
	public static bool TrueExchange(ref bool location1)
	{
		long location2 = Convert.ToInt64(location1);
		location1 = true;
		return Interlocked.CompareExchange(ref location2, 1L, 0L) != 0;
	}

	public static bool FalseExchange(ref bool location1)
	{
		long location2 = Convert.ToInt64(location1);
		location1 = false;
		return Interlocked.CompareExchange(ref location2, 1L, 0L) != 0;
	}
}
