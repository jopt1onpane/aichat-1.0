using System;
using System.Linq;
using System.Threading;
using Bulbul;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using MyUtil;
using VContainer;

public class HeroineStateGameStartDirection : HeroineBaseState
{
	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private RoomCameraManager _roomCameraManager;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private DebugService _debugService;

	[Inject]
	private SmallTalkSelector _smallTalkSelector;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private TimeOfDayProvider _timeOfDayProvider;

	private GameStartTalkSelector _gameStartTalkSelector = new GameStartTalkSelector();

	private CancellationTokenSource _tokenSource = new CancellationTokenSource();

	public override void OnRegisterToStateMachine()
	{
	}

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		_directionService.GameStartCameraDirection.Stop();
		if (_debugService.IsDebugGameStartDirectionEnabled)
		{
			if (_debugService.DebugGameStartDirectionScenarioType != GameStartDirectionScenarioType.GameStart_LessTowDays)
			{
				DebugCameraTouchSpeakAnimation().Forget();
			}
			else
			{
				DebugNormalSpeakAnimation().Forget();
			}
		}
		else if (_debugService.IsUsePhotographyDebugEnabled)
		{
			CameraTouchSpeakAnimation(0).Forget();
		}
		else
		{
			if (TryPlayChristmas2025EventOpening())
			{
				return;
			}
			if (SaveDataManager.Instance.PlayerData.IsFirstLogin || SaveDataManager.Instance.PlayerData.IsNeedTutorial)
			{
				CameraTouchSpeakAnimation(0).Forget();
				return;
			}
			DateTime lastLoginDateTime = SaveDataManager.Instance.PlayerData.LastLoginDateTime;
			TimeSpan timeSpan = DateTime.Now - lastLoginDateTime;
			if (timeSpan.TotalDays >= 1.0 || lastLoginDateTime.Day != DateTime.Now.Day)
			{
				CameraTouchSpeakAnimation((int)timeSpan.TotalDays).Forget();
			}
			else
			{
				NormalSpeakAnimation((int)timeSpan.TotalDays).Forget();
			}
		}
		async UniTask CameraTouchSpeakAnimation(int elapsedTotalDays)
		{
			_directionService.GameStartCameraDirection.Play();
			bool isPhotographyDebug = _debugService.IsUsePhotographyDebugEnabled;
			string scenarioGroupId = (isPhotographyDebug ? ReadyPhotographyGameStartDirection() : ReadyHeroineSpeak(elapsedTotalDays, isCameraTouch: true));
			if (IsFirstCommandMotionNotChange(scenarioGroupId))
			{
				UniTask uniTask = UniTask.WaitUntil(() => base.Owner.HeroineService.IsCurrentAnimation("GameStartDirectionLoop"));
				UniTask uniTask2 = UniTask.WaitForSeconds(5, ignoreTimeScale: false, PlayerLoopTiming.Update, _tokenSource.Token);
				await UniTask.WhenAny(uniTask, uniTask2);
			}
			if (isPhotographyDebug)
			{
				StartPhotographyHeroineSpeak();
			}
			else
			{
				StartHeroineSpeak();
			}
			EndSpeak().Forget();
		}
		static ScenarioType ConvertToScenarioType(GameStartDirectionScenarioType debugScenarioType)
		{
			return debugScenarioType switch
			{
				GameStartDirectionScenarioType.GameStart_First_CameraTouch => ScenarioType.GameStart_First_CameraTouch, 
				GameStartDirectionScenarioType.GameStart_LessTowDays_CameraTouch => ScenarioType.GameStart_LessTowDays_CameraTouch, 
				GameStartDirectionScenarioType.GameStart_LessTowDays => ScenarioType.GameStart_LessTowDays, 
				GameStartDirectionScenarioType.GameStart_LessHarfMonth_CameraTouch => ScenarioType.GameStart_LessHarfMonth_CameraTouch, 
				GameStartDirectionScenarioType.GameStart_GreaterHarfMonth_CameraTouch => ScenarioType.GameStart_GreaterHarfMonth_CameraTouch, 
				GameStartDirectionScenarioType.GameStart_GreaterMonth_CameraTouch => ScenarioType.GameStart_GreaterMonth_CameraTouch, 
				GameStartDirectionScenarioType.GameStart_GreaterYear_AND_PlayOneHundred_CameraTouch => ScenarioType.GameStart_GreaterYear_AND_PlayOneHundred_CameraTouch, 
				GameStartDirectionScenarioType.GameStart_LessTowDays_CameraTouch_Morning => ScenarioType.GameStart_LessTowDays_CameraTouch_Morning, 
				GameStartDirectionScenarioType.GameStart_LessTowDays_CameraTouch_Noon => ScenarioType.GameStart_LessTowDays_CameraTouch_Noon, 
				GameStartDirectionScenarioType.GameStart_LessTowDays_CameraTouch_Evening => ScenarioType.GameStart_LessTowDays_CameraTouch_Evening, 
				GameStartDirectionScenarioType.GameStart_LessTowDays_CameraTouch_Night => ScenarioType.GameStart_LessTowDays_CameraTouch_Night, 
				_ => throw new ArgumentException($"Invalid GameStartDirectionScenarioType: {debugScenarioType}"), 
			};
		}
		async UniTask DebugCameraTouchSpeakAnimation()
		{
			_directionService.GameStartCameraDirection.Play();
			UniTask uniTask = UniTask.WaitUntil(() => base.Owner.HeroineService.IsCurrentAnimation("GameStartDirectionLoop"));
			UniTask uniTask2 = UniTask.WaitForSeconds(5, ignoreTimeScale: false, PlayerLoopTiming.Update, _tokenSource.Token);
			await UniTask.WhenAny(uniTask, uniTask2);
			ReadyDebugGameStartDirection(isCameraTouch: true);
			StartDebugHeroineSpeak();
			EndSpeak().Forget();
		}
		async UniTask DebugNormalSpeakAnimation()
		{
			ReadyDebugGameStartDirection(isCameraTouch: false);
			await UniTask.WaitUntil(() => base.Owner.LoadDirectionService.IsFinishFadeIn(LoadDirectionService.DirectionType.Normal));
			await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
			StartDebugHeroineSpeak();
			EndSpeak().Forget();
		}
		async UniTask EndSpeak()
		{
			UniTask uniTask = UniTask.WaitUntil(() => !_scenarioReader.IsPlayingScenario());
			UniTask uniTask2 = UniTask.WaitForSeconds(20, ignoreTimeScale: false, PlayerLoopTiming.Update, _tokenSource.Token);
			await UniTask.WhenAny(uniTask, uniTask2);
			_scenarioReader.EndStory();
			base.Owner.HeroineService.ChangeHeroineAnimationForTrigger(HeroineService.AnimationType.GameStart_End);
			base.Owner.HeroineService.ChangeLookScaleAnimation(0f, 2f);
			base.Owner.InitFacialAfterDelay(0.77f);
			if (SaveDataManager.Instance.PlayerData.IsNeedTutorial || _smallTalkSelector.IsDecisionNextEpisode)
			{
				base.Owner.StartHeroineClickReaction();
			}
			else
			{
				base.Owner.StartBreak();
			}
		}
		ScenarioType GetTimeOfDayScenarioType()
		{
			return _timeOfDayProvider.GetCurrentTimeOfDayType() switch
			{
				TimeOfDayProvider.TimeOfDayType.Morning => ScenarioType.GameStart_LessTowDays_CameraTouch_Morning, 
				TimeOfDayProvider.TimeOfDayType.Noon => ScenarioType.GameStart_LessTowDays_CameraTouch_Noon, 
				TimeOfDayProvider.TimeOfDayType.Evening => ScenarioType.GameStart_LessTowDays_CameraTouch_Evening, 
				TimeOfDayProvider.TimeOfDayType.Night => ScenarioType.GameStart_LessTowDays_CameraTouch_Night, 
				_ => ScenarioType.GameStart_LessTowDays_CameraTouch, 
			};
		}
		bool IsFirstCommandMotionNotChange(string scenarioGroupId)
		{
			return _masterDataLoader.NovelMasterList.FirstOrDefault((NovelData n) => n.ScenarioGroupID == scenarioGroupId).BodyMotion == -1;
		}
		async UniTask NormalSpeakAnimation(int elapsedTotalDays)
		{
			ReadyHeroineSpeak(elapsedTotalDays, isCameraTouch: false);
			LoadDirectionService.DirectionType loadDirectionType = LoadDirectionService.DirectionType.Normal;
			if (SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber == 32f)
			{
				loadDirectionType = LoadDirectionService.DirectionType.Error;
			}
			await UniTask.WaitUntil(() => base.Owner.LoadDirectionService.IsFinishFadeIn(loadDirectionType));
			await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
			StartHeroineSpeak();
			EndSpeak().Forget();
		}
		void ReadyDebugGameStartDirection(bool isCameraTouch)
		{
			ScenarioType scenarioType = ConvertToScenarioType(_debugService.DebugGameStartDirectionScenarioType);
			int episodeNumber = _debugService.DebugGameStartDirectionEpisodeNumber;
			_debugService.IsDebugGameStartDirectionEnabled = false;
			ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
			_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto);
		}
		string ReadyHeroineSpeak(int elapsedTotalDays, bool isCameraTouch)
		{
			ScenarioType scenarioType = ScenarioType.None;
			if (SaveDataManager.Instance.PlayerData.IsFirstLogin || SaveDataManager.Instance.PlayerData.IsNeedTutorial)
			{
				scenarioType = ScenarioType.GameStart_First_CameraTouch;
			}
			else if (isCameraTouch && SaveDataManager.Instance.PlayerData.TotalLoginCount % 100 == 0)
			{
				scenarioType = ScenarioType.GameStart_GreaterYear_AND_PlayOneHundred_CameraTouch;
			}
			else if (elapsedTotalDays <= 2)
			{
				if (isCameraTouch)
				{
					if (ProbabilityUtility.IsOccurredInPercent(35f))
					{
						scenarioType = GetTimeOfDayScenarioType();
					}
					else
					{
						scenarioType = ScenarioType.GameStart_LessTowDays_CameraTouch;
					}
				}
				else
				{
					scenarioType = ScenarioType.GameStart_LessTowDays;
				}
			}
			else if (elapsedTotalDays <= 14)
			{
				scenarioType = ScenarioType.GameStart_LessHarfMonth_CameraTouch;
			}
			else if (elapsedTotalDays <= 30)
			{
				scenarioType = ScenarioType.GameStart_GreaterHarfMonth_CameraTouch;
			}
			else
			{
				scenarioType = ScenarioType.GameStart_GreaterMonth_CameraTouch;
			}
			int episodeNumber = _gameStartTalkSelector.GetGameStartTalk(scenarioType);
			_gameStartTalkSelector.UseGameStartTalk(scenarioType);
			ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
			_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto);
			return scenarioGroupData.ID;
		}
		string ReadyPhotographyGameStartDirection()
		{
			int episodeNumber = 1;
			ScenarioType scenarioType = ScenarioType.Photograph_GameStart_CameraTouch;
			ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
			_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto);
			return scenarioGroupData.ID;
		}
		void StartDebugHeroineSpeak()
		{
			_scenarioReader.StartMainStory();
		}
		void StartHeroineSpeak()
		{
			_scenarioReader.StartMainStory();
		}
		void StartPhotographyHeroineSpeak()
		{
			_scenarioReader.DebugPhotographStartMainStory();
		}
		bool TryPlayChristmas2025EventOpening()
		{
			LimitedTimeEventSaveData limitedTimeEventSaveData = SaveDataManager.Instance.LimitedTimeEventSaveData;
			bool num = limitedTimeEventSaveData.CurrentType.Value == LimitedTimeEventType.Christmas2025;
			bool flag = !limitedTimeEventSaveData.Christmas2025SaveData._hasSeenOpening;
			if (!num || !flag)
			{
				return false;
			}
			ScenarioType scenarioType = ScenarioType.Event_2025_Christmas_GameStart;
			int episodeNumber = 1;
			ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
			_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: true, isForceInitAnim: true);
			_scenarioReader.StartMainStory();
			EventEndSpeak().Forget();
			return true;
			async UniTask EventEndSpeak()
			{
				UniTask uniTask = UniTask.WaitUntil(() => !_scenarioReader.IsPlayingScenario());
				await UniTask.WhenAny(uniTask);
				_scenarioReader.EndStory();
				limitedTimeEventSaveData.Christmas2025SaveData._hasSeenOpening = true;
				SaveDataManager.Instance.SaveLimitedTimeEventData();
				base.Owner.HeroineService.ChangeLookScaleAnimation(0f, 2f);
				base.Owner.InitFacial();
				base.Owner.StartBreak();
			}
		}
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
	}

	public override bool IsPossibleClickReaction()
	{
		return true;
	}

	public override bool IsPossibleTalk()
	{
		return true;
	}

	public override void ToPossibleClickReaction()
	{
	}
}
