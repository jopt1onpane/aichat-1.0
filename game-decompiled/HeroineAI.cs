using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bulbul;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using MyUtil;
using R3;
using UnityEngine;
using VContainer;

public class HeroineAI : MonoBehaviour
{
	public enum ActionStateType
	{
		None = -1,
		GameStartDirection = 0,
		WantTalk = 1,
		MainStory = 2,
		ClickHeroine = 3,
		WildStretchFullBody = 4,
		WildStretchShoulder = 5,
		WildTea = 6,
		WildGuts = 7,
		WildOpenWindow = 8,
		WildCloseWindow = 9,
		FromPcToBook = 10,
		FromPcToReport = 11,
		FromBookToPc = 12,
		FromBookToReport = 13,
		FromReportToPc = 14,
		FromReportToBook = 15,
		WorkPC = 16,
		WorkBook = 17,
		WorkReport = 18,
		BreakMovie = 19,
		BreakReadBook = 20,
		BreakListenMusic = 21,
		BreakTeaTime = 22,
		BreakSleep = 23,
		BreakForward = 24,
		LeaveChairGoSofa = 25,
		LeaveChairGoFar = 26,
		GameEndTalk = 27,
		EventNewYearCountdown = 10000
	}

	public enum ActionType
	{
		None,
		Work,
		Break
	}

	public enum UpdateStateType
	{
		Idle,
		WaitFinishAction,
		EndWaitFinishAction,
		Changing
	}

	private UpdateStateType _currentUpdateState;

	[Inject]
	[HideInInspector]
	private HeroineService _heroineService;

	[Inject]
	[HideInInspector]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private LoadDirectionService _loadDirectionService;

	[Inject]
	private FacilityClickHeroine _facilityClickHeroine;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private FacilityVoiceTextScenario _facilityVoiceTextScenario;

	[Inject]
	private DebugService _debugService;

	[Inject]
	private MotionSoundController _motionSoundController;

	[Inject]
	private ISpecialService _collaborationService;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private ScenarioGroupMasterWrapper _scenarioGroupMasterWrapper;

	[Inject]
	private LimitedTimeEventService _limitedTimeEventService;

	[SerializeField]
	[Header("AIを使用するか")]
	private bool _isUse = true;

	[SerializeField]
	[Header("次の行動を選択するスクリプト")]
	private NextActionSelector _nextActionSelector;

	[SerializeField]
	[Header("自分からプレイヤーに声を掛けるか管理するスクリプト")]
	private HeroineSelfTalkController _selfTalkController;

	[Header("ステート\nストーリー")]
	[SerializeField]
	[Header("開始演出用")]
	private HeroineStateGameStartDirection _stateGameStartDirection;

	[SerializeField]
	[Header("話したがっている")]
	private HeroineWantTalk _wantTalk;

	[SerializeField]
	[Header("ストーリー用")]
	private HeroineStateStory _stateStory;

	[Header("ヒロインクリック")]
	[SerializeField]
	[Header("ヒロインクリック用")]
	private HeroineStateClickHeroine _stateClickHeroine;

	[Header("中間モーション")]
	[SerializeField]
	[Header("中間\u3000全身ストレッチ")]
	private HeroineStateWildStretchFullBody _stateWildStretchFullBody;

	[SerializeField]
	[Header("中間\u3000肩ストレッチ")]
	private HeroineStateWildStretchShoulder _stateWildStretchShoulder;

	[SerializeField]
	[Header("中間\u3000お茶")]
	private HeroineStateWildTea _stateWildTea;

	[SerializeField]
	[Header("中間\u3000ガッツ")]
	private HeroineStateWildGuts _stateGuts;

	[SerializeField]
	[Header("中間\u3000窓を開ける")]
	private HeroineStateWildOpenWindow _stateOpenWindow;

	[SerializeField]
	[Header("中間\u3000窓を閉める")]
	private HeroineStateWildCloseWindow _stateCloseWindow;

	[SerializeField]
	[Header("オブジェクト入れ替え\nPCから本")]
	private HeroineStateFromPcToBook _stateWildFromPcToBook;

	[SerializeField]
	[Header("レポートから本")]
	private HeroineStateFromReportToBook _stateWildFromReportToBook;

	[SerializeField]
	[Header("本からPC")]
	private HeroineStateFromBookToPc _stateWildFromBookToPc;

	[SerializeField]
	[Header("レポートからPC")]
	private HeroineStateFromReportToPc _stateWildFromReportToPc;

	[SerializeField]
	[Header("本からレポート")]
	private HeroineStateFromBookToReport _stateWildFromBookToReport;

	[SerializeField]
	[Header("PCからレポート")]
	private HeroineStateFromPcToReport _stateWildFromPcToReport;

	[Header("作業")]
	[SerializeField]
	[Header("作業\u3000PC")]
	private HeroineStateWorkPC _stateWorkPC;

	[SerializeField]
	[Header("作業\u3000本＋PC")]
	private HeroineStateWorkBook _stateWorkBook;

	[SerializeField]
	[Header("作業\u3000レポート")]
	private HeroineStateWorkReport _stateWorkReport;

