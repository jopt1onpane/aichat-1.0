using Bulbul;
using Bulbul.MasterData;

public class ReactionTalkSelector
{
	private CurrentTimeSelfTalkSelector _currentTimeSelfTalkSelector = new CurrentTimeSelfTalkSelector();

	private WorkedTimeSelfTalkSelector _workedTimeSelfTalkSelector = new WorkedTimeSelfTalkSelector();

	private RandomList _normalClickShortTalkList = new RandomList();

	private RandomList _workClickShortTalkList = new RandomList();

	private RandomList _workClickShortTalkMorningList = new RandomList();

	private RandomList _workClickShortTalkNoonList = new RandomList();

	private RandomList _workClickShortTalkEveningList = new RandomList();

	private RandomList _workClickShortTalkNightList = new RandomList();

	private RandomList _breakClickShortTalkList = new RandomList();

	private RandomList _breakClickShortTalkMorningList = new RandomList();

	private RandomList _breakClickShortTalkNoonList = new RandomList();

	private RandomList _breakClickShortTalkEveningList = new RandomList();

	private RandomList _breakClickShortTalkNightList = new RandomList();

	private RandomList _breakHeroineSelShortTalkfList = new RandomList();

	private RandomList _breakHeroineSelfShortTalkMorningList = new RandomList();

	private RandomList _breakHeroineSelfShortTalkNoonList = new RandomList();

	private RandomList _breakHeroineSelfShortTalkEveningList = new RandomList();

	private RandomList _breakHeroineSelfShortTalkNightList = new RandomList();

	private RandomList _answerChoiceEpisodeList = new RandomList();

	public void Setup()
	{
		_currentTimeSelfTalkSelector.Setup();
		_workedTimeSelfTalkSelector.Setup();
		_normalClickShortTalkList.Setup(1, 23);
		_workClickShortTalkList.Setup(1, 23);
		_workClickShortTalkMorningList.Setup(1, 5);
		_workClickShortTalkNoonList.Setup(1, 5);
		_workClickShortTalkEveningList.Setup(1, 5);
		_workClickShortTalkNightList.Setup(1, 5);
		_breakClickShortTalkList.Setup(1, 20);
		_breakClickShortTalkMorningList.Setup(1, 10);
		_breakClickShortTalkNoonList.Setup(1, 5);
		_breakClickShortTalkEveningList.Setup(1, 5);
		_breakClickShortTalkNightList.Setup(1, 11);
		_breakHeroineSelShortTalkfList.Setup(1, 22);
		_breakHeroineSelfShortTalkMorningList.Setup(1, 7);
		_breakHeroineSelfShortTalkNoonList.Setup(1, 9);
		_breakHeroineSelfShortTalkEveningList.Setup(1, 7);
		_breakHeroineSelfShortTalkNightList.Setup(1, 7);
		_answerChoiceEpisodeList.Setup(1, 20);
	}

	public int GetNextNormalClickShortTalk()
	{
		return _normalClickShortTalkList.GetNext();
	}

	public void UseNextNormalClickShortTalk()
	{
		_normalClickShortTalkList.UseNext();
	}

	public void RemoveSelectNumberNormalClickShortTalk(int useNumber)
	{
		_normalClickShortTalkList.RemoveFromList(useNumber);
	}

	public (ScenarioType scenarioType, int episodeNumber) GetNextWorkClickShortTalkSelection(TimeOfDayProvider.TimeOfDayType timeOfDayType)
	{
		RandomList workTimeOfDayList = GetWorkTimeOfDayList(timeOfDayType);
		bool isEmpty = _workClickShortTalkList.IsEmpty;
		bool isEmpty2 = workTimeOfDayList.IsEmpty;
		if (isEmpty && isEmpty2)
		{
			_workClickShortTalkList.ForceResetIfEmpty();
			workTimeOfDayList.ForceResetIfEmpty();
		}
		if ((isEmpty && !isEmpty2) || (!(!isEmpty && isEmpty2) && ProbabilityUtility.IsOccurredInPercent(50f)))
		{
			int next = workTimeOfDayList.GetNext();
			workTimeOfDayList.UseNext();
			return (scenarioType: GetWorkTimeOfDayScenarioType(timeOfDayType), episodeNumber: next);
		}
		int next2 = _workClickShortTalkList.GetNext();
		_workClickShortTalkList.UseNext();
		return (scenarioType: ScenarioType.HeroineClickWork, episodeNumber: next2);
	}

