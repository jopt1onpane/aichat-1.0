using System;
using NestopiSystem;

namespace Bulbul;

public class SpecialLunaNewYear2026Util : SpecialScenarioUtil
{
	public static readonly SpecialLunaNewYear2026Util Instance = new SpecialLunaNewYear2026Util();

	private bool isValid => SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsValid.Value;

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
		return SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LastReadEpisodeNumber == 1;
	}

	public override bool IsNeedNewIcon()
	{
		if (IsUnlocked() && SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsNeedUseNewIcon && isValid)
		{
			return true;
		}
		return false;
	}

	public override bool IsNeedUsePossibleAnnounce()
	{
		if (!SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsFinishedUsePossibleAnnounce && SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber >= 10f && IsValidPeriod() && isValid)
		{
			return true;
		}
		return false;
	}

	public override bool IsNeedUseFinishAnnounce()
	{
		if (!SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsFinishedCompleteAnnounce && IsReadAll())
		{
			return true;
		}
		return false;
	}

	public override bool IsValidPeriod()
	{
		return DateTime.Now.IsBetween(MyDefine.SpecialLunaNewYear2026UnlockStartDate, MyDefine.SpecialLunaNewYear2026UnlockEndDate);
	}
}
