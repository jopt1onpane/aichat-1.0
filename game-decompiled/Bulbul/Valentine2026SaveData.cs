using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class Valentine2026SaveData
{
	public int NextEpisodeNumber;

	public int LastReadEpisodeNumber;

	public bool IsNeedUseNewIcon;

	public bool IsFinishedUsePossibleAnnounce;

	public bool IsFinishedCompleteAnnounce;

	public LevelData LevelData;

	public Valentine2026SaveData()
	{
		NextEpisodeNumber = 1;
		LastReadEpisodeNumber = 0;
		IsNeedUseNewIcon = true;
		IsFinishedUsePossibleAnnounce = false;
		IsFinishedCompleteAnnounce = false;
		LevelData = new LevelData();
	}
}
