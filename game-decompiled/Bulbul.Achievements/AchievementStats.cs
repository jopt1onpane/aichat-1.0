using R3;

namespace Bulbul.Achievements;

public class AchievementStats
{
	public readonly AchievementCategory AchievementCategory;

	public readonly ReactiveProperty<int> Progress;

	public AchievementStats(AchievementCategory achievementCategory, int progress)
	{
		AchievementCategory = achievementCategory;
		Progress = new ReactiveProperty<int>(progress);
	}

	public static AchievementStats Create(AchievementCategory achievementType, int progress)
	{
		return new AchievementStats(achievementType, progress);
	}
}
