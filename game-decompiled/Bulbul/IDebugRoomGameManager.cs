using Bulbul.MasterData;

namespace Bulbul;

public interface IDebugRoomGameManager
{
	void DebugSkipSelectStory(int episodeNumber);

	void DebuPlayStory(ScenarioType scenarioType, float episodeNumber);

	void DebugSkipAllStory();

	void DebugPlayScenario(string labelId);

	void DebugPlayerLevelReflesh();
}