	[Header("休憩")]
	[SerializeField]
	[Header("休憩\u3000動画試聴")]
	private HeroineStateBreakMovie _stateBreakMovie;

	[SerializeField]
	[Header("休憩\u3000前傾姿勢で休憩")]
	private HeroineStateBreakForward _stateBreakForward;

	[SerializeField]
	[Header("休憩\u3000読書")]
	private HeroineStateBreakReadBook _stateBreakReadBook;

	[SerializeField]
	[Header("休憩\u3000音楽を聴く")]
	private HeroineStateBreakListenMusic _stateBreakListenMusic;

	[SerializeField]
	[Header("休憩\u3000ティータイム")]
	private HeroineStateBreakTeaTime _stateBreakTeaTime;

	[SerializeField]
	[Header("休憩\u3000睡眠")]
	private HeroineStateBreakSleep _stateBreakSleep;

	[Header("離席")]
	[SerializeField]
	[Header("離席\u3000ソファ")]
	private HeroineStateLeaveChairGoSofa _stateLeaveChairGoSofa;

	[SerializeField]
	[Header("離席\u3000デスクから大きく離れる")]
	private HeroineStateLeaveChairGoFar _stateLeaveChairGoFar;

	[Header("ゲーム終了")]
	[SerializeField]
	private HeroineGameEndTalk _gameEndTalk;

	[Header("イベント")]
	[SerializeField]
	private HeroineStateEventNewYearCountdown _stateEventNewYearCountdown;

	private HeroineCommonParameter _commonParameter = new HeroineCommonParameter();

	private StateMachineMonobehavior<HeroineAI> _actionStateMachine;

	private HeroineFacialController _heroineFacialController = new HeroineFacialController();

	private HeroineVoiceController _heroineVoiceController = new HeroineVoiceController();

	private PomodoroVoiceSelector _pomodoroVoiceSelector = new PomodoroVoiceSelector();

	private LeaveChairJudge _leaveChairJudge = new LeaveChairJudge();

	private ITalkSelector _normalWorkStartVoiceSelector = new LeaveStartVoiceSelector();

	private ActionStateType _currentActionState;

	private Action _changeNextAction;

	private CancellationTokenSource _cts;

	private bool _isSleeping;

	private bool _isWantJumpUp;

	private int _pomodoroVoicePlayType = -1;

	private bool _isCanPlayPomodoroVoice;

	private bool _isPlayingPomodoroAction;

	private bool _isPlayedEventNewYearCountdown;

	private ActionStateType _actionStateBeforeLeaveChair;

	private bool _isCurrentGameEndDirection;

	public UpdateStateType CurrentUpdateState => _currentUpdateState;

	public HeroineService HeroineService => _heroineService;

	public MasterDataLoader MasterDataLoader => _masterDataLoader;

	public LoadDirectionService LoadDirectionService => _loadDirectionService;

	public MotionSoundController MotionSoundController => _motionSoundController;

	public HeroineCommonParameter CommonParameter => _commonParameter;

	public bool IsFinishedVoice => _heroineVoiceController.IsFinishedVoice;

	public bool IsSleeping => _isSleeping;

	public bool IsWantJumpUp => _isWantJumpUp;

	public bool IsCanPlayPomodoroVoice => _isCanPlayPomodoroVoice;

	public bool IsPlayingPomodoroAction => _isPlayingPomodoroAction;

	public bool IsCurrentGameEndDirection => _isCurrentGameEndDirection;

	public bool IsUse => _isUse;

	public void InitSleepFlg()
	{
		_isSleeping = false;
		_isWantJumpUp = false;
	}

	public void ActivateSleepFlg()
	{
		_isSleeping = true;
	}

	public void WantJumpUp()
	{
		_isWantJumpUp = true;
	}

