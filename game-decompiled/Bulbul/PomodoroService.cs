using System;
using System.Linq;
using System.Threading;
using Bulbul.MasterData;
using Bulbul.Mobile;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class PomodoroService : IDisposable
{
	private enum MainState
	{
		Idle,
		Work,
		Rest,
		Pause,
		TalkStartReady,
		Talking,
		TalkEnd
	}

	public enum PomodoroType
	{
		Work,
		Break,
		Complete
	}

	private MainState _mainState;

	private PomodoroType _currentPomodoroType = PomodoroType.Complete;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private PlayerLevelService _playerLevelService;

	[Inject]
	private PlayerPointService _playerPointService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private PomodoroTimerService _pomodoroTimerService;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private FacilityLockEventService _facilityLockEventService;

	[Inject]
	private IPomodoroCompletedScheduler pomodoroCompletedScheduler;

	private bool _isAcceptInput;

	private ReactiveProperty<int> _currentLoopCount = new ReactiveProperty<int>(1);

	private float _choiceTalkAvailableTimer;

	private bool _isChoiceTalkAvailable;

	private bool _hasUsedChoiceTalk;

	private float _choiceTalkProbability;

	private bool _isWorkSkipped;

	private const float SaveIntervalSeconds = 60f;

	private float _saveTimer;

	private double _sessionStartWorkSeconds;

	private float _lastWorkStartTimeSeconds = float.MinValue;

	private float _lastWorkEndTimeSeconds = float.MinValue;

	private float _lastPomodoroTotalWorkHours;

	private bool _isLastPomodoroFinishedMidway;

	private Subject<Unit> _onStartPomodoro = new Subject<Unit>();

	private Subject<PomodoroType> _onPlayPomodoro = new Subject<PomodoroType>();

	private Subject<Unit> _onPause = new Subject<Unit>();

	private Subject<PomodoroType> _onUnpause = new Subject<PomodoroType>();

	private Subject<(PomodoroType, TimeSpan, TimeSpan)> _onUpdatePomodoro = new Subject<(PomodoroType, TimeSpan, TimeSpan)>();

	private Subject<Unit> _onStartWork = new Subject<Unit>();

	private Subject<Unit> _onEndWork = new Subject<Unit>();

	private Subject<Unit> _onStartBreak = new Subject<Unit>();

	private Subject<Unit> _onEndBreak = new Subject<Unit>();

	private Subject<PomodoroType> _onCompletePomodoro = new Subject<PomodoroType>();

	private Subject<Unit> _onUpdateWorkHour = new Subject<Unit>();

	private Subject<float> _onPreAddExpAndPointFromCompletePomodoro = new Subject<float>();

	private readonly CompositeDisposable disposable = new CompositeDisposable();

	private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

	private IInterstitialAdController _interstitialAdController;

	public PomodoroType CurrentPomodoroType => _currentPomodoroType;

	public ReadOnlyReactiveProperty<int> CurrentLoopCount => _currentLoopCount;

	public float LastWorkStartTimeSeconds => _lastWorkStartTimeSeconds;

	public float LastWorkEndTimeSeconds => _lastWorkEndTimeSeconds;

	public float LastPomodoroTotalWorkHours => _lastPomodoroTotalWorkHours;

	public bool IsLastPomodoroFinishedMidway => _isLastPomodoroFinishedMidway;

	public Observable<Unit> OnStartPomodoro => _onStartPomodoro;

	public Observable<PomodoroType> OnPlayPomodoro => _onPlayPomodoro;

	public Observable<Unit> OnPause => _onPause;

	public Observable<PomodoroType> OnUnpause => _onUnpause;

	public Observable<(PomodoroType, TimeSpan, TimeSpan)> OnUpdatePomodoro => _onUpdatePomodoro;

	public Observable<Unit> OnStartWork => _onStartWork;

	public Observable<Unit> OnEndWork => _onEndWork;

	public Observable<Unit> OnStartBreak => _onStartBreak;

	public Observable<Unit> OnEndBreak => _onEndBreak;

	public Observable<PomodoroType> OnCompletePomodoro => _onCompletePomodoro;

	public Observable<Unit> OnUpdateWorkHour => _onUpdateWorkHour;

	public Observable<float> OnPreAddExpAndPointFromCompletePomodoro => _onPreAddExpAndPointFromCompletePomodoro;

	public void Dispose()
	{
		disposable.Dispose();
		cancellation.Cancel();
		cancellation.Dispose();
	}

	public void Setup()
	{
		if (DevicePlatform.Steam.IsMobile() && _interstitialAdController == null)
		{
			_interstitialAdController = RoomLifetimeScope.Resolve<IInterstitialAdController>();
		}
		_pomodoroTimerService.SetUp();
		_currentPomodoroType = PomodoroType.Complete;
		_mainState = MainState.Idle;
		_isAcceptInput = true;
		int currentLevel = SaveDataManager.Instance.LevelData.CurrentLevel;
		_ = SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber;
		if (SaveDataManager.Instance.ScenarioProgressData.CanShowConnectionLostNextEpisode && currentLevel < 33)
		{
			SaveDataManager.Instance.ScenarioProgressData.CanShowConnectionLostNextEpisode = false;
			SaveDataManager.Instance.SaveScenarioProgressData();
		}
		RestoreWorkTimeOnStartup();
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnLock, delegate
		{
			if (IsTimerRunning())
			{
				Pause(isPauseByPlayer: false);
			}
			_isAcceptInput = false;
		}).AddTo(disposable);
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnUnlock, delegate
		{
			if (IsTimerRunning())
			{
				UnPause(isPlaySound: false);
			}
			_isAcceptInput = true;
		}).AddTo(disposable);
	}

	public void Update()
	{
		_pomodoroTimerService.Update();
		if (_mainState == MainState.Work)
		{
			_saveTimer += Time.deltaTime;
			if (_saveTimer >= 60f)
			{
				SaveCurrentWorkTime();
				_saveTimer = 0f;
			}
		}
		else
		{
			_saveTimer = 0f;
		}
		if (_isChoiceTalkAvailable && _choiceTalkAvailableTimer > 0f)
		{
			_choiceTalkAvailableTimer -= Time.deltaTime;
			if (_choiceTalkAvailableTimer <= 0f)
			{
				_isChoiceTalkAvailable = false;
				_choiceTalkProbability = 0f;
			}
		}
		switch (_mainState)
		{
		case MainState.Work:
		{
			_onUpdatePomodoro.OnNext((PomodoroType.Work, _pomodoroTimerService.RemainTimeSpan(), _pomodoroTimerService.CurrentTimeSpan()));
			double num = _pomodoroTimerService.GetUseTimeSeconds();
			SaveDataManager.Instance.PlayerData.CurrentWorkSeconds = _sessionStartWorkSeconds + num;
			if (_pomodoroTimerService.IsTimerEnd())
			{
				_lastWorkEndTimeSeconds = Time.time;
				OnTimerEnd();
			}
			break;
		}
		case MainState.Rest:
			_onUpdatePomodoro.OnNext((PomodoroType.Break, _pomodoroTimerService.RemainTimeSpan(), _pomodoroTimerService.CurrentTimeSpan()));
			if (_pomodoroTimerService.IsTimerEnd())
			{
				OnTimerEnd();
			}
			break;
		case MainState.Idle:
		case MainState.Pause:
			break;
		}
	}

	public void MoveAheadTimer(float seconds)
	{
		if (_mainState != MainState.Work)
		{
			_ = _mainState;
			_ = 2;
		}
		if (seconds <= 0f)
		{
		}
		while (seconds > 0f)
		{
			seconds = _pomodoroTimerService.MoveAheadTime(seconds);
			if (_mainState == MainState.Work)
			{
				double num = _pomodoroTimerService.GetUseTimeSeconds();
				SaveDataManager.Instance.PlayerData.CurrentWorkSeconds = _sessionStartWorkSeconds + num;
			}
			if (_pomodoroTimerService.IsTimerEnd())
			{
				OnTimerEnd();
			}
			if (_currentPomodoroType == PomodoroType.Complete)
			{
				seconds = 0f;
				break;
			}
		}
	}

	public void StartPomodoro()
	{
		if (IsActiveInput())
		{
			_currentLoopCount.Value = 1;
			_sessionStartWorkSeconds = SaveDataManager.Instance.PlayerData.CurrentWorkSeconds;
			SaveDataManager.Instance.PomodoroData.OnStartPomodoro();
			SaveDataManager.Instance.SavePomodoroData();
			ResetChoiceTalk();
			_isLastPomodoroFinishedMidway = false;
			PlayPomodoroTimer(PomodoroType.Work);
			_heroineService.StartPomodoroTimer().Forget();
			_onStartPomodoro.OnNext(Unit.Default);
			_isWorkSkipped = false;
		}
	}

	private void PlayPomodoroTimer(PomodoroType type)
	{
		_pomodoroTimerService.PlayTimer(type);
		switch (type)
		{
		case PomodoroType.Work:
			_currentPomodoroType = PomodoroType.Work;
			_mainState = MainState.Work;
			_onStartWork.OnNext(Unit.Default);
			_lastWorkStartTimeSeconds = Time.time;
			break;
		case PomodoroType.Break:
			_currentPomodoroType = PomodoroType.Break;
			_mainState = MainState.Rest;
			_onStartBreak.OnNext(Unit.Default);
			break;
		}
		_systemSeService.PlayTimerStart();
		_onPlayPomodoro.OnNext(type);
	}

	public void PlayOrPausePomodoroTimer()
	{
		if (IsActiveInput())
		{
			switch (_mainState)
			{
			case MainState.Work:
			case MainState.Rest:
				Pause();
				break;
			case MainState.Pause:
				UnPause();
				break;
			}
		}
	}

	private void Pause(bool isPauseByPlayer = true)
	{
		_pomodoroTimerService.PauseTimer();
		_mainState = MainState.Pause;
		if (isPauseByPlayer)
		{
			_onPause.OnNext(Unit.Default);
			_systemSeService.PlayTimerPause();
		}
	}

	private void UnPause(bool isPlaySound = true)
	{
		_pomodoroTimerService.UnpauseTimer();
		switch (_currentPomodoroType)
		{
		case PomodoroType.Work:
			_mainState = MainState.Work;
			break;
		case PomodoroType.Break:
			_mainState = MainState.Rest;
			break;
		default:
			_mainState = MainState.Idle;
			break;
		}
		_onUnpause.OnNext(_currentPomodoroType);
		if (isPlaySound)
		{
			_systemSeService.PlayTimerStart();
		}
	}

	public void OnTimerEnd()
	{
		if (!IsActiveInput())
		{
			return;
		}
		if (_currentLoopCount.Value >= SaveDataManager.Instance.PomodoroData.LoopCount.Value)
		{
			CompletePomodoroTimer().Forget();
			return;
		}
		switch (_currentPomodoroType)
		{
		case PomodoroType.Work:
			_onEndWork.OnNext(Unit.Default);
			_sessionStartWorkSeconds = SaveDataManager.Instance.PlayerData.CurrentWorkSeconds;
			PlayPomodoroTimer(PomodoroType.Break);
			_heroineService.OnPomodoroWorkEnd().Forget();
			EnableChoiceTalk();
			break;
		case PomodoroType.Break:
			_onEndBreak.OnNext(Unit.Default);
			_sessionStartWorkSeconds = SaveDataManager.Instance.PlayerData.CurrentWorkSeconds;
			PlayPomodoroTimer(PomodoroType.Work);
			_currentLoopCount.Value++;
			_heroineService.OnPomodoroBreakTimeEnd().Forget();
			ResetChoiceTalk();
			break;
		}
	}

	public async UniTask CompletePomodoroTimer()
	{
		_isAcceptInput = false;
		if (_directionService.GamePlayingDefect.IsConnectionLost() && _directionService.GamePlayingDefect.IsPossibleReconnect() && !_isWorkSkipped)
		{
			SaveDataManager.Instance.ScenarioProgressData.CanShowConnectionLostNextEpisode = true;
			SaveDataManager.Instance.SaveScenarioProgressData();
		}
		double currentWorkSeconds = SaveDataManager.Instance.PlayerData.CurrentWorkSeconds;
		_lastPomodoroTotalWorkHours = (float)(currentWorkSeconds / 60.0 / 60.0);
		SaveDataManager.Instance.CalendarData.GetDailyData(DateTime.Now).WorkTimeSeconds += (float)currentWorkSeconds;
		SaveDataManager.Instance.CalendarData.SaveDateData(DateTime.Now);
		SaveDataManager.Instance.PlayerData.PomodoroTotalWorkSeconds += (float)currentWorkSeconds;
		UpdateLastStoryUnlockWorkTime();
		UpdateLastStoryUnlockFlg();
		double num = CalculateExpFromWorkTime(currentWorkSeconds);
		int point = CalculatePointFromWorkTime(currentWorkSeconds);
		bool flag = false;
		if (DevicePlatform.Steam.IsMobile())
		{
			flag = _interstitialAdController?.IsNeedAd ?? false;
		}
		_onPreAddExpAndPointFromCompletePomodoro.OnNext((float)num);
		_playerLevelService.AddExp((float)num);
		_playerPointService.AddPoint(point);
		SaveDataManager.Instance.PlayerData.CurrentWorkSeconds = 0.0;
		SaveDataManager.Instance.SavePlayerData();
		_systemSeService.PlayTimerEnd();
		PomodoroType completePomodoroType = _currentPomodoroType;
		_currentPomodoroType = PomodoroType.Complete;
		_mainState = MainState.Idle;
		if (DevicePlatform.Steam.IsMobile() && flag)
		{
			CancellationToken token = cancellation.Token;
			if (_interstitialAdController.CanShowAd())
			{
				await _interstitialAdController.ShowAdAsync(token);
			}
			else
			{
				UniTask defer = UniTask.Defer((this, token), ((PomodoroService @this, CancellationToken ct) x) => x.@this._interstitialAdController.ShowAdAsync(x.ct));
				pomodoroCompletedScheduler.Schedule(this, _interstitialAdController.CanShowAd, defer, token).Forget();
			}
		}
		EnableChoiceTalk();
		_heroineService.OnPomodoroComplete().Forget();
		_onCompletePomodoro.OnNext(completePomodoroType);
		_onUpdateWorkHour.OnNext(Unit.Default);
	}

	private void UpdateLastStoryUnlockWorkTime()
	{
		if (!SaveDataManager.Instance.ScenarioProgressData.CanShowConnectionLostNextEpisode)
		{
			int currentLevel = SaveDataManager.Instance.LevelData.CurrentLevel;
			float finishReadMainEpisodeNumber = SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber;
			if (currentLevel >= 33 && finishReadMainEpisodeNumber < 32f)
			{
				SaveDataManager.Instance.PlayerData.LastStoryUnlockWorkSeconds += SaveDataManager.Instance.PlayerData.CurrentWorkSeconds;
				SaveDataManager.Instance.SavePlayerData();
			}
		}
	}

	private void UpdateLastStoryUnlockFlg()
	{
		if (!SaveDataManager.Instance.ScenarioProgressData.CanShowConnectionLostNextEpisode && SaveDataManager.Instance.PlayerData.LastStoryUnlockWorkTime.TotalMinutes >= 50.0)
		{
			SaveDataManager.Instance.ScenarioProgressData.CanShowConnectionLostNextEpisode = true;
			SaveDataManager.Instance.SaveScenarioProgressData();
		}
	}

	public bool IsTalkStartReady()
	{
		return _mainState == MainState.TalkStartReady;
	}

	public bool IsTalkPlayEnd()
	{
		return _mainState == MainState.TalkEnd;
	}

	public void StartTalk()
	{
		ScenarioType scenarioType = ScenarioType.ShortConversation;
		int episodeNumber = UnityEngine.Random.Range(1, 21);
		ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
		if (scenarioGroupData == null)
		{
			Debug.LogError($"ポモドーロ終了時のエピソードが見つから無かったため、最初の番号の会話を再生します。見つからなかった番号は{episodeNumber}番。");
			episodeNumber = 1;
			scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == ScenarioType.ShortConversation && x.EpisodeNumber == (float)episodeNumber);
		}
		_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto);
		_scenarioReader.StartMainStory();
		_mainState = MainState.Talking;
		CancellationTokenSource talkCancellationTokenSource = new CancellationTokenSource();
		ForceFinishStoryForLeave().Forget();
		_scenarioReader.AddEndCallback(delegate
		{
			_mainState = MainState.TalkEnd;
			talkCancellationTokenSource.Cancel();
		});
		async UniTask ForceFinishStoryForLeave()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(15.0), ignoreTimeScale: false, PlayerLoopTiming.Update, talkCancellationTokenSource.Token);
			_scenarioReader.EndStory();
		}
	}

	public void EndTalk()
	{
		_mainState = MainState.Idle;
	}

	public bool IsCurrentWorking()
	{
		return _currentPomodoroType == PomodoroType.Work;
	}

	public bool IsCurrentResting()
	{
		return _currentPomodoroType == PomodoroType.Break;
	}

	public bool IsTimerRunning()
	{
		if (_mainState != MainState.Work && _mainState != MainState.Rest)
		{
			return _mainState == MainState.Pause;
		}
		return true;
	}

	public void SetWorkMinutes(int minutes)
	{
		if (IsActiveInput())
		{
			if (minutes >= 999)
			{
				minutes = 999;
			}
			else if (minutes <= 1)
			{
				minutes = 1;
			}
			SaveDataManager.Instance.PomodoroData.WorkMinutes.Value = minutes;
			SaveDataManager.Instance.SavePomodoroData();
		}
	}

	public void IncrementWorkMinutes()
	{
		if (IsActiveInput())
		{
			int value = SaveDataManager.Instance.PomodoroData.WorkMinutes.Value;
			SetWorkMinutes(++value);
			_systemSeService.PlayClick();
		}
	}

	public void DecrementWorkMinutes()
	{
		if (IsActiveInput())
		{
			int value = SaveDataManager.Instance.PomodoroData.WorkMinutes.Value;
			SetWorkMinutes(--value);
			_systemSeService.PlayCancel();
		}
	}

	public void SetBreakMinutes(int minutes)
	{
		if (IsActiveInput())
		{
			if (minutes >= 999)
			{
				minutes = 999;
			}
			else if (minutes <= 1)
			{
				minutes = 1;
			}
			SaveDataManager.Instance.PomodoroData.BreakMinutes.Value = minutes;
			SaveDataManager.Instance.SavePomodoroData();
		}
	}

	public void IncrementBreakMinutes()
	{
		if (IsActiveInput())
		{
			int value = SaveDataManager.Instance.PomodoroData.BreakMinutes.Value;
			SetBreakMinutes(++value);
			_systemSeService.PlayClick();
		}
	}

	public void DecrementBreakMinutes()
	{
		if (IsActiveInput())
		{
			int value = SaveDataManager.Instance.PomodoroData.BreakMinutes.Value;
			SetBreakMinutes(--value);
			_systemSeService.PlayCancel();
		}
	}

	public void SetLoopCount(int loopCount)
	{
		if (IsActiveInput())
		{
			if (loopCount >= 99)
			{
				loopCount = 99;
			}
			else if (loopCount <= 1)
			{
				loopCount = 1;
			}
			SaveDataManager.Instance.PomodoroData.LoopCount.Value = loopCount;
			SaveDataManager.Instance.SavePomodoroData();
		}
	}

	public void IncrementLoopCount()
	{
		if (IsActiveInput())
		{
			int value = SaveDataManager.Instance.PomodoroData.LoopCount.Value;
			SetLoopCount(++value);
			_systemSeService.PlayClick();
		}
	}

	public void DecrementLoopCount()
	{
		if (IsActiveInput())
		{
			int value = SaveDataManager.Instance.PomodoroData.LoopCount.Value;
			SetLoopCount(--value);
			_systemSeService.PlayCancel();
		}
	}

	public void ResetTimer()
	{
		if (IsActiveInput())
		{
			_isLastPomodoroFinishedMidway = true;
			_isWorkSkipped = true;
			_lastWorkEndTimeSeconds = Time.time;
			_pomodoroTimerService.ResetTimer();
			CompletePomodoroTimer().Forget();
		}
	}

	public void SkipTimer()
	{
		if (IsActiveInput())
		{
			if (_currentPomodoroType == PomodoroType.Work)
			{
				_isWorkSkipped = true;
			}
			_pomodoroTimerService.SkipTimer();
		}
	}

	public bool IsChoiceTalkAvailable()
	{
		if (_isChoiceTalkAvailable && !_hasUsedChoiceTalk)
		{
			return _choiceTalkAvailableTimer > 0f;
		}
		return false;
	}

	public float GetAnswerChoiceProbability()
	{
		return _choiceTalkProbability;
	}

	public void UseChoiceTalk()
	{
		_hasUsedChoiceTalk = true;
		_isChoiceTalkAvailable = false;
		_choiceTalkProbability = 0f;
	}

	private void EnableChoiceTalk()
	{
		float maxTalkProbability = _masterDataLoader.GamePomodoroTalkData.MaxTalkProbability;
		float maxWorkedMinutes = _masterDataLoader.GamePomodoroTalkData.MaxWorkedMinutes;
		float num = (float)(SaveDataManager.Instance.PlayerData.CurrentWorkSeconds / 60.0);
		float num2 = 0f;
		num2 = ((!(num > maxWorkedMinutes)) ? (num / maxWorkedMinutes * maxTalkProbability) : maxTalkProbability);
		_isChoiceTalkAvailable = true;
		_hasUsedChoiceTalk = false;
		_choiceTalkProbability = num2;
		_choiceTalkAvailableTimer = _masterDataLoader.GamePomodoroTalkData.PomodoroChoiceTalkAvailableTimeMinutes * 60f;
	}

	private void ResetChoiceTalk()
	{
		_isChoiceTalkAvailable = false;
		_hasUsedChoiceTalk = false;
		_choiceTalkAvailableTimer = 0f;
		_choiceTalkProbability = 0f;
	}

	private bool IsMainStoryType(ScenarioType scenarioType)
	{
		switch (scenarioType)
		{
		case ScenarioType.MainScenario:
		case ScenarioType.AfterScenario:
		case ScenarioType.DLCScenario:
		case ScenarioType.GameStart_First_CameraTouch:
		case ScenarioType.GameStart_LessTowDays_CameraTouch:
		case ScenarioType.GameStart_LessTowDays:
		case ScenarioType.GameStart_LessHarfMonth_CameraTouch:
		case ScenarioType.GameStart_GreaterHarfMonth_CameraTouch:
		case ScenarioType.GameStart_GreaterMonth_CameraTouch:
		case ScenarioType.GameStart_GreaterYear_AND_PlayOneHundred_CameraTouch:
		case ScenarioType.Tutorial:
		case ScenarioType.GameEnd:
		case ScenarioType.Photograph_GameStart_CameraTouch:
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Morning:
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Noon:
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Evening:
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Night:
			return true;
		default:
			return false;
		}
	}

	private bool IsActiveInput()
	{
		return _isAcceptInput;
	}

	public void ActivateInput()
	{
		_isAcceptInput = true;
	}

	public void RestoreWorkTimeOnStartup()
	{
		double currentWorkSeconds = SaveDataManager.Instance.PlayerData.CurrentWorkSeconds;
		if (currentWorkSeconds > 0.0)
		{
			SaveDataManager.Instance.PlayerData.PomodoroTotalWorkSeconds += currentWorkSeconds;
			DateTime lastWorkStartDateTimeString = SaveDataManager.Instance.PomodoroData.LastWorkStartDateTimeString;
			SaveDataManager.Instance.CalendarData.GetDailyData(lastWorkStartDateTimeString).WorkTimeSeconds += (float)currentWorkSeconds;
			SaveDataManager.Instance.CalendarData.SaveDateData(lastWorkStartDateTimeString);
			UpdateLastStoryUnlockWorkTime();
			double num = CalculateExpFromWorkTime(currentWorkSeconds);
			if (num > 0.0)
			{
				AddExpSilent((float)num);
			}
			int num2 = CalculatePointFromWorkTime(currentWorkSeconds);
			if (num2 > 0)
			{
				_playerPointService.AddPoint(num2);
			}
			SaveDataManager.Instance.PlayerData.CurrentWorkSeconds = 0.0;
			SaveDataManager.Instance.SavePlayerData();
			_onUpdateWorkHour.OnNext(Unit.Default);
		}
	}

	private void AddExpSilent(float exp)
	{
		LevelData levelSaveData = GetLevelSaveData();
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.AlterEgo)
		{
			float num = 0f;
			num = ((levelSaveData.CurrentLevel < 2) ? (levelSaveData.NextLevelNecessaryExp - levelSaveData.CurrentExp) : 0f);
			if (exp >= num)
			{
				exp = num;
			}
		}
		else if (SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber < 31f)
		{
			float num2 = levelSaveData.CalculateTargetLevelNecessaryExp(_masterDataLoader, 32);
			if (exp > num2)
			{
				exp = num2;
			}
		}
		levelSaveData.AddExp(exp);
		while (levelSaveData.CurrentExp >= levelSaveData.NextLevelNecessaryExp)
		{
			LevelUpSilent(levelSaveData);
		}
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.AlterEgo)
		{
			SaveDataManager.Instance.SaveCollaborationData();
		}
		else
		{
			SaveDataManager.Instance.SavePlayerData();
		}
	}

	private void LevelUpSilent(LevelData levelData)
	{
		if (levelData.CurrentExp >= levelData.NextLevelNecessaryExp)
		{
			levelData.LevelUp(_masterDataLoader);
			SaveDataManager.Instance.ScenarioProgressData.UpdateNextMainEpisode(_masterDataLoader);
		}
	}

	private LevelData GetLevelSaveData()
	{
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.AlterEgo)
		{
			return SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LevelData;
		}
		return SaveDataManager.Instance.PlayerData.LevelData;
	}

	private double CalculateExpFromWorkTime(double workTimeSeconds)
	{
		double num = workTimeSeconds / 60.0;
		double num2 = workTimeSeconds / 3600.0;
		double num3 = num;
		if (num2 >= (double)_masterDataLoader.LevelUpInfoData.PomodoroExpBaseMaxHour)
		{
			num3 = TimeSpan.FromHours(_masterDataLoader.LevelUpInfoData.PomodoroExpBaseMaxHour).TotalMinutes;
		}
		double num4 = 0.0;
		while (num3 >= (double)_masterDataLoader.LevelUpInfoData.PomodoroExpBaseMinutes && _masterDataLoader.LevelUpInfoData.PomodoroExpBaseMinutes != 0f)
		{
			num4 += (double)_masterDataLoader.LevelUpInfoData.PomodoroExpBaseMax;
			num3 -= (double)_masterDataLoader.LevelUpInfoData.PomodoroExpBaseMinutes;
		}
		double num5 = 1.0 - ((double)_masterDataLoader.LevelUpInfoData.PomodoroExpBaseMinutes - num3) / (double)_masterDataLoader.LevelUpInfoData.PomodoroExpBaseMinutes;
		num4 += (double)_masterDataLoader.LevelUpInfoData.PomodoroExpBaseMax * num5;
		if (double.IsNaN(num4))
		{
			num4 = 0.0;
		}
		if (SaveDataManager.Instance.LevelData.CurrentLevel == 1 && num4 < (double)SaveDataManager.Instance.LevelData.NextLevelNecessaryExp)
		{
			num4 = SaveDataManager.Instance.LevelData.NextLevelNecessaryExp;
		}
		return num4;
	}

	private int CalculatePointFromWorkTime(double workTimeSeconds)
	{
		double num = workTimeSeconds / 60.0;
		double num2 = workTimeSeconds / 3600.0;
		double num3 = num;
		if (num2 >= (double)_masterDataLoader.PointUpInfoData.PomodoroPointBaseMaxHour)
		{
			num3 = TimeSpan.FromHours(_masterDataLoader.PointUpInfoData.PomodoroPointBaseMaxHour).TotalMinutes;
		}
		double num4 = 0.0;
		while (num3 >= (double)_masterDataLoader.PointUpInfoData.PomodoroPointBaseMinutes && _masterDataLoader.PointUpInfoData.PomodoroPointBaseMinutes != 0f)
		{
			num4 += (double)_masterDataLoader.PointUpInfoData.PomodoroPointBaseMax;
			num3 -= (double)_masterDataLoader.PointUpInfoData.PomodoroPointBaseMinutes;
		}
		double num5 = 1.0 - ((double)_masterDataLoader.PointUpInfoData.PomodoroPointBaseMinutes - num3) / (double)_masterDataLoader.PointUpInfoData.PomodoroPointBaseMinutes;
		num4 += (double)_masterDataLoader.PointUpInfoData.PomodoroPointBaseMax * num5;
		if (double.IsNaN(num4))
		{
			num4 = 0.0;
		}
		return (int)num4;
	}

	private void SaveCurrentWorkTime()
	{
		SaveDataManager.Instance.SavePlayerData();
	}

	private double CalcExpLimitForLockedStandardEditionPlayer(double gotExp, int limitLv)
	{
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value != SpecialService.CollaborationType.None)
		{
			return gotExp;
		}
		int currentLevel = SaveDataManager.Instance.LevelData.CurrentLevel;
		float currentExp = SaveDataManager.Instance.LevelData.CurrentExp;
		if (currentLevel >= limitLv)
		{
			return 0.0;
		}
		double num = 0.0;
		for (int i = currentLevel - 1; i < limitLv - 1; i++)
		{
			float num2 = _masterDataLoader.LevelUpInfoData.NextLevelNecessaryExpArray[i];
			num += (double)num2;
		}
		num -= (double)currentExp;
		if (num <= 0.0)
		{
			return 0.0;
		}
		if (gotExp > num)
		{
			return num;
		}
		return gotExp;
	}

	public bool CheckNextSkipCompleted()
	{
		if (CurrentPomodoroType == PomodoroType.Complete)
		{
			return true;
		}
		if (CurrentPomodoroType == PomodoroType.Work)
		{
			return _currentLoopCount.CurrentValue >= SaveDataManager.Instance.PomodoroData.LoopCount.Value;
		}
		return false;
	}
}
