using System;
using System.Collections;
using System.Collections.Generic;

namespace Bulbul.Achievements;

public class DefaultAchievementList : IAchievementList, IReadOnlyCollection<AchievementStats>, IEnumerable<AchievementStats>, IEnumerable
{
	public int Count => 0;

	public IEnumerator<AchievementStats> GetEnumerator()
	{
		return (IEnumerator<AchievementStats>)Array.Empty<AchievementStats>().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public int SetProgress(AchievementCategory category, int progress)
	{
		return -1;
	}

	public int ProgressIncrement(AchievementCategory category, int count = 1)
	{
		return -1;
	}

	public AchievementStats GetAchievement(AchievementCategory category)
	{
		return null;
	}
}
