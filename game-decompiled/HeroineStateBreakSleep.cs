using System;
using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using MyUtil;
using UnityEngine;
using VContainer;

public class HeroineStateBreakSleep : HeroineBaseState
{
	private enum MainState
	{
		Idle,
		UpdateMotion,
		ChangingMotion,
		JumpUp1,
		JumpUp2,
		JumpUp3
	}

	private MainState _mainState;

	[Inject]
	private AchievementService _achievementService;

	[Header("ベース再生時の変動量\n100になったら眠る")]
	[SerializeField]
	[Range(0f, 100f)]
	[Header("うとうとしている時\n眠気増加割合")]
	public float SleepinessAddAmount;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	public float BaseDelaySecondsMin;

	[SerializeField]
	public float BaseDelaySecondsMax;

	[SerializeField]
	[Header("眠っている時\n眠気減少割合")]
	[Range(0f, 100f)]
	public float SleepinessSubtractAmount;

	[SerializeField]
	[Header("起きる可能性のある眠気の下限値")]
	[Range(0f, 100f)]
	public float CanSleepEndProbabilityMin;

	[SerializeField]
	[Header("起きる確率")]
	[Range(0f, 100f)]
	public int SleepEndProbability;

	[Space(20f)]
	[Header("調整確認用に表示しているパラメータ")]
	[SerializeField]
	[Header("眠気")]
	private float _sleepinessAmount;

	private MyStopWatch _animUpdateStopWatch = new MyStopWatch();

	private MyStopWatch _nextPageInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _previousPageInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _interestInvalidStopWatch = new MyStopWatch();

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.BreakBase005);
		_sleepinessAmount = 0f;
		base.Owner.InitSleepFlg();
		_animUpdateStopWatch.Watch.Restart();
		_mainState = MainState.Idle;
	}

	protected override void OnUpdate()
	{
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		switch (_mainState)
		{
		case MainState.Idle:
			if (base.Owner.IsSleeping && base.Owner.IsWantJumpUp)
			{
				base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.BreakBase005_JumpUp);
				_mainState = MainState.JumpUp1;
			}
			else if (_animUpdateStopWatch.IsElapsedTargetTime() && base.Owner.HeroineService.IsEndAnimation())
			{
				_mainState = MainState.UpdateMotion;
			}
			break;
		case MainState.UpdateMotion:
			UpdateAnimation();
			_mainState = MainState.ChangingMotion;
			break;
		case MainState.ChangingMotion:
			if (currentAnimationType == base.Owner.HeroineService.NextAnimation)
			{
				_mainState = MainState.Idle;
			}
			else if (currentAnimationType != base.Owner.HeroineService.BeforeChangeAnimation && base.Owner.HeroineService.IsEndAnimation())
			{
				_mainState = MainState.Idle;
			}
			break;
		case MainState.JumpUp1:
			if (base.Owner.HeroineService.IsEndAnimation("jumpwakeup_loop") && base.Owner.HeroineService.IsFinishedVoice)
			{
				JumpUpEnd().Forget();
				_mainState = MainState.JumpUp2;
			}
			break;
		case MainState.JumpUp2:
			if (base.Owner.HeroineService.IsEndAnimation("BreakBase005_JumpUp_End") && base.Owner.HeroineService.IsFinishedVoice)
			{
				base.Owner.WantChangeBreakAction();
				_mainState = MainState.JumpUp3;
			}
			break;
		case MainState.JumpUp3:
			break;
		}
		async UniTaskVoid JumpUpEnd()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.BreakBase005_JumpUp_End);
			_achievementService.OnEndWakeUpReaction();
		}
	}

	public void UpdateAnimation()
	{
		if (!_animUpdateStopWatch.IsElapsedTargetTime())
		{
			return;
		}
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		if (base.Owner.HeroineService.IsEndAnimation())
		{
			HeroineService.AnimationType animationType = HeroineService.AnimationType.BreakBase005;
			if ((uint)(currentAnimationType - 800) <= 3u)
			{
				animationType = GetNextAnimation();
			}
			else
			{
				animationType = HeroineService.AnimationType.BreakBase005;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			base.Owner.HeroineService.ChangeAnimation(animationType);
			_animUpdateStopWatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			HeroineService.AnimationType result = HeroineService.AnimationType.BreakBase005;
			if (base.Owner.IsSleeping)
			{
				_sleepinessAmount = Mathf.Clamp(_sleepinessAmount - SleepinessSubtractAmount, 0f, 100f);
				if (_sleepinessAmount < CanSleepEndProbabilityMin && UnityEngine.Random.Range(1, 101) <= SleepEndProbability)
				{
					base.Owner.WantChangeBreakAction();
				}
			}
			else
			{
				_sleepinessAmount = Mathf.Clamp(_sleepinessAmount + SleepinessAddAmount, 0f, 100f);
				if (_sleepinessAmount >= 100f)
				{
					result = HeroineService.AnimationType.BreakBase005_Sleep;
					base.Owner.ActivateSleepFlg();
				}
			}
			_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			return result;
		}
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		base.Owner.WantChangeBreakAction();
		base.Owner.InitSleepFlg();
		_nextPageInvalidStopWatch.Watch.Stop();
		_nextPageInvalidStopWatch.SetTargetSeconds(0f);
		_previousPageInvalidStopWatch.Watch.Stop();
		_previousPageInvalidStopWatch.SetTargetSeconds(0f);
	}

	public override bool IsPossibleClickReaction()
	{
		if (base.Owner.IsSleeping && !base.Owner.IsWantJumpUp && base.Owner.HeroineService.IsCurrentAnimation("anim_break_sleep_sub_fall_loop"))
		{
			return true;
		}
		return false;
	}

	public override bool IsPossibleTalk()
	{
		if (base.Owner.IsSleeping && !base.Owner.IsWantJumpUp && base.Owner.HeroineService.IsCurrentAnimation("anim_break_sleep_sub_fall_loop"))
		{
			return true;
		}
		return false;
	}

	public override void ToPossibleClickReaction()
	{
	}

	public override UniTask ReadyFinishState(CancellationToken token)
	{
		return UniTask.CompletedTask;
	}
}
