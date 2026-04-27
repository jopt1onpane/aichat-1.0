using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class BearsRestaurantSaveData
{
	public int NextEpisodeNumber;

	public int LastReadEpisodeNumber;

	public bool IsNeedUseNewIcon;

	public bool IsFinishedUsePossibleAnnounce;

	public bool IsFinishedCompleteAnnounce;

	public LevelData LevelData;

	public BearsRestaurantSaveData()
	{
		NextEpisodeNumber = 1;
		LastReadEpisodeNumber = 0;
		IsNeedUseNewIcon = true;
		IsFinishedUsePossibleAnnounce = false;
		IsFinishedCompleteAnnounce = false;
		LevelData = new LevelData();
	}
}
