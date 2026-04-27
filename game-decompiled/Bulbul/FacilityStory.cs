using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bulbul.MasterData;
using ObservableCollections;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilityStory : MonoBehaviour
{
	private enum MainState
	{
		Idle,
		StoryStartReady,
		PlayingStory,
		PlayEndStory
	}

	public enum StoryTag
	{
		Main,
		Special
	}

	private MainState _mainState;

	private ReactiveProperty<StoryTag> _currentStoryTag = new ReactiveProperty<StoryTag>();

	[Inject]
	private ScenarioGroupMasterWrapper scenarioGroupMaster;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private ISpecialService _specialService;

	[Inject]
	private IStorySelectUI _storySelectUI;

	[Inject]
	private IStorySystemUI _storySystemUI;

	[SerializeField]
	private ScenarioType scenarioType = ScenarioType.MainScenario;

	[SerializeField]
	[Header("Auto/Click")]
	private ScenarioReader.ScenarioReadMode _readMode = ScenarioReader.ScenarioReadMode.Auto;

	private readonly ObservableList<ScenarioGroupData> playableScenario = new ObservableList<ScenarioGroupData>();

	private bool _isTalkLog;

	private ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>();

	private float _episodeNumber = 1f;

	private Subject<Unit> _onEndNewMainStory = new Subject<Unit>();

	private Subject<Unit> _onCloseFromCloseButton = new Subject<Unit>();

	public ReadOnlyReactiveProperty<StoryTag> CurrentStoryTag => _currentStoryTag;

	public IReadOnlyObservableList<ScenarioGroupData> PlayableScenario => playableScenario;

	public bool IsTalkLog => _isTalkLog;

	public ReadOnlyReactiveProperty<bool> IsActive => _isActive;

	public float EpisodeNumber => _episodeNumber;

	public Observable<Unit> OnEndNewMainStory => _onEndNewMainStory;

	public Observable<Unit> OnCloseFromCloseButton => _onCloseFromCloseButton;

	public void Setup(Action onSkipTutorialAction)
	{
		_storySystemUI.Setup();
		_ = SaveDataManager.Instance.LevelData.CurrentLevel;
		playableScenario.AddRange(scenarioGroupMaster.PlayedMainScenario(includeExtraScenario: true));
		_storySelectUI.Setup(this);
		ObservableSubscribeExtensions.Subscribe(_storySystemUI.OnClickButtonSkip, delegate
		{
			if (_scenarioReader.IsPlayingTutorial())
			{
				onSkipTutorialAction();
			}
			else
			{
				_scenarioReader.SkipStory();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_storySelectUI.OnClickCloseButton, delegate
		{
			_systemSeService.PlayCancel();
			Deactivate();
			_onCloseFromCloseButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_storySelectUI.OnClickTagChangeToMainButton, delegate
		{
			_currentStoryTag.Value = StoryTag.Main;
			_systemSeService.PlayClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_storySelectUI.OnClickTagChangeToSpecialButton, delegate
		{
			_currentStoryTag.Value = StoryTag.Special;
			_systemSeService.PlayClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_currentStoryTag, delegate
		{
			UpdateStoryList();
		}).AddTo(this);
		Deactivate();
	}

	public void UpdateStoryList()
	{
		ScenarioGroupData[] items = null;
		switch (_currentStoryTag.Value)
		{
		case StoryTag.Main:
			items = scenarioGroupMaster.PlayedMainScenario(includeExtraScenario: true).ToArray();
			break;
		case StoryTag.Special:
			items = scenarioGroupMaster.PlayedSpecialScenario().ToArray();
			break;
		}
		playableScenario.Clear();
		playableScenario.AddRange(items);
	}

	public void UpdateFacility()
	{
		switch (_mainState)
		{
		}
	}

	public void StartStory(ScenarioType type, float episodeNumber, bool isTalkLog = false)
	{
		scenarioType = type;
		_episodeNumber = episodeNumber;
		_isTalkLog = isTalkLog;
		_mainState = MainState.StoryStartReady;
	}

	public bool IsStoryStartReady()
	{
		return _mainState == MainState.StoryStartReady;
	}

	public bool IsStoryPlayEnd()
	{
		MainState mainState = _mainState;
		if (mainState == MainState.Idle || mainState == MainState.PlayEndStory)
		{
			return true;
		}
		return false;
	}

	public void Ready()
	{
		ScenarioGroupData scenarioGroupData = scenarioGroupMaster.Data.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == _episodeNumber);
		_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, _readMode, _isTalkLog);
	}

	public void StartStory()
	{
		_scenarioReader.StartMainStory();
		_scenarioReader.AddEndCallback(delegate
		{
			_mainState = MainState.PlayEndStory;
		});
		_mainState = MainState.PlayingStory;
	}

	public void StartTutorialStory(Action onEndAction)
	{
		_scenarioReader.StartClickHeroineReaction();
		_scenarioReader.AddEndCallback(delegate
		{
			_mainState = MainState.PlayEndStory;
			onEndAction();
		});
		scenarioType = ScenarioType.Tutorial;
		_episodeNumber = 1f;
	}

	public void EndStory()
	{
		SaveScenarioPlayedLog();
		if (!_isTalkLog && _scenarioReader.IsPlayingLongStory())
		{
			_onEndNewMainStory.OnNext(Unit.Default);
		}
		_scenarioReader.EndTidyingForStory();
		_isTalkLog = false;
		_mainState = MainState.Idle;
	}

	public void SaveScenarioPlayedLog()
	{
		ScenarioGroupData scenarioGroupData = scenarioGroupMaster.Data.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == _episodeNumber);
		if (scenarioGroupData != null)
		{
			SaveDataManager.Instance.ScenarioProgressData.FinishReadEpisode(scenarioType, _episodeNumber, _masterDataLoader);
			if (!_isTalkLog)
			{
				_specialService.OnEndStory(scenarioType, (int)_episodeNumber);
			}
			if (!SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains(scenarioGroupData.ID))
			{
				SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Add(scenarioGroupData.ID);
				SaveDataManager.Instance.SaveScenarioProgressData();
			}
			UpdateStoryList();
		}
	}

	public void Activate()
	{
		_isActive.Value = true;
		_currentStoryTag.Value = StoryTag.Main;
		_storySelectUI.Activate();
	}

	public void Deactivate()
	{
		_isActive.Value = false;
		_storySelectUI.Deactivate();
	}

	public void DebugPlayScenario(string labelId)
	{
		scenarioGroupMaster.Data.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == _episodeNumber);
		_scenarioReader.DebugPlayScenario(labelId);
	}

	public void DebugSkipSelectStory(int episodeNumber)
	{
		IReadOnlyList<ScenarioGroupData> data = scenarioGroupMaster.Data;
		int num = 0;
		float num2 = 0f;
		foreach (ScenarioGroupData item in data)
		{
			if ((float)episodeNumber >= item.EpisodeNumber)
			{
				if (!SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains(item.ID))
				{
					SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Add(item.ID);
				}
				if (item.Scenario == ScenarioType.MainScenario && item.EpisodeNumber > num2)
				{
					num2 = item.EpisodeNumber;
				}
				if (item.UnlockLevel > num)
				{
					num = item.UnlockLevel;
				}
			}
		}
		if (num2 > 0f)
		{
			SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber = num2;
		}
		LevelData levelData = SaveDataManager.Instance.LevelData;
		if (num != levelData.CurrentLevel)
		{
			if (num > levelData.CurrentLevel)
			{
				while (levelData.CurrentLevel < num)
				{
					float num3 = levelData.NextLevelNecessaryExp - levelData.CurrentExp;
					if (num3 > 0f)
					{
						levelData.AddExp(num3);
					}
					levelData.LevelUp(_masterDataLoader);
				}
			}
			else
			{
				FieldInfo field = typeof(LevelData).GetField("_currentLevel", BindingFlags.Instance | BindingFlags.NonPublic);
				FieldInfo field2 = typeof(LevelData).GetField("_currentExp", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field2 != null)
				{
					field.SetValue(levelData, num);
					field2.SetValue(levelData, 0f);
					levelData.SetupNextLevelNecessaryExp(_masterDataLoader);
				}
			}
		}
		SaveDataManager.Instance.ScenarioProgressData.UpdateNextMainEpisode(_masterDataLoader);
		SaveDataManager.Instance.ScenarioProgressData.CanShowConnectionLostNextEpisode = false;
		SaveDataManager.Instance.SaveScenarioProgressData();
		SaveDataManager.Instance.SavePlayerData();
		ScenarioGroupData[] items = scenarioGroupMaster.PlayedMainScenario(includeExtraScenario: true).ToArray();
		playableScenario.Clear();
		playableScenario.AddRange(items);
		if (_storySelectUI != null)
		{
			_storySelectUI.Setup(this);
		}
	}

	public void DebugSkipAllStory()
	{
		foreach (ScenarioGroupData datum in scenarioGroupMaster.Data)
		{
			SaveDataManager.Instance.ScenarioProgressData.FinishReadEpisode(datum.Scenario, datum.EpisodeNumber, _masterDataLoader);
			if (!SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains(datum.ID))
			{
				SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Add(datum.ID);
			}
		}
		SaveDataManager.Instance.SaveScenarioProgressData();
		UpdateStoryList();
	}
}
