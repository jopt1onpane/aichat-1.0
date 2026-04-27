using Bulbul.MasterData;

namespace Bulbul;

public class LeaveStartVoiceSelector : ITalkSelector
{
	private RandomList _voiceList = new RandomList();

	public void Setup()
	{
		_voiceList.Setup(1, 4);
	}

	public bool IsNeedPlayVoice()
	{
		return true;
	}

	public int TakeNextVoice()
	{
		int next = _voiceList.GetNext();
		_voiceList.UseNext();
		return next;
	}

	public ScenarioType GetScenarioType()
	{
		return ScenarioType.SpeakWord_PomodoroStart;
	}
}
