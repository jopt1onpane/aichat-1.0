using System.Collections;
using System.Collections.Generic;

namespace Bulbul.Achievements;

public interface IAchievementList : IReadOnlyCollection<AchievementStats>, IEnumerable<AchievementStats>, IEnumerable
{
	int SetProgress(AchievementCategory category, int progress);

	int ProgressIncrement(AchievementCategory category, int count = 1);

	AchievementStats GetAchievement(AchievementCategory category);
}
