using System.Collections.Generic;
using System.Linq;
using VContainer;

namespace Bulbul;

public class UnlockMusic
{
	[Inject]
	private MusicService musicService;

	private MasterDataLoader _masterDataLoader;

	public bool IsNeedGetNewAnnounce;

	private const string withoutYou = "428d0ecc-81d7-1c75-09bf-0970e0608b5f";

	public void Setup(MasterDataLoader masterDataLoader)
	{
		_masterDataLoader = masterDataLoader;
	}

	public void UnlockUpdate(UnlockItemService.ConditionsType conditionsType, string arg1, string arg2 = "0", string arg3 = "0")
	{
		IReadOnlyList<MusicData> musicDataList = _masterDataLoader.MusicDataList;
		if (conditionsType != UnlockItemService.ConditionsType.Scenario || !(arg1 == "main_32"))
		{
			return;
		}
		MusicData musicData = musicDataList.FirstOrDefault((MusicData x) => x.UUID == "428d0ecc-81d7-1c75-09bf-0970e0608b5f");
		if (musicData != null && musicService.AllMusicList.All((GameAudioInfo x) => x.UUID != "428d0ecc-81d7-1c75-09bf-0970e0608b5f"))
		{
			GameAudioInfo music = GameAudioInfo.CreateByMaster(musicData);
			if (musicService.AddMusicItem(music))
			{
				IsNeedGetNewAnnounce = true;
			}
		}
	}

	public bool IsUnlocked(string uuid)
	{
		if (uuid == "428d0ecc-81d7-1c75-09bf-0970e0608b5f")
		{
			return SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains("main_32");
		}
		return true;
	}
}
