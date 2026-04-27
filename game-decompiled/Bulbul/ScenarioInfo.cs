using Bulbul.MasterData;

namespace Bulbul;

public class ScenarioInfo
{
	public ScenarioType ScenarioType { get; private set; }

	public int EpisodeNumber { get; private set; }

	public ScenarioInfo(ScenarioType scenarioType, int episodeNumber)
	{
		ScenarioType = scenarioType;
		EpisodeNumber = episodeNumber;
	}
}
