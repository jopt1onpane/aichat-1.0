using System.Linq;
using KanKikuchi.AudioManager;
using VContainer;

namespace Bulbul;

public class SystemSeService
{
	[Inject]
	private MasterDataLoader _masterDataLoader;

	public void Play(SystemSeParam parameter)
	{
		SystemSeMasterData systemSeMasterData = _masterDataLoader.SystemSeMasterList.FirstOrDefault((SystemSeMasterData x) => x.SeSound == parameter.SeSound);
		if (systemSeMasterData == null)
		{
			Debug.LogWarning($"{parameter.SeSound}用環境音がない");
			return;
		}
		SingletonMonoBehaviour<SEManager>.Instance.GetAudioPlayerByName(systemSeMasterData.AudioClipName);
		SingletonMonoBehaviour<SEManager>.Instance.Play(systemSeMasterData.AudioClip);
	}

	public void Stop(SystemSeType sound)
	{
		SystemSeMasterData systemSeMasterData = _masterDataLoader.SystemSeMasterList.FirstOrDefault((SystemSeMasterData x) => x.SeSound == sound);
		if (systemSeMasterData != null)
		{
			SingletonMonoBehaviour<SEManager>.Instance.Stop(systemSeMasterData.AudioClipName);
		}
	}

	public void PlayGameStartCameraNoise()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.GameStartCameraNoise,
			IsAllowsDuplicate = true
		});
	}

	public void PlayClick()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.Click,
			IsAllowsDuplicate = true
		});
	}

	public void PlayCancel()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.Cancel,
			IsAllowsDuplicate = true
		});
	}

	public void PlayLevelUp()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.LevelUp,
			IsAllowsDuplicate = false
		});
	}

	public void PlayTaskComplete()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.TaskComplete,
			IsAllowsDuplicate = true
		});
	}

	public void PlayTimerStart()
	{
		if (SaveDataManager.Instance.SettingData.IsPlayPomodoroSe.Value)
		{
			Play(new SystemSeParam
			{
				SeSound = SystemSeType.TimerStart,
				IsAllowsDuplicate = true
			});
		}
	}

	public void PlayTimerPause()
	{
		if (SaveDataManager.Instance.SettingData.IsPlayPomodoroSe.Value)
		{
			Play(new SystemSeParam
			{
				SeSound = SystemSeType.TimerPause,
				IsAllowsDuplicate = true
			});
		}
	}

	public void PlayTimerEnd()
	{
		if (SaveDataManager.Instance.SettingData.IsPlayPomodoroSe.Value)
		{
			Play(new SystemSeParam
			{
				SeSound = SystemSeType.TimerEnd,
				IsAllowsDuplicate = true
			});
		}
	}

	public void PlayScenarioStart()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.ScenarioStart,
			IsAllowsDuplicate = true
		});
	}

	public void PlaySpecialOpening()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.SpecialOpening,
			IsAllowsDuplicate = false
		});
	}

	public void PlayPulldownOpen()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.PulldownOpen,
			IsAllowsDuplicate = true
		});
	}

	public void PlayPulldownClose()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.PulldownClose,
			IsAllowsDuplicate = true
		});
	}

	public void PlayBuyItem()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.BuyItem,
			IsAllowsDuplicate = true
		});
	}

	public void PlaySelect()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.Select,
			IsAllowsDuplicate = true
		});
	}

	public void PlayCancelSelect()
	{
		Play(new SystemSeParam
		{
			SeSound = SystemSeType.CancelSelect,
			IsAllowsDuplicate = true
		});
	}
}
