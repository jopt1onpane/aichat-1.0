using System;
using System.Linq;
using System.Threading;
using Bulbul.MasterData;
using Bulbul.Mobile;
using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using NestopiSystem.DIContainers;
using NestopiSystem.Steam;
using R3;
using UnityEngine;
using UnityEngine.Device;
using VContainer;

namespace Bulbul;

public class RoomGameManager : MonoBehaviour, IRoomGameSceneState, IRoomGameLongTalkNotice
{
	public enum MainState
	{
		Debug,
		DemoEnd,
		Load,
		Initialize,
		TalkingGameStartDirection,
		EndGameStartDirection,
		Tutorial0,
		Tutorial1,
		Tutorial2,
		Tutorial3,
		Tutorial4,
		Tutorial5,
		Tutorial6,
		DemoTutorialFinish,
		Idle,
		StoryReady,
		StoryPlayStart,
		StoryPlaying,
		StoryEndTidying,
		PlayingTalk,
		ExitGame0,
		Release
	}

	private ReactiveProperty<MainState> _mainState = new ReactiveProperty<MainState>();

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private LoadDirectionService _loadDirectionService;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private SceneFadeControllerProvider _sceneFadeControllerProvider;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private AudioMixerGroupContainer _audioMixer;

	[Inject]
	private SettingService _settingService;

	[Inject]
	private SteamManager _steamManager;

	[Inject]
	private DateService _dateService;

	[Inject]
	private TutorialService _tutorialService;

	[Inject]
	private CursorService _cursorService;

	[Inject]
	private TooltipService _tooltipService;

	[Inject]
	private SmallAnnounceService _smallAnnounceService;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private FacilityVoiceTextScenario _facilityVoiceTextScenario;

	[Inject]
	private ApplicationFocusService _applicationFocusService;

	[Inject]
	private SmallTalkSelector _smallTalkSelector;

	[Inject]
	private DecorationService _decorationService;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private UnlockMusic _unlockMusic;

	[Inject]
	private FireworksSoundService _fireworksSoundService;

	[Inject]
	private EndCreditsService _endCreditsService;

	[Inject]
	private RoomCameraManager _roomCameraManager;

	[Inject]
	private AchievementService _achievementService;

	[Inject]
	private ISpecialService _specialService;

	[Inject]
	private LimitedTimeEventService _limitedTimeEventService;

	[Inject]
	private ScenarioGroupMasterWrapper _scenarioGroupMaster;

	[Inject]
	private PlayerPointService _playerPointService;

	[Inject]
	private OneTimePointGrantService _oneTimePointGrantService;

	[Inject]
	private IAutoTimeWindowViewChanger _autoTimeWindowViewChanger;

	[Inject]
	private DebugService _debugService;

	[Inject]
	private ILevelUpDirectionController _levelUpDirectionController;

	[Inject]
	private IPomodoroTalkController _pomodoroTalkController;

	[Inject]
	private IStoryController _storyController;

	[Inject]
	private IApplyEnvironmentWindowController _applyEnvironmentWindowController;

	[Inject]
	private IMusicPlayerController _musicPlayerController;

	[Inject]
	private IUIShowManager _uiShowManager;

	[Inject]
	private IRaycastBlocker _raycastBlocker;

	[Inject]
	private IPlatformRoot _platformRoot;

	[Inject]
	private ExitOnlyPoorConnectionViewController _exitOnlyPoorConnectionViewController;

	[Inject]
	private SaveDataIO saveDataIO;

	[Inject]
	private SaveDataDirtyManager saveDataDirtyManager;

	private MusicService _musicService;

	[Header("画面上に表示していない機能")]
	[SerializeField]
	[Header("ヒロインクリック機能")]
	private FacilityClickHeroine _facilityClickHeroine;

	[SerializeField]
	private GameObject[] _leftUIObjects;

	private float[] _leftUIObjectsActivePosY;

	private float[] _leftUIObjectsDeactivePosY;

	[SerializeField]
	private GameObject[] _rightUIObjects;

