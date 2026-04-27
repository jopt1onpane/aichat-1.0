using System;
using Bulbul;
using Bulbul.Achievements;
using R3;
using VContainer;

public class AchievementService : IDisposable
{
	[Inject]
	private IAchievementList _achievements;

	[Inject]
	private IAchievementNotice _notice;

	private DisposableBag _disposable;

	public void Setup()
	{
		_notice.Setup();
		if (!SaveDataManager.Instance.PlayerData.IsNeedTutorial)
		{
			ProceedOnlyOnce(AchievementCategory.COMPLETE_TUTORIAL_COUNT, isUseSave: false);
		}
		if (SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains("smalltalk_0" + 1))
		{
			ProceedOnlyOnce(AchievementCategory.SMALL_TALK_HEATSTROKE, isUseSave: false);
		}
		if (SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains("smalltalk_0" + 2))
		{
			ProceedOnlyOnce(AchievementCategory.SMALL_TALK_GALAXY_EXPRESS, isUseSave: false);
		}
		if (SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains("smalltalk_0" + 3))
		{
			ProceedOnlyOnce(AchievementCategory.SMALL_TALK_CAT_HEADPHONE, isUseSave: false);
		}
		if (SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains("smalltalk_0" + 4))
		{
			ProceedOnlyOnce(AchievementCategory.SMALL_TALK_CHICADA, isUseSave: false);
		}
		int num = (int)SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber;
		if (num >= 33)
		{
			ProceedOnlyOnce(AchievementCategory.READ_GLASSES_STORY, isUseSave: false);
		}
		if (num >= 34)
		{
			ProceedOnlyOnce(AchievementCategory.PUBLISH_SUMMERTIME_OVERDRIVE, isUseSave: false);
		}
		if (num >= 36)
		{
			ProceedOnlyOnce(AchievementCategory.READ_ALL_MAIN_STORY, isUseSave: false);
		}
		UpdateTotalPomodoroWorkHour();
		AchievementSaveData achievementSaveData = SaveDataManager.Instance.AchievementSaveData;
		UnlockOnlyOnceAchievement(AchievementCategory.COMPLETE_TODO_COUNT);
		UnlockOnlyOnceAchievement(AchievementCategory.COMPLETE_POMODORO_COUNT);
		UnlockOnlyOnceAchievement(AchievementCategory.END_EDIT_NOTE_COUNT);
		UnlockOnlyOnceAchievement(AchievementCategory.END_EDIT_DIARY_COUNT);
		UnlockOnlyOnceAchievement(AchievementCategory.IMPORT_MUSIC_COUNT);
		UnlockOnlyOnceAchievement(AchievementCategory.CHANGE_ENVIROMENT_COUNT);
		UnlockOnlyOnceAchievement(AchievementCategory.FINISH_GAME_FROM_EXIT_CALL_BUTTON_COUNT);
		UnlockOnlyOnceAchievement(AchievementCategory.TALK_WORKING_COUNT);
		UnlockOnlyOnceAchievement(AchievementCategory.TALK_BREAKING_COUNT);
		UnlockOnlyOnceAchievement(AchievementCategory.TALK_AFTER_WORK_COUNT);
		UnlockOnlyOnceAchievement(AchievementCategory.DRINK_HOT);
		UnlockOnlyOnceAchievement(AchievementCategory.WAKE_UP_HEROINE);
		ObservableSubscribeExtensions.Subscribe(_notice.OnEndFirstTutorial, delegate
		{
			ProceedOnlyOnce(AchievementCategory.COMPLETE_TUTORIAL_COUNT);
		}).AddTo(ref _disposable);
		ObservableSubscribeExtensions.Subscribe(_notice.OnEndNewMainStory, delegate
		{
			switch ((int)SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber)
			{
			case 33:
				ProceedOnlyOnce(AchievementCategory.READ_GLASSES_STORY);
				break;
			case 34:
				ProceedOnlyOnce(AchievementCategory.PUBLISH_SUMMERTIME_OVERDRIVE);
				break;
			case 36:
				ProceedOnlyOnce(AchievementCategory.READ_ALL_MAIN_STORY);
				break;
			}
		}).AddTo(ref _disposable);
		ObservableSubscribeExtensions.Subscribe(_notice.OnCompleteTodo, delegate
		{
			ProceedOnlyOnce(AchievementCategory.COMPLETE_TODO_COUNT);
		}).AddTo(ref _disposable);
		ObservableSubscribeExtensions.Subscribe(_notice.OnCompletePomodoro, delegate
		{
			ProceedOnlyOnce(AchievementCategory.COMPLETE_POMODORO_COUNT);
		}).AddTo(ref _disposable);
		ObservableSubscribeExtensions.Subscribe(_notice.OnUpdateWorkHour, delegate
		{
			UpdateTotalPomodoroWorkHour();
		}).AddTo(ref _disposable);
		ObservableSubscribeExtensions.Subscribe(_notice.OnEndEditNote, delegate
		{
			ProceedOnlyOnce(AchievementCategory.END_EDIT_NOTE_COUNT);
		}).AddTo(ref _disposable);
		ObservableSubscribeExtensions.Subscribe(_notice.OnEndEditDiary, delegate
		{
			ProceedOnlyOnce(AchievementCategory.END_EDIT_DIARY_COUNT);
		}).AddTo(ref _disposable);
		ObservableSubscribeExtensions.Subscribe(_notice.OnCompleteImportMusic, delegate
		{
			ProceedOnlyOnce(AchievementCategory.IMPORT_MUSIC_COUNT);
		}).AddTo(ref _disposable);
		void UnlockOnlyOnceAchievement(AchievementCategory achievement)
		{
			if (achievementSaveData.IsUnlocked(achievement.ToString()))
			{
				ProceedOnlyOnce(achievement, isUseSave: false);
			}
		}
	}