	public void Setup(Animator animator)
	{
		_nextActionSelector.Setup();
		_pomodoroVoiceSelector.Setup();
		_leaveChairJudge.Setup();
		_normalWorkStartVoiceSelector.Setup();
		_heroineFacialController.Setup(animator);
		_heroineVoiceController.Setup(animator);
		_motionSoundController.Setup(_heroineVoiceController);
		_selfTalkController.Setup(_motionSoundController);
		if ((object)_heroineService == null)
		{
			_heroineService = RoomLifetimeScope.Resolve<HeroineService>();
		}
		if (_masterDataLoader == null)
		{
			_masterDataLoader = RoomLifetimeScope.Resolve<MasterDataLoader>();
		}
		_actionStateMachine = new StateMachineMonobehavior<HeroineAI>(this);
		_actionStateMachine.Add(_stateGameStartDirection);
		_actionStateMachine.AddAnyTransition<HeroineStateGameStartDirection>(0);
		_actionStateMachine.Add(_wantTalk);
		_actionStateMachine.AddAnyTransition<HeroineWantTalk>(1);
		_actionStateMachine.Add(_stateStory);
		_actionStateMachine.AddAnyTransition<HeroineStateStory>(2);
		_actionStateMachine.Add(_stateClickHeroine);
		_actionStateMachine.AddAnyTransition<HeroineStateClickHeroine>(3);
		_actionStateMachine.Add(_stateWildStretchFullBody);
		_actionStateMachine.AddAnyTransition<HeroineStateWildStretchFullBody>(4);
		_actionStateMachine.Add(_stateWildStretchShoulder);
		_actionStateMachine.AddAnyTransition<HeroineStateWildStretchShoulder>(5);
		_actionStateMachine.Add(_stateWildTea);
		_actionStateMachine.AddAnyTransition<HeroineStateWildTea>(6);
		_actionStateMachine.Add(_stateGuts);
		_actionStateMachine.AddAnyTransition<HeroineStateWildGuts>(7);
		_actionStateMachine.Add(_stateOpenWindow);
		_actionStateMachine.AddAnyTransition<HeroineStateWildOpenWindow>(8);
		_actionStateMachine.Add(_stateCloseWindow);
		_actionStateMachine.AddAnyTransition<HeroineStateWildCloseWindow>(9);
		_actionStateMachine.Add(_stateWildFromPcToBook);
		_actionStateMachine.AddAnyTransition<HeroineStateFromPcToBook>(10);
		_actionStateMachine.Add(_stateWildFromReportToBook);
		_actionStateMachine.AddAnyTransition<HeroineStateFromReportToBook>(15);
		_actionStateMachine.Add(_stateWildFromBookToPc);
		_actionStateMachine.AddAnyTransition<HeroineStateFromBookToPc>(12);
		_actionStateMachine.Add(_stateWildFromReportToPc);
		_actionStateMachine.AddAnyTransition<HeroineStateFromReportToPc>(14);
		_actionStateMachine.Add(_stateWildFromBookToReport);
		_actionStateMachine.AddAnyTransition<HeroineStateFromBookToReport>(13);
		_actionStateMachine.Add(_stateWildFromPcToReport);
		_actionStateMachine.AddAnyTransition<HeroineStateFromPcToReport>(11);
		_actionStateMachine.Add(_stateWorkPC);
		_actionStateMachine.AddAnyTransition<HeroineStateWorkPC>(16);
		_actionStateMachine.Add(_stateWorkBook);
		_actionStateMachine.AddAnyTransition<HeroineStateWorkBook>(17);
		_actionStateMachine.Add(_stateWorkReport);
		_actionStateMachine.AddAnyTransition<HeroineStateWorkReport>(18);
		_actionStateMachine.Add(_stateBreakMovie);
		_actionStateMachine.AddAnyTransition<HeroineStateBreakMovie>(19);
		_actionStateMachine.Add(_stateBreakForward);
		_actionStateMachine.AddAnyTransition<HeroineStateBreakForward>(24);
		_actionStateMachine.Add(_stateBreakReadBook);
		_actionStateMachine.AddAnyTransition<HeroineStateBreakReadBook>(20);
		_actionStateMachine.Add(_stateBreakListenMusic);
		_actionStateMachine.AddAnyTransition<HeroineStateBreakListenMusic>(21);
		_actionStateMachine.Add(_stateBreakTeaTime);
		_actionStateMachine.AddAnyTransition<HeroineStateBreakTeaTime>(22);
		_actionStateMachine.Add(_stateBreakSleep);
		_actionStateMachine.AddAnyTransition<HeroineStateBreakSleep>(23);
		_actionStateMachine.Add(_stateLeaveChairGoSofa);
		_actionStateMachine.AddAnyTransition<HeroineStateLeaveChairGoSofa>(25);
		_actionStateMachine.Add(_stateLeaveChairGoFar);
		_actionStateMachine.AddAnyTransition<HeroineStateLeaveChairGoFar>(26);
		_actionStateMachine.Add(_gameEndTalk);
		_actionStateMachine.AddAnyTransition<HeroineGameEndTalk>(27);
		_actionStateMachine.Add(_stateEventNewYearCountdown);
		_actionStateMachine.AddAnyTransition<HeroineStateEventNewYearCountdown>(10000);
		_actionStateMachine.Start<HeroineStateStory>();
		ObservableSubscribeExtensions.Subscribe(SaveDataManager.Instance.CollaborationSaveData.CurrentType, delegate
		{
			if (!_pomodoroService.IsTimerRunning())
			{
				if (IsNeedChangeWantTalk())
				{
					StartWantTalk();
				}
				else
				{
					ChangeCurrentMatcheAction();
				}
			}
		}).AddTo(this);
	}

	public void StartGameStartDirection()
	{
		_actionStateMachine.Dispatch(0);
	}

	public void StartWantTalk()
	{
		_actionStateMachine.Dispatch(1);
	}

