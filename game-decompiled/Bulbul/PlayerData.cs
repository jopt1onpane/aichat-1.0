using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[Obsolete]
[DoNotObfuscateClass]
public class PlayerData
{
	public LevelData LevelData;

	[ES3Serializable]
	private string _lastLoginDateTimeString;

	[ES3Serializable]
	public int TotalLoginCount;

	[ES3Serializable]
	public bool IsNeedTutorial = true;

	[ES3Serializable]
	public double CurrentWorkSeconds;

	[ES3Serializable]
	public double PomodoroTotalWorkSeconds;

	[ES3Serializable]
	public double LastStoryUnlockWorkSeconds;

	public int Point;

	[ES3Serializable]
	public float DemoRemainSeconds;

	[ES3Serializable]
	public bool IsNeedDemoStoryEndAnnounce;

	[ES3Serializable]
	public bool HasReceivedFirstPoint;

	[ES3NonSerializable]
	public DateTime LastLoginDateTime => DateTime.ParseExact(_lastLoginDateTimeString, "yyyyMMddHHmmss", null);

	public bool IsFirstLogin => _lastLoginDateTimeString == string.Empty;

	public TimeSpan PomodoroTotalWorkTime => TimeSpan.FromSeconds(PomodoroTotalWorkSeconds);

	public TimeSpan LastStoryUnlockWorkTime => TimeSpan.FromSeconds(LastStoryUnlockWorkSeconds);

	public PlayerData()
	{
		LevelData = new LevelData();
		TotalLoginCount = 0;
		IsNeedTutorial = true;
		DemoRemainSeconds = 9000f;
		IsNeedDemoStoryEndAnnounce = true;
		_lastLoginDateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
	}

	public void SetLastLoginDateTime()
	{
		_lastLoginDateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
	}

	public void Login()
	{
		if (string.IsNullOrEmpty(_lastLoginDateTimeString))
		{
			TotalLoginCount++;
		}
		else if (LastLoginDateTime.Year != DateTime.Now.Year || LastLoginDateTime.Month != DateTime.Now.Month || LastLoginDateTime.Day != DateTime.Now.Day)
		{
			TotalLoginCount++;
		}
	}

	public PlayerDataV3 ToV3()
	{
		PlayerDataV3 playerDataV = new PlayerDataV3();
		playerDataV.LevelData = LevelData;
		playerDataV.TotalLoginCount = TotalLoginCount;
		playerDataV.IsNeedTutorial = IsNeedTutorial;
		playerDataV.CurrentWorkSeconds = CurrentWorkSeconds;
		playerDataV.PomodoroTotalWorkSeconds = PomodoroTotalWorkSeconds;
		playerDataV.LastStoryUnlockWorkSeconds = LastStoryUnlockWorkSeconds;
		playerDataV.DemoRemainSeconds = DemoRemainSeconds;
		playerDataV.IsNeedDemoStoryEndAnnounce = IsNeedDemoStoryEndAnnounce;
		playerDataV.HasReceivedFirstPoint = HasReceivedFirstPoint;
		playerDataV.SetLastLoginDateTimeStringForMigration(_lastLoginDateTimeString);
		return playerDataV;
	}
}
