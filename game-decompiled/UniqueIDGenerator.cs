using System;

public class UniqueIDGenerator
{
	private static ulong prevID = ulong.MaxValue;

	public static ulong GetNewID()
	{
		ulong num = (ulong)DateTime.Now.Ticks;
		if (num == prevID)
		{
			num++;
		}
		prevID = num;
		return num;
	}
}
