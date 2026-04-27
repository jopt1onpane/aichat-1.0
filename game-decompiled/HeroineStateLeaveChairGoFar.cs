using System.Linq;
using System.Threading;
using Bulbul;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using MyUtil;
using UnityEngine;
using VContainer;

public class HeroineStateLeaveChairGoFar : HeroineBaseState
{
	private enum Phase
	{
		None,
		GoToFar,
		Stay,
		StartEatSnack,
		WaitEnd
	}

	private const string LeaveChairNormalAnimName = "anim_wild_leavechair_normal_go";

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[SerializeField]
	private HeroineCupcake _cupCake;

	private Phase _phase;

	private float _startSeconds;

	private float _stayMaxSeconds;

	private float _soundPlayAtSeconds = -1f;

	private bool _soundPlayed;

	private bool _isTakeCupcake;

	private SnackEatStartVoiceSelector _snackEatStartVoiceSelector = new SnackEatStartVoiceSelector();

	private CancellationTokenSource _cts;

	public sealed override void OnRegisterToStateMachine()
	{
		_snackEatStartVoiceSelector.Setup();
	}

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		_phase = Phase.GoToFar;
		_startSeconds = Time.time;
		_soundPlayed = false;
		LeaveChairData leaveChairData = base.Owner.MasterDataLoader.HeroineAIMasterData.LeaveChairData;
		_stayMaxSeconds = Random.Range(leaveChairData.KitchenStaySecondsMin, leaveChairData.KitchenStaySecondsMax);
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Wild003_LeaveChair_Normal);
		float num = Random.Range(0f, 100f);
		_isTakeCupcake = num <= _masterDataLoader.HeroineAIMasterData.LeaveChairData.CupcakeTakeProbability;
	}

	protected override void OnUpdate()
	{
		switch (_phase)
		{
		case Phase.GoToFar:
			if (base.Owner.HeroineService.IsEndAnimation("anim_wild_leavechair_normal_go") && !base.Owner.MotionSoundController.IsPlayingWalkSound())
			{
				_soundPlayAtSeconds = Random.Range((Time.time - _startSeconds) * 0.2f, _stayMaxSeconds * 0.8f);
				_phase = Phase.Stay;
			}
			break;
		case Phase.Stay:
		{
			float num = Time.time - _startSeconds;
			if (!_soundPlayed && num >= _soundPlayAtSeconds)
			{
				PlayLeaveSeatKitchenSound();
				_soundPlayed = true;
			}
			if (num >= _stayMaxSeconds)
			{
				_cts?.Cancel();
				_cts = null;
				_cts = new CancellationTokenSource();
				ComeBack(_cts.Token).Forget();
			}
			break;
		}
		case Phase.StartEatSnack:
			if (base.Owner.HeroineService.IsCurrentAnimation(HeroineService.AnimationType.Wild003_LeaveChair_Cupcake_Comeback_Loop.ToString()))
			{
				ScenarioType scenarioType = ScenarioType.SpeakWord_Snack_Eat_Start;
				int episodeNumber = _snackEatStartVoiceSelector.TakeNextVoice();
				ScenarioGroupData scenarioGroupData = _masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.Scenario == scenarioType && x.EpisodeNumber == (float)episodeNumber);
				_scenarioReader.StartReady(scenarioType, scenarioGroupData.ID, ScenarioReader.ScenarioReadMode.Auto, isTalkLog: false, isUseMask: false);
				_scenarioReader.StartVoiceTextScenario();
				_phase = Phase.WaitEnd;
				_scenarioReader.AddEndCallback(delegate
				{
					base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Base001);
					base.Owner.InitFacialAfterDelay(0.77f);
				});
			}
			break;
		case Phase.WaitEnd:
			if (base.Owner.HeroineService.IsEndAnimation(HeroineService.AnimationType.Base001.ToString()))
			{
				base.Owner.OnLeaveChairFinished();
			}
			break;
		}
	}

	private async UniTask ComeBack(CancellationToken token)
	{
		await UniTask.WaitUntil(() => !base.Owner.MotionSoundController.IsPlayingKitchenSound());
		if (base.Owner.IsCurrentGameEndDirection)
		{
			base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Base001);
			_phase = Phase.WaitEnd;
		}
		else if (_isTakeCupcake)
		{
			_cupCake.TakeCupcake();
			base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Wild003_LeaveChair_Cupcake_Comeback);
			_phase = Phase.StartEatSnack;
		}
		else
		{
			base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Base001);
			_phase = Phase.WaitEnd;
		}
	}

	private void PlayLeaveSeatKitchenSound()
	{
		LeaveChairKitchenSoundKind leaveChairKitchenSoundKind = LeaveChairKitchenSoundKind.Random;
		int num = 0;
		switch ((!_isTakeCupcake) ? ((leaveChairKitchenSoundKind == LeaveChairKitchenSoundKind.Random) ? Random.Range(0, 2) : ((int)(leaveChairKitchenSoundKind - 1))) : 3)
		{
		case 0:
			base.Owner.MotionSoundController.PlayKitchenWaterOnly_1();
			break;
		case 1:
			base.Owner.MotionSoundController.PlayKitchenWashingCup_1();
			break;
		default:
			base.Owner.MotionSoundController.PlayFridgeSequence();
			break;
		}
	}

	public override bool IsPossibleClickReaction()
	{
		return false;
	}

	public override bool IsPossibleTalk()
	{
		return false;
	}

	public override void ToPossibleClickReaction()
	{
	}

	public override async UniTask ReadyFinishState(CancellationToken token)
	{
		_phase = Phase.None;
		_cts?.Cancel();
		_isTakeCupcake = false;
		LeaveChairData data = base.Owner.MasterDataLoader.HeroineAIMasterData.LeaveChairData;
		await UniTask.WaitUntil(() => Time.time - _startSeconds > data.SofaForceStaySeconds, PlayerLoopTiming.Update, token);
		await ComeBack(token);
		await UniTask.WaitUntil(() => base.Owner.HeroineService.IsCurrentAnimation(HeroineService.AnimationType.Base001.ToString()), PlayerLoopTiming.Update, token);
	}
}
