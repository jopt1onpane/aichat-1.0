using System.Collections.Generic;
using System.Linq;
using Bulbul.MasterData;
using FastEnumUtility;

namespace Bulbul;

public class ScenarioGroupMasterWrapper
{
	private readonly MasterDataLoader masterDataLoader;

	public IReadOnlyList<ScenarioGroupData> Data => masterDataLoader.ScenarioGroupMasterList;

	public ScenarioGroupMasterWrapper(MasterDataLoader masterDataLoader)
	{
		this.masterDataLoader = masterDataLoader;
	}

	public IEnumerable<ScenarioGroupData> PlayedMainScenario(bool includeExtraScenario)
	{
		List<ScenarioGroupData> list = new List<ScenarioGroupData>();
		if (!SaveDataManager.Instance.PlayerData.IsNeedTutorial)
		{
			ScenarioGroupData item = masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == ScenarioType.Tutorial && x.EpisodeNumber == 1f);
			list.Add(item);
		}
		float playedMainEpisode = SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber;
		IEnumerable<ScenarioGroupData> enumerable = masterDataLoader.ScenarioGroupMasterList.Where((ScenarioGroupData x) => x.Scenario == ScenarioType.MainScenario && playedMainEpisode >= x.EpisodeNumber);
		if (includeExtraScenario)
		{
			List<string> readScenarioIDs = SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs;
			enumerable = enumerable.Union(masterDataLoader.ScenarioGroupMasterList.Where((ScenarioGroupData x) => x.Scenario == ScenarioType.ExtraScenario && readScenarioIDs.Contains(x.ID)));
		}
		foreach (ScenarioGroupData item2 in enumerable)
		{
			list.Add(item2);
		}
		return list;
	}

	public IEnumerable<ScenarioGroupData> PlayedSpecialScenario()
	{
		List<string> playedIds = SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs;
		if (playedIds == null || playedIds.Count == 0)
		{
			return Enumerable.Empty<ScenarioGroupData>();
		}
		return from x in masterDataLoader.ScenarioGroupMasterList
			where (x.Scenario == ScenarioType.Special_AlterEgo || x.Scenario == ScenarioType.Special_BearsRestaurant || x.Scenario == ScenarioType.Special_Valentine2026 || x.Scenario == ScenarioType.Special_LunaNewYear2026 || x.Scenario == ScenarioType.Special_NearSpring2026) && playedIds.Contains(x.ID)
			orderby x.EpisodeNumber
			select x;
	}

	public IEnumerable<ScenarioGroupData> PlayableScenario(int level)
	{
		return masterDataLoader.ScenarioGroupMasterList.Where(delegate(ScenarioGroupData x)
		{
			ScenarioType scenario = x.Scenario;
			return (scenario == ScenarioType.MainScenario || scenario == ScenarioType.AfterScenario || scenario == ScenarioType.DLCScenario) && level >= x.UnlockLevel;
		});
	}

	public IEnumerable<ScenarioGroupData> GetScenarioWithLevel(int level)
	{
		return masterDataLoader.ScenarioGroupMasterList.Where(delegate(ScenarioGroupData x)
		{
			ScenarioType scenario = x.Scenario;
			return (scenario == ScenarioType.MainScenario || scenario == ScenarioType.AfterScenario || scenario == ScenarioType.DLCScenario) && level == x.UnlockLevel;
		});
	}

	public IEnumerable<ScenarioGroupData> GetPlayableExtraScenario(bool includeSeen)
	{
		IEnumerable<ScenarioGroupData> enumerable = masterDataLoader.ScenarioGroupMasterList.Where((ScenarioGroupData x) => x.Scenario == ScenarioType.ExtraScenario && IsContentTypeConditionOK(x));
		if (!includeSeen)
		{
			enumerable = enumerable.Where((ScenarioGroupData x) => !SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains(x.ID));
		}
		return enumerable;
	}

	public IEnumerable<ScenarioGroupData> GetPlayableEventAprilFoolScenario(bool includeSeen)
	{
		IEnumerable<ScenarioGroupData> enumerable = masterDataLoader.ScenarioGroupMasterList.Where((ScenarioGroupData x) => x.Scenario == ScenarioType.Event_2026_AprilFool && IsContentTypeConditionOK(x));
		if (!includeSeen)
		{
			enumerable = enumerable.Where((ScenarioGroupData x) => !SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains(x.ID));
		}
		return enumerable;
	}

	private bool IsContentTypeConditionOK(ScenarioGroupData scenario)
	{
		if (!FastEnum.TryParse<SmallTalkSelector.ContentType>(scenario.ContentType, out var result))
		{
			Debug.LogError("ContentType is not valid: " + scenario.ContentType);
			return false;
		}
		return result switch
		{
			SmallTalkSelector.ContentType.None => true, 
			SmallTalkSelector.ContentType.PurchaseEnvironment => SaveDataManager.Instance.PointPurchaseData.PurchasedEnvironments.Contains(scenario.Arg1), 
			_ => false, 
		};
	}
}
