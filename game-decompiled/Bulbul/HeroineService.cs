using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

[RequireComponent(typeof(Animator))]
public class HeroineService : MonoBehaviour
{
	public enum AnimationType
	{
		NotChange = -1,
		Base001 = 0,
		Base001_Motion1_FrustrationRight = 1,
		Base001_Motion2_FrustrationLeft = 2,
		Base001_Motion3_PressHands = 3,
		Base001_Motion4_Dropshoulders = 4,
		Base001_Motion5_Shy = 5,
		Base001_Motion6_Jump = 6,
		Base001_Motion7_Confidence = 7,
		Base001_Motion8_Thinking = 8,
		Base001_Motion9_Start_Thinking2 = 9,
		Base001_Motion10_Start_Thinking3 = 10,
		Base001_Motion11_Start_Lookdown = 11,
		Base001_Motion12_Start_Nod = 12,
		Base001_Motion13_Start_ShakeHead = 13,
		Base001_Motion14_Start_LookPenguin = 14,
		Base001_Motion15_Start_Introduce = 15,
		Base001_Motion16 = 16,
		Base001_Motion17 = 17,
		Base001_Motion18 = 18,
		Base001_Motion19_Copycat = 19,
		Base001_Motion20_Eieio = 20,
		Base001_Motion21_Distress = 21,
		Base001_Motion22_Feellarge = 22,
		Base001_Motion23_Look_And_Regret = 23,
		Base001_Motion24_Touch_Glasses = 24,
		Wild001_Motion1_StretchFllBody = 50,
		Wild001_Motion2_StretchShoulder = 51,
		Wild001_Motion3_Tea = 52,
		Wild001_Motion4_Guts = 53,
		Wild001_Motion5_Wet = 54,
		Wild001_Motion6_Banzai = 55,
		Wild001_Motion7_Eieio = 56,
		Wild001_Motion8_OpenWindow = 57,
		Wild001_Motion9_CloseWindow = 58,
		Wild001_Motion10_Breath = 59,
		Wild001_Motion11_Guts_2 = 60,
		Wild001_Motion12_Good = 61,
		Wild001_Motion13_Fidget = 62,
		Wild001_Motion14_Arm = 63,
		Wild001_Motion15_Tired = 64,
		Wild001_Motion16_CompleteTask = 65,
		Wild001_Motion17_DeepBreath = 66,
		Wild001_Motion18_Tired_2 = 67,
		Wild001_Motion19_Gymnastics = 68,
		Wild001_Motion20_Stretch_2 = 69,
		Wild001_Motion21_DryEye = 70,
		Wild001_Motion22_Guts_3 = 71,
		Wild001_Motion23_Yawn_1 = 72,
		Wild001_Motion24_Yawn_2 = 73,
		Wild002_FromPcToBook = 100,
		Wild002_FromReportToBook = 101,
		Wild002_FromBookToPc = 102,
		Wild002_FromReportToPc = 103,
		Wild002_FromBookToReport = 104,
		Wild002_FromPcToReport = 105,
		Wild003_LeaveChair_Normal = 150,
		Wild003_LeaveChair_Sofa = 151,
		Wild003_LeaveChair_Sofa_Thinking = 152,
		Wild003_LeaveChair_Sofa_Look_Outside = 153,
		Wild003_LeaveChair_Sofa_Control_Headphone = 154,
		Wild003_LeaveChair_Cupcake_Comeback = 170,
		Wild003_LeaveChair_Cupcake_Comeback_Loop = 171,
		WorkBase001 = 200,
		WorkBase001_Stop = 201,
		WorkBase001_Thinking = 202,
		WorkBase001_DrinkTea = 203,
		WorkBase001_Break = 204,
		WorkBase001_Break2 = 205,
		WorkBase002 = 250,
		WorkBase002_KeyType = 251,
		WorkBase002_Thinking = 252,
		WorkBase002_PageFlip = 253,
		WorkBase002_Loop2 = 254,
		WorkBase002_Loop2_PageFlip = 255,
		WorkBase002_DrinkTea = 256,
		WorkBase003 = 300,
		WorkBase003_SmallThinking = 301,
		WorkBase003_BigThinking = 302,
		WorkBase003_Loop2 = 303,
		WorkBase003_DrinkTea = 304,
		WorkBase003_BrushAwayTrash = 305,
		GameStart_Start = 400,
		GameStart_End = 401,
		Desk_Click_Normal_Reaction = 402,
		Desk_Click_Work_Reaction = 403,
		Desk_Click_Rest_Reaction = 404,
		Enable_Talk = 405,
		BreakBase001 = 600,
		BreakBase001_Laugh = 601,
		BreakBase001_Suspenseful = 602,
		BreakBase001_OutTrash = 603,
		BreakBase001_Loop2 = 604,
		BreakBase002 = 650,
		BreakBase002_NextPage = 651,
		BreakBase002_PreviousPage = 652,
		BreakBase002_Interest = 653,
		BreakBase003_ListenMusicLow = 700,
		BreakBase003_ListenMusicHigh = 701,
		BreakBase004 = 750,
		BreakBase004_DrinkTea = 751,
		BreakBase004_Stretch = 752,
		BreakBase004_DrinkHot = 753,
		BreakBase004_UseCoffeeMaker = 754,
		BreakBase004_CoolingDrinkTea = 755,
		BreakBase005 = 800,
		BreakBase005_Sleep = 801,
		BreakBase005_GetUpSlowly = 802,
		BreakBase005_JumpUp = 803,
		BreakBase005_JumpUp_End = 804,
		BreakBase006 = 850,
		BreakBase006_Interest = 851,
		BreakBase006_Keyboard = 852,
		BreakBase006_PlayPenLoop = 853,
		BreakBase006_DropPen = 854,
		Story_SubBase001 = 1000,
		Story_SubBase001_Joy = 1001,
		Story_SubBase001_Sad = 1002,
		Story_SubBase001_Fun = 1003,
		Story_SubBase001_Guts = 1004,
		Story_SubBase002 = 1100,
		Story_SubBase002_Joy = 1101,
		Story_SubBase002_Sad = 1102,
		Story_SubBase002_Fun = 1103,
		Story_SubBase003 = 1200,
		Story_SubBase003_Sad = 1201,
		Story_SubBase003_Fun = 1202,
		Story_SubBase004 = 1300,
		Story_SubBase004_Agree = 1301,
		Story_SubBase004_Frustration = 1302,
		Story_SubBase005 = 1400,
		Story_SubBase005_LookDown = 1401,
		Story_SubBase005_Nod = 1402,
		Story_SubBase005_Denial = 1403,
		Story_SubBase005_SpeakBlur = 1404,
		Story_SubBase005_StartClimax = 1405,
		Story_SubBase005_Climax_1 = 1406,
		Story_SubBase005_Climax_2 = 1407,
		Story_SubBase006 = 1500,
		Story_SubBase006_001 = 1501,
		Story_SubBase006_002 = 1502,
		Story_SubBase006_003 = 1503,
		Story_SubBase007 = 1600,
		Story_SubBase007_001 = 1601,
		Story_SubBase007_002 = 1602,
		Story_SubBase007_003 = 1603,
		WantTalk_Base_001 = 5000,
		WantTalk_Base_001_WaveHandShortTime = 5001,
		WantTalk_Base_001_LeaningForward = 5002,
		GameStart_Base_001 = 6500,
		GameEnd_Base_001 = 7000,
		Event_Christmas_StartSitFromOutSide = 10000,
		Event_Christmas_Talk_001 = 10001,
		Event_NewYear_Countdown_001 = 10100,
		Event_NewYear_Countdown_002 = 10101,
		Event_NewYear_Countdown_003 = 10102,
		Event_NewYear_Countdown_004 = 10103
	}

