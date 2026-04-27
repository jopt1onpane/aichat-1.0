using Bulbul.MasterData;

namespace Bulbul;

public interface ITalkSelector
{
	void Setup();

	bool IsNeedPlayVoice();

	int TakeNextVoice();

	ScenarioType GetScenarioType();
}
