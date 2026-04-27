using System;

namespace MagicLightProbes;

public static class RandomGen
{
	private static Random _global = new Random();

	[ThreadStatic]
	private static Random _local;

	public static int Next(int min, int max)
	{
		Random random = _local;
		if (random == null)
		{
			int seed;
			lock (_global)
			{
				seed = _global.Next(min, max);
			}
			random = (_local = new Random(seed));
		}
		return random.Next(min, max);
	}
}
