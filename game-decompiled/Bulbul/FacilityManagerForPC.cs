using System;
using Bulbul.MasterData;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilityManagerForPC : MonoBehaviour, ILevelUpDirectionController, IPomodoroTalkController, IDebugRoomGameManager, IStoryController, IMusicPlayerController
{
	[SerializeField]
	private FacilityTodo _facilityTodo;

	[SerializeField]
	private FacilityNote _facilityNote;

	[SerializeField]
	private FacilityCalendar _facilityCalendar;

	[SerializeField]
	private FacilityStory _facilityStory;

	[SerializeField]
	private FacilityDecoration _facilityDecoration;

	[SerializeField]
	private FacilityPlayerLevel _facilityPlayerLevel;

	[SerializeField]
	private FacilityPomodoro _facilityPomodoro;

	[SerializeField]
	private FacilityMusic _facilityMusic;

	[SerializeField]
	private FacilitySetting _facilitySetting;

	[Inject]
	private UnlockItemService _unlockItemService;

	public bool IsReadyStartLevelUpDirection => _facilityPlayerLevel.IsReadyStartLevelUpDirection();

	public bool IsEndLevelUpDirection => _facilityPlayerLevel.IsEndLevelUpDirection();

	public bool IsTalkStartReady => _facilityPomodoro.IsTalkStartReady();

	public bool IsTalkPlayEnd => _facilityPomodoro.IsTalkPlayEnd();

	public bool IsCurrentWorking => _facilityPomodoro.IsCurrentWorking();

	public bool IsCurrentResting => _facilityPomodoro.IsCurrentResting();

	public bool IsTalkLog => _facilityStory.IsTalkLog;

	public bool IsStoryStartReady => _facilityStory.IsStoryStartReady();

	public bool IsStoryPlayEnd => _facilityStory.IsStoryPlayEnd();

	public float EpisodeNumber => _facilityStory.EpisodeNumber;

	public bool IsPaused => _facilityMusic.IsPaused;

	public void StartLevelUpDirection()
	{
		_facilityPlayerLevel.StartLevelUpDirection();
	}

	public void EndLevelUpDirection()
	{
		_facilityPlayerLevel.EndLevelUpDirection();
	}

	public void StartStory()
	{
		_facilityStory.StartStory();
	}

	public void EndStory()
	{
		_facilityStory.EndStory();
	}

	public void Ready()
	{
		_facilityStory.Ready();
	}

	public void SaveScenarioPlayedLog()
	{
		_facilityStory.SaveScenarioPlayedLog();
	}

	public void StartTutorialStory(Action onEndAction)
	{
		_facilityStory.StartTutorialStory(onEndAction);
	}

	public void StartStory(ScenarioType scenarioType, float episodeNum, bool isTalkLog = false)
	{
		_facilityStory.StartStory(scenarioType, episodeNum, isTalkLog);
	}

	public void StartTalk()
	{
		_facilityPomodoro.StartTalk();
	}

	public void EndTalk()
	{
		_facilityPomodoro.EndTalk();
	}

	public void OnClickButtonPlayListPlayMusicButton(GameAudioInfo info)
	{
		_facilityMusic.OnClickButtonPlayListPlayMusicButton(info);
	}

	public void PauseMusic()
	{
		_facilityMusic.PauseMusic();
	}

	public void UnPauseMusic()
	{
		_facilityMusic.UnPauseMusic();
	}

	public void Release()
	{
		_facilityMusic.Release();
	}

	public void DebugSkipAllStory()
	{
		_facilityStory.DebugSkipAllStory();
		_unlockItemService.UnlockUpdateByData();
	}

	public void DebugSkipSelectStory(int episodeNumber)
	{
		_facilityStory.DebugSkipSelectStory(episodeNumber);
		_facilityPlayerLevel.RefreshUI();
		_unlockItemService.UnlockUpdateByData();
	}

	public void DebuPlayStory(ScenarioType scenarioType, float episodeNumber)
	{
		_facilityStory.StartStory(scenarioType, episodeNumber);
	}

	public void DebugPlayScenario(string labelId)
	{
		_facilityStory.DebugPlayScenario(labelId);
	}

	public void DebugPlayerLevelReflesh()
	{
		_facilityPlayerLevel.RefreshUI();
	}
}
