using UnityEngine;

namespace Bulbul;

public static class ProbabilityUtility
{
	public static bool IsOccurredInPercent(float probability)
	{
		return Random.Range(0f, 99.9f) < probability;
	}
}
