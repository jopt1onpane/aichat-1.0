using System;

namespace Bulbul.MasterData;

[Serializable]
public class ScenarioGroupData
{
	public string ID;

	public string TitleLocalizationID;

	public string SubtitleLocalizationID;

	public ScenarioType Scenario;

	public float EpisodeNumber;

	public int UnlockLevel;

	public string UnlockStoryEpisode;

	public string PlayTiming;

	public string ContentType;

	public string Arg1;

	public string Arg2;

	public string Arg3;
}
