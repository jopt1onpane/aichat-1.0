using Bulbul.MasterData;

namespace Bulbul;

public class GameEndTalkSelector
{
	private ITalkSelector _longEndTalkSelector = new GameLongTimeEndTalkSelector();

	private ITalkSelector _shortEndTalkSelector = new GameShortTimeEndTalkSelector();

	private ITalkSelector _normalEndTalkSelector = new GameNormalEndTalkSelector();

	public void Setup()
	{
		_longEndTalkSelector.Setup();
		_shortEndTalkSelector.Setup();
		_normalEndTalkSelector.Setup();
	}

	public ScenarioInfo TakeNextVoice()
	{
		ITalkSelector talkSelector = null;
		if (_longEndTalkSelector.IsNeedPlayVoice())
		{
			talkSelector = _longEndTalkSelector;
		}
		else if (_shortEndTalkSelector.IsNeedPlayVoice())
		{
			talkSelector = _shortEndTalkSelector;
		}
		else if (_normalEndTalkSelector.IsNeedPlayVoice())
		{
			talkSelector = _normalEndTalkSelector;
		}
		if (talkSelector != null)
		{
			ScenarioType scenarioType = talkSelector.GetScenarioType();
			int episodeNumber = talkSelector.TakeNextVoice();
			return new ScenarioInfo(scenarioType, episodeNumber);
		}
		return null;
	}
}