	private float[] _rightUIObjectsActivePosY;

	private float[] _rightUIObjectsDeactivePosY;

	[SerializeField]
	private GameObject[] _bottomUIObjects;

	private float[] _bottomUIObjectsActivePosY;

	private float[] _bottomUIObjectsDeactivePosY;

	private UniTask _loadTask;

	private bool _isPausedMusicBeforeStory;

	private CancellationTokenSource _exitGameCts = new CancellationTokenSource();

	private Subject<ScenarioType> _onReadyLongStoryAfterFade = new Subject<ScenarioType>();

	private Subject<ScenarioType> _onEndLongStoryBeforeFade = new Subject<ScenarioType>();

	public MainState CurrentMainState => _mainState.CurrentValue;

	public Observable<ScenarioType> OnReadyLongStoryAfterFade => _onReadyLongStoryAfterFade;

	public Observable<ScenarioType> OnEndLongStoryBeforeFade => _onEndLongStoryBeforeFade;

	private void Awake()
	{
		SaveDataManager.Instance.PlayerData.Login();
		_oneTimePointGrantService.CheckAndGrantFirstPoint();
		_raycastBlocker.Block();
		_uiShowManager.Setup();
		_uiShowManager.AllUIDeactivate().Forget();
		if (_musicService == null)
		{
			_musicService = ProjectLifetimeScope.Resolve<MusicService>();
		}
		Initialize();
		FadeIn().Forget();
		if (_directionService.GamePlayingDefect.IsConnectionLost())
		{
			_mainState.Value = MainState.EndGameStartDirection;
		}
		else if (SaveDataManager.Instance.PlayerData.IsFirstLogin || SaveDataManager.Instance.PlayerData.IsNeedTutorial)
		{
			_mainState.Value = MainState.EndGameStartDirection;
		}
		else
		{
			_heroineService.OnGameStart();
			_mainState.Value = MainState.TalkingGameStartDirection;
		}
		_mainState.Where((MainState x) => x == MainState.ExitGame0).Take(1).SubscribeAwait(this, async delegate(MainState _, RoomGameManager @this, CancellationToken ct)
		{
			if (!@this.saveDataDirtyManager.IsUploading)
			{
				await @this.saveDataDirtyManager.FlashAsync(ct);
			}
		}, AwaitOperation.Drop)
			.AddTo(this);
		async UniTask FadeIn()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
			LoadDirectionService.DirectionType type = LoadDirectionService.DirectionType.Normal;
			if (SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber == 32f)
			{
				type = LoadDirectionService.DirectionType.Error;
			}
			_loadDirectionService.FadeInGame(type, DevicePlatform.Steam.IsPC());
		}
	}

	private void Initialize()
	{
		_tooltipService.Setup();
		_smallAnnounceService.Setup();
		_achievementService.Setup();
		_specialService.Setup();
		_limitedTimeEventService.Setup();
		_applicationFocusService.UpdateBeforeIsFocus(isFocus: true);
		_playerPointService.Setup();
		_unlockItemService.Setup();
		_settingService.Setup();
		_decorationService.Setup();
		_platformRoot.Setup(delegate
		{
			_mainState.Value = MainState.Tutorial6;
		});
		_endCreditsService.Setup();
		_facilityClickHeroine.Setup();
		_facilityVoiceTextScenario.Setup();
		_dateService.Setup();
		_heroineService.Setup();
		_cursorService.Setup(_heroineService, this, _tooltipService, _smallAnnounceService);
		SaveDataManager.Instance.ScenarioProgressData.UpdateNextMainEpisode(_masterDataLoader);
		_directionService.Setup();
		PlayNeedUseDefectDirection();
		_limitedTimeEventService.ActivateEvent();
		_smallTalkSelector.Setup();
		if (SaveDataManager.Instance.PlayerData.IsNeedTutorial || SaveDataManager.Instance.LimitedTimeEventSaveData.CurrentType.Value == LimitedTimeEventType.Christmas2025 || SaveDataManager.Instance.LimitedTimeEventSaveData.CurrentType.Value == LimitedTimeEventType.AprilFool2026)
		{
			return;
		}
		DateTime lastLoginDateTime = SaveDataManager.Instance.PlayerData.LastLoginDateTime;
		if (!((DateTime.Now - lastLoginDateTime).TotalDays >= 1.0) && lastLoginDateTime.Day == DateTime.Now.Day)
		{
			return;
		}
		_smallTalkSelector.LotterySmallTalk(SmallTalkSelector.PlayTiming.GameStart);
		if (_smallTalkSelector.IsDecisionNextEpisode)
		{
			int num = 3;
			if (_smallTalkSelector.CurrentEpisodeNumber == (float)num)
			{
				_decorationService.ChangeDecoration(DecorationService.DecorationSkinType.Headphone_2, isSave: true);
			}
		}
	}

	public void SetMainState(MainState state)
	{
		_mainState.Value = state;
	}

	private void Update()
	{
		InputController.Instance.Update();
		_cursorService.UpdateCursor();
		switch (_mainState.Value)
		{
		case MainState.TalkingGameStartDirection:
			if (_heroineService.GetCurrentAIState() != HeroineAI.ActionStateType.GameStartDirection && _heroineService.GetCurrentAIState() != HeroineAI.ActionStateType.MainStory)
			{
				_mainState.Value = MainState.EndGameStartDirection;
			}
			break;
		case MainState.EndGameStartDirection:
			_settingService.UpdateFrameRateForApplicationFocus();
			if (SaveDataManager.Instance.PlayerData.IsNeedTutorial)
			{
				_mainState.Value = MainState.Tutorial1;
				GameAudioInfo gameAudioInfo = _musicService.CurrentPlayList.FirstOrDefault((GameAudioInfo x) => x.Title == "Nostalgia");
				if (gameAudioInfo != null)
				{
					_musicPlayerController.OnClickButtonPlayListPlayMusicButton(gameAudioInfo);
					_musicService.SetRepeat(isrepeat: true);
					AudioPlayer player = SingletonMonoBehaviour<MusicManager>.Instance.GetPlayer(_musicService.PlayingMusic.AudioClip);
					if (player != null)
					{
						player.ChangeVolumeRate(0f);
						SingletonMonoBehaviour<MusicManager>.Instance.FadeFromCurrentVolume(_musicService.PlayingMusic.AudioClip.name, 1.5f, 1f);
					}
				}
			}
			else
			{
				if (_smallTalkSelector.IsDecisionNextEpisode)
				{
					_facilityClickHeroine.PlaySmallTalk(_smallTalkSelector.CurrentEpisodeNumber);
				}
				_uiShowManager.AllUIActivate().Forget();
				if (SaveDataManager.Instance.MusicSetting.IsGameStartPlayMusic)
				{
					_musicPlayerController.UnPauseMusic();
				}
				if (DevicePlatform.Steam.IsMobile())
				{
					ScreenOrientationManagerForMobile.Instance.SetAutoRotateFree();
				}
				_roomCameraManager.OnEndStory();
				_mainState.Value = MainState.Idle;
			}
			_raycastBlocker.Release();
			break;
		case MainState.Tutorial1:
		{
			if (!_scenarioReader.IsPlayingTutorial())
			{
				ScenarioType scenarioType = ScenarioType.Tutorial;
				ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == 1f);
				_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: false);
			}
			Action onEndAction = delegate
			{
				_mainState.Value = MainState.Tutorial6;
			};
			_storyController.StartTutorialStory(onEndAction);
			_mainState.Value = MainState.Tutorial2;
			break;
		}
		case MainState.Tutorial2:
			if (_scenarioReader.IsReadNextForScript())
			{
				_mainState.Value = MainState.Tutorial3;
			}
			break;
		case MainState.Tutorial3:
			_heroineService.OnTutorialWantTalk();
			_mainState.Value = MainState.Tutorial4;
			break;
		case MainState.Tutorial4:
			_heroineService.UpdateHeroineAI();
			_facilityClickHeroine.UpdateFacility();
			if (_facilityClickHeroine.IsReactionStartReady())
			{
				_scenarioReader.ReadNextForScript();
				_mainState.Value = MainState.Tutorial5;
			}
			break;
		case MainState.Tutorial6:
			if (_storyController.IsTalkLog)
			{
				_mainState.Value = MainState.StoryEndTidying;
				EndPlayedStory(EndAction).Forget();
			}
			else
			{
				_musicService.SetRepeat(isrepeat: false);
				_tutorialService.OnEndFirstTutorialProcess();
				_tutorialService.OpenTutorial(TutorialService.TutorialPageType.ScreenUI, TutorialService.TutorialPageOpenType.HelpAll);
				_roomCameraManager.OnEndStory();
				SkipTutorial();
				_mainState.Value = MainState.Idle;
			}
			SaveDataManager.Instance.PlayerData.IsNeedTutorial = false;
			_storyController.SaveScenarioPlayedLog();
			SaveDataManager.Instance.SavePlayerData();
			break;
		case MainState.DemoTutorialFinish:
			if (_tutorialService.IsCloseTutorialPage())
			{
				_mainState.Value = MainState.Idle;
			}
			break;
		case MainState.Idle:
			_platformRoot.UpdatePlatform();
			_facilityClickHeroine.UpdateFacility();
			_facilityVoiceTextScenario.UpdateFacility();
			if (!_uiShowManager.IsShowUI && Input.GetMouseButtonDown(0))
			{
				_uiShowManager.AllUIActivate().Forget();
			}
			if (_directionService.GamePlayingDefect.IsConnectionLost() && SaveDataManager.Instance.ScenarioProgressData.IsPossibleTalkNextMainEpisode())
			{
				_storyController.StartStory(ScenarioType.MainScenario, SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber);
			}
			if (_storyController.IsStoryStartReady)
			{
				_facilityClickHeroine.CancelReaction();
				ReadyPlayStory().Forget();
				_mainState.Value = MainState.StoryReady;
			}
			else if (_pomodoroTalkController.IsTalkStartReady)
			{
				PlayStartPomodoroTalk();
			}
			else if (_facilityClickHeroine.IsReactionStartReady())
			{
				PlayHeroineTouchReaction();
			}
			else if (_facilityClickHeroine.IsReactionPlayEnd())
			{
				OnEndPlayedReaction();
			}
			else if (_facilityVoiceTextScenario.IsStartReady())
			{
				_facilityVoiceTextScenario.StartScenario();
			}
			else if (_facilityVoiceTextScenario.IsPlayEnd())
			{
				_facilityVoiceTextScenario.EndScenario();
			}
			else if (_unlockItemService.Environment.IsNeedGetNewAnnounce)
			{
				if (!_directionService.SlideFadeAnnounce.IsPlaying)
				{
					_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.UnlockEnvironment);
					_unlockItemService.Environment.IsNeedGetNewAnnounce = false;
				}
			}
			else if (_unlockItemService.Decoration.IsNeedGetNewAnnounce)
			{
				if (!_directionService.SlideFadeAnnounce.IsPlaying)
				{
					_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.UnlockDecoration);
					_unlockItemService.Decoration.IsNeedGetNewAnnounce = false;
				}
			}
			else if (_unlockMusic.IsNeedGetNewAnnounce)
			{
				if (!_directionService.SlideFadeAnnounce.IsPlaying)
				{
					_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.UnlockMusic);
					_unlockMusic.IsNeedGetNewAnnounce = false;
				}
			}
			else if (_specialService.IsNeedUsePossibleAnnounce())
			{
				_specialService.PlayPossibleAnnounce();
				_specialService.CreateList();
				_specialService.SetEnableNewIconMark();
			}
			else if (_specialService.IsNeedUseFinishAnnounce())
			{
				_specialService.PlayFinishAnnounce();
			}
			else if (_levelUpDirectionController.IsReadyStartLevelUpDirection)
			{
				if (!_directionService.SlideFadeAnnounce.IsPlaying)
				{
					_levelUpDirectionController.StartLevelUpDirection();
					if (SaveDataManager.Instance.ScenarioProgressData.IsPossibleTalkNextMainEpisode() && _directionService.SlideFadeAnnounce.IsPossibleWantTalkAnnounce)
					{
						_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.WantTalk);
					}
				}
			}
			else if (_levelUpDirectionController.IsEndLevelUpDirection)
			{
				_levelUpDirectionController.EndLevelUpDirection();
			}
			_heroineService.UpdateHeroineAI();
			break;
		case MainState.StoryReady:
			_platformRoot.UpdateMusicOnly();
			break;
		case MainState.StoryPlayStart:
			PlayStartStory();
			break;
		case MainState.StoryPlaying:
			if (_storyController.IsStoryPlayEnd)
			{
				_mainState.Value = MainState.StoryEndTidying;
				EndPlayedStory().Forget();
			}
			break;
		case MainState.StoryEndTidying:
			_platformRoot.UpdateMusicOnly();
			break;
		case MainState.PlayingTalk:
			if (_pomodoroTalkController.IsTalkPlayEnd)
			{
				OnEndPlayedTalk();
			}
			break;
		case MainState.ExitGame0:
			_heroineService.UpdateHeroineAI();
			_exitOnlyPoorConnectionViewController?.Activate();
			if (DevicePlatform.Steam.IsMobile())
			{
				ScreenOrientationManagerForMobile.Instance.LockRotatePortrait();
			}
			if (_loadDirectionService.IsFinishGameEndDirection() && !saveDataDirtyManager.IsUploading)
			{
				_mainState.Value = MainState.Release;
			}
			break;
		case MainState.Release:
			_musicPlayerController.Release();
			ExitGame();
			break;
		case MainState.Debug:
			if (!(_debugService == null) && _debugService.IsNeedTidyingGameEndDirection)
			{
				_heroineService.UpdateHeroineAI();
				if (!_debugService.IsDebugGameEndDirectionEnabled && _loadDirectionService.IsFinishGameEndDirection())
				{
					_debugService.IsNeedTidyingGameEndDirection = false;
					_loadDirectionService.DebugImmediateStartFadeIn();
					_heroineService.OnEndStory(0);
					_uiShowManager.AllUIActivate().Forget();
					_mainState.Value = MainState.Idle;
				}
			}
			break;
		}
		if (!DevicePlatform.Steam.IsMobile() && Input.GetKeyDown(KeyCode.Escape))
		{
			_settingService.ChangeWindowMode(WindowModeType.Window);
		}
		void EndAction()
		{
			SkipTutorial();
		}
	}

	private async UniTask ReadyPlayStory()
	{
		_sceneFadeControllerProvider.Controller.FadeOut().Forget();
		if (!_directionService.GamePlayingDefect.IsConnectionLost())
		{
			_systemSeService.PlayScenarioStart();
		}
		_isPausedMusicBeforeStory = _musicPlayerController.IsPaused;
		bool isPauseMusic = false;
		if (_musicPlayerController.IsPaused || _musicService.PlayingMusic == null)
		{
			isPauseMusic = true;
		}
		else
		{
			Action callback = delegate
			{
				isPauseMusic = true;
			};
			SingletonMonoBehaviour<MusicManager>.Instance.FadeFromCurrentVolume(_musicService.PlayingMusic.AudioClip.name, 1.5f, 0f, callback);
		}
		_audioMixer.FadeVolumeForScenario(0f, 1.5f, delegate
		{
			SingletonMonoBehaviour<AmbientBGMManager>.Instance.Pause();
			_fireworksSoundService.Stop();
		});
		await WaitWithTimeout(() => _sceneFadeControllerProvider.Controller.IsComplete() && isPauseMusic, 30f);
		_autoTimeWindowViewChanger.SetPossibleUseAutoChanger(possible: false);
		_musicPlayerController.PauseMusic();
		_heroineService.HeroineUseObjectController.StoryReady();
		_directionService.GamePlayingDefect.Init();
		_roomCameraManager.OnStartStory();
		_storyController.Ready();
		_limitedTimeEventService.OnStoryReady(_scenarioReader.PlayingScenarioType);
		if (DevicePlatform.Steam.IsMobile() && _scenarioReader.IsPlayingTutorial())
		{
			ScreenOrientationManagerForMobile.Instance.LockRotatePortrait();
		}
		_onReadyLongStoryAfterFade.OnNext(_scenarioReader.PlayingScenarioType);
		if (_scenarioReader.IsPlayingLongStory())
		{
			_uiShowManager.AdjustUIForPlayScenario();
		}
		else if (_scenarioReader.IsPlayingTutorial())
		{
			_uiShowManager.AdjustUIForStartTutorialScenario();
		}
		await UniTask.Delay(TimeSpan.FromSeconds(1.5));
		_roomCameraManager.SkipCameraBlend();
		_scenarioReader.PlayStoryStartMusic();
		_scenarioReader.PlayStoryStartAmbientBGM();
		_audioMixer.FadeVolumeForScenario(1f, 1.5f);
		_sceneFadeControllerProvider.Controller.FadeIn().Forget();
		await WaitWithTimeout(() => _sceneFadeControllerProvider.Controller.IsComplete() && isPauseMusic, 30f);
		_mainState.Value = MainState.StoryPlayStart;
	}

	private void PlayStartStory()
	{
		switch (_scenarioReader.PlayingScenarioType)
		{
		case ScenarioType.Tutorial:
			_mainState.Value = MainState.Tutorial1;
			break;
		case ScenarioType.MainScenario:
		case ScenarioType.AfterScenario:
		case ScenarioType.DLCScenario:
		case ScenarioType.ExtraScenario:
		case ScenarioType.Special_AlterEgo:
		case ScenarioType.Special_BearsRestaurant:
		case ScenarioType.Special_Valentine2026:
		case ScenarioType.Special_LunaNewYear2026:
		case ScenarioType.Special_NearSpring2026:
		case ScenarioType.Event_2026_AprilFool:
			_storyController.StartStory();
			_mainState.Value = MainState.StoryPlaying;
			break;
		case ScenarioType.SmallTalk:
			_storyController.StartStory();
			_mainState.Value = MainState.StoryPlaying;
			break;
		default:
			Debug.LogError($"PlayStartStory()にScenarioType⇒{_scenarioReader.PlayingScenarioType}が定義されていません。追加してください。");
			break;
		}
	}

	private async UniTask EndPlayedStory(Action endAction = null)
	{
		if (_storyController.EpisodeNumber != 32f)
		{
			await EndPlayedStoryForNormal(endAction);
		}
		else
		{
			await EndPlayedStoryForLastStory();
		}
		_mainState.Value = MainState.Idle;
	}

	private UniTask EndPlayedStoryForLastStory()
	{
		ScenarioType playingScenarioType = _scenarioReader.PlayingScenarioType;
		_storyController.EndStory();
		_sceneFadeControllerProvider.Controller.ImmediateFadeIn();
		_uiShowManager.AdjustUIForEndScenario();
		_uiShowManager.AllUIActivate().Forget();
		if (DevicePlatform.Steam.IsMobile())
		{
			ScreenOrientationManagerForMobile.Instance.SetAutoRotateFree();
		}
		_roomCameraManager.OnEndStory();
		if (!_isPausedMusicBeforeStory)
		{
			_musicPlayerController.UnPauseMusic();
		}
		if (_musicService.PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.FadeFromCurrentVolume(_musicService.PlayingMusic.AudioClip.name, 1.5f, 1f);
		}
		_autoTimeWindowViewChanger.SetPossibleUseAutoChanger(possible: true);
		_applyEnvironmentWindowController.ApplyWindowBySavedata();
		_decorationService.ApplyDecorationBySavedata();
		_heroineService.HeroineUseObjectController.StoryTidying();
		SingletonMonoBehaviour<AmbientBGMManager>.Instance.UnPause();
		SingletonMonoBehaviour<VoiceManager>.Instance.FadeAllFromCurrentVolume(0f, 1f);
		_onEndLongStoryBeforeFade.OnNext(playingScenarioType);
		return UniTask.CompletedTask;
	}

	private async UniTask EndPlayedStoryForNormal(Action endAction = null)
	{
		ScenarioType playingScenario = _scenarioReader.PlayingScenarioType;
		_sceneFadeControllerProvider.Controller.FadeOut().Forget();
		_audioMixer.FadeVolumeForScenario(0f, 1.5f);
		await _scenarioReader.FadeOutScenario();
		await WaitWithTimeout(() => _sceneFadeControllerProvider.Controller.IsComplete(), 30f);
		_roomCameraManager.OnEndStory();
		_uiShowManager.AdjustUIForEndScenario();
		_storyController.EndStory();
		_facilityVoiceTextScenario.EndScenario();
		_autoTimeWindowViewChanger.SetPossibleUseAutoChanger(possible: true);
		_applyEnvironmentWindowController.ApplyWindowBySavedata();
		_decorationService.ApplyDecorationBySavedata();
		_limitedTimeEventService.OnStoryTidying();
		_heroineService.HeroineUseObjectController.StoryTidying();
		endAction?.Invoke();
		PlayNeedUseDefectDirection();
		await UniTask.Delay(TimeSpan.FromSeconds(0.699999988079071));
		_roomCameraManager.SkipCameraBlend();
		_sceneFadeControllerProvider.Controller.FadeIn().Forget();
		if (!_isPausedMusicBeforeStory)
		{
			_musicPlayerController.UnPauseMusic();
		}
		if (_musicService.PlayingMusic != null)
		{
			SingletonMonoBehaviour<MusicManager>.Instance.FadeFromCurrentVolume(_musicService.PlayingMusic.AudioClip.name, 1.5f, 1f);
		}
		SingletonMonoBehaviour<AmbientBGMManager>.Instance.UnPause();
		_audioMixer.FadeVolumeForScenario(1f, 1.5f);
		SingletonMonoBehaviour<VoiceManager>.Instance.FadeAllFromCurrentVolume(0f, 1f);
		if (DevicePlatform.Steam.IsMobile())
		{
			ScreenOrientationManagerForMobile.Instance.SetAutoRotateFree();
		}
		_onEndLongStoryBeforeFade.OnNext(playingScenario);
		await WaitWithTimeout(() => _sceneFadeControllerProvider.Controller.IsComplete(), 30f);
	}

	private async UniTask WaitWithTimeout(Func<bool> condition, float timeoutSeconds)
	{
		UniTask uniTask = UniTask.WaitUntil(condition);
		UniTask uniTask2 = UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds));
		await UniTask.WhenAny(uniTask, uniTask2);
	}

	private void PlayStartPomodoroTalk()
	{
		_uiShowManager.AdjustUIForPlayScenario();
		_pomodoroTalkController.StartTalk();
		_mainState.Value = MainState.PlayingTalk;
	}

	private void OnEndPlayedTalk()
	{
		_uiShowManager.AdjustUIForEndScenario();
		_pomodoroTalkController.EndTalk();
		_heroineService.ChangeLookScaleAnimation(0f, 0.5f);
		_mainState.Value = MainState.Idle;
	}

	public void PlayHeroineTouchReaction()
	{
		ScenarioType scenarioType = ScenarioType.HeroineClickNormal;
		if (_heroineService.IsSleeping())
		{
			_facilityClickHeroine.CancelReaction();
			_heroineService.WantJumpUp();
			return;
		}
		if (_pomodoroTalkController.IsCurrentWorking)
		{
			scenarioType = ScenarioType.HeroineClickWork;
		}
		else
		{
			if (_heroineService.GetCurrentAIState() == HeroineAI.ActionStateType.WantTalk)
			{
				if (_limitedTimeEventService.IsContainCurrentEvent(LimitedTimeEventType.AprilFool2026) && _scenarioGroupMaster.GetPlayableEventAprilFoolScenario(includeSeen: false).Any())
				{
					_facilityClickHeroine.StartReaction(ScenarioType.Event_2026_AprilFool);
					return;
				}
				ScenarioType scenarioType2 = ScenarioType.MainScenario;
				float num = 0f;
				if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.None)
				{
					if (SaveDataManager.Instance.ScenarioProgressData.IsPossibleTalkNextMainEpisode())
					{
						scenarioType2 = ScenarioType.MainScenario;
						num = SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber;
					}
					else
					{
						ScenarioGroupData scenarioGroupData = _scenarioGroupMaster.GetPlayableExtraScenario(includeSeen: false).FirstOrDefault();
						if (scenarioGroupData == null)
						{
							_facilityClickHeroine.CancelReaction();
							return;
						}
						scenarioType2 = ScenarioType.ExtraScenario;
						num = scenarioGroupData.EpisodeNumber;
					}
				}
				else
				{
					if (!_specialService.IsPossibleReadNextSpecialEpisodeNumber())
					{
						_facilityClickHeroine.CancelReaction();
						return;
					}
					scenarioType2 = _specialService.GetNextLongTalkScenarioType();
					num = _specialService.GetNextEpisodeNumber(scenarioType2);
					if (num == -1f)
					{
						_facilityClickHeroine.CancelReaction();
						return;
					}
				}
				_storyController.StartStory(scenarioType2, num);
				return;
			}
			if (_pomodoroTalkController.IsCurrentResting)
			{
				scenarioType = ScenarioType.HeroineClickBreak;
			}
		}
		_facilityClickHeroine.StartReaction(scenarioType);
	}

	private void OnEndPlayedReaction()
	{
		_facilityClickHeroine.EndReaction();
		_heroineService.LookInitSlowly();
		_heroineService.InitHeroineFacialAfterDelay(0.77f);
		_mainState.Value = MainState.Idle;
	}

	private bool PlayNeedUseDefectDirection()
	{
		GamePlayingDefectDirection.DefectType defectType = _directionService.GamePlayingDefect.CheckNeedUseDefectDirection();
		_directionService.GamePlayingDefect.PlayDefectDirection(defectType);
		if (defectType != GamePlayingDefectDirection.DefectType.None)
		{
			return true;
		}
		return false;
	}

	public bool IsCanUseFacility()
	{
		if (_mainState.Value == MainState.Load || _mainState.Value == MainState.Initialize)
		{
			return false;
		}
		return true;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (SaveDataManager.Instance.SettingData.IsUseVerticalSync.Value)
		{
			if (hasFocus)
			{
				_settingService.ApplyVerticalSync(isUse: true);
			}
			else
			{
				_settingService.ApplyVerticalSync(isUse: false);
			}
		}
		_settingService.UpdateFrameRateForApplicationFocus();
		_applicationFocusService.OnFocusChanged(hasFocus);
		UpdateFocusNextFrameAsync(hasFocus).Forget();
		async UniTaskVoid UpdateFocusNextFrameAsync(bool isFocus)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
			_applicationFocusService.UpdateBeforeIsFocus(isFocus);
		}
	}

	private void ExitGame()
	{
		SaveDataManager.Instance.DeleteSafeSaveKey();
		UnityEngine.Device.Application.Quit();
	}

	private void OnApplicationQuit()
	{
		_directionService.EndTidying();
		_achievementService.Dispose();
	}

	public void SkipTutorial()
	{
		_scenarioReader.EndStory();
		_uiShowManager.AdjustUIForEndScenario();
		_uiShowManager.AllUIActivate().Forget();
		_raycastBlocker.Release();
		_heroineService.OnEndStory(0);
		OnEndPlayedReaction();
		if (DevicePlatform.Steam.IsMobile())
		{
			ScreenOrientationManagerForMobile.Instance.SetAutoRotateFree();
		}
		_mainState.Value = MainState.Idle;
	}
}
