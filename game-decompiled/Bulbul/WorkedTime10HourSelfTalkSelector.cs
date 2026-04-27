using System;
using Bulbul.MasterData;

namespace Bulbul;

public class WorkedTime10HourSelfTalkSelector : ITalkSelector
{
	private RandomList _episodeList = new RandomList();

	private DateTime _playedDate = DateTime.MinValue;

	public void Setup()
	{
		_episodeList.Setup(3, 4);
	}

	public bool IsNeedPlayVoice()
	{
		if (_playedDate == DateTime.Now.Date)
		{
			return false;
		}
		return ProbabilityUtility.IsOccurredInPercent(20f);
	}

	public int TakeNextVoice()
	{
		if (_episodeList.IsEmpty)
		{
			_episodeList.ForceResetIfEmpty();
		}
		int next = _episodeList.GetNext();
		_episodeList.UseNext();
		_playedDate = DateTime.Now.Date;
		return next;
	}

	public ScenarioType GetScenarioType()
	{
		return ScenarioType.HeroineSelfShortTalk_WorkedTime;
	}
}
