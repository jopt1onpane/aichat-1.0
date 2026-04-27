using System;
using System.Collections.Generic;
using System.Linq;
using Bulbul;
using Bulbul.MasterData;
using UnityEngine;
using VContainer;

public class SmallTalkSelector
{
	public enum PlayTiming
	{
		None,
		Break,
		GameStart,
		GameStartAndBreak
	}

	public enum ContentType
	{
		None,
		SmallTalk_CatHeadphone,
		SmallTalk_Cicada,
		SmallTalk_PassageOfTime,
		PurchaseEnvironment
	}

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private ScenarioGroupMasterWrapper _scenarioGroupMaster;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private IEnvironmentUIService _environmentUIService;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private AchievementService _achievementService;

	[Inject]
	private DecorationDataService _decorationDataService;

	private Dictionary<float, ScenarioGroupData> _cache;

	private DateTime _beforePlayDateTime;

	private float _beforeEpisodeNumber = -1f;

	private float _currentEpisodeNumber = -1f;

	public float CurrentEpisodeNumber => _currentEpisodeNumber;

	public bool IsDecisionNextEpisode => _currentEpisodeNumber >= 0f;

	public void Setup()
	{
		_cache = _masterDataLoader.ScenarioGroupMasterList.Where((ScenarioGroupData x) => x.Scenario == ScenarioType.SmallTalk).ToDictionary((ScenarioGroupData x) => x.EpisodeNumber, (ScenarioGroupData x) => x);
	}

	public float LotterySmallTalk(PlayTiming currentPlayTiming)
	{
		if (_directionService.GamePlayingDefect.IsUsing())
		{
			return -1f;
		}
		if (!IsElapsedInterval())
		{
			return -1f;
		}
		List<float> notSeenEpisodeList = new List<float>();
		CreateCanPlayEpisodeList(out var seenEpisodeList, out notSeenEpisodeList);
		if (seenEpisodeList.Count == 0 && notSeenEpisodeList.Count == 0)
		{
			return -1f;
		}
		float random = UnityEngine.Random.Range(0f, 100f);
		float num = -1f;
		num = CandidateEpisode(isSeenList: false, notSeenEpisodeList);
		if (num != -1f)
		{
			_currentEpisodeNumber = num;
			return _currentEpisodeNumber;
		}
		num = CandidateEpisode(isSeenList: true, seenEpisodeList);
		if (num != -1f)
		{
			_currentEpisodeNumber = num;
			return _currentEpisodeNumber;
		}
		return -1f;
		float CandidateEpisode(bool isSeenList, List<float> targetEpisodeList)
		{
			List<float> list = new List<float>();
			foreach (float targetEpisode in targetEpisodeList)
			{
				float num2 = 0f;
				num2 = ((!isSeenList) ? float.Parse(_cache[targetEpisode].Arg1) : float.Parse(_cache[targetEpisode].Arg2));
				if (num2 >= random)
				{
					list.Add(targetEpisode);
				}
			}
			float result = -1f;
			if (list.Count > 0)
			{
				result = list[UnityEngine.Random.Range(0, list.Count)];
			}
			return result;
		}
		void CreateCanPlayEpisodeList(out List<float> reference, out List<float> reference2)
		{
			reference = new List<float>();
			reference2 = new List<float>();
			foreach (KeyValuePair<float, ScenarioGroupData> item in _cache)
			{
				if (item.Value.EpisodeNumber != _beforeEpisodeNumber && (item.Value.UnlockStoryEpisode == "None" || SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains(item.Value.UnlockStoryEpisode)))
				{
					PlayTiming targetPlayTiming = (PlayTiming)Enum.Parse(typeof(PlayTiming), item.Value.PlayTiming);
					switch (currentPlayTiming)
					{
					case PlayTiming.Break:
						if (!IsBreakTargetPlayTiming(targetPlayTiming))
						{
							continue;
						}
						break;
					case PlayTiming.GameStart:
					case PlayTiming.GameStartAndBreak:
						if (!IsGameStartTargetPlayTiming(targetPlayTiming))
						{
							continue;
						}
						break;
					}
					switch ((ContentType)Enum.Parse(typeof(ContentType), item.Value.ContentType))
					{
					case ContentType.SmallTalk_CatHeadphone:
						if (currentPlayTiming == PlayTiming.GameStart)
						{
							if (IsSeenEpisode(item.Value.ID))
							{
								continue;
							}
						}
						else if (currentPlayTiming == PlayTiming.Break && (!_decorationDataService.IsDecorationActive(DecorationService.DecorationSkinType.Headphone_2).CurrentValue || !IsSeenEpisode(item.Value.ID)))
						{
							continue;
						}
						break;
					case ContentType.SmallTalk_Cicada:
						if ((double)int.Parse(item.Value.Arg3) > SaveDataManager.Instance.PlayerData.PomodoroTotalWorkTime.TotalHours)
						{
							continue;
						}
						break;
					case ContentType.SmallTalk_PassageOfTime:
					{
						TimeSpan timeSpan = DateTime.Now - _environmentUIService.LastChangeWindowDateTime;
						if ((double)int.Parse(item.Value.Arg3) > timeSpan.TotalHours)
						{
							continue;
						}
						break;
					}
					}
					if (IsSeenEpisode(item.Value.ID))
					{
						reference.Add(item.Key);
					}
					else
					{
						reference2.Add(item.Key);
					}
				}
			}
		}
		static bool IsBreakTargetPlayTiming(PlayTiming targetPlayTiming)
		{
			if (targetPlayTiming == PlayTiming.Break || targetPlayTiming == PlayTiming.GameStartAndBreak)
			{
				return true;
			}
			return false;
		}
		bool IsElapsedInterval()
		{
			_ = _beforePlayDateTime;
			if ((DateTime.Now - _beforePlayDateTime).TotalMinutes < (double)_masterDataLoader.SmallTalkData.IntervalMinutes)
			{
				return false;
			}
			return true;
		}
		static bool IsGameStartTargetPlayTiming(PlayTiming targetPlayTiming)
		{
			if (targetPlayTiming == PlayTiming.GameStart || targetPlayTiming == PlayTiming.GameStartAndBreak)
			{
				return true;
			}
			return false;
		}
		static bool IsSeenEpisode(string episodeID)
		{
			return SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains(episodeID);
		}
	}

	public void OnStartTalk()
	{
		_beforePlayDateTime = DateTime.Now;
	}

	public void OnEndTalk()
	{
		ScenarioGroupData scenarioGroupData = _scenarioGroupMaster.Data.FirstOrDefault((ScenarioGroupData x) => x.Scenario == _scenarioReader.PlayingScenarioType && x.EpisodeNumber == _currentEpisodeNumber);
		if (scenarioGroupData != null && !SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains(scenarioGroupData.ID))
		{
			_achievementService.OnEndSmallTalk((int)_currentEpisodeNumber);
			SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Add(scenarioGroupData.ID);
			SaveDataManager.Instance.SaveScenarioProgressData();
		}
		_beforeEpisodeNumber = _currentEpisodeNumber;
		_currentEpisodeNumber = -1f;
	}
}
