using Bulbul.MasterData;
using NestopiSystem.DIContainers;

namespace Bulbul;

public class LongWorkFinishVoiceSelector : ITalkSelector
{
	private RandomList _voiceList = new RandomList();

	private PomodoroService _pomodoroService;

	private LongWorkFinishVoiceData _voiceData;

	public void Setup()
	{
		_pomodoroService = RoomLifetimeScope.Resolve<PomodoroService>();
		_voiceData = ProjectLifetimeScope.Resolve<MasterDataLoader>().GamePomodoroVoiceData.LongWorkFinishVoiceData;
		_voiceList.Setup(1, 7);
	}

	public bool IsNeedPlayVoice()
	{
		float lastPomodoroTotalWorkHours = _pomodoroService.LastPomodoroTotalWorkHours;
		float num = _voiceData.JudgeLongMinutes / 60f;
		if (lastPomodoroTotalWorkHours < num)
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
		return ScenarioType.SpeakWord_Pomodoro_LongWorkFinish;
	}
}
