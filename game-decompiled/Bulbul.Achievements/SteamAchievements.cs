using System.Collections;
using System.Collections.Generic;
using FastEnumUtility;
using NestopiSystem;
using NestopiSystem.Steam;
using Steamworks;
using UnityEngine;

namespace Bulbul.Achievements;

public class SteamAchievements : IAchievementList, IReadOnlyCollection<AchievementStats>, IEnumerable<AchievementStats>, IEnumerable
{
	private readonly List<AchievementStats> achievements = new List<AchievementStats>();

	private readonly SteamManager steamManager;

	private SteamAchievementEventBroker achievementEventBroker;

	public int Count => achievements.Count;

	public SteamAchievements(SteamManager steamManager)
	{
		this.steamManager = steamManager;
		achievementEventBroker = new SteamAchievementEventBroker(steamManager);
	}

	public int SetProgress(AchievementCategory category, int progress)
	{
		if (!steamManager.IsInitialized)
		{
			return -1;
		}
		progress = Mathf.Max(progress, 0);
		if (!TrySetStat(category, progress))
		{
			return -1;
		}
		return progress;
	}

	public int ProgressIncrement(AchievementCategory category, int count = 1)
	{
		if (!steamManager.IsInitialized)
		{
			return -1;
		}
		if (!TryGetStat(category, out var progress))
		{
			return -1;
		}
		progress = Mathf.Max(progress + count, 0);
		if (!TrySetStat(category, progress))
		{
			return -1;
		}
		return progress;
	}

	private bool TryGetStat(AchievementCategory achievement, out int progress)
	{
		if (SteamUserStats.GetStat(achievement.ToName(), out progress))
		{
			SetLocalProgress(achievement, progress);
			return true;
		}
		return false;
	}

	private bool TrySetStat(AchievementCategory achievement, int progress)
	{
		if (SteamUserStats.SetStat(achievement.ToName(), progress))
		{
			SetLocalProgress(achievement, progress);
			SteamUserStats.StoreStats();
			return true;
		}
		return false;
	}

	private AchievementStats SetLocalProgress(AchievementCategory category, int progress)
	{
		if (achievements.TryGetFirst((AchievementStats a) => a.AchievementCategory == category, out var first))
		{
			first.Progress.Value = progress;
			return first;
		}
		achievements.Add(first = AchievementStats.Create(category, progress));
		return first;
	}

	public AchievementStats GetAchievement(AchievementCategory category)
	{
		if (achievements.TryGetFirst((AchievementStats a) => a.AchievementCategory == category, out var first))
		{
			return first;
		}
		if (TryGetStat(category, out var progress))
		{
			first = AchievementStats.Create(category, progress);
			achievements.Add(first);
			return first;
		}
		return null;
	}

	IEnumerator<AchievementStats> IEnumerable<AchievementStats>.GetEnumerator()
	{
		return achievements.GetEnumerator();
	}

	public List<AchievementStats>.Enumerator GetEnumerator()
	{
		return achievements.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