	public enum RoomPlace
	{
		Desk
	}

	private Animator _animator;

	private AnimationType _currentAnimationType;

	[SerializeField]
	private Transform[] _heroineTransform;

	[SerializeField]
	private bool _isWarpPosition = true;

	private RoomPlace _currentPlace;

	[SerializeField]
	private HeroineAI _heroineAI;

	[SerializeField]
	private HeroineUseObjectController _heroineUseObjectController;

	[Header("視線制御")]
	[SerializeField]
	private HeroineLookDirectionService _lookDirectionService;

	[Inject]
	private RoomCameraManager _roomCameraManager;

	[Inject]
	private PomodoroTimerService _pomodoroTimerController;

	[Inject]
	private SceneFadeControllerProvider _sceneFadeControllerProvider;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private ISpecialService _collaborationService;

	private const string AnimationKey = "MotionType";

	private const string AnimationSubBaseKey = "UseSubBase";

	private Tween _lookTween;

	private AnimationType _beforeChangeAnimation;

	private AnimationType _nextAnimation;

	public HeroineUseObjectController HeroineUseObjectController => _heroineUseObjectController;

	public HeroineLookDirectionService LookDirectionService => _lookDirectionService;

	public bool IsFinishedVoice => _heroineAI.IsFinishedVoice;

