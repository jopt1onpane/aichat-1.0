using System;
using System.Collections.Generic;
using Bulbul.MasterData;

namespace Bulbul;

public class CurrentTime7HourSelfTalkSelector : ITalkSelector
{
	private RandomList _episodeList = new RandomList();

	private HashSet<int> _playedEpisodes = new HashSet<int>();

	private DateTime _playedDate = DateTime.MinValue;

	public void Setup()
	{
		_episodeList.Setup(1, 1);
	}

	public bool IsNeedPlayVoice()
	{
		ResetIfNewDay();
		if (_playedEpisodes.Contains(1))
		{
			return false;
		}
		DateTime now = DateTime.Now;
		if (now.Hour != 7 || now.Minute > 29)
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
		_playedEpisodes.Add(next);
		_playedDate = DateTime.Now.Date;
		return next;
	}

	public ScenarioType GetScenarioType()
	{
		return ScenarioType.HeroineSelfShortTalk_CurrentTime;
	}

	private void ResetIfNewDay()
	{
		if (_playedDate != DateTime.MinValue && _playedDate != DateTime.Now.Date)
		{
			_playedEpisodes.Clear();
			_episodeList.ForceResetIfEmpty();
			_playedDate = DateTime.MinValue;
		}
	}
}