	private RandomList GetWorkTimeOfDayList(TimeOfDayProvider.TimeOfDayType timeOfDayType)
	{
		return timeOfDayType switch
		{
			TimeOfDayProvider.TimeOfDayType.Morning => _workClickShortTalkMorningList, 
			TimeOfDayProvider.TimeOfDayType.Noon => _workClickShortTalkNoonList, 
			TimeOfDayProvider.TimeOfDayType.Evening => _workClickShortTalkEveningList, 
			TimeOfDayProvider.TimeOfDayType.Night => _workClickShortTalkNightList, 
			_ => _workClickShortTalkMorningList, 
		};
	}

	private static ScenarioType GetWorkTimeOfDayScenarioType(TimeOfDayProvider.TimeOfDayType timeOfDayType)
	{
		return timeOfDayType switch
		{
			TimeOfDayProvider.TimeOfDayType.Morning => ScenarioType.HeroineClickWork_Morning, 
			TimeOfDayProvider.TimeOfDayType.Noon => ScenarioType.HeroineClickWork_Noon, 
			TimeOfDayProvider.TimeOfDayType.Evening => ScenarioType.HeroineClickWork_Evening, 
			TimeOfDayProvider.TimeOfDayType.Night => ScenarioType.HeroineClickWork_Night, 
			_ => ScenarioType.HeroineClickWork_Morning, 
		};
	}

	public (ScenarioType scenarioType, int episodeNumber) GetNextBreakClickShortTalkSelection(TimeOfDayProvider.TimeOfDayType timeOfDayType)
	{
		RandomList breakClickTimeOfDayList = GetBreakClickTimeOfDayList(timeOfDayType);
		bool isEmpty = _breakClickShortTalkList.IsEmpty;
		bool isEmpty2 = breakClickTimeOfDayList.IsEmpty;
		if (isEmpty && isEmpty2)
		{
			_breakClickShortTalkList.ForceResetIfEmpty();
			breakClickTimeOfDayList.ForceResetIfEmpty();
		}
		if ((isEmpty && !isEmpty2) || (!(!isEmpty && isEmpty2) && ProbabilityUtility.IsOccurredInPercent(50f)))
		{
			int next = breakClickTimeOfDayList.GetNext();
			breakClickTimeOfDayList.UseNext();
			return (scenarioType: GetBreakClickTimeOfDayScenarioType(timeOfDayType), episodeNumber: next);
		}
		int next2 = _breakClickShortTalkList.GetNext();
		_breakClickShortTalkList.UseNext();
		return (scenarioType: ScenarioType.HeroineClickBreak, episodeNumber: next2);
	}

	private RandomList GetBreakClickTimeOfDayList(TimeOfDayProvider.TimeOfDayType timeOfDayType)
	{
		return timeOfDayType switch
		{
			TimeOfDayProvider.TimeOfDayType.Morning => _breakClickShortTalkMorningList, 
			TimeOfDayProvider.TimeOfDayType.Noon => _breakClickShortTalkNoonList, 
			TimeOfDayProvider.TimeOfDayType.Evening => _breakClickShortTalkEveningList, 
			TimeOfDayProvider.TimeOfDayType.Night => _breakClickShortTalkNightList, 
			_ => _breakClickShortTalkMorningList, 
		};
	}

	private static ScenarioType GetBreakClickTimeOfDayScenarioType(TimeOfDayProvider.TimeOfDayType timeOfDayType)
	{
		return timeOfDayType switch
		{
			TimeOfDayProvider.TimeOfDayType.Morning => ScenarioType.HeroineClickBreak_Morning, 
			TimeOfDayProvider.TimeOfDayType.Noon => ScenarioType.HeroineClickBreak_Noon, 
			TimeOfDayProvider.TimeOfDayType.Evening => ScenarioType.HeroineClickBreak_Evening, 
			TimeOfDayProvider.TimeOfDayType.Night => ScenarioType.HeroineClickBreak_Night, 
			_ => ScenarioType.HeroineClickBreak_Morning, 
		};
	}

	public int GetNextWorkClickShortTalk()
	{
		return _workClickShortTalkList.GetNext();
	}

	public void UseNextWorkClickShortTalk()
	{
		_workClickShortTalkList.UseNext();
	}