	public AnimationType BeforeChangeAnimation => _beforeChangeAnimation;

	public AnimationType NextAnimation => _nextAnimation;

	public void Setup()
	{
		_animator = GetComponent<Animator>();
		_heroineAI.Setup(_animator);
		_heroineUseObjectController.Setup(this);
		_lookDirectionService.Setup(_animator, this);
		_directionService.GamePlayingDefect.IsUseConnectionLost.Subscribe(delegate(bool isUse)
		{
			if (isUse)
			{
				ChangeHeroineAnimationImmediately(0);
				_animator.speed = 0f;
				_heroineAI.SetIsUse(isUse: false);
			}
			else
			{
				_animator.speed = 1f;
				_heroineAI.SetIsUse(isUse: true);
			}
		}).AddTo(this);
		_roomCameraManager.Setup();
	}

	private void OnAnimatorIK(int layerIndex)
	{
		_lookDirectionService.ApplyValue(layerIndex);
	}

	private void LateUpdate()
	{
		_lookDirectionService.ManualLateUpdate();
	}

	public void UpdateHeroineAI()
	{
		_heroineAI.UpdateHeroineAI();
	}

	public void OnGameStart()
	{
		_animator.ResetTrigger(AnimationType.GameStart_Start.ToString());
		_animator.ResetTrigger(AnimationType.GameStart_End.ToString());
		_heroineAI.StartGameStartDirection();
	}

	public void OnTutorialWantTalk()
	{
		_heroineAI.StartWantTalk();
	}

	public async UniTask ChangeViewPoint(int movementIndex, Action onChangeEnd = null)
	{
		_sceneFadeControllerProvider.Controller.FadeOut().Forget();
		await UniTask.WaitUntil(() => _sceneFadeControllerProvider.Controller.IsComplete());
		ChangeHeroineAnimationForInteger(movementIndex);
		ChangeHeroineTransform(movementIndex).Forget();
		_roomCameraManager.ChangeCameraTransform(movementIndex);
		_sceneFadeControllerProvider.Controller.FadeIn().Forget();
		onChangeEnd?.Invoke();
	}

	public HeroineAI.ActionStateType GetCurrentAIState()
	{
		return _heroineAI.GetCurrentState();
	}

	public HeroineAI.ActionStateType GetBeforeAIState()
	{
		return _heroineAI.GetBeforeState();
	}

	public bool IsNeedChangeWantTalk()
	{
		return _heroineAI.IsNeedChangeWantTalk();
	}

	public void ScenarioStartReady()
	{
	}

	public void OnStoryStartReady(int firstBodyMotionType, int facialType)
	{
		_heroineAI.MainStoryStartReady();
		_lookDirectionService.InitLookImmediate();
		ChangeHeroineAnimationImmediately(firstBodyMotionType);
		ChangeHeroineFacialAnimation(facialType);
	}

	public void OnStartStory()
	{
	}

	public void OnEndStory(int changeBodyMotionType)
	{
		CancelVoice();
		ChangeHeroineAnimationImmediately(changeBodyMotionType);
		_lookDirectionService.InitLookImmediate();
		InitHeroineFacial();
		_heroineAI.FinishMainStory();
	}

