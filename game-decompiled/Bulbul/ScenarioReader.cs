using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KanKikuchi.AudioManager;
using NestopiSystem;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class ScenarioReader : IDisposable
{
	public enum ScenarioReadMode : byte
	{
		Click,
		Auto
	}

	[Inject]
	private IStorySystemUI _myStorySystemUI;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private LocalizationMasterWrapper localizationMaster;

	[Inject]
	private WindowViewService _windowViewService;

	[Inject]
	private TutorialService _tutorialService;

	[Inject]
	private IUIShowManager _uiShowManager;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private DecorationService _decorationService;

	[Inject]
	private LimitedTimeEventService _limitedTimeEventService;

	[Inject]
	private EndCreditsService _endCreditsService;

	[Inject]
	private IApplyEnvironmentWindowController _applyEnvironmentWindowController;

	[Inject]
	private LoadDirectionService _loadDirectionService;

	[Inject]
	private AudioMixerGroupContainer _audioMixer;

	[Inject]
	private MusicService _musicService;

	[Inject]
	private FacilityLockEventService _facilityLockEventService;

	private MusicData _playingMusicData;

	private NovelData _firstMusicNovel;

	private List<NovelData> _startPlayAmbientSoundDataList = new List<NovelData>();

	private readonly List<NovelData> _currentGroupNovels = new List<NovelData>();

	private readonly List<LocalizationData> _currentGroupLocalizations = new List<LocalizationData>();

	private int _currentMessageIndex;

	private int _startStoryIndex;

	private NovelData _currentNovel;

	private bool _isFinishedDisplayingText;

	private readonly List<Action> _endCallbackList = new List<Action>();

	private ScenarioType _playingScenarioType;

	private string _playingScenarioGroupID;

	private ScenarioReadMode _readMode = ScenarioReadMode.Auto;

	private IDisposable _voiceTextDisposable;

	private DisposableBag disposable;

	private CancellationTokenSource _autoTextCts = new CancellationTokenSource();

	private CancellationTokenSource _endStoryCts = new CancellationTokenSource();

	private bool _isPlayingScenario;

	private bool _isUseMask;

	private SelectedData _selectionData = new SelectedData();

	private ScenarioCommandTextLoop _textLoop = new ScenarioCommandTextLoop();

	private bool _isPlayingTalkLog;

	private bool _isEndWaitReadNextForScript;

	private Subject<ScenarioType> _onStartReady = new Subject<ScenarioType>();

	private Subject<ScenarioType> _onEndStory = new Subject<ScenarioType>();

	private readonly Subject<Unit> onEndTidyingForStory = new Subject<Unit>();

	private readonly Subject<Unit> onEndTidyingInStory = new Subject<Unit>();

	public ScenarioType PlayingScenarioType => _playingScenarioType;

	public string PlayingScenarioGroupID => _playingScenarioGroupID;

	public Observable<ScenarioType> OnStartReady => _onStartReady;

	public Observable<ScenarioType> OnEndStory => _onEndStory;

	public Observable<Unit> OnEndTidyingForStory => onEndTidyingForStory;

	public Observable<Unit> OnEndTidyingInStory => onEndTidyingInStory;

	public void Dispose()
	{
		Release();
	}

	private void Release()
	{
		_currentGroupNovels.Clear();
		_currentGroupLocalizations.Clear();
		_endCallbackList.Clear();
		disposable.Dispose();
		_currentNovel = null;
		_autoTextCts?.Cancel();
		_autoTextCts?.Dispose();
		_autoTextCts = null;
	}

	private Ease ParseEaseType(string easeString)
	{
		if (string.IsNullOrEmpty(easeString))
		{
			return Ease.Linear;
		}
		if (Enum.TryParse<Ease>(easeString, ignoreCase: true, out var result))
		{
			return result;
		}
		Debug.LogWarning("Invalid Ease type: " + easeString + ". Using Linear as default.");
		return Ease.Linear;
	}

	public bool IsReadNextForScript()
	{
		return _currentNovel.Command == CommandType.ReadNextForScript;
	}

	public void StartReady(ScenarioType scenarioType, string groupId, ScenarioReadMode readMode, bool isTalkLog = false, bool isUseMask = true, bool isForceInitAnim = false)
	{
		if (_endStoryCts == null || _endStoryCts.IsCancellationRequested)
		{
			_endStoryCts?.Dispose();
			_endStoryCts = new CancellationTokenSource();
		}
		_playingScenarioType = scenarioType;
		_onStartReady.OnNext(_playingScenarioType);
		_playingMusicData = null;
		_firstMusicNovel = null;
		_isPlayingScenario = true;
		_isUseMask = isUseMask;
		_readMode = readMode;
		_isPlayingTalkLog = isTalkLog;
		_currentMessageIndex = 0;
		_startStoryIndex = 0;
		_currentGroupNovels.Clear();
		_currentGroupNovels.AddRange(_masterDataLoader.NovelMasterList.Where((NovelData n) => n.ScenarioGroupID == groupId));
		_playingScenarioGroupID = _currentGroupNovels[0].ScenarioGroupID;
		string[] localizationIds = _currentGroupNovels.Select((NovelData x) => x.LocalizationID).ToArray();
		_currentGroupLocalizations.Clear();
		_currentGroupLocalizations.AddRange(_masterDataLoader.LocalizationList.Values.Where((LocalizationData n) => Enumerable.Contains(localizationIds, n.ID)));
		_isFinishedDisplayingText = false;
		_heroineService.ScenarioStartReady();
		if (IsPlayingTutorial())
		{
			_tutorialService.ReadyTutorial();
		}
		int firstMotionCommandIndex;
		bool _isSettedStartStoryIndex;
		if (IsPlayingLongStory() || isTalkLog)
		{
			_endCallbackList.Clear();
			_myStorySystemUI.MainStoryReady(_currentGroupNovels[0].ScenarioGroupID, scenarioType);
			firstMotionCommandIndex = -1;
			_isSettedStartStoryIndex = false;
			List<NovelData> list = new List<NovelData>();
			while (firstMotionCommandIndex == -1 || _startStoryIndex == -1)
			{
				switch (_currentGroupNovels[_currentMessageIndex].Command)
				{
				case CommandType.Text:
				case CommandType.ChangeMotion:
				case CommandType.AddTextLoop:
				case CommandType.DirectionGameStartCamera:
					UpdateStartCommandIndex();
					UpdateFirstMotionCommandIndex();
					break;
				case CommandType.WaitForSeconds:
					UpdateStartCommandIndex();
					break;
				case CommandType.PlayBGM:
					_firstMusicNovel = _currentGroupNovels[_currentMessageIndex];
					break;
				case CommandType.TalkLogOnlyPlayBGM:
					if (isTalkLog)
					{
						_firstMusicNovel = _currentGroupNovels[_currentMessageIndex];
					}
					break;
				case CommandType.PlayAmbientBGM:
					if (!_startPlayAmbientSoundDataList.Contains(_currentGroupNovels[_currentMessageIndex]))
					{
						_startPlayAmbientSoundDataList.Add(_currentGroupNovels[_currentMessageIndex]);
					}
					break;
				case CommandType.ChangeWindowView:
					list.Add(_currentGroupNovels[_currentMessageIndex]);
					break;
				case CommandType.ChangeLayout:
				{
					string arg = _currentGroupNovels[_currentMessageIndex].Arg1;
					DeskType deskType;
					if (_decorationService.ParseDecorationTypeForString(arg, out var decorationType))
					{
						bool isSave = false;
						if ((uint)(decorationType - 7) <= 4u && _unlockItemService.Decoration.GetLockState(DecorationService.DecorationSkinType.Book_5).IsLocked.CurrentValue)
						{
							ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.ID == _playingScenarioGroupID);
							if (scenarioGroupData != null && scenarioGroupData.Scenario == ScenarioType.MainScenario && SaveDataManager.Instance.ScenarioProgressData.IsLatestMainEpisode(scenarioGroupData.EpisodeNumber))
							{
								isSave = true;
							}
						}
						CommandChangeLayout(decorationType, isSave);
					}
					else if (_heroineService.HeroineUseObjectController.ParseDeskTypeForString(arg, out deskType))
					{
						_heroineService.HeroineUseObjectController.ImmediateChangeDeskType(deskType);
					}
					break;
				}
				}
				_currentMessageIndex++;
			}
			float num = -1f;
			float num2 = -1f;
			float num3 = -1f;
			int isUseBlind = -1;
			float num4 = -1f;
			float num5 = -1f;
			float num6 = -1f;
			int num7 = _startStoryIndex - 1;
			if (num7 >= 0)
			{
				for (int num8 = num7; num8 >= 0; num8--)
				{
					switch (_currentGroupNovels[num8].Command)
					{
					case CommandType.DirectionGlitch1:
						if (num == -1f)
						{
							num = ((_currentGroupNovels[num8].Arg2 != -1f) ? 0f : float.Parse(_currentGroupNovels[num8].Arg1));
						}
						break;
					case CommandType.DirectionScreenMove:
						if (num2 == -1f)
						{
							num2 = ((_currentGroupNovels[num8].Arg2 != -1f) ? 0f : float.Parse(_currentGroupNovels[num8].Arg1));
						}
						break;
					case CommandType.DirectionBlackOut:
						if (num3 == -1f)
						{
							num3 = ((_currentGroupNovels[num8].Arg2 != -1f) ? 0f : float.Parse(_currentGroupNovels[num8].Arg1));
							isUseBlind = int.Parse(_currentGroupNovels[num8].Arg3);
						}
						break;
					case CommandType.DirectionMonochromeNoise:
						if (num4 == -1f)
						{
							num4 = float.Parse(_currentGroupNovels[num8].Arg1);
						}
						break;
					case CommandType.DirectionChromaticAberration:
						if (num5 == -1f)
						{
							num5 = float.Parse(_currentGroupNovels[num8].Arg1);
						}
						break;
					case CommandType.VoiceDefectRatio:
						if (num6 == -1f)
						{
							num6 = float.Parse(_currentGroupNovels[num8].Arg1);
						}
						break;
					}
				}
			}
			_directionService.Glitch.ImmediateChange(num);
			_directionService.ScreenMove.ImmediateChange(num2);
			_directionService.BlackOut.ImmediateChange(num3, isUseBlind);
			_directionService.MonochromeNoise.ImmediateChange(num4);
			_directionService.ChromaticAberration.ImmediateChange(num5);
			if (list.Count > 0)
			{
				_windowViewService.ResetOtherThanTimeOfDay();
				foreach (NovelData item in list)
				{
					CommandChangeWindowView(item);
				}
			}
			int num9 = _currentGroupNovels[firstMotionCommandIndex].BodyMotion;
			if (_currentGroupNovels[firstMotionCommandIndex].BodyMotion == -1)
			{
				num9 = 0;
			}
			_heroineService.OnStoryStartReady(num9, _currentGroupNovels[firstMotionCommandIndex].FacialMotion);
			HeroineService.AnimationType animType = (HeroineService.AnimationType)num9;
			_heroineService.HeroineUseObjectController.ImmediateChangeDeskUseObjectAnimation(animType);
			_heroineService.HeroineUseObjectController.ImmediateCurrentChangeDesk();
			_myStorySystemUI.ActivateSkipButton();
		}
		else if (isForceInitAnim)
		{
			_heroineService.OnStoryStartReady(_currentGroupNovels[0].BodyMotion, _currentGroupNovels[0].FacialMotion);
		}
		void UpdateFirstMotionCommandIndex()
		{
			if (firstMotionCommandIndex == -1)
			{
				firstMotionCommandIndex = _currentMessageIndex;
			}
		}
		void UpdateStartCommandIndex()
		{
			if (!_isSettedStartStoryIndex)
			{
				_startStoryIndex = _currentMessageIndex;
				_isSettedStartStoryIndex = true;
			}
		}
	}

	private async UniTask AutoNext(float waitSeconds = 0f)
	{
		_heroineService.EndNoVoiceTalk();
		int num = _currentMessageIndex + 1;
		int msgIndex = _currentMessageIndex;
		bool flag = false;
		if (num >= _currentGroupNovels.Count)
		{
			_autoTextCts?.Cancel();
			_autoTextCts?.Dispose();
			_autoTextCts = null;
			_autoTextCts = new CancellationTokenSource();
			flag = await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _autoTextCts.Token).SuppressCancellationThrow();
		}
		else if (_currentGroupNovels[num].Command == CommandType.Selection)
		{
			ReadNext();
		}
		else
		{
			_autoTextCts?.Cancel();
			_autoTextCts?.Dispose();
			_autoTextCts = null;
			_autoTextCts = new CancellationTokenSource();
			flag = await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _autoTextCts.Token).SuppressCancellationThrow();
		}
		if (!flag && msgIndex == _currentMessageIndex && _readMode == ScenarioReadMode.Auto)
		{
			ReadNext();
		}
	}

	public bool IsPlayingTutorial()
	{
		return _playingScenarioType == ScenarioType.Tutorial;
	}

	public bool IsPlayingMiddleTalk()
	{
		if (_playingScenarioType != ScenarioType.SmallTalk)
		{
			return _playingScenarioType == ScenarioType.Event_2026_AprilFool;
		}
		return true;
	}

	public bool IsPlayingLongStory()
	{
		return _playingScenarioType.IsLongStory();
	}

	public bool IsPlayingLongStoryOrTutorial()
	{
		if (_playingScenarioType.IsLongStoryOrTutorial())
		{
			return _isPlayingScenario;
		}
		return false;
	}

	private bool IsGameStartStory()
	{
		if (_playingScenarioType == ScenarioType.GameStart_First_CameraTouch || _playingScenarioType == ScenarioType.GameStart_LessTowDays_CameraTouch || _playingScenarioType == ScenarioType.GameStart_LessTowDays_CameraTouch_Morning || _playingScenarioType == ScenarioType.GameStart_LessTowDays_CameraTouch_Noon || _playingScenarioType == ScenarioType.GameStart_LessTowDays_CameraTouch_Evening || _playingScenarioType == ScenarioType.GameStart_LessTowDays_CameraTouch_Night || _playingScenarioType == ScenarioType.GameStart_LessTowDays || _playingScenarioType == ScenarioType.GameStart_LessHarfMonth_CameraTouch || _playingScenarioType == ScenarioType.GameStart_GreaterHarfMonth_CameraTouch)
		{
			return true;
		}
		return false;
	}

	public void ChangeReadMode(ScenarioReadMode readMode)
	{
		_readMode = readMode;
		if (_readMode == ScenarioReadMode.Auto && _isFinishedDisplayingText)
		{
			AutoNext().Forget();
		}
	}

	public void PlayStoryStartMusic()
	{
		if (_firstMusicNovel != null)
		{
			CommandPlayBGM(_firstMusicNovel);
		}
	}

	public void PlayStoryStartAmbientBGM()
	{
		foreach (NovelData startPlayAmbientSoundData in _startPlayAmbientSoundDataList)
		{
			CommandPlayAmbientBGM(startPlayAmbientSoundData);
		}
	}

	public void StartMainStory()
	{
		_myStorySystemUI.Begin(_isUseMask);
		_myStorySystemUI.AddOnClickCallback(NextMessageByClick);
		_myStorySystemUI.CurrentTextMessage().AddOnTextShowedCallback(CompleteDisplayText);
		_heroineService.OnStartStory();
		ReadSentence(_startStoryIndex).Forget();
	}

	public void DebugPhotographStartMainStory()
	{
		_heroineService.OnStartStory();
		ReadSentence(_startStoryIndex).Forget();
	}

	public void StartClickHeroineReaction()
	{
		if (IsPlayingMiddleTalk())
		{
			_facilityLockEventService.SendLockEvent();
		}
		_myStorySystemUI.Begin(_isUseMask);
		_myStorySystemUI.AddOnClickCallback(NextMessageByClick);
		_myStorySystemUI.CurrentTextMessage().AddOnTextShowedCallback(CompleteDisplayText);
		_heroineService.OnStartClickHeroineReaction();
		ReadSentence(_startStoryIndex).Forget();
	}

	public void StartVoiceTextScenario()
	{
		LocalizationData data = _currentGroupLocalizations.FirstOrDefault((LocalizationData x) => x.ID == _currentGroupNovels[_startStoryIndex].LocalizationID);
		if (!localizationMaster.Get(data).IsNullOrEmpty())
		{
			_myStorySystemUI.Begin(_isUseMask);
			_myStorySystemUI.AddOnClickCallback(NextMessageByClick);
			_myStorySystemUI.CurrentTextMessage().AddOnTextShowedCallback(CompleteDisplayText);
		}
		ReadSentence(_startStoryIndex).Forget();
	}

	public void EndStory()
	{
		_unlockItemService.UnlockUpdate(UnlockItemService.ConditionsType.Scenario, _playingScenarioGroupID);
		_isPlayingScenario = false;
		_currentMessageIndex = 0;
		if (!IsPlayingLongStory() && !_isPlayingTalkLog)
		{
			_endStoryCts.Cancel();
			_tutorialService.Cancel();
			_endCreditsService.CancelPlay();
			_myStorySystemUI.FadeController.ImmediateFadeIn();
			_myStorySystemUI.Finish();
			_heroineService.CancelVoice();
		}
		InvokeEndCallbacks();
		_voiceTextDisposable?.Dispose();
		_voiceTextDisposable = null;
		_startPlayAmbientSoundDataList.Clear();
		_textLoop.EndTidying();
		_onEndStory.OnNext(_playingScenarioType);
		if (IsPlayingMiddleTalk())
		{
			_facilityLockEventService.SendUnlockEvent();
		}
		Release();
		void InvokeEndCallbacks()
		{
			Action[] array = _endCallbackList.ToArray();
			foreach (Action action in array)
			{
				try
				{
					action?.Invoke();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			_endCallbackList.Clear();
		}
	}

	public void EndTidyingForStory()
	{
		_endStoryCts.Cancel();
		_tutorialService.Cancel();
		_myStorySystemUI.FadeController.ImmediateFadeIn();
		_endCreditsService.CancelPlay();
		SingletonMonoBehaviour<ScenarioMusicManager>.Instance.Stop();
		SingletonMonoBehaviour<ScenarioAmbientBGMManager>.Instance.Stop();
		_directionService.EndTidying();
		_tutorialService.EndTidying();
		_myStorySystemUI.Finish();
		_myStorySystemUI.DeactivateAutoButton();
		_myStorySystemUI.DeactivateSkipButton();
		_heroineService.OnEndStory(0);
		_windowViewService.ResetOtherThanTimeOfDay();
		onEndTidyingForStory.OnNext(Unit.Default);
	}

	public void EndClickHeroineReaction()
	{
		_heroineService.OnEndClickHeroineReaction();
	}

	public void AddEndCallback(Action callback)
	{
		if (_endCallbackList.Contains(callback))
		{
			Debug.LogError(callback?.Method.Name + " is already added");
		}
		else
		{
			_endCallbackList.Add(callback);
		}
	}

	private void CompleteDisplayText()
	{
		_isFinishedDisplayingText = true;
	}

	private void NextMessageByClick()
	{
		if (_currentNovel != null)
		{
			CommandType command = _currentNovel.Command;
			if ((command == CommandType.Text || command == CommandType.WaitInput) && _currentNovel.Command == CommandType.Text && !_isFinishedDisplayingText)
			{
				_myStorySystemUI.CurrentTextMessage().DisplayAllText();
			}
		}
	}

	public void SkipStory()
	{
		EndStory();
	}

	private void ReadNext()
	{
		int num = _currentMessageIndex + 1;
		if (num >= _currentGroupNovels.Count)
		{
			EndStory();
		}
		else
		{
			ReadSentence(num).Forget();
		}
	}

	public void ReadNextForScript()
	{
		if (IsReadNextForScript())
		{
			_isEndWaitReadNextForScript = true;
		}
	}

	private async UniTaskVoid ReadSentence(int nextIndex)
	{
		if (nextIndex < 0 || _currentGroupNovels.Count <= nextIndex)
		{
			return;
		}
		_currentMessageIndex = nextIndex;
		_currentNovel = _currentGroupNovels[nextIndex];
		bool isCanceled = false;
		switch (_currentNovel.Command)
		{
		case CommandType.Text:
			await CommandText(_currentNovel);
			break;
		case CommandType.VoiceText:
			await CommandVoiceText(_currentNovel);
			break;
		case CommandType.ChangeMotion:
			await ChangeMotion();
			ReadNext();
			break;
		case CommandType.PomodoroVoiceText:
		{
			NovelData novelData = _currentNovel;
			if (novelData.Arg2 == -1f)
			{
				await CommandPomodoroVoiceText(novelData);
				_heroineService.OnPlayPomodoroVoice();
				_heroineService.LookInitSlowly();
				_heroineService.InitHeroineFacialAfterDelay(0.77f);
				CommandPomodoroVoiceChangeMotion(novelData);
			}
			else
			{
				_heroineService.SettingAdjustTimingVoiceType(int.Parse(novelData.Arg1));
				CommandPomodoroVoiceChangeMotion(novelData);
				isCanceled = await WaitWithTimeout(() => _heroineService.IsCanPlayPomodoroVoice(), 30f, _endStoryCts.Token);
				if (isCanceled)
				{
					_heroineService.OnEndPomodoroVoice();
					break;
				}
				await CommandPomodoroVoiceText(novelData);
				_heroineService.OnPlayPomodoroVoice();
			}
			_heroineService.OnEndPomodoroVoice();
			break;
		}
		case CommandType.WaitForSeconds:
			await CommandWaitForSeconds(_currentNovel);
			ReadNext();
			break;
		case CommandType.WaitInput:
			CommandWaitInput();
			break;
		case CommandType.Selection:
			CommandSelection();
			break;
		case CommandType.Jump:
			SelectionJump(_currentNovel);
			break;
		case CommandType.PlayBGM:
			CommandPlayBGM(_currentNovel);
			ReadNext();
			break;
		case CommandType.PlayMusicServiceBGM:
			CommandPlayMusicServiceBGM(_currentNovel);
			ReadNext();
			break;
		case CommandType.PlaySE:
			PlaySE().Forget();
			ReadNext();
			break;
		case CommandType.PlayAmbientBGM:
			CommandPlayAmbientBGM(_currentNovel);
			ReadNext();
			break;
		case CommandType.StopBGM:
			CommandStopBGM(_currentNovel);
			ReadNext();
			break;
		case CommandType.StopSE:
			CommandStopSE(_currentNovel);
			ReadNext();
			break;
		case CommandType.StopAmbientBGM:
			CommandStopAmbientBGM(_currentNovel);
			ReadNext();
			break;
		case CommandType.ChangeWindowView:
			CommandChangeWindowView(_currentNovel);
			ReadNext();
			break;
		case CommandType.EndScenario:
			CommandEndScenario();
			break;
		case CommandType.StartSelectOnce:
			_selectionData.Reset();
			ReadNext();
			break;
		case CommandType.TutorialSelection:
			CommandTutorialSelection();
			break;
		case CommandType.TutorialFocusPomodoro:
			_myStorySystemUI.DeactivateNormalText();
			_uiShowManager.TutorialOtherUIActivate().Forget();
			isCanceled = await _tutorialService.FocusPomodoro(3.5f).SuppressCancellationThrow();
			if (!isCanceled)
			{
				isCanceled = await _uiShowManager.TutorialOtherUIDeactivate(_endStoryCts.Token).SuppressCancellationThrow();
				if (!isCanceled)
				{
					ReadNext();
				}
			}
			break;
		case CommandType.TutorialFocusTodo:
			_myStorySystemUI.DeactivateNormalText();
			_uiShowManager.TutorialOtherUIActivate().Forget();
			isCanceled = await _tutorialService.FocusTodo(3.5f).SuppressCancellationThrow();
			if (!isCanceled)
			{
				isCanceled = await _uiShowManager.TutorialOtherUIDeactivate(_endStoryCts.Token).SuppressCancellationThrow();
				if (!isCanceled)
				{
					ReadNext();
				}
			}
			break;
		case CommandType.TutorialFocusNote:
			_myStorySystemUI.DeactivateNormalText();
			_uiShowManager.TutorialOtherUIActivate().Forget();
			isCanceled = await _tutorialService.FocusNote(3.5f).SuppressCancellationThrow();
			if (!isCanceled)
			{
				isCanceled = await _uiShowManager.TutorialOtherUIDeactivate(_endStoryCts.Token).SuppressCancellationThrow();
				if (!isCanceled)
				{
					ReadNext();
				}
			}
			break;
		case CommandType.TutorialOpenExplanation:
			_myStorySystemUI.DeactivateNormalText();
			_myStorySystemUI.DeactivateTransparentButton();
			_tutorialService.OpenTutorial(TutorialService.TutorialPageType.ScreenUI, TutorialService.TutorialPageOpenType.HelpAll);
			isCanceled = await UniTask.WaitUntil(() => _tutorialService.IsCloseTutorialPage(), PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				_myStorySystemUI.ActivateTransparentButton();
				ReadNext();
			}
			break;
		case CommandType.TalkLogOnlyPlayBGM:
			if (_isPlayingTalkLog)
			{
				CommandPlayBGM(_currentNovel);
			}
			ReadNext();
			break;
		case CommandType.ChangeLayout:
		{
			if (_decorationService.ParseDecorationTypeForString(_currentGroupNovels[_currentMessageIndex].Arg1, out var decorationType))
			{
				CommandChangeLayout(decorationType);
			}
			ReadNext();
			break;
		}
		case CommandType.ChangeVolumeBGM:
			ChangeVolumeBGMCommand(_currentNovel);
			ReadNext();
			break;
		case CommandType.ReadNextForScript:
			_myStorySystemUI.DeactivateNormalText();
			_myStorySystemUI.DeactivateTransparentButton();
			_isEndWaitReadNextForScript = false;
			isCanceled = await UniTask.WaitUntil(() => _isEndWaitReadNextForScript, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				if (_playingScenarioType == ScenarioType.MainScenario)
				{
					_heroineService.OnStartStory();
				}
				else
				{
					_heroineService.OnStartClickHeroineReaction();
				}
				_myStorySystemUI.ActivateTransparentButton();
				ReadNext();
			}
			break;
		case CommandType.DeactivateNormalText:
			_myStorySystemUI.DeactivateNormalText();
			ReadNext();
			break;
		case CommandType.IfBetweenDateTime:
		{
			string[] array = _currentNovel.Arg1.Split('~');
			DateTime startTime = DateTime.ParseExact(array[0], "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
			DateTime endTime = DateTime.ParseExact(array[1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
			NovelData next = (DateTime.Now.IsBetween(startTime, endTime) ? new NovelData
			{
				ID = _currentNovel.ID + "to" + _currentNovel.Arg3,
				Arg1 = _currentNovel.Arg3
			} : new NovelData
			{
				ID = _currentNovel.ID + "to" + _currentNovel.Arg4,
				Arg1 = _currentNovel.Arg4
			});
			SelectionJump(next);
			break;
		}
		case CommandType.AddTextLoop:
			_textLoop.AddTextData(_currentNovel);
			ReadNext();
			break;
		case CommandType.StartTextLoop:
			while (_textLoop.MainState != ScenarioCommandTextLoop.LoopState.End)
			{
				switch (_textLoop.MainState)
				{
				case ScenarioCommandTextLoop.LoopState.Idle:
					_textLoop.StartSetup(_currentNovel);
					isCanceled = await CommandTextLoopPlayText(_textLoop.GetNextLoopTextNovel()).SuppressCancellationThrow();
					if (isCanceled)
					{
						return;
					}
					_textLoop.UseNextLoopTextNovel();
					ShowSelection().Forget();
					break;
				case ScenarioCommandTextLoop.LoopState.Looping:
					if (_textLoop.IsSelectedSelection)
					{
						SelectionJump(_textLoop.SelectedNovelData);
						_textLoop.EndLoop();
						break;
					}
					isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(_textLoop.GetNextLoopTextRandomDelayTime()), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
					if (isCanceled)
					{
						return;
					}
					if (_textLoop.IsSelectedSelection)
					{
						SelectionJump(_textLoop.SelectedNovelData);
						_textLoop.EndLoop();
					}
					else
					{
						await CommandTextLoopPlayText(_textLoop.GetNextLoopTextNovel());
						_textLoop.UseNextLoopTextNovel();
					}
					break;
				}
				isCanceled = await UniTask.NextFrame(_endStoryCts.Token).SuppressCancellationThrow();
				if (isCanceled)
				{
					break;
				}
			}
			break;
		case CommandType.BlackFadeOut:
		{
			float toSecond = float.Parse(_currentNovel.Arg1);
			Ease ease = ParseEaseType(_currentNovel.Arg3);
			await _myStorySystemUI.FadeController.FadeOut(toSecond, ease);
			ReadNext();
			break;
		}
		case CommandType.StartEndCredits:
			await _endCreditsService.Play();
			_myStorySystemUI.ChangeMessageType(StorySystemUI.MessageType.Normal);
			_myStorySystemUI.DeactivateBottomBackImage();
			_myStorySystemUI.Begin(_isUseMask);
			_myStorySystemUI.AddOnClickCallback(NextMessageByClick);
			_myStorySystemUI.CurrentTextMessage().AddOnTextShowedCallback(CompleteDisplayText);
			ReadNext();
			break;
		case CommandType.FinishEndCredits:
		{
			bool isLandscape = true;
			await _loadDirectionService.FinishEndCredits(isLandscape);
			_myStorySystemUI.FadeController.ImmediateFadeIn();
			_applyEnvironmentWindowController.ApplyWindowBySavedata();
			_decorationService.ApplyDecorationBySavedata();
			_limitedTimeEventService.OnStoryTidying();
			_heroineService.HeroineUseObjectController.StoryTidying();
			onEndTidyingInStory.OnNext(Unit.Default);
			_loadDirectionService.FadeInGame(LoadDirectionService.DirectionType.Normal, isLandscape);
			SingletonMonoBehaviour<AmbientBGMManager>.Instance.UnPause();
			_audioMixer.FadeVolumeForScenario(1f, 1.5f);
			ReadNext();
			break;
		}
		case CommandType.SetDirectionDelaySeconds:
			_directionService.SetPlayDelaySeconds(float.Parse(_currentNovel.Arg1));
			ReadNext();
			break;
		case CommandType.DirectionGlitch1:
			PlayGlitch().Forget();
			ReadNext();
			break;
		case CommandType.DirectionScreenMove:
			PlayScreenMove().Forget();
			ReadNext();
			break;
		case CommandType.DirectionBlackOut:
			PlayBlackOut().Forget();
			ReadNext();
			break;
		case CommandType.DirectionMonochromeNoise:
			PlayMonochromeNoise().Forget();
			ReadNext();
			break;
		case CommandType.DirectionChromaticAberration:
			PlayChromaticAberration().Forget();
			ReadNext();
			break;
		case CommandType.DirectionAdjustAllUnstable:
			PlayAdjustAllUnstable().Forget();
			ReadNext();
			break;
		case CommandType.CameraStop_1:
			PlayCameraStop_().Forget();
			ReadNext();
			break;
		case CommandType.ScreenSharing_1_Play:
			PlayScreenSharing_().Forget();
			ReadNext();
			break;
		case CommandType.ScreenSharing_1_End:
			EndScreenSharing_().Forget();
			ReadNext();
			break;
		case CommandType.VoiceDefectRatio:
			ReadNext();
			break;
		case CommandType.DirectionGameStartCamera:
			_directionService.GameStartCameraDirection.Play();
			ReadNext();
			break;
		case CommandType.SunglassesShineForSatone:
		{
			float waitStartSeconds2 = float.Parse(_currentNovel.Arg1);
			float startDuration2 = _currentNovel.Arg2;
			float waitDuration2 = float.Parse(_currentNovel.Arg3);
			float endDuration2 = float.Parse(_currentNovel.Arg4);
			ShineSunglassesForSatone().Forget();
			ReadNext();
			break;
		}
		case CommandType.SunglassesShineForKouchan:
		{
			float waitStartSeconds = float.Parse(_currentNovel.Arg1);
			float startDuration = _currentNovel.Arg2;
			float waitDuration = float.Parse(_currentNovel.Arg3);
			float endDuration = float.Parse(_currentNovel.Arg4);
			ShineSunglassesForKouchan().Forget();
			ReadNext();
			break;
		}
		}
		async UniTask ChangeMotion()
		{
			NovelData novelData2 = _currentNovel;
			float num = float.Parse(_currentNovel.Arg1);
			float endDelaySeconds = _currentNovel.Arg2;
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(num), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				CommandChangeMotion(novelData2);
				if (endDelaySeconds == -1f)
				{
					int bodyMotion = novelData2.BodyMotion;
					HeroineService.AnimationType animationType = (HeroineService.AnimationType)bodyMotion;
					isCanceled = await WaitWithTimeout(() => _heroineService.IsEndAnimation(animationType.ToString()), 30f, _endStoryCts.Token);
				}
				else
				{
					isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(endDelaySeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
					_ = isCanceled;
				}
			}
		}
		async UniTask EndScreenSharing_()
		{
			float playDelaySecondsForStory = _directionService.PlayDelaySecondsForStory;
			_directionService.OnUsePlayDelaySeconds();
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(playDelaySecondsForStory), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				_directionService.ScreenSharing.End();
			}
		}
		async UniTask PlayAdjustAllUnstable()
		{
			float ratio = float.Parse(_currentNovel.Arg1);
			float toSecond2 = _currentNovel.Arg2;
			Ease easeType = ParseEaseType(_currentNovel.Arg3);
			float playDelaySecondsForStory = _directionService.PlayDelaySecondsForStory;
			_directionService.OnUsePlayDelaySeconds();
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(playDelaySecondsForStory), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				_directionService.AdjustAllUnstable.Play(ratio, toSecond2, easeType);
			}
		}
		async UniTask PlayBlackOut()
		{
			float ratio = float.Parse(_currentNovel.Arg1);
			float stopDelaySeconds = _currentNovel.Arg2;
			int isUseBlind = 0;
			if (!_currentNovel.Arg3.IsNullOrEmpty())
			{
				isUseBlind = int.Parse(_currentNovel.Arg3);
			}
			float playDelaySecondsForStory = _directionService.PlayDelaySecondsForStory;
			_directionService.OnUsePlayDelaySeconds();
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(playDelaySecondsForStory), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				_directionService.BlackOut.Play(ratio, isUseBlind);
				if (stopDelaySeconds != -1f)
				{
					isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(stopDelaySeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
					if (!isCanceled)
					{
						_directionService.BlackOut.Stop();
					}
				}
			}
		}
		async UniTask PlayCameraStop_()
		{
			float stopDelaySeconds = _currentNovel.Arg2;
			float playDelaySecondsForStory = _directionService.PlayDelaySecondsForStory;
			_directionService.OnUsePlayDelaySeconds();
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(playDelaySecondsForStory), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				_directionService.CameraStop.Play().Forget();
				if (stopDelaySeconds != -1f)
				{
					isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(stopDelaySeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
					if (!isCanceled)
					{
						_directionService.CameraStop.Stop();
					}
				}
			}
		}
		async UniTask PlayChromaticAberration()
		{
			float ratio = float.Parse(_currentNovel.Arg1);
			float toSecond2 = _currentNovel.Arg2;
			Ease easeType = ParseEaseType(_currentNovel.Arg3);
			float playDelaySecondsForStory = _directionService.PlayDelaySecondsForStory;
			_directionService.OnUsePlayDelaySeconds();
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(playDelaySecondsForStory), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				_directionService.ChromaticAberration.Play(ratio, toSecond2, easeType);
			}
		}
		async UniTask PlayGlitch()
		{
			float ratio = float.Parse(_currentNovel.Arg1);
			float stopDelaySeconds = _currentNovel.Arg2;
			float playDelaySecondsForStory = _directionService.PlayDelaySecondsForStory;
			_directionService.OnUsePlayDelaySeconds();
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(playDelaySecondsForStory), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				_directionService.Glitch.PlayGlitch1(ratio);
				if (stopDelaySeconds != -1f)
				{
					isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(stopDelaySeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
					if (!isCanceled)
					{
						_directionService.Glitch.StopGlitch1();
					}
				}
			}
		}
		async UniTask PlayMonochromeNoise()
		{
			float ratio = float.Parse(_currentNovel.Arg1);
			float toSecond2 = _currentNovel.Arg2;
			Ease easeType = ParseEaseType(_currentNovel.Arg3);
			float playDelaySecondsForStory = _directionService.PlayDelaySecondsForStory;
			_directionService.OnUsePlayDelaySeconds();
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(playDelaySecondsForStory), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				_directionService.MonochromeNoise.Play(ratio, toSecond2, easeType);
			}
		}
		async UniTask PlayScreenMove()
		{
			float ratio = float.Parse(_currentNovel.Arg1);
			float stopDelaySeconds = _currentNovel.Arg2;
			float playDelaySecondsForStory = _directionService.PlayDelaySecondsForStory;
			_directionService.OnUsePlayDelaySeconds();
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(playDelaySecondsForStory), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				_directionService.ScreenMove.Play(ratio);
				if (stopDelaySeconds != -1f)
				{
					isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(stopDelaySeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
					if (!isCanceled)
					{
						_directionService.ScreenMove.Stop();
					}
				}
			}
		}
		async UniTask PlayScreenSharing_()
		{
			float playDelaySecondsForStory = _directionService.PlayDelaySecondsForStory;
			_directionService.OnUsePlayDelaySeconds();
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(playDelaySecondsForStory), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				_directionService.ScreenSharing.Play();
			}
		}
		async UniTask PlaySE()
		{
			NovelData novelData2 = _currentNovel;
			bool isLoop = _currentNovel.Arg2 == 1f;
			float num = float.Parse(_currentNovel.Arg3);
			float stopDelaySeconds = float.Parse(_currentNovel.Arg4);
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(num), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				CommandPlaySE(novelData2, isLoop);
				if (stopDelaySeconds != -1f)
				{
					isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(stopDelaySeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
					if (!isCanceled)
					{
						CommandStopSE(novelData2);
					}
				}
			}
		}
		async UniTaskVoid ShowSelection()
		{
			isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(float.Parse(_currentNovel.Arg4)), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow();
			if (!isCanceled)
			{
				CommandTextLoopSelection(_currentNovel);
			}
		}
	}

	private async UniTask CommandText(NovelData next)
	{
		_isFinishedDisplayingText = false;
		LocalizationData localize = _currentGroupLocalizations.FirstOrDefault((LocalizationData x) => x.ID == next.LocalizationID);
		_heroineService.ChangeHeroineAnimationForInteger(next.BodyMotion);
		int facialType = 0;
		if (next.FacialMotion == 0)
		{
			if (_heroineService.IsCloseEye())
			{
				facialType = 18;
			}
		}
		else
		{
			facialType = next.FacialMotion;
		}
		_heroineService.ChangeHeroineFacialAnimation(facialType);
		_heroineService.ChangeLookScaleAnimation(next.LookScale, next.LookSpeedSeconds, next.LookEaseType);
		if (_currentNovel.Arg1 != string.Empty && await UniTask.Delay(TimeSpan.FromSeconds(float.Parse(_currentNovel.Arg1)), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow())
		{
			return;
		}
		if (next.LocalizationID == "0" || next.LocalizationID == string.Empty)
		{
			Debug.LogError(next.ID + "のLocalizationIDが" + next.LocalizationID + "です。（意図通りであれば問題なし）");
			_isFinishedDisplayingText = true;
		}
		else
		{
			string text = localizationMaster.Get(localize);
			_myStorySystemUI.CurrentTextMessage().StartText(text);
			if (_myStorySystemUI.CurrentMessageType == StorySystemUI.MessageType.Normal && !_myStorySystemUI.IsActiveNormalText())
			{
				_myStorySystemUI.ActivateNormalText();
			}
			bool isMoveMouse = int.Parse(next.Arg3) == 0;
			_heroineService.PlayVoice(localize.Voice, isMoveMouse, isStory: true).Forget();
		}
		if (!(await UniTask.WaitUntil(() => _isFinishedDisplayingText && _heroineService.IsFinishedVoice, PlayerLoopTiming.Update, _endStoryCts.Token).SuppressCancellationThrow()))
		{
			float arg = _currentNovel.Arg2;
			await AutoNext(arg);
		}
	}

	private async UniTask CommandVoiceText(NovelData next)
	{
		LocalizationData localizationData = _currentGroupLocalizations.FirstOrDefault((LocalizationData x) => x.ID == next.LocalizationID);
		string text = localizationMaster.Get(localizationData);
		if (text.IsNullOrEmpty())
		{
			_isFinishedDisplayingText = true;
		}
		else
		{
			_isFinishedDisplayingText = false;
			_myStorySystemUI.CurrentTextMessage().StartText(text);
			if (_myStorySystemUI.CurrentMessageType == StorySystemUI.MessageType.Normal && !_myStorySystemUI.IsActiveNormalText())
			{
				_myStorySystemUI.ActivateNormalText();
			}
		}
		bool isMoveMouse = int.Parse(next.Arg1) == 1;
		_heroineService.PlayVoice(localizationData.Voice, isMoveMouse, isStory: true).Forget();
		if (!(await WaitWithTimeout(() => _isFinishedDisplayingText && _heroineService.IsFinishedVoice, 30f, _endStoryCts.Token)))
		{
			_ = _currentNovel.Arg2;
			await AutoNext();
		}
	}

	private async UniTask CommandPomodoroVoiceText(NovelData next)
	{
		int facialType = 0;
		if (next.FacialMotion == 0)
		{
			if (_heroineService.IsCloseEye())
			{
				facialType = 18;
			}
		}
		else
		{
			facialType = next.FacialMotion;
		}
		_heroineService.ChangeHeroineFacialAnimation(facialType);
		_heroineService.ChangeLookScaleAnimation(next.LookScale, next.LookSpeedSeconds, next.LookEaseType);
		LocalizationData localizationData = _currentGroupLocalizations.FirstOrDefault((LocalizationData x) => x.ID == next.LocalizationID);
		string text = localizationMaster.Get(localizationData);
		if (text.IsNullOrEmpty())
		{
			_isFinishedDisplayingText = true;
		}
		else
		{
			_isFinishedDisplayingText = false;
			_myStorySystemUI.CurrentTextMessage().StartText(text);
			if (_myStorySystemUI.CurrentMessageType == StorySystemUI.MessageType.Normal && !_myStorySystemUI.IsActiveNormalText())
			{
				_myStorySystemUI.ActivateNormalText();
			}
		}
		bool isMoveMouse = int.Parse(next.Arg3) != -1;
		_heroineService.PlayVoice(localizationData.Voice, isMoveMouse, isStory: true).Forget();
		if (await WaitWithTimeout(() => _isFinishedDisplayingText && _heroineService.IsFinishedVoice, 30f, _endStoryCts.Token))
		{
			return;
		}
		if (next.BodyMotion != -1)
		{
			await WaitWithTimeout(() => _heroineService.IsEndAnimation(), 30f, _endStoryCts.Token);
		}
		_ = _currentNovel.Arg2;
		await AutoNext();
	}

	private async UniTask CommandTextLoopPlayText(NovelData next)
	{
		_isFinishedDisplayingText = false;
		LocalizationData localize = _currentGroupLocalizations.FirstOrDefault((LocalizationData x) => x.ID == next.LocalizationID);
		_heroineService.ChangeHeroineAnimationForInteger(next.BodyMotion);
		_heroineService.ChangeHeroineFacialAnimation(next.FacialMotion);
		_heroineService.ChangeLookScaleAnimation(next.LookScale, next.LookSpeedSeconds, next.LookEaseType);
		if (_currentNovel.Arg1 != string.Empty)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(float.Parse(_currentNovel.Arg1)), ignoreTimeScale: false, PlayerLoopTiming.Update, _endStoryCts.Token);
		}
		if (next.LocalizationID == "0" || next.LocalizationID == string.Empty)
		{
			Debug.LogError(next.ID + "のLocalizationIDが" + next.LocalizationID + "です。（意図通りであれば問題なし）");
			_isFinishedDisplayingText = true;
		}
		else
		{
			string text = localizationMaster.Get(localize);
			_myStorySystemUI.CurrentTextMessage().StartText(text);
			if (_myStorySystemUI.CurrentMessageType == StorySystemUI.MessageType.Normal && !_myStorySystemUI.IsActiveNormalText())
			{
				_myStorySystemUI.ActivateNormalText();
			}
			_heroineService.PlayVoice(localize.Voice, isMoveMouse: true, isStory: true).Forget();
		}
		await UniTask.WaitUntil(() => _isFinishedDisplayingText && _heroineService.IsFinishedVoice, PlayerLoopTiming.Update, _endStoryCts.Token);
	}

	private void CommandTextLoopSelection(NovelData textLoopStartNovelData)
	{
		string selectionId = textLoopStartNovelData.Arg3;
		NovelData item = _currentGroupNovels.First((NovelData x) => x.ID == selectionId);
		int startIndex = _currentGroupNovels.IndexOf(item);
		List<SentenceSelectionButtonsUI.ButtonEventInfo> buttonEventInfo = GetButtonInfo(startIndex);
		_myStorySystemUI.SelectionButtonsUI.EnableButtons(buttonEventInfo);
		List<SentenceSelectionButtonsUI.ButtonEventInfo> GetButtonInfo(int num)
		{
			List<SentenceSelectionButtonsUI.ButtonEventInfo> list = new List<SentenceSelectionButtonsUI.ButtonEventInfo>();
			for (int i = num; i < _currentGroupNovels.Count; i++)
			{
				NovelData sentence = _currentGroupNovels[i];
				if (sentence.Command != CommandType.Selection)
				{
					break;
				}
				LocalizationData data = _currentGroupLocalizations.FirstOrDefault((LocalizationData x) => x.ID == sentence.LocalizationID);
				list.Add(new SentenceSelectionButtonsUI.ButtonEventInfo
				{
					SelectionText = localizationMaster.Get(data),
					Callback = delegate
					{
						_textLoop.OnSelectedSelection(sentence);
					}
				});
			}
			return list;
		}
	}

	private void CommandChangeMotion(NovelData next)
	{
		_heroineService.ChangeHeroineAnimationForInteger(next.BodyMotion);
		_heroineService.ChangeHeroineFacialAnimation(next.FacialMotion);
		_heroineService.ChangeLookScaleAnimation(next.LookScale, next.LookSpeedSeconds, next.LookEaseType);
	}

	private void CommandPomodoroVoiceChangeMotion(NovelData next)
	{
		_heroineService.ChangeHeroineAnimationForInteger(next.BodyMotion);
	}

	private async UniTask CommandWaitForSeconds(NovelData next)
	{
		if (!float.TryParse(next.Arg1, out var result))
		{
			Debug.LogWarning($"failed to parse. ID: {next.ID}, Command:{next.Command},Arg1:{next.Arg1}");
		}
		await UniTask.Delay(TimeSpan.FromSeconds(result), ignoreTimeScale: false, PlayerLoopTiming.Update, _myStorySystemUI.CancellationTokenOnDestroy);
	}

	private void CommandWaitInput()
	{
	}

	private void CommandSelection()
	{
		int startIndex = GetStartIndex();
		List<SentenceSelectionButtonsUI.ButtonEventInfo> buttonEventInfo = GetButtonInfo(startIndex);
		_myStorySystemUI.SelectionButtonsUI.EnableButtons(buttonEventInfo);
		List<SentenceSelectionButtonsUI.ButtonEventInfo> GetButtonInfo(int num)
		{
			List<SentenceSelectionButtonsUI.ButtonEventInfo> list = new List<SentenceSelectionButtonsUI.ButtonEventInfo>();
			for (int i = num; i < _currentGroupNovels.Count; i++)
			{
				NovelData sentence = _currentGroupNovels[i];
				if (sentence.Command != CommandType.Selection)
				{
					break;
				}
				LocalizationData data = _currentGroupLocalizations.FirstOrDefault((LocalizationData x) => x.ID == sentence.LocalizationID);
				list.Add(new SentenceSelectionButtonsUI.ButtonEventInfo
				{
					SelectionText = localizationMaster.Get(data),
					Callback = delegate
					{
						SelectionJump(sentence);
					}
				});
			}
			return list;
		}
		int GetStartIndex()
		{
			int result = _currentMessageIndex;
			int num = _currentMessageIndex;
			while (num >= 0 && _currentGroupNovels[num].Command == CommandType.Selection)
			{
				result = num;
				num--;
			}
			return result;
		}
	}

	private void CommandTutorialSelection()
	{
		int startIndex = GetStartIndex();
		List<SentenceSelectionButtonsUI.ButtonEventInfo> buttonEventInfo = GetButtonInfo(startIndex);
		_myStorySystemUI.SelectionButtonsUI.EnableButtons(buttonEventInfo);
		_myStorySystemUI.DeactivateNormalText();
		List<SentenceSelectionButtonsUI.ButtonEventInfo> GetButtonInfo(int num)
		{
			List<SentenceSelectionButtonsUI.ButtonEventInfo> list = new List<SentenceSelectionButtonsUI.ButtonEventInfo>();
			for (int i = num; i < _currentGroupNovels.Count; i++)
			{
				NovelData sentence = _currentGroupNovels[i];
				if (sentence.Command != CommandType.TutorialSelection)
				{
					break;
				}
				LocalizationData data = _currentGroupLocalizations.FirstOrDefault((LocalizationData x) => x.ID == sentence.LocalizationID);
				bool isUsed = (_selectionData.IsUsedID(sentence.Arg2) ? true : false);
				list.Add(new SentenceSelectionButtonsUI.ButtonEventInfo
				{
					SelectionText = localizationMaster.Get(data),
					Callback = delegate
					{
						_selectionData.UseSelection(sentence.Arg2);
						SelectionJump(sentence);
					},
					IsUsed = isUsed
				});
			}
			return list;
		}
		int GetStartIndex()
		{
			int result = _currentMessageIndex;
			int num = _currentMessageIndex;
			while (num >= 0 && _currentGroupNovels[num].Command == CommandType.TutorialSelection)
			{
				result = num;
				num--;
			}
			return result;
		}
	}

	private void SelectionJump(NovelData next)
	{
		_myStorySystemUI.DeactivateNormalText();
		if (string.IsNullOrEmpty(next.Arg1))
		{
			Debug.LogError($"Arg1 is empty. ID: {next.ID}, Command:{next.Command}");
			return;
		}
		string jumpId = next.Arg1;
		NovelData novelData = _currentGroupNovels.FirstOrDefault((NovelData x) => x.ID == jumpId);
		if (novelData == null)
		{
			Debug.LogError($"jumpId {jumpId} is not found. ID: {next.ID}, Command:{next.Command}");
			return;
		}
		int nextIndex = _currentGroupNovels.IndexOf(novelData);
		ReadSentence(nextIndex).Forget();
	}

	private void CommandPlayBGM(NovelData next)
	{
		if (string.IsNullOrEmpty(next.Arg1))
		{
			Debug.LogError($"Arg1 is empty. ID: {next.ID}, Command:{next.Command}");
			return;
		}
		string bgmName = next.Arg1;
		MusicData nextMusic = _masterDataLoader.MusicDataList.FirstOrDefault((MusicData x) => x.Title == bgmName);
		if (nextMusic == null)
		{
			nextMusic = _masterDataLoader.DirectionMusicDataList.FirstOrDefault((DirectionMusicData x) => x.Title == bgmName).ConvertMusicData();
		}
		if (nextMusic == null)
		{
			Debug.LogError("ScenarioReader: BGM⇒" + bgmName + "が見つかりませんでした。名前が合っているか、BGMが登録されているか確認してください。");
			return;
		}
		float volumeRatio = next.Arg2;
		if (volumeRatio == 0f)
		{
			volumeRatio = 1f;
		}
		else
		{
			volumeRatio = Mathf.Clamp(volumeRatio, 0f, 1f);
		}
		float startSeconds = float.Parse(next.Arg3);
		float fadeSeconds = float.Parse(next.Arg4);
		if (fadeSeconds == 0f)
		{
			fadeSeconds = 1.5f;
		}
		if (_playingMusicData != null)
		{
			Action callback = delegate
			{
				SingletonMonoBehaviour<ScenarioMusicManager>.Instance.Stop(_playingMusicData.AudioClipName);
				PlayMusicFadeIn(nextMusic, volumeRatio, startSeconds, fadeSeconds);
			};
			SingletonMonoBehaviour<ScenarioMusicManager>.Instance.FadeFromCurrentVolume(_playingMusicData.AudioClipName, 1.5f, 0f, callback);
		}
		else
		{
			PlayMusicFadeIn(nextMusic, volumeRatio, startSeconds, fadeSeconds);
		}
	}

	private void CommandPlayMusicServiceBGM(NovelData next)
	{
		if (string.IsNullOrEmpty(next.Arg1))
		{
			Debug.LogError($"Arg1 is empty. ID: {next.ID}, Command:{next.Command}");
			return;
		}
		string bgmName = next.Arg1;
		if (_musicService != null)
		{
			GameAudioInfo gameAudioInfo = _musicService.AllMusicList.FirstOrDefault((GameAudioInfo x) => x.Title == bgmName);
			if (gameAudioInfo == null)
			{
				Debug.LogError("ScenarioReader: MusicServiceでBGM⇒" + bgmName + "が見つかりませんでした。名前が合っているか、BGMが登録されているか確認してください。");
			}
			else
			{
				PlayMusicFromMusicService(gameAudioInfo).Forget();
			}
		}
	}

	private async UniTaskVoid PlayMusicFromMusicService(GameAudioInfo gameAudioInfo)
	{
		if (_endStoryCts != null && !_endStoryCts.IsCancellationRequested)
		{
			if (await gameAudioInfo.GetAudioClip(_endStoryCts.Token) == null)
			{
				Debug.LogError("ScenarioReader: AudioClipのロードに失敗しました。Title: " + gameAudioInfo.Title);
			}
			else
			{
				_musicService.PlayArugumentMusic(gameAudioInfo, MusicChangeKind.Manual);
			}
		}
	}

	private void PlayMusicFadeIn(MusicData playMusicData, float volumeRatio = 1f, float startSeconds = 0f, float fadeSeconds = 1.5f)
	{
		bool isLoop = _playingScenarioGroupID != "main_32";
		AudioPlayer audioPlayer = SingletonMonoBehaviour<ScenarioMusicManager>.Instance.Play(playMusicData.AudioClip, 0f, 0f, 1f, isLoop);
		if (audioPlayer != null)
		{
			SingletonMonoBehaviour<ScenarioMusicManager>.Instance.FadeFromCurrentVolume(playMusicData.AudioClipName, fadeSeconds, volumeRatio);
			audioPlayer.AudioSource.time = startSeconds;
			_playingMusicData = playMusicData;
		}
	}

	private void CommandPlaySE(NovelData next, bool isLoop)
	{
		if (string.IsNullOrEmpty(next.Arg1))
		{
			Debug.LogError($"Arg1 is empty. ID: {next.ID}, Command:{next.Command}");
			return;
		}
		string arg = next.Arg1;
		SingletonMonoBehaviour<SEManager>.Instance.Play(arg, 1f, 0f, 1f, isLoop);
	}

	private void CommandPlayAmbientBGM(NovelData next)
	{
		if (string.IsNullOrEmpty(next.Arg1))
		{
			Debug.LogError($"Arg1 is empty. ID: {next.ID}, Command:{next.Command}");
			return;
		}
		string ambientBgmName = next.Arg1;
		AmbientSoundMasterData ambientSoundMasterData = _masterDataLoader.AmbientMasterList.FirstOrDefault((AmbientSoundMasterData x) => x.AmbientSound == (AmbientSoundType)Enum.Parse(typeof(AmbientSoundType), ambientBgmName));
		if (ambientSoundMasterData != null)
		{
			float arg = next.Arg2;
			arg = ((arg != 0f) ? Mathf.Clamp(arg, 0f, 1f) : 0.5f);
			PlayAmbientBGMFadeIn(ambientSoundMasterData, arg);
		}
	}

	private void PlayAmbientBGMFadeIn(AmbientSoundMasterData playMusicData, float volumeRatio = 0.5f)
	{
		try
		{
			SingletonMonoBehaviour<ScenarioAmbientBGMManager>.Instance.Play(playMusicData.AudioClip, 0f, 0f, 1f, isLoop: true, allowsDuplicate: true);
			SingletonMonoBehaviour<ScenarioAmbientBGMManager>.Instance.Fade(playMusicData.AudioClipName, 1.5f, 0f, volumeRatio);
		}
		catch
		{
			Debug.LogError("ScenarioReader: 意図通りに環境音が再生できなかった。");
		}
	}

	private void CommandStopBGM(NovelData next)
	{
		if (string.IsNullOrEmpty(next.Arg1))
		{
			foreach (AudioPlayer audioPlayer in SingletonMonoBehaviour<ScenarioMusicManager>.Instance.AudioPlayerList)
			{
				audioPlayer.Stop();
			}
			_playingMusicData = null;
			return;
		}
		string arg = next.Arg1;
		if (!string.IsNullOrEmpty(arg))
		{
			SingletonMonoBehaviour<ScenarioMusicManager>.Instance.GetAudioPlayerByName(arg)?.Stop();
			if (arg == _playingMusicData?.AudioClipName)
			{
				_playingMusicData = null;
			}
		}
	}

	private void CommandStopSE(NovelData next)
	{
		if (string.IsNullOrEmpty(next.Arg1))
		{
			foreach (AudioPlayer audioPlayer in SingletonMonoBehaviour<SEManager>.Instance.AudioPlayerList)
			{
				audioPlayer.Stop();
			}
			SingletonMonoBehaviour<SEManager>.Instance.Stop();
		}
		else
		{
			string arg = next.Arg1;
			SingletonMonoBehaviour<SEManager>.Instance.GetAudioPlayerByName(arg)?.Stop();
		}
	}

	private void CommandStopAmbientBGM(NovelData next)
	{
		if (string.IsNullOrEmpty(next.Arg1) || next.Arg1 == "0")
		{
			foreach (AudioPlayer audioPlayer in SingletonMonoBehaviour<ScenarioAmbientBGMManager>.Instance.AudioPlayerList)
			{
				StopAmbientBGMFadeOut(audioPlayer.AudioSource);
			}
			return;
		}
		foreach (AudioPlayer audioPlayer2 in SingletonMonoBehaviour<ScenarioAmbientBGMManager>.Instance.AudioPlayerList)
		{
			if (audioPlayer2.CurrentAudioName == next.Arg1)
			{
				StopAmbientBGMFadeOut(audioPlayer2.AudioSource);
				break;
			}
		}
	}

	private void StopAmbientBGMFadeOut(AudioSource audioSource)
	{
		Action callback = delegate
		{
			SingletonMonoBehaviour<ScenarioAmbientBGMManager>.Instance.Stop(audioSource.clip);
		};
		try
		{
			SingletonMonoBehaviour<ScenarioAmbientBGMManager>.Instance.FadeFromCurrentVolume(audioSource.clip.name, 1.5f, 0f, callback);
		}
		catch
		{
		}
	}

	private void CommandChangeWindowView(NovelData next)
	{
		if (!string.IsNullOrEmpty(next.Arg1) && Enum.TryParse<WindowViewType>(next.Arg1, out var result))
		{
			if (next.Arg2 == 1f)
			{
				_windowViewService.ActivateWindow(result);
			}
			else if (next.Arg2 == 0f)
			{
				_windowViewService.DeactivateWindow(result);
			}
			else
			{
				Debug.LogError($"ChangeWindowView: NovelMasterのArg2に想定していない値{next.Arg2}が設定されています。\n表示する場合は1、非表示にする場合は0をNovelMasterに設定してください。");
			}
		}
	}

	private void CommandChangeLayout(DecorationService.DecorationSkinType type, bool isSave = false)
	{
		_decorationService.ChangeDecoration(type, isSave);
	}

	private void ChangeVolumeBGMCommand(NovelData next)
	{
		if (string.IsNullOrEmpty(next.Arg1))
		{
			Debug.LogError($"Arg1 is empty. ID: {next.ID}, Command:{next.Command}");
			return;
		}
		string bgmName = next.Arg1;
		MusicData musicData = _masterDataLoader.MusicDataList.FirstOrDefault((MusicData x) => x.Title == bgmName);
		if (musicData == null)
		{
			musicData = _masterDataLoader.DirectionMusicDataList.FirstOrDefault((DirectionMusicData x) => x.Title == bgmName).ConvertMusicData();
		}
		if (musicData == null)
		{
			Debug.LogError("ScenarioReader: BGM⇒" + bgmName + "が見つかりませんでした。名前が合っているか、BGMが登録されているか確認してください。");
			return;
		}
		float arg = next.Arg2;
		arg = Mathf.Clamp(arg, 0f, 1f);
		float duration = float.Parse(next.Arg3);
		SingletonMonoBehaviour<ScenarioMusicManager>.Instance.FadeFromCurrentVolume(musicData.AudioClipName, duration, arg);
	}

	private void CommandEndScenario()
	{
		EndStory();
	}

	private async UniTask<bool> WaitWithTimeout(Func<bool> condition, float timeoutSeconds, CancellationToken token)
	{
		UniTask uniTask = UniTask.WaitUntil(condition, PlayerLoopTiming.Update, token);
		UniTask uniTask2 = UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds));
		return (await UniTask.WhenAny(uniTask, uniTask2).SuppressCancellationThrow()).Item1;
	}

	public async UniTask FadeOutScenario()
	{
		bool isEndFadeOut = false;
		if (!SingletonMonoBehaviour<ScenarioMusicManager>.Instance.IsPlaying() || _playingMusicData == null)
		{
			isEndFadeOut = true;
		}
		else
		{
			Action callback = delegate
			{
				SingletonMonoBehaviour<ScenarioMusicManager>.Instance.Pause();
				isEndFadeOut = true;
			};
			SingletonMonoBehaviour<ScenarioMusicManager>.Instance.FadeFromCurrentVolume(_playingMusicData.AudioClip.name, 1.5f, 0f, callback);
		}
		await UniTask.WaitUntil(() => isEndFadeOut);
	}

	public bool IsPlayingScenario()
	{
		return _isPlayingScenario;
	}

	public void DebugDeactivateParentUI()
	{
		_myStorySystemUI.DebugDeactivateParentUI();
	}

	public void DebugActivateParentUI()
	{
		_myStorySystemUI.DebugActivateParentUI();
	}

	public void DebugPlayScenario(string labelId = null, ScenarioReadMode readMode = ScenarioReadMode.Auto)
	{
		_autoTextCts?.Cancel();
		_autoTextCts?.Dispose();
		_autoTextCts = null;
		_voiceTextDisposable?.Dispose();
		_voiceTextDisposable = null;
		_startPlayAmbientSoundDataList.Clear();
		_endCallbackList.Clear();
		Release();
		_myStorySystemUI.DebugClearAll();
		_endStoryCts?.Cancel();
		_endStoryCts?.Dispose();
		_endStoryCts = new CancellationTokenSource();
		_directionService.EndTidying();
		_playingScenarioType = ScenarioType.MainScenario;
		_onStartReady.OnNext(_playingScenarioType);
		_playingMusicData = null;
		_isPlayingScenario = true;
		_readMode = readMode;
		_currentMessageIndex = 0;
		_startStoryIndex = 0;
		_currentGroupNovels.Clear();
		NovelData targetNovelData = _masterDataLoader.NovelMasterList.FirstOrDefault((NovelData n) => n.ID == labelId);
		_ = targetNovelData;
		_currentGroupNovels.AddRange(_masterDataLoader.NovelMasterList.Where((NovelData n) => n.ScenarioGroupID == targetNovelData.ScenarioGroupID));
		_playingScenarioGroupID = _currentGroupNovels[0].ScenarioGroupID;
		string[] localizationIds = _currentGroupNovels.Select((NovelData x) => x.LocalizationID).ToArray();
		_currentGroupLocalizations.Clear();
		_currentGroupLocalizations.AddRange(_masterDataLoader.LocalizationList.Values.Where((LocalizationData n) => Enumerable.Contains(localizationIds, n.ID)));
		_isFinishedDisplayingText = false;
		_heroineService.ScenarioStartReady();
		if (IsPlayingTutorial())
		{
			_tutorialService.ReadyTutorial();
		}
		if (IsPlayingLongStory())
		{
			_endCallbackList.Clear();
			_myStorySystemUI.MainStoryReady(_currentGroupNovels[0].ScenarioGroupID, ScenarioType.MainScenario);
			int num = 0;
			int num2 = 0;
			List<NovelData> list = new List<NovelData>();
			while (_currentGroupNovels[_currentMessageIndex].ID != labelId)
			{
				switch (_currentGroupNovels[_currentMessageIndex].Command)
				{
				case CommandType.PlayBGM:
				case CommandType.TalkLogOnlyPlayBGM:
					_firstMusicNovel = _currentGroupNovels[_currentMessageIndex];
					break;
				case CommandType.PlayAmbientBGM:
				{
					string ambientBgmName = _currentGroupNovels[_currentMessageIndex].Arg1;
					new AmbientSoundMasterData();
					_masterDataLoader.AmbientMasterList.FirstOrDefault((AmbientSoundMasterData x) => x.AmbientSound == (AmbientSoundType)Enum.Parse(typeof(AmbientSoundType), ambientBgmName));
					if (!_startPlayAmbientSoundDataList.Contains(_currentGroupNovels[_currentMessageIndex]))
					{
						_startPlayAmbientSoundDataList.Add(_currentGroupNovels[_currentMessageIndex]);
					}
					break;
				}
				case CommandType.ChangeWindowView:
					list.Add(_currentGroupNovels[_currentMessageIndex]);
					break;
				case CommandType.ChangeLayout:
				{
					string arg = _currentGroupNovels[_currentMessageIndex].Arg1;
					DeskType deskType;
					if (_decorationService.ParseDecorationTypeForString(arg, out var decorationType))
					{
						bool isSave = false;
						if ((uint)(decorationType - 7) <= 4u && _unlockItemService.Decoration.GetLockState(DecorationService.DecorationSkinType.Book_5).IsLocked.CurrentValue)
						{
							ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.ID == _playingScenarioGroupID);
							if (scenarioGroupData != null && scenarioGroupData.Scenario == ScenarioType.MainScenario && SaveDataManager.Instance.ScenarioProgressData.IsLatestMainEpisode(scenarioGroupData.EpisodeNumber))
							{
								isSave = true;
							}
						}
						CommandChangeLayout(decorationType, isSave);
					}
					else if (_heroineService.HeroineUseObjectController.ParseDeskTypeForString(arg, out deskType))
					{
						_heroineService.HeroineUseObjectController.ImmediateChangeDeskType(deskType);
					}
					break;
				}
				}
				if (_currentGroupNovels[_currentMessageIndex].Command == CommandType.Text || _currentGroupNovels[_currentMessageIndex].Command == CommandType.ChangeMotion)
				{
					num = 0;
					num2 = _currentMessageIndex;
				}
				else if (_currentGroupNovels[_currentMessageIndex].Command >= CommandType.SetDirectionDelaySeconds && num == 0)
				{
					num = _currentMessageIndex;
				}
				_currentMessageIndex++;
			}
			if (_currentGroupNovels[_currentMessageIndex].Command == CommandType.Text || _currentGroupNovels[_currentMessageIndex].Command == CommandType.ChangeMotion)
			{
				num2 = _currentMessageIndex;
			}
			if (_currentGroupNovels[num2].Command != CommandType.Text || _currentGroupNovels[_currentMessageIndex].Command == CommandType.ChangeMotion)
			{
				for (int num3 = num2; num3 < _currentGroupNovels.Count; num3++)
				{
					if (_currentGroupNovels[num3].Command == CommandType.Text || _currentGroupNovels[_currentMessageIndex].Command == CommandType.ChangeMotion)
					{
						num2 = num3;
						break;
					}
				}
			}
			int num4 = -1;
			float num5 = -1f;
			for (int num6 = num2; num6 >= 0; num6--)
			{
				if (_currentGroupNovels[num6].Command == CommandType.Text || _currentGroupNovels[num6].Command == CommandType.ChangeMotion)
				{
					if (num4 == -1 && _currentGroupNovels[num6].BodyMotion != -1)
					{
						num4 = _currentGroupNovels[num6].BodyMotion;
					}
					if (num4 == -1)
					{
						num5 = _currentGroupNovels[num6].LookScale;
					}
				}
			}
			int bodyMotionType = ((num4 == -1) ? _currentGroupNovels[num2].BodyMotion : num4);
			float lookScale = ((num5 == -1f) ? _currentGroupNovels[num2].LookScale : num5);
			bool flag = false;
			flag = num4 != -1;
			_heroineService.DebugOnStoryStartReady(bodyMotionType, lookScale, _currentGroupNovels[num2].FacialMotion, flag);
			if (num == 0)
			{
				_startStoryIndex = _currentMessageIndex;
			}
			else
			{
				_startStoryIndex = num;
			}
			float num7 = -1f;
			float num8 = -1f;
			float num9 = -1f;
			int isUseBlind = 0;
			float num10 = -1f;
			float num11 = -1f;
			float num12 = -1f;
			int num13 = _startStoryIndex - 1;
			if (num13 >= 0)
			{
				for (int num14 = num13; num14 >= 0; num14--)
				{
					switch (_currentGroupNovels[num14].Command)
					{
					case CommandType.DirectionGlitch1:
						if (num7 == -1f)
						{
							num7 = ((_currentGroupNovels[num14].Arg2 != -1f) ? 0f : float.Parse(_currentGroupNovels[num14].Arg1));
						}
						break;
					case CommandType.DirectionScreenMove:
						if (num8 == -1f)
						{
							num8 = ((_currentGroupNovels[num14].Arg2 != -1f) ? 0f : float.Parse(_currentGroupNovels[num14].Arg1));
						}
						break;
					case CommandType.DirectionBlackOut:
						if (num9 == -1f)
						{
							num9 = ((_currentGroupNovels[num14].Arg2 != -1f) ? 0f : float.Parse(_currentGroupNovels[num14].Arg1));
							isUseBlind = int.Parse(_currentGroupNovels[num14].Arg3);
						}
						break;
					case CommandType.DirectionMonochromeNoise:
						if (num10 == -1f)
						{
							num10 = float.Parse(_currentGroupNovels[num14].Arg1);
						}
						break;
					case CommandType.DirectionChromaticAberration:
						if (num11 == -1f)
						{
							num11 = float.Parse(_currentGroupNovels[num14].Arg1);
						}
						break;
					case CommandType.VoiceDefectRatio:
						if (num12 == -1f)
						{
							num12 = float.Parse(_currentGroupNovels[num14].Arg1);
						}
						break;
					}
				}
			}
			_directionService.Glitch.ImmediateChange(num7);
			_directionService.ScreenMove.ImmediateChange(num8);
			_directionService.BlackOut.ImmediateChange(num9, isUseBlind);
			_directionService.MonochromeNoise.ImmediateChange(num10);
			_directionService.ChromaticAberration.ImmediateChange(num11);
			if (list.Count > 0)
			{
				_windowViewService.ResetOtherThanTimeOfDay();
				foreach (NovelData item in list)
				{
					CommandChangeWindowView(item);
				}
			}
			HeroineService.AnimationType bodyMotion = (HeroineService.AnimationType)_currentGroupNovels[num2].BodyMotion;
			_heroineService.HeroineUseObjectController.ImmediateChangeDeskUseObjectAnimation(bodyMotion);
			_heroineService.HeroineUseObjectController.ImmediateCurrentChangeDesk();
			_myStorySystemUI.ActivateSkipButton();
		}
		else if (IsGameStartStory())
		{
			_heroineService.OnStoryStartReady(_currentGroupNovels[0].BodyMotion, _currentGroupNovels[0].FacialMotion);
		}
		StartMainStory();
	}
}
