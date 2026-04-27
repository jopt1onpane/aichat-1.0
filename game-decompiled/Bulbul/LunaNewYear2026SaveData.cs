using System;
using GUPS.Obfuscator.Attribute;
using R3;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class LunaNewYear2026SaveData
{
	public int NextEpisodeNumber;

	public int LastReadEpisodeNumber;

	public bool IsNeedUseNewIcon;

	public bool IsFinishedUsePossibleAnnounce;

	public bool IsFinishedCompleteAnnounce;

	public LevelData LevelData;

	[ES3Serializable]
	public SerializableReactiveProperty<bool> IsValid { get; private set; }

	public LunaNewYear2026SaveData()
	{
		NextEpisodeNumber = 1;
		LastReadEpisodeNumber = 0;
		IsNeedUseNewIcon = true;
		IsFinishedUsePossibleAnnounce = false;
		IsFinishedCompleteAnnounce = false;
		LevelData = new LevelData();
		IsValid = new SerializableReactiveProperty<bool>(value: false);
	}
}