	public void OnEndTutorial()
	{
		ChangeLookScaleAnimation(0f, 1.5f);
		InitHeroineFacial();
		_heroineAI.FinishTutorial();
	}

	public bool IsPossibleClickHeroineReaction()
	{
		return _heroineAI.IsPossibleClickHeroineReaction();
	}

	public void OnStartClickHeroineReaction()
	{
		_heroineAI.StartHeroineClickReaction();
	}

	public void OnEndClickHeroineReaction()
	{
		_heroineAI.FinishHeroineClickReaction();
	}

	public void EndGame()
	{
		_heroineAI.EndGame();
	}

	public void ChangeHeroineAnimationImmediately(int changeBodyMotionType)
	{
		_heroineAI.CancelChangeAction();
		ChangeHeroineAnimationForInteger(changeBodyMotionType);
		AnimationType animationType = (AnimationType)changeBodyMotionType;
		_animator.Play(animationType.ToString(), 0, 0f);
	}

	public void ChangeAnimation(AnimationType nextAnim)
	{
		_beforeChangeAnimation = GetCurrentAnimationType();
		_nextAnimation = nextAnim;
		ChangeHeroineAnimationForInteger((int)nextAnim);
	}

	public void ChangeHeroineAnimationForInteger(int bodyMotionType)
	{
		AnimationType animationType = (AnimationType)bodyMotionType;
		switch (animationType)
		{
		case AnimationType.NotChange:
			return;
		case AnimationType.Wild003_LeaveChair_Sofa_Control_Headphone:
		case AnimationType.WorkBase002_PageFlip:
		case AnimationType.WorkBase002_Loop2_PageFlip:
		case AnimationType.WorkBase002_DrinkTea:
		case AnimationType.WorkBase003_DrinkTea:
		case AnimationType.WorkBase003_BrushAwayTrash:
		case AnimationType.GameStart_Start:
		case AnimationType.GameStart_End:
		case AnimationType.BreakBase002_NextPage:
		case AnimationType.BreakBase002_PreviousPage:
		case AnimationType.BreakBase004_CoolingDrinkTea:
		case AnimationType.Story_SubBase001_Joy:
		case AnimationType.Story_SubBase001_Sad:
		case AnimationType.Story_SubBase001_Fun:
		case AnimationType.Story_SubBase001_Guts:
		case AnimationType.Story_SubBase002_Joy:
		case AnimationType.Story_SubBase002_Sad:
		case AnimationType.Story_SubBase002_Fun:
		case AnimationType.Story_SubBase003_Sad:
		case AnimationType.Story_SubBase003_Fun:
		case AnimationType.Story_SubBase004_Agree:
		case AnimationType.Story_SubBase004_Frustration:
			ResetAllTrigger();
			_animator.SetTrigger(animationType.ToString());
			break;
		}
		_animator.SetInteger("MotionType", bodyMotionType);
		_currentAnimationType = animationType;
	}

	public void ResetAllTrigger()
	{
		_animator.ResetTrigger(AnimationType.Wild003_LeaveChair_Sofa_Control_Headphone.ToString());
		_animator.ResetTrigger(AnimationType.GameStart_Start.ToString());
		_animator.ResetTrigger(AnimationType.GameStart_End.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase001_Joy.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase001_Sad.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase001_Fun.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase001_Guts.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase002_Joy.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase002_Sad.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase002_Fun.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase003_Sad.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase003_Fun.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase004_Agree.ToString());
		_animator.ResetTrigger(AnimationType.Story_SubBase004_Frustration.ToString());
		_animator.ResetTrigger(AnimationType.WorkBase002_PageFlip.ToString());
		_animator.ResetTrigger(AnimationType.WorkBase002_Loop2_PageFlip.ToString());
		_animator.ResetTrigger(AnimationType.WorkBase002_DrinkTea.ToString());
		_animator.ResetTrigger(AnimationType.WorkBase003_DrinkTea.ToString());
		_animator.ResetTrigger(AnimationType.WorkBase003_BrushAwayTrash.ToString());
		_animator.ResetTrigger(AnimationType.BreakBase002_NextPage.ToString());
		_animator.ResetTrigger(AnimationType.BreakBase002_PreviousPage.ToString());
		_animator.ResetTrigger(AnimationType.BreakBase004_CoolingDrinkTea.ToString());
	}