	public void OnChangeWindowView()
	{
		ProceedOnlyOnce(AchievementCategory.CHANGE_ENVIROMENT_COUNT);
	}

	public void OnExitCallTalk()
	{
		ProceedOnlyOnce(AchievementCategory.FINISH_GAME_FROM_EXIT_CALL_BUTTON_COUNT);
	}

	public void OnStartTalkWorking()
	{
		ProceedOnlyOnce(AchievementCategory.TALK_WORKING_COUNT);
	}

	public void OnStartTalkBreaking()
	{
		ProceedOnlyOnce(AchievementCategory.TALK_BREAKING_COUNT);
	}

	public void OnEndTalkAfterWork()
	{
		ProceedOnlyOnce(AchievementCategory.TALK_AFTER_WORK_COUNT);
	}

	public void OnEndSmallTalk(int episodeNumber)
	{
		switch (episodeNumber)
		{
		case 1:
			ProceedOnlyOnce(AchievementCategory.SMALL_TALK_HEATSTROKE);
			break;
		case 2:
			ProceedOnlyOnce(AchievementCategory.SMALL_TALK_GALAXY_EXPRESS);
			break;
		case 3:
			ProceedOnlyOnce(AchievementCategory.SMALL_TALK_CAT_HEADPHONE);
			break;
		case 4:
			ProceedOnlyOnce(AchievementCategory.SMALL_TALK_CHICADA);
			break;
		}
	}

	public void OnEndDrinkHotReaction()
	{
		ProceedOnlyOnce(AchievementCategory.DRINK_HOT);
	}

	public void OnEndWakeUpReaction()
	{
		ProceedOnlyOnce(AchievementCategory.WAKE_UP_HEROINE);
	}

	public void OnClickKouPenguin()
	{
		AchievementCategory achievementCategory = AchievementCategory.CLICK_KOU_COUNT;
		AchievementStats achievement = _achievements.GetAchievement(achievementCategory);
		if (achievement == null)
		{
			Debug.LogError(string.Format("{0} : {1}が見つかりませんでした。", "AchievementService", achievementCategory));
		}
		else if (achievement.Progress.Value != 5)
		{
			_achievements.ProgressIncrement(achievementCategory);
		}
	}

	public void UpdateTotalPomodoroWorkHour()
	{
		AchievementCategory achievementCategory = AchievementCategory.POMODORO_WORK_HOURS_COUNT;
		AchievementStats achievement = _achievements.GetAchievement(achievementCategory);
		if (achievement == null)
		{
			Debug.LogError(string.Format("{0} : {1}が見つかりませんでした。", "AchievementService", achievementCategory));
			return;
		}
		int num = (int)Math.Floor(SaveDataManager.Instance.PlayerData.PomodoroTotalWorkSeconds / 3600.0);
		if (achievement.Progress.Value < num)
		{
			_achievements.SetProgress(achievementCategory, num);
		}
	}

	private void ProceedOnlyOnce(AchievementCategory category, bool isUseSave = true)
	{
		if (!SaveDataManager.Instance.AchievementSaveData.IsUnlocked(category.ToString()))
		{
			SaveDataManager.Instance.AchievementSaveData.AddUnlocked(category.ToString());
			if (isUseSave)
			{
				SaveDataManager.Instance.SaveAchievementSaveData();
			}
		}
		AchievementStats achievement = _achievements.GetAchievement(category);
		if (achievement == null)
		{
			Debug.LogError(string.Format("{0} : {1}が見つかりませんでした。", "AchievementService", category));
		}
		else if (achievement.Progress.Value != 1)
		{
			_achievements.SetProgress(category, 1);
		}
	}

	public void Dispose()
	{
		_disposable.Dispose();
	}
}