	public void UpdateHeroineAI()
	{
		if (!_isUse)
		{
			return;
		}
		_actionStateMachine.Update();
		switch (_currentUpdateState)
		{
		case UpdateStateType.Idle:
			SelfTalkToPlayer();
			AutoActionChange();
			break;
		case UpdateStateType.EndWaitFinishAction:
			if (_changeNextAction == null)
			{
				_currentUpdateState = UpdateStateType.Idle;
				break;
			}
			_currentUpdateState = UpdateStateType.Changing;
			_changeNextAction();
			break;
		case UpdateStateType.WaitFinishAction:
		case UpdateStateType.Changing:
			break;
		}
		void SelfTalkToPlayer()
		{
			if (SaveDataManager.Instance.SettingData.IsPlaySelfTalk.Value)
			{
				ActionType currentStateActionType = GetCurrentStateActionType((ActionStateType)_actionStateMachine.CurrentStateKey);
				if (currentStateActionType != ActionType.Work && currentStateActionType == ActionType.Break)
				{
					_selfTalkController.UpdateLottery();
					if (_selfTalkController.IsTalkReady && _facilityClickHeroine.ReactionReady(FacilityClickHeroine.ReactionType.HeroineSelf))
					{
						_selfTalkController.UseSelfTalk();
					}
				}
			}
		}
	}

	private void AutoActionChange()
	{
		if (_currentActionState == ActionStateType.EventNewYearCountdown)
		{
			return;
		}
		if (IsNeedChangeEventNewYearCountdown())
		{
			EventNewYearCountdownUpdate();
		}
		ActionType currentStateActionType = GetCurrentStateActionType((ActionStateType)_actionStateMachine.CurrentStateKey);
		if (currentStateActionType != ActionType.Work)
		{
			_ = 2;
			if (IsNeedChangeWantTalk())
			{
				WantTalkUpdate();
			}
			else if (_leaveChairJudge.NeedLeaveChair())
			{
				LeaveChairUpdate(_leaveChairJudge.Destination);
			}
			else
			{
				BreakUpdate();
			}
		}
		void BreakUpdate()
		{
			if (_nextActionSelector.IsPossibleChangeBreakType())
			{
				bool isUseWildMotion = true;
				ChangeCurrentMatcheAction(isUseWildMotion);
			}
		}
		void EventNewYearCountdownUpdate()
		{
			if (_currentActionState != ActionStateType.EventNewYearCountdown)
			{
				_isPlayedEventNewYearCountdown = true;
				_cts = null;
				_cts = new CancellationTokenSource();
				_changeNextAction = delegate
				{
					ChangeState(ActionStateType.EventNewYearCountdown);
					OnEndChangeAction();
				};
				StartNextActionReady().Forget();
			}
		}
		void LeaveChairUpdate(LeaveChairJudge.LeaveChairDestination leaveChairDestination)
		{
			_actionStateBeforeLeaveChair = _currentActionState;
			_leaveChairJudge.OnLeaveChair();
			_cts?.Cancel();
			_cts = null;
			_cts = new CancellationTokenSource();
			_changeNextAction = delegate
			{
				PlayLeaveChairVoiceThenGo(leaveChairDestination).Forget();
			};
			StartNextActionReady().Forget();
		}
		void WantTalkUpdate()
		{
			if (_currentActionState != ActionStateType.WantTalk)
			{
				_cts = null;
				_cts = new CancellationTokenSource();
				_changeNextAction = delegate
				{
					ChangeState(ActionStateType.WantTalk);
					OnEndChangeAction();
				};
				StartNextActionReady().Forget();
			}
		}
	}

	private void OnEndChangeAction()
	{
		_changeNextAction = null;
		_currentUpdateState = UpdateStateType.Idle;
	}

	private async UniTask StartNextActionReady()
	{
		_currentUpdateState = UpdateStateType.WaitFinishAction;
		CancellationToken ct = _cts.Token;
		await UniTask.WaitUntil(() => !_scenarioReader.IsPlayingScenario(), PlayerLoopTiming.Update, ct);
		ct.ThrowIfCancellationRequested();
		await ((HeroineBaseState)_actionStateMachine.CurrentState).ReadyFinishState(ct);
		ct.ThrowIfCancellationRequested();
		_currentUpdateState = UpdateStateType.EndWaitFinishAction;
	}

	public bool IsPossibleChangeAction()
	{
		switch (_currentUpdateState)
		{
		case UpdateStateType.Idle:
		case UpdateStateType.WaitFinishAction:
		case UpdateStateType.EndWaitFinishAction:
			return true;
		case UpdateStateType.Changing:
			return false;
		default:
			return false;
		}
	}

	public void ChangeCurrentMatcheAction(bool isUseWildMotion = false)
	{
		_cts = null;
		_cts = new CancellationTokenSource();
		_changeNextAction = delegate
		{
			if (isUseWildMotion)
			{
				_nextActionSelector.UpdateNextWorkAction();
				_nextActionSelector.UpdateNextBreakAction();
				ActionStateType nextWildState = _nextActionSelector.GetNextWildState();
				if (nextWildState == ActionStateType.None)
				{
					ChangeToMatchAction();
				}
				else
				{
					_actionStateMachine.Dispatch((int)nextWildState);
					_ = (HeroineBaseState)_actionStateMachine.CurrentState;
					StartNextActionReady().Forget();
					_changeNextAction = delegate
					{
						ChangeToMatchAction();
					};
				}
			}
			else
			{
				ChangeToMatchAction();
			}
		};
		StartNextActionReady().Forget();
		void ChangeToMatchAction()
		{
			if (_pomodoroService.CurrentPomodoroType == PomodoroService.PomodoroType.Work)
			{
				StartWork();
			}
			else
			{
				StartBreak();
			}
			OnEndChangeAction();
		}
	}

