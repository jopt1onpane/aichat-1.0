using System;
using System.Collections.Generic;

public class ShuffleUtils
{
	private static Random random = new Random();

	public static void Shuffle<T>(IList<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = random.Next(num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}
}
