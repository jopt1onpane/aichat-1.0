using Bulbul.MasterData;
using NestopiSystem.DIContainers;

namespace Bulbul;

public class PomodoroMidwayFinishVoiceSelector : ITalkSelector
{
	private RandomList _voiceList = new RandomList();

	private PomodoroService _pomodoroService;

	private LongWorkStartVoiceData _voiceData;

	public void Setup()
	{
		_pomodoroService = RoomLifetimeScope.Resolve<PomodoroService>();
		_voiceData = ProjectLifetimeScope.Resolve<MasterDataLoader>().GamePomodoroVoiceData.LongWorkStartVoiceData;
		_voiceList.Setup(1, 10);
	}

	public bool IsNeedPlayVoice()
	{
		if (!_pomodoroService.IsLastPomodoroFinishedMidway)
		{
			return false;
		}
		if (!ProbabilityUtility.IsOccurredInPercent(_voiceData.SelectionProbability))
		{
			return false;
		}
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
		return ScenarioType.SpeakWord_Pomodoro_MidwayFinish;
	}
}