	public void ChangeHeroineAnimationForTrigger(AnimationType type)
	{
		_animator.SetTrigger(type.ToString());
		_currentAnimationType = type;
	}

	public bool IsCloseEye()
	{
		if (_heroineAI.GetCurrentState() != HeroineAI.ActionStateType.BreakSleep && _heroineAI.GetCurrentState() != HeroineAI.ActionStateType.BreakListenMusic && _heroineAI.GetBeforeState() != HeroineAI.ActionStateType.BreakSleep)
		{
			return _heroineAI.GetBeforeState() == HeroineAI.ActionStateType.BreakListenMusic;
		}
		return true;
	}

	public void ChangeHeroineFacialAnimation(int facialType)
	{
		_heroineAI.ChangeFacial(facialType);
	}

	public void InitHeroineFacial()
	{
		_heroineAI.InitFacial();
	}

	public void InitHeroineFacialAfterDelay(float delaySeconds)
	{
		_heroineAI.InitFacialAfterDelay(delaySeconds);
	}

	public void ChangeLookScaleAnimation(float lookScale, float lookSpeedSeconds, Ease lookEaseType = Ease.Unset)
	{
		_lookDirectionService.ChangeLookScaleByManualForStory(lookScale, lookSpeedSeconds, lookEaseType);
	}

	public void ChangeLookScaleByManual(float lookScale, float lookSpeedSeconds, Ease lookEaseType = Ease.Unset)
	{
		_lookDirectionService.ChangeLookScaleByManual(lookScale, lookSpeedSeconds, lookEaseType);
	}

	public void LookInitSlowly()
	{
		_lookDirectionService.PlayFromLook(1.5f, Ease.OutQuad).Forget();
	}

	public bool IsPlayingClickReactionAnimation()
	{
		if (_currentAnimationType == AnimationType.Desk_Click_Normal_Reaction || _currentAnimationType == AnimationType.Desk_Click_Work_Reaction || _currentAnimationType == AnimationType.Desk_Click_Rest_Reaction)
		{
			return true;
		}
		return false;
	}

	public AnimationType GetCurrentAnimationType()
	{
		return _currentAnimationType;
	}

	public bool IsLeaveChair()
	{
		if (GetCurrentAIState() == HeroineAI.ActionStateType.LeaveChairGoSofa || GetCurrentAIState() == HeroineAI.ActionStateType.LeaveChairGoFar)
		{
			return true;
		}
		return false;
	}

	public UniTask StartPomodoroTimer()
	{
		if (_heroineAI.IsPossibleChangeAction())
		{
			_heroineAI.ChangePomodoroActionAsync().Forget();
		}
		return UniTask.CompletedTask;
	}

	public UniTask OnPomodoroWorkEnd()
	{
		if (_heroineAI.IsPossibleChangeAction())
		{
			_heroineAI.ChangePomodoroActionAsync().Forget();
		}
		return UniTask.CompletedTask;
	}

	public UniTask OnPomodoroBreakTimeEnd()
	{
		StartPomodoroTimer().Forget();
		return UniTask.CompletedTask;
	}

	public UniTask OnPomodoroComplete()
	{
		if (_heroineAI.IsPossibleChangeAction())
		{
			_heroineAI.ChangePomodoroActionAsync().Forget();
		}
		return UniTask.CompletedTask;
	}

	public async UniTask ChangeHeroineTransform(int movementIndex)
	{
		if (movementIndex >= _heroineTransform.Length)
		{
			Debug.LogError($"positionNo is over movementNo:{movementIndex} _heroineTransform.Length:{_heroineTransform.Length}");
			return;
		}
		Transform toTransform = _heroineTransform[movementIndex];
		if (_isWarpPosition)
		{
			await UniTask.NextFrame();
			_animator.transform.SetPositionAndRotation(toTransform.position, toTransform.rotation);
		}
	}

