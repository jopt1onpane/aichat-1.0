using Bulbul.MasterData;
using NestopiSystem.DIContainers;
using UnityEngine;

namespace Bulbul;

public class ContinuousWorkStartVoiceSelector : ITalkSelector
{
	private RandomList _voiceList = new RandomList();

	private PomodoroService _pomodoroService;

	private ContinuousWorkStartVoiceData _voiceData;

	public void Setup()
	{
		_pomodoroService = RoomLifetimeScope.Resolve<PomodoroService>();
		_voiceData = ProjectLifetimeScope.Resolve<MasterDataLoader>().GamePomodoroVoiceData.ContinuousWorkStartVoiceData;
		_voiceList.Setup(1, 10);
	}

	public bool IsNeedPlayVoice()
	{
		if (_pomodoroService.CurrentLoopCount.CurrentValue != 1)
		{
			return false;
		}
		float num = Time.time - _pomodoroService.LastWorkEndTimeSeconds;
		float num2 = _voiceData.ContinuousMinutes * 60f;
		if (num > num2)
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
		return ScenarioType.SpeakWord_Pomodoro_WorkContinuousStart;
	}
}
