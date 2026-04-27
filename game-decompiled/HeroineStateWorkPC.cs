using Bulbul;
using MyUtil;
using UnityEngine;

public class HeroineStateWorkPC : HeroineBaseState
{
	private enum MainState
	{
		Idle,
		UpdateMotion,
		ChangingMotion
	}

	private MainState _mainState;

	[Header("ベース再生時の変動量\u3000ベースの値から割合で各値に移す\n全部合わせて0～1の間になるようにする")]
	[SerializeField]
	[Range(0f, 1f)]
	[Header("問題の変動量")]
	private float ProblemChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("疲労の変動量")]
	private float FatigueChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("飲み物を飲むの変動量")]
	private float DrinkTeaChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("休憩の変動量")]
	private float BreakChangingAmount;

	[SerializeField]
	[Header("ベース\n初期値")]
	public float InitBaseAmount;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	public float BaseDelaySecondsMin;

	[SerializeField]
	public float BaseDelaySecondsMax;

	[Header("問題\nアニメーション変更Delay")]
	[SerializeField]
	private float ProblemDelaySecondsMin;

	[SerializeField]
	private float ProblemDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float ProblemSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float ProblemInvalidSeconds;

	[Header("疲労\nアニメーション変更Delay")]
	[SerializeField]
	private float FatigueDelaySecondsMin;

	[SerializeField]
	private float FatigueDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float FatigueSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float FatigueInvalidSeconds;

	[Header("飲み物を飲む\nアニメーション変更Delay")]
	[SerializeField]
	private float DrinkTeaDelaySecondsMin;

	[SerializeField]
	private float DrinkTeaDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float DrinkTeaSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float DrinkTeaInvalidSeconds;

	[Header("休憩したい\nアニメーション変更Delay")]
	[SerializeField]
	private float BreakDelaySecondsMin;

	[SerializeField]
	private float BreakDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float BreakSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float BreakInvalidSeconds;

	[SerializeField]
	[Header("ステート開始後何秒間無効化するか")]
	public float StateStartBreakInvalidSeconds;

	[Space(20f)]
	[Header("調整確認用に表示しているパラメータ\n値が高いものに関連するモーションが再生されやすくなる")]
	[SerializeField]
	[Header("基本")]
	private float _baseAmount;

	[SerializeField]
	[Header("問題")]
	private float _problemAmount;

	[SerializeField]
	[Header("疲労")]
	private float _fatigueAmount;

	[SerializeField]
	[Header("飲み物を飲む")]
	private float _drinkTeaAmount;

	[SerializeField]
	[Header("休憩")]
	private float _breakAmount;

	private HeroineService.AnimationType _beforeProblemAnimation;

	private MyStopWatch _animUpdateStopWatch = new MyStopWatch();

	private MyStopWatch _problemInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _fatigueInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _drinkTeaInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _breakInvalidStopWatch = new MyStopWatch();

	public override void OnRegisterToStateMachine()
	{
	}

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.WorkBase001);
		_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
		_animUpdateStopWatch.Watch.Restart();
		_baseAmount = InitBaseAmount;
		_problemAmount = 0f;
		_fatigueAmount = 0f;
		_drinkTeaAmount = 0f;
		_breakAmount = 0f;
		_breakInvalidStopWatch.SetTargetSeconds(StateStartBreakInvalidSeconds);
	}

	protected override void OnUpdate()
	{
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		switch (_mainState)
		{
		case MainState.Idle:
			if (_animUpdateStopWatch.IsElapsedTargetTime() && base.Owner.HeroineService.IsEndAnimation())
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
		}
	}

	public void UpdateAnimation()
	{
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		if (base.Owner.HeroineService.IsEndAnimation())
		{
			HeroineService.AnimationType animationType = HeroineService.AnimationType.Base001;
			if ((uint)(currentAnimationType - 200) <= 5u)
			{
				animationType = GetNextAnimation();
			}
			else
			{
				animationType = HeroineService.AnimationType.WorkBase001;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			base.Owner.HeroineService.ChangeAnimation(animationType);
			_animUpdateStopWatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			float maxInclusive = _baseAmount + _problemAmount + _fatigueAmount + _drinkTeaAmount + _breakAmount;
			float num = Random.Range(0f, maxInclusive);
			HeroineService.AnimationType result;
			if (num < _baseAmount)
			{
				result = HeroineService.AnimationType.WorkBase001;
				float num2 = (_problemInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * ProblemChangingAmount) : 0f);
				float num3 = (_fatigueInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * FatigueChangingAmount) : 0f);
				float num4 = (_drinkTeaInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * DrinkTeaChangingAmount) : 0f);
				float num5 = (_breakInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * BreakChangingAmount) : 0f);
				_baseAmount -= num2 + num3 + num5;
				_problemAmount += num2;
				_fatigueAmount += num3;
				_drinkTeaAmount += num4;
				_breakAmount += num5;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			else if (num < _baseAmount + _problemAmount)
			{
				result = (_beforeProblemAnimation = ((_beforeProblemAnimation == HeroineService.AnimationType.WorkBase001_Thinking) ? HeroineService.AnimationType.WorkBase001_Stop : HeroineService.AnimationType.WorkBase001_Thinking));
				float num6 = _problemAmount * ProblemSubtractRatio;
				_problemAmount -= num6;
				_baseAmount += num6;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(ProblemDelaySecondsMin, ProblemDelaySecondsMax);
				_problemInvalidStopWatch.SetTargetSeconds(ProblemInvalidSeconds);
				_problemInvalidStopWatch.Watch.Restart();
			}
			else if (num < _baseAmount + _problemAmount + _fatigueAmount)
			{
				result = HeroineService.AnimationType.WorkBase001_Stop;
				float num7 = _fatigueAmount * FatigueSubtractRatio;
				_fatigueAmount -= num7;
				_baseAmount += num7;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(FatigueDelaySecondsMin, FatigueDelaySecondsMax);
				_fatigueInvalidStopWatch.SetTargetSeconds(FatigueInvalidSeconds);
				_fatigueInvalidStopWatch.Watch.Restart();
			}
			else if (num < _baseAmount + _problemAmount + _fatigueAmount + _drinkTeaAmount)
			{
				result = HeroineService.AnimationType.WorkBase001_DrinkTea;
				float num8 = _drinkTeaAmount * DrinkTeaSubtractRatio;
				_drinkTeaAmount -= num8;
				_baseAmount += num8;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(DrinkTeaDelaySecondsMin, DrinkTeaDelaySecondsMax);
				_drinkTeaInvalidStopWatch.SetTargetSeconds(DrinkTeaInvalidSeconds);
				_drinkTeaInvalidStopWatch.Watch.Restart();
			}
			else
			{
				result = ((!(Random.value <= 0.5f)) ? HeroineService.AnimationType.WorkBase001_Break2 : HeroineService.AnimationType.WorkBase001_Break);
				float num9 = _breakAmount * BreakSubtractRatio;
				_breakAmount -= num9;
				_baseAmount += num9;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BreakDelaySecondsMin, BreakDelaySecondsMax);
				_breakInvalidStopWatch.SetTargetSeconds(BreakInvalidSeconds);
				_breakInvalidStopWatch.Watch.Restart();
			}
			return result;
		}
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		_problemInvalidStopWatch.Watch.Stop();
		_problemInvalidStopWatch.SetTargetSeconds(0f);
		_fatigueInvalidStopWatch.Watch.Stop();
		_fatigueInvalidStopWatch.SetTargetSeconds(0f);
		_drinkTeaInvalidStopWatch.Watch.Stop();
		_drinkTeaInvalidStopWatch.SetTargetSeconds(0f);
		_breakInvalidStopWatch.Watch.Stop();
		_breakInvalidStopWatch.SetTargetSeconds(0f);
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
