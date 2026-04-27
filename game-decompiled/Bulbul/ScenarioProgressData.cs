using System;
using System.Collections.Generic;
using System.Linq;
using Bulbul.MasterData;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class ScenarioProgressData
{
	public float FinishReadMainEpisodeNumber;

	public float NextEpisodeNumber;

	public int NextEpisodeUnlockLevel;

	public bool CanShowConnectionLostNextEpisode;

	public List<string> PlayedScenarioGroupIDs = new List<string>();

	public void UpdateNextMainEpisode(MasterDataLoader masterDataLoader)
	{
		float nextEpisodeNumber = FinishReadMainEpisodeNumber + 1f;
		ScenarioGroupData scenarioGroupData = masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == ScenarioType.MainScenario && x.EpisodeNumber == nextEpisodeNumber);
		if (scenarioGroupData != null)
		{
			SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber = scenarioGroupData.EpisodeNumber;
			SaveDataManager.Instance.ScenarioProgressData.NextEpisodeUnlockLevel = scenarioGroupData.UnlockLevel;
		}
	}

	public bool IsPossibleTalkNextMainEpisode()
	{
		if (NextEpisodeNumber == 32f)
		{
			if (CanShowConnectionLostNextEpisode)
			{
				return true;
			}
		}
		else
		{
			int currentLevel = SaveDataManager.Instance.PlayerData.LevelData.CurrentLevel;
			if (NextEpisodeNumber > FinishReadMainEpisodeNumber && currentLevel >= NextEpisodeUnlockLevel)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsLatestMainEpisode(float episodeNumber)
	{
		return episodeNumber >= NextEpisodeNumber;
	}

	public void FinishReadEpisode(ScenarioType scenarioType, float episodeNumber, MasterDataLoader masterDataLoader)
	{
		if (scenarioType == ScenarioType.MainScenario && episodeNumber == NextEpisodeNumber)
		{
			FinishReadMainEpisodeNumber = NextEpisodeNumber;
			UpdateNextMainEpisode(masterDataLoader);
		}
		SaveDataManager.Instance.SaveScenarioProgressData();
	}
}
