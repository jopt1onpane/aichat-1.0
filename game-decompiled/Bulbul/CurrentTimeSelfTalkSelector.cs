namespace Bulbul;

public class CurrentTimeSelfTalkSelector
{
	public enum CurrentTimeSelfTalkEpisodeNumber
	{
		CurrentTime_7Hour_1 = 1,
		CurrentTime_12Hour_1,
		CurrentTime_17Hour_1,
		CurrentTime_24Hour_1,
		CurrentTime_24Hour_2
	}

	private ITalkSelector _7HourSelector = new CurrentTime7HourSelfTalkSelector();

	private ITalkSelector _12HourSelector = new CurrentTime12HourSelfTalkSelector();

	private ITalkSelector _17HourSelector = new CurrentTime17HourSelfTalkSelector();

	private ITalkSelector _24HourSelector = new CurrentTime24HourSelfTalkSelector();

	public void Setup()
	{
		_7HourSelector.Setup();
		_12HourSelector.Setup();
		_17HourSelector.Setup();
		_24HourSelector.Setup();
	}

	public ScenarioInfo TakeNextVoice()
	{
		ITalkSelector talkSelector = null;
		if (_7HourSelector.IsNeedPlayVoice())
		{
			talkSelector = _7HourSelector;
		}
		else if (_12HourSelector.IsNeedPlayVoice())
		{
			talkSelector = _12HourSelector;
		}
		else if (_17HourSelector.IsNeedPlayVoice())
		{
			talkSelector = _17HourSelector;
		}
		else if (_24HourSelector.IsNeedPlayVoice())
		{
			talkSelector = _24HourSelector;
		}
		if (talkSelector == null)
		{
			return null;
		}
		return new ScenarioInfo(talkSelector.GetScenarioType(), talkSelector.TakeNextVoice());
	}
}