	public int GetNextBreakClickShortTalk()
	{
		return _breakClickShortTalkList.GetNext();
	}

	public void UseNextBreakClickShortTalk()
	{
		_breakClickShortTalkList.UseNext();
	}

	public void RemoveSelectNumberBreakClickShortTalk(int useNumber)
	{
		_breakClickShortTalkList.RemoveFromList(useNumber);
	}

	public (ScenarioType scenarioType, int episodeNumber) GetNextBreakHeroineSelfShortTalkSelection(TimeOfDayProvider.TimeOfDayType timeOfDayType)
	{
		ScenarioInfo scenarioInfo = _currentTimeSelfTalkSelector.TakeNextVoice();
		if (scenarioInfo != null)
		{
			return (scenarioType: scenarioInfo.ScenarioType, episodeNumber: scenarioInfo.EpisodeNumber);
		}
		ScenarioInfo scenarioInfo2 = _workedTimeSelfTalkSelector.TakeNextVoice();
		if (scenarioInfo2 != null)
		{
			return (scenarioType: scenarioInfo2.ScenarioType, episodeNumber: scenarioInfo2.EpisodeNumber);
		}
		RandomList timeOfDayList = GetTimeOfDayList(timeOfDayType);
		bool isEmpty = _breakHeroineSelShortTalkfList.IsEmpty;
		bool isEmpty2 = timeOfDayList.IsEmpty;
		if (isEmpty && isEmpty2)
		{
			_breakHeroineSelShortTalkfList.ForceResetIfEmpty();
			timeOfDayList.ForceResetIfEmpty();
		}
		if ((isEmpty && !isEmpty2) || (!(!isEmpty && isEmpty2) && ProbabilityUtility.IsOccurredInPercent(50f)))
		{
			int next = timeOfDayList.GetNext();
			timeOfDayList.UseNext();
			return (scenarioType: GetTimeOfDayScenarioType(timeOfDayType), episodeNumber: next);
		}
		int next2 = _breakHeroineSelShortTalkfList.GetNext();
		_breakHeroineSelShortTalkfList.UseNext();
		return (scenarioType: ScenarioType.HeroineSelfShortTalkBreak, episodeNumber: next2);
	}

	private RandomList GetTimeOfDayList(TimeOfDayProvider.TimeOfDayType timeOfDayType)
	{
		return timeOfDayType switch
		{
			TimeOfDayProvider.TimeOfDayType.Morning => _breakHeroineSelfShortTalkMorningList, 
			TimeOfDayProvider.TimeOfDayType.Noon => _breakHeroineSelfShortTalkNoonList, 
			TimeOfDayProvider.TimeOfDayType.Evening => _breakHeroineSelfShortTalkEveningList, 
			TimeOfDayProvider.TimeOfDayType.Night => _breakHeroineSelfShortTalkNightList, 
			_ => _breakHeroineSelfShortTalkMorningList, 
		};
	}

	private static ScenarioType GetTimeOfDayScenarioType(TimeOfDayProvider.TimeOfDayType timeOfDayType)
	{
		return timeOfDayType switch
		{
			TimeOfDayProvider.TimeOfDayType.Morning => ScenarioType.HeroineSelfShortTalkBreak_Morning, 
			TimeOfDayProvider.TimeOfDayType.Noon => ScenarioType.HeroineSelfShortTalkBreak_Noon, 
			TimeOfDayProvider.TimeOfDayType.Evening => ScenarioType.HeroineSelfShortTalkBreak_Evening, 
			TimeOfDayProvider.TimeOfDayType.Night => ScenarioType.HeroineSelfShortTalkBreak_Night, 
			_ => ScenarioType.HeroineSelfShortTalkBreak_Morning, 
		};
	}

	public int GetNextBreakHeroineSelfShortTalk()
	{
		return _breakHeroineSelShortTalkfList.GetNext();
	}

	public void UseNextBreakHeroineSelfShortTalk()
	{
		_breakHeroineSelShortTalkfList.UseNext();
	}

	public void RemoveSelectNumberBreakHeroineSelfShortTalk(int useNumber)
	{
		_breakHeroineSelShortTalkfList.RemoveFromList(useNumber);
	}

	public int GetNextAnswerChoiceEpisode()
	{
		return _answerChoiceEpisodeList.GetNext();
	}

	public void UseNextAnswerChoiceEpisode()
	{
		_answerChoiceEpisodeList.UseNext();
	}
}
