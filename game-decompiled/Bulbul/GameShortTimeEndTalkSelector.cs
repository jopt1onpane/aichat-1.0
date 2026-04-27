using System;
using Bulbul.MasterData;
using NestopiSystem.DIContainers;

namespace Bulbul;

public class GameShortTimeEndTalkSelector : ITalkSelector
{
	private RandomList _voiceList = new RandomList();

	private GameShortTimeEndTalkData _talkData;

	public void Setup()
	{
		_talkData = ProjectLifetimeScope.Resolve<MasterDataLoader>().GameEndTalkData.GameShortTimeEndTalkData;
		_voiceList.Setup(1, 5);
	}

	public bool IsNeedPlayVoice()
	{
		if ((DateTime.Now - SaveDataManager.Instance.PlayerData.LastLoginDateTime).TotalHours > (double)_talkData.JudgeShortHours)
		{
			return false;
		}
		if (!ProbabilityUtility.IsOccurredInPercent(_talkData.SelectionProbability))
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
		return ScenarioType.GameShortTimeEnd;
	}
}
