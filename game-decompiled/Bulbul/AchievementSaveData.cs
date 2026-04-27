using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class AchievementSaveData
{
	[ES3Serializable]
	private List<string> _unlockedAchievements = new List<string>();

	public bool IsUnlocked(string achievement)
	{
		return _unlockedAchievements.Contains(achievement);
	}

	public void AddUnlocked(string achievement)
	{
		_unlockedAchievements.Add(achievement);
	}
}