	public UniTask ChangePomodoroActionAsync()
	{
		_isPlayingPomodoroAction = true;
		_selfTalkController.RestartTalkDelayTime();
		_cts?.Cancel();
		bool isCanceled = false;
		_cts = null;
		_cts = new CancellationTokenSource();
		DateTime startTime = DateTime.Now;
		_changeNextAction = delegate
		{
			ChangeState(ActionStateType.ClickHeroine);
			_pomodoroVoicePlayType = 0;
			PomodoroVoiceAction();
		};
		StartNextActionReady().Forget();
		return UniTask.CompletedTask;
		void ChangeCurrentPomodoroAction()
		{
			ActionType actionType = _pomodoroService.CurrentPomodoroType switch
			{
				PomodoroService.PomodoroType.Work => ActionType.Work, 
				PomodoroService.PomodoroType.Break => ActionType.Break, 
				PomodoroService.PomodoroType.Complete => ActionType.Break, 
				_ => throw new ArgumentException($"Invalid PomodoroType: {_pomodoroService.CurrentPomodoroType}"), 
			};
			ChangeState(_nextActionSelector.GetNextAction(actionType));
			if (actionType == ActionType.Break)
			{
				_nextActionSelector.UseNextBreakAction();
			}
			OnEndChangeAction();
		}
		async UniTask<bool> PlayPomodoroVoiceDirection()
		{
			int episodeNumber = 0;
			NovelData targetNovel = null;
			ScenarioType scenarioType;
			if (_debugService.IsDebugPomodoroVoiceEnabled)
			{
				episodeNumber = _pomodoroService.CurrentPomodoroType switch
				{
					PomodoroService.PomodoroType.Work => _debugService.DebugPomodoroWorkVoiceEpisodeNumber, 
					PomodoroService.PomodoroType.Break => _debugService.DebugPomodoroBreakVoiceEpisodeNumber, 
					PomodoroService.PomodoroType.Complete => _debugService.DebugPomodoroFinishVoiceEpisodeNumber, 
					_ => throw new ArgumentException($"Invalid PomodoroType: {_pomodoroService.CurrentPomodoroType}"), 
				};
				scenarioType = _pomodoroService.CurrentPomodoroType switch
				{
					PomodoroService.PomodoroType.Work => (!_debugService.IsDebugPomodoroLongWorkStartVoicePlayEnabled) ? ((!_debugService.IsDebugPomodoroContinuousWorkVoicePlayEnabled) ? ScenarioType.SpeakWord_PomodoroStart : ScenarioType.SpeakWord_Pomodoro_WorkContinuousStart) : ScenarioType.SpeakWord_Pomodoro_WorkLongStart, 
					PomodoroService.PomodoroType.Break => (!_debugService.IsDebugPomodoroShortWorkedBreakStartVoicePlayEnabled) ? ((!_debugService.IsDebugPomodoroLongWorkedBreakStartVoicePlayEnabled) ? ScenarioType.SpeakWord_PomodoroBreak : ScenarioType.SpeakWord_Pomodoro_LongWorkedBreakStart) : ScenarioType.SpeakWord_Pomodoro_ShortWorkedBreakStart, 
					PomodoroService.PomodoroType.Complete => (!_debugService.IsDebugPomodoroLongWorkFinishVoicePlayEnabled) ? ((!_debugService.IsDebugPomodoroShortWorkFinishVoicePlayEnabled) ? ((!_debugService.IsDebugPomodoroMidwayFinishVoicePlayEnabled) ? ScenarioType.SpeakWord_PomodoroFinish : ScenarioType.SpeakWord_Pomodoro_MidwayFinish) : ScenarioType.SpeakWord_Pomodoro_ShortWorkFinish) : ScenarioType.SpeakWord_Pomodoro_LongWorkFinish, 
					_ => throw new ArgumentException($"Invalid PomodoroType: {_pomodoroService.CurrentPomodoroType}"), 
				};
			}
			else
			{
				ScenarioInfo randomPomodoroScenarioInfo = GetRandomPomodoroScenarioInfo(_pomodoroService.CurrentPomodoroType);
				if (randomPomodoroScenarioInfo == null)
				{
					Debug.LogError("ChangePomodoroActionAsync: ポモドーロのシナリオ情報が見つかりませんでした。");
					return false;
				}
				episodeNumber = randomPomodoroScenarioInfo.EpisodeNumber;
				scenarioType = randomPomodoroScenarioInfo.ScenarioType;
			}
			ScenarioGroupData scenarioGroup = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
			IEnumerable<NovelData> source = _masterDataLoader.NovelMasterList.Where((NovelData n) => n.ScenarioGroupID == scenarioGroup.ID);
			targetNovel = source.FirstOrDefault((NovelData x) => x.Command == CommandType.PomodoroVoiceText);
			if (targetNovel == null)
			{
				Debug.LogError($"ChangePomodoroActionAsync: episodeNumber{episodeNumber}が見つかりませんでした。");
			}
			else
			{
				bool flag = false;
				if (targetNovel.BodyMotion == -1)
				{
					flag = true;
					_isCanPlayPomodoroVoice = true;
				}
				else
				{
					_pomodoroVoicePlayType = int.Parse(targetNovel.Arg1);
					_isCanPlayPomodoroVoice = _pomodoroVoicePlayType == 0;
				}
				_facilityVoiceTextScenario.WantPlayVoiceTextScenario(scenarioType, episodeNumber);
				if (!flag)
				{
					if (targetNovel.Arg2 == -1f)
					{
						isCanceled = await WaitWithTimeout(delegate
						{
							HeroineService heroineService = _heroineService;
							HeroineService.AnimationType bodyMotion = (HeroineService.AnimationType)targetNovel.BodyMotion;
							return heroineService.IsEndAnimation(bodyMotion.ToString());
						}, 30f, _cts.Token);
					}
					else
					{
						isCanceled = await WaitWithTimeout(() => _pomodoroVoicePlayType == -1, 30f, _cts.Token);
						if (!_heroineService.IsCurrentAnimation(HeroineService.AnimationType.Base001.ToString()))
						{
							isCanceled = await WaitWithTimeout(() => _heroineService.IsEndAnimation(), 30f, _cts.Token);
						}
					}
				}
				if (isCanceled)
				{
					return true;
				}
			}
			return false;
		}
		async void PomodoroVoiceAction()
		{
			_heroineService.ChangeAnimation(HeroineService.AnimationType.Base001);
			isCanceled = await WaitWithTimeout(() => _heroineService.IsCurrentAnimation(HeroineService.AnimationType.Base001.ToString()), 30f, _cts.Token);
			if (isCanceled)
			{
				OnEndPomodoroVoice();
			}
			else
			{
				if ((DateTime.Now - startTime).TotalSeconds < 5.0)
				{
					isCanceled = await PlayPomodoroVoiceDirection();
					_heroineService.LookInitSlowly();
					_heroineService.InitHeroineFacialAfterDelay(0.77f);
				}
				else
				{
					OnEndPomodoroVoice();
				}
				if (isCanceled)
				{
					OnEndPomodoroVoice();
				}
				else
				{
					_nextActionSelector.UpdateNextWorkAction();
					_nextActionSelector.UpdateNextBreakAction();
					ActionStateType nextWildState = _nextActionSelector.GetNextWildState();
					if (IsNeedChangeDesk(nextWildState) && _pomodoroService.CurrentPomodoroType == PomodoroService.PomodoroType.Work)
					{
						_actionStateMachine.Dispatch((int)nextWildState);
						_ = (HeroineBaseState)_actionStateMachine.CurrentState;
						_nextActionSelector.UseNextWorkAction();
						StartNextActionReady().Forget();
						_changeNextAction = delegate
						{
							ChangeCurrentPomodoroAction();
						};
					}
					else
					{
						ChangeCurrentPomodoroAction();
					}
				}
			}
		}
	}

