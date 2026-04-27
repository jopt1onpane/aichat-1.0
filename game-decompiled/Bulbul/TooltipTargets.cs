using System.Collections.Generic;

namespace Bulbul;

public static class TooltipTargets
{
	private static readonly List<TooltipTarget> _targets = new List<TooltipTarget>(16);

	public static IReadOnlyList<TooltipTarget> Targets => _targets;

	public static void Add(TooltipTarget target)
	{
		_targets.Add(target);
	}

	public static void Remove(TooltipTarget target)
	{
		_targets.Remove(target);
	}
}
