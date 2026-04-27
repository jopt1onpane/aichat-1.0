using Bulbul.MasterData;
using NestopiSystem.DIContainers;
using UnityEngine;

namespace Bulbul;

public class LongWorkedBreakStartVoiceSelector : ITalkSelector
{
	private RandomList _voiceList = new RandomList();

	private PomodoroService _pomodoroService;

	private LongWorkedBreakStartVoiceData _voiceData;

	public void Setup()
	{
		_pomodoroService = RoomLifetimeScope.Resolve<PomodoroService>();
		_voiceData = ProjectLifetimeScope.Resolve<MasterDataLoader>().GamePomodoroVoiceData.LongWorkedBreakStartVoiceData;
		_voiceList.Setup(1, 6);
	}

	public bool IsNeedPlayVoice()
	{
		float num = Time.time - _pomodoroService.LastWorkStartTimeSeconds;
		float judgeLongMinutes = _voiceData.JudgeLongMinutes;
		if (num < judgeLongMinutes * 60f)
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
		return ScenarioType.SpeakWord_Pomodoro_LongWorkedBreakStart;
	}
}