	public void SettingAdjustTimingVoiceType(int playType)
	{
		_pomodoroVoicePlayType = playType;
	}

	public void PlayPomodoroVoice(int playType)
	{
		if (_pomodoroVoicePlayType == playType)
		{
			_isCanPlayPomodoroVoice = true;
		}
	}

	public void OnPlayPomodoroVoice()
	{
		_pomodoroVoicePlayType = -1;
	}

	public void OnEndPomodoroVoice()
	{
		_pomodoroVoicePlayType = -1;
		_isCanPlayPomodoroVoice = false;
		_isPlayingPomodoroAction = false;
	}

	public void CancelChangeAction()
	{
		_cts?.Cancel();
		_currentUpdateState = UpdateStateType.Idle;
	}

	public void StartWork()
	{
		ChangeState(_nextActionSelector.GetNextAction(ActionType.Work));
		_nextActionSelector.UseNextWorkAction();
	}

	public void StartBreak()
	{
		ChangeState(_nextActionSelector.GetNextAction(ActionType.Break));
		_nextActionSelector.UseNextBreakAction();
		_selfTalkController.OnStartBreak();
	}

	private async UniTask ReadyChangePomodoroState(PomodoroService.PomodoroType nextPomodoroType)
	{
		bool flag = false;
		switch (nextPomodoroType)
		{
		case PomodoroService.PomodoroType.Work:
		{
			ActionStateType currentState = GetCurrentState();
			if ((uint)(currentState - 16) <= 2u)
			{
				flag = true;
			}
			break;
		}
		case PomodoroService.PomodoroType.Break:
		case PomodoroService.PomodoroType.Complete:
		{
			ActionStateType currentState = GetCurrentState();
			if ((uint)(currentState - 19) <= 5u)
			{
				flag = true;
			}
			break;
		}
		}
		if (!flag)
		{
			await ((HeroineBaseState)_actionStateMachine.CurrentState).ReadyFinishState(_cts.Token);
		}
	}

