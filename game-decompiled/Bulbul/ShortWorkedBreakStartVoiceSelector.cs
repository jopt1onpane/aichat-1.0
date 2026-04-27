using Bulbul.MasterData;
using NestopiSystem.DIContainers;
using UnityEngine;

namespace Bulbul;

public class ShortWorkedBreakStartVoiceSelector : ITalkSelector
{
	private RandomList _voiceList = new RandomList();

	private PomodoroService _pomodoroService;

	private ShortWorkedBreakStartVoiceData _voiceData;

	public void Setup()
	{
		_pomodoroService = RoomLifetimeScope.Resolve<PomodoroService>();
		_voiceData = ProjectLifetimeScope.Resolve<MasterDataLoader>().GamePomodoroVoiceData.ShortWorkedBreakStartVoiceData;
		_voiceList.Setup(1, 10);
	}

	public bool IsNeedPlayVoice()
	{
		float num = Time.time - _pomodoroService.LastWorkEndTimeSeconds;
		float shortWorkMinutes = _voiceData.ShortWorkMinutes;
		if (num > shortWorkMinutes * 60f)
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
		return ScenarioType.SpeakWord_Pomodoro_ShortWorkedBreakStart;
	}
}
