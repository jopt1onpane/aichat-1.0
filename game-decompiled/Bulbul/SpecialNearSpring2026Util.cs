using System;
using NestopiSystem;

namespace Bulbul;

public class SpecialNearSpring2026Util : SpecialScenarioUtil
{
	public override bool IsUnlocked()
	{
		if (SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber >= 10f && IsValidPeriod())
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
		return SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.LastReadEpisodeNumber == 1;
	}

	public override bool IsNeedNewIcon()
	{
		if (IsUnlocked() && SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.IsNeedUseNewIcon)
		{
			return true;
		}
		return false;
	}

	public override bool IsNeedUsePossibleAnnounce()
	{
		if (!SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.IsFinishedUsePossibleAnnounce && SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber >= 10f && IsValidPeriod())
		{
			return true;
		}
		return false;
	}

	public override bool IsNeedUseFinishAnnounce()
	{
		if (!SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.IsFinishedCompleteAnnounce && IsReadAll())
		{
			return true;
		}
		return false;
	}

	public override bool IsValidPeriod()
	{
		return DateTime.Now.IsBetween(MyDefine.SpecialNearSpring2026UnlockStartDate, MyDefine.SpecialNearSpring2026UnlockEndDate);
	}
}