	public async UniTaskVoid PlayVoice(string voice, bool isMoveMouse, bool isStory = false)
	{
		_heroineAI.PlayVoice(voice, isMoveMouse, isStory);
		await UniTask.CompletedTask;
	}

	public void CancelVoice()
	{
		_heroineAI.CancelVoice();
	}

	public void EndNoVoiceTalk()
	{
		_heroineAI.EndNoVoiceTalk();
	}

	public bool IsPossibleTalk()
	{
		return _heroineAI.IsPossibleTalk();
	}

	public bool IsSleeping()
	{
		return _heroineAI.IsSleeping;
	}

	public void WantJumpUp()
	{
		_heroineAI.WantJumpUp();
	}

	public bool IsCanPlayPomodoroVoice()
	{
		return _heroineAI.IsCanPlayPomodoroVoice;
	}

	public bool IsPlayingPomodoroAction()
	{
		return _heroineAI.IsPlayingPomodoroAction;
	}

	public void SettingAdjustTimingVoiceType(int playType)
	{
		_heroineAI.SettingAdjustTimingVoiceType(playType);
	}

	public void PlayPomodoroVoice(int playType)
	{
		_heroineAI.PlayPomodoroVoice(playType);
	}

	public void OnPlayPomodoroVoice()
	{
		_heroineAI.OnPlayPomodoroVoice();
	}

	public void OnEndPomodoroVoice()
	{
		_heroineAI.OnEndPomodoroVoice();
	}

	public void OnChangeHeroineLookCamera(RoomCameraManager.VirtualCameraType cameraType)
	{
		if (cameraType < RoomCameraManager.VirtualCameraType.Initial || RoomCameraManager.VirtualCameraType.MAX <= cameraType)
		{
			Debug.LogError(string.Format("[{0}] {1} :: 意図しないカメラタイプが指定されています。 CameraType: {2}", "HeroineService", "OnChangeHeroineLookCamera", cameraType));
		}
		_roomCameraManager.VirtualCameraSwitch(cameraType);
	}

	public bool IsEndAnimation(int animLoopCount = 0)
	{
		return MyAnimatorUtil.IsEndAnimation(_animator, null, animLoopCount);
	}

	public bool IsEndAnimation(string name)
	{
		return MyAnimatorUtil.IsEndAnimation(_animator, name);
	}

	public bool IsCurrentAnimation(string name)
	{
		return MyAnimatorUtil.IsCurrentAnimation(_animator, name);
	}

	public bool GetHeroineAIUse()
	{
		return _heroineAI.IsUse;
	}

	public void SetHeroineAIUse(bool use)
	{
		_heroineAI.SetIsUse(use);
	}

	public HeroineAI.UpdateStateType GetHeroineUpdateStateType()
	{
		return _heroineAI.CurrentUpdateState;
	}

	public void DebugChangeState(HeroineAI.ActionStateType nextAction)
	{
		_heroineAI.DebugChangeState(nextAction);
	}

	public void DebugForceLeaveChair(LeaveChairJudge.LeaveChairDestination destination)
	{
		_heroineAI.DebugForceLeaveChair(destination);
	}

	public void DebugOnStoryStartReady(int bodyMotionType, float lookScale, int facialType, bool isPlayMiddle)
	{
		_heroineAI.MainStoryStartReady();
		if (isPlayMiddle)
		{
			DebugChangeHeroineAnimationImmediately(bodyMotionType, 1f);
		}
		else
		{
			DebugChangeHeroineAnimationImmediately(bodyMotionType, 0f);
		}
		_lookDirectionService.InitLookImmediate();
		ChangeHeroineFacialAnimation(facialType);
	}

	public void DebugChangeHeroineAnimationImmediately(int bodyMotionType, float playTime)
	{
		_heroineAI.CancelChangeAction();
		ChangeHeroineAnimationForInteger(bodyMotionType);
		AnimationType animationType = (AnimationType)bodyMotionType;
		_animator.Play(animationType.ToString(), 0, 0f);
	}
}
