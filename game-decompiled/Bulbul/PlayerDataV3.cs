using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class PlayerDataV3
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

	[ES3Serializable]
	public float DemoRemainSeconds;

	[ES3Serializable]
	public bool IsNeedDemoStoryEndAnnounce;

	[ES3Serializable]
	public bool HasReceivedFirstPoint;

	[ES3Serializable]
	public ulong ReadNewsID;

	[ES3NonSerializable]
	public DateTime LastLoginDateTime => DateTime.ParseExact(_lastLoginDateTimeString, "yyyyMMddHHmmss", null);

	public bool IsFirstLogin => _lastLoginDateTimeString == string.Empty;

	public TimeSpan PomodoroTotalWorkTime => TimeSpan.FromSeconds(PomodoroTotalWorkSeconds);

	public TimeSpan LastStoryUnlockWorkTime => TimeSpan.FromSeconds(LastStoryUnlockWorkSeconds);

	public PlayerDataV3()
	{
		LevelData = new LevelData();
		TotalLoginCount = 0;
		IsNeedTutorial = true;
		DemoRemainSeconds = 9000f;
		IsNeedDemoStoryEndAnnounce = true;
		_lastLoginDateTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
		ReadNewsID = 0uL;
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

	internal void SetLastLoginDateTimeStringForMigration(string value)
	{
		_lastLoginDateTimeString = value ?? string.Empty;
	}
}