	private ActionType GetCurrentStateActionType(ActionStateType stateType)
	{
		switch (stateType)
		{
		case ActionStateType.WorkPC:
		case ActionStateType.WorkBook:
		case ActionStateType.WorkReport:
			return ActionType.Work;
		case ActionStateType.BreakMovie:
		case ActionStateType.BreakReadBook:
		case ActionStateType.BreakListenMusic:
		case ActionStateType.BreakTeaTime:
		case ActionStateType.BreakSleep:
		case ActionStateType.BreakForward:
		case ActionStateType.LeaveChairGoSofa:
		case ActionStateType.LeaveChairGoFar:
			return ActionType.Break;
		default:
			return ActionType.None;
		}
	}

	public void DebugForceLeaveChair(LeaveChairJudge.LeaveChairDestination destination)
	{
		if (destination != LeaveChairJudge.LeaveChairDestination.None && IsPossibleChangeAction())
		{
			_actionStateBeforeLeaveChair = _currentActionState;
			_cts?.Cancel();
			_cts = null;
			_cts = new CancellationTokenSource();
			_changeNextAction = delegate
			{
				PlayLeaveChairVoiceThenGo(destination).Forget();
			};
			StartNextActionReady().Forget();
		}
	}

	private async UniTaskVoid PlayLeaveChairVoiceThenGo(LeaveChairJudge.LeaveChairDestination leaveChairDestination)
	{
		_heroineService.InitHeroineFacial();
		_heroineService.ChangeAnimation(HeroineService.AnimationType.Base001);
		if (!(await WaitWithTimeout(() => _heroineService.IsCurrentAnimation(HeroineService.AnimationType.Base001.ToString()), 30f, _cts.Token)))
		{
			int episodeNumber = _normalWorkStartVoiceSelector.TakeNextVoice();
			_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.SpeakWord_LeaveChair, episodeNumber);
			if (!(await WaitWithTimeout(() => _scenarioReader.IsPlayingScenario(), 30f, _cts.Token)) && !(await WaitWithTimeout(() => !_scenarioReader.IsPlayingScenario(), 30f, _cts.Token)))
			{
				ActionStateType actionStateType = ((leaveChairDestination == LeaveChairJudge.LeaveChairDestination.Sofa) ? ActionStateType.LeaveChairGoSofa : ActionStateType.LeaveChairGoFar);
				_actionStateMachine.Dispatch((int)actionStateType);
				_currentActionState = actionStateType;
				ResetLeaveChairState();
			}
		}
	}

	private void ResetLeaveChairState()
	{
		_heroineService.LookInitSlowly();
		_changeNextAction = null;
		_currentUpdateState = UpdateStateType.Idle;
	}

	public void OnLeaveChairFinished()
	{
		ChangeState(_actionStateBeforeLeaveChair);
	}

	private ScenarioInfo GetRandomPomodoroScenarioInfo(PomodoroService.PomodoroType pomodoroType)
	{
		return pomodoroType switch
		{
			PomodoroService.PomodoroType.Work => _pomodoroVoiceSelector.TakeNextWorkStartVoice(), 
			PomodoroService.PomodoroType.Break => _pomodoroVoiceSelector.TakeNextBreakStartVoice(), 
			PomodoroService.PomodoroType.Complete => _pomodoroVoiceSelector.TakeNextFinishVoice(), 
			_ => null, 
		};
	}

	public bool IsNeedChangeWantTalk()
	{
		bool flag = false;
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.None)
		{
			if (!_pomodoroService.IsTimerRunning())
			{
				if (SaveDataManager.Instance.ScenarioProgressData.IsPossibleTalkNextMainEpisode())
				{
					flag = true;
				}
				else if (!_directionService.GamePlayingDefect.IsUseDefectForEpisodeDirection() && _scenarioGroupMasterWrapper.GetPlayableExtraScenario(includeSeen: false).Any())
				{
					flag = true;
				}
			}
		}
		else if (_collaborationService.IsPossibleReadNextSpecialEpisodeNumber() && !_pomodoroService.IsTimerRunning())
		{
			flag = true;
		}
		if (!flag && SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber != 32f && _limitedTimeEventService.IsContainCurrentEvent(LimitedTimeEventType.AprilFool2026) && _scenarioGroupMasterWrapper.GetPlayableEventAprilFoolScenario(includeSeen: false).Any())
		{
			flag = true;
		}
		return flag;
	}

	public bool IsNeedChangeEventNewYearCountdown()
	{
		if (_isPlayedEventNewYearCountdown)
		{
			return false;
		}
		if (DateTime.Now.Year != 2025 || DateTime.Now.Month != 12 || DateTime.Now.Day != 31)
		{
			return false;
		}
		DateTime dateTime = new DateTime(2026, 1, 1, 0, 0, 0);
		int num = 20;
		DateTime dateTime2 = dateTime.Subtract(TimeSpan.FromSeconds(num));
		return DateTime.Now >= dateTime2;
	}

	private void ChangeState(ActionStateType nextAction)
	{
		_actionStateMachine.Dispatch((int)nextAction);
		_currentActionState = nextAction;
	}

	public void DebugChangeState(ActionStateType nextAction)
	{
		ChangeState(nextAction);
	}

	public bool IsNeedChangeDesk(ActionStateType actionType)
	{
		if ((uint)(actionType - 10) <= 5u)
		{
			return true;
		}
		return false;
	}

	public void MainStoryStartReady()
	{
		_actionStateMachine.Dispatch(2);
		_heroineFacialController.OnStartStory();
		_isPlayingPomodoroAction = false;
	}

	public void FinishMainStory()
	{
		if (IsPossibleChangeAction())
		{
			ChangeCurrentMatcheAction();
		}
		_heroineFacialController.OnFinishStory();
		_selfTalkController.RestartTalkDelayTime();
		_isPlayingPomodoroAction = false;
	}

	public void FinishTutorial()
	{
		ChangeState(ActionStateType.ClickHeroine);
		if (IsPossibleChangeAction())
		{
			ChangeCurrentMatcheAction();
		}
		_heroineFacialController.OnFinishTutorial();
		_selfTalkController.RestartTalkDelayTime();
	}

	public void SkipTutorial()
	{
		ChangeState(ActionStateType.None);
		_heroineFacialController.OnFinishTutorial();
		_selfTalkController.RestartTalkDelayTime();
	}

	public bool IsPossibleClickHeroineReaction()
	{
		if (IsPlayingPomodoroAction)
		{
			return false;
		}
		return ((HeroineBaseState)_actionStateMachine.CurrentState).IsPossibleClickReaction();
	}

	public void StartHeroineClickReaction()
	{
		_actionStateMachine.Dispatch(3);
		_heroineFacialController.OnStartHeroineClickReaction();
	}

	public void FinishHeroineClickReaction()
	{
		if (_currentUpdateState != UpdateStateType.Changing)
		{
			switch (GetCurrentStateActionType(_currentActionState))
			{
			case ActionType.Work:
				_actionStateMachine.Dispatch((int)_currentActionState);
				break;
			case ActionType.Break:
			{
				ActionStateType nextAction = _nextActionSelector.GetNextAction(ActionType.Break);
				if (nextAction == ActionStateType.WantTalk)
				{
					ChangeState(nextAction);
					break;
				}
				_nextActionSelector.WantChangeBreakAction(isClickEnd: true);
				_actionStateMachine.Dispatch((int)_currentActionState);
				break;
			}
			default:
				_nextActionSelector.UpdateNextBreakAction(isClickEnd: true);
				ChangeState(_nextActionSelector.GetNextAction(ActionType.Break));
				_nextActionSelector.UseNextBreakAction(isClickEnd: true);
				break;
			}
		}
		_heroineFacialController.OnFinishHeroineClickReaction();
		_selfTalkController.RestartTalkDelayTime();
	}

	public void EndGame()
	{
		_cts?.Cancel();
		_cts = null;
		_cts = new CancellationTokenSource();
		_isCurrentGameEndDirection = true;
		StartNextActionReady().Forget();
		_changeNextAction = delegate
		{
			_actionStateMachine.Dispatch(27);
			_currentActionState = ActionStateType.GameEndTalk;
			OnEndChangeAction();
		};
	}

	public ActionStateType GetCurrentState()
	{
		return (ActionStateType)_actionStateMachine.CurrentStateKey;
	}

	public ActionStateType GetBeforeState()
	{
		return (ActionStateType)_actionStateMachine.BeforeStateKey;
	}

	public void PlayVoice(string voice, bool isMoveMouse, bool isStory = false)
	{
		_heroineVoiceController.PlayVoice(voice, isMoveMouse, isStory).Forget();
	}

	public void CancelVoice()
	{
		_heroineVoiceController.CancelVoice();
	}

	public void EndNoVoiceTalk()
	{
		_heroineVoiceController.EndNoVoiceTalk();
	}

	public bool IsPossibleTalk()
	{
		return ((HeroineBaseState)_actionStateMachine.CurrentState).IsPossibleClickReaction();
	}

	public void ChangeFacial(int facialType)
	{
		_heroineFacialController.ChangeFacial(facialType);
	}

	public void InitFacial()
	{
		_heroineFacialController.InitFacial();
	}

	public void InitFacialAfterDelay(float delaySeconds)
	{
		_heroineFacialController.InitFacialAfterDelay(delaySeconds).Forget();
	}

	public void WantChangeBreakAction()
	{
		_nextActionSelector.WantChangeBreakAction();
	}

	public void SetIsUse(bool isUse)
	{
		_isUse = isUse;
	}

	private async UniTask<bool> WaitWithTimeout(Func<bool> condition, float timeoutSeconds, CancellationToken token)
	{
		UniTask uniTask = UniTask.WaitUntil(condition, PlayerLoopTiming.Update, token);
		UniTask uniTask2 = UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, token).ContinueWith(delegate
		{
		});
		return (await UniTask.WhenAny(uniTask, uniTask2).SuppressCancellationThrow()).Item1;
	}
}
