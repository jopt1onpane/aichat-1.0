using Bulbul;
using Bulbul.MasterData;

public class PomodoroVoiceSelector
{
	private ITalkSelector _normalWorkStartVoiceSelector = new NormalWorkStartVoiceSelector();

	private ITalkSelector _longWorkStartVoiceSelector = new LongWorkStartVoiceSelector();

	private ITalkSelector _continuousWorkStartVoiceSelector = new ContinuousWorkStartVoiceSelector();

	private ITalkSelector _normalBreakStartVoiceSelector = new NormalBreakStartVoiceSelector();

	private ITalkSelector _shortWorkedBreakStartVoiceSelector = new ShortWorkedBreakStartVoiceSelector();

	private ITalkSelector _longWorkedBreakStartVoiceSelector = new LongWorkedBreakStartVoiceSelector();

	private ITalkSelector _normalPomodoroFinishVoiceSelector = new NormalPomodoroFinishVoiceSelector();

	private ITalkSelector _longWorkFinishVoiceSelector = new LongWorkFinishVoiceSelector();

	private ITalkSelector _shortWorkFinishVoiceSelector = new ShortWorkFinishVoiceSelector();

	private ITalkSelector _midwayFinishVoiceSelector = new PomodoroMidwayFinishVoiceSelector();

	public ScenarioType ScenarioType { get; private set; }

	public void Setup()
	{
		ScenarioType = ScenarioType.None;
		_normalWorkStartVoiceSelector.Setup();
		_longWorkStartVoiceSelector.Setup();
		_continuousWorkStartVoiceSelector.Setup();
		_normalBreakStartVoiceSelector.Setup();
		_shortWorkedBreakStartVoiceSelector.Setup();
		_longWorkedBreakStartVoiceSelector.Setup();
		_normalPomodoroFinishVoiceSelector.Setup();
		_longWorkFinishVoiceSelector.Setup();
		_shortWorkFinishVoiceSelector.Setup();
		_midwayFinishVoiceSelector.Setup();
	}

	public ScenarioInfo TakeNextWorkStartVoice()
	{
		ITalkSelector talkSelector = null;
		if (_longWorkStartVoiceSelector.IsNeedPlayVoice())
		{
			talkSelector = _longWorkStartVoiceSelector;
		}
		else if (_continuousWorkStartVoiceSelector.IsNeedPlayVoice())
		{
			talkSelector = _continuousWorkStartVoiceSelector;
		}
		else if (_normalWorkStartVoiceSelector.IsNeedPlayVoice())
		{
			talkSelector = _normalWorkStartVoiceSelector;
		}
		if (talkSelector != null)
		{
			ScenarioType scenarioType = talkSelector.GetScenarioType();
			int episodeNumber = talkSelector.TakeNextVoice();
			return new ScenarioInfo(scenarioType, episodeNumber);
		}
		return null;
	}

	public ScenarioInfo TakeNextBreakStartVoice()
	{
		ITalkSelector talkSelector = null;
		if (_shortWorkedBreakStartVoiceSelector.IsNeedPlayVoice())
		{
			talkSelector = _shortWorkedBreakStartVoiceSelector;
		}
		if (_longWorkedBreakStartVoiceSelector.IsNeedPlayVoice())
		{
			talkSelector = _longWorkedBreakStartVoiceSelector;
		}
		else if (_normalBreakStartVoiceSelector.IsNeedPlayVoice())
		{
			talkSelector = _normalBreakStartVoiceSelector;
		}
		if (talkSelector != null)
		{
			ScenarioType scenarioType = talkSelector.GetScenarioType();
			int episodeNumber = talkSelector.TakeNextVoice();
			return new ScenarioInfo(scenarioType, episodeNumber);
		}
		return null;
	}

	public ScenarioInfo TakeNextFinishVoice()
	{
		ITalkSelector talkSelector = null;
		if (_longWorkFinishVoiceSelector.IsNeedPlayVoice())
		{
			talkSelector = _longWorkFinishVoiceSelector;
		}
		else if (_shortWorkFinishVoiceSelector.IsNeedPlayVoice())
		{
			talkSelector = _shortWorkFinishVoiceSelector;
		}
		else if (_midwayFinishVoiceSelector.IsNeedPlayVoice())
		{
			talkSelector = _midwayFinishVoiceSelector;
		}
		else if (_normalPomodoroFinishVoiceSelector.IsNeedPlayVoice())
		{
			talkSelector = _normalPomodoroFinishVoiceSelector;
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
