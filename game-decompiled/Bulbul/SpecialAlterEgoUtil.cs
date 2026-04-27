namespace Bulbul;

public class SpecialAlterEgoUtil : SpecialScenarioUtil
{
	public override bool IsUnlocked()
	{
		if (SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber >= 10f)
		{
			return true;
		}
		return false;
	}

	public override bool IsInProgress()
	{
		return !IsReadAll();
	}

	public override bool IsReadAll()
	{
		return SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LastReadEpisodeNumber == 2;
	}

	public override bool IsNeedNewIcon()
	{
		if (IsUnlocked() && SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.IsNeedUseNewIcon)
		{
			return true;
		}
		return false;
	}

	public override bool IsNeedUsePossibleAnnounce()
	{
		if (!SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.IsFinishedUsePossibleAnnounce && SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber >= 10f)
		{
			return true;
		}
		return false;
	}

	public override bool IsNeedUseFinishAnnounce()
	{
		if (!SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.IsFinishedCompleteAnnounce && IsReadAll())
		{
			return true;
		}
		return false;
	}
}
