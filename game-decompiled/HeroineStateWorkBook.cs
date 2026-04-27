using System;
using Bulbul;
using MyUtil;
using UnityEngine;

public class HeroineStateWorkBook : HeroineBaseState
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
	[Header("理解の変動量")]
	private float UnderstandChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("アウトプットの変動量")]
	private float OutputChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("ループ2（体勢を変えて本を読む）の変動量")]
	private float Loop2ChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("飲み物を飲むの変動量")]
	private float DrinkTeaChangingAmount;

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

	[Header("理解\nアニメーション変更Delay")]
	[SerializeField]
	private float UnderstandDelaySecondsMin;

	[SerializeField]
	private float UnderstandDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float UnderstandSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float UnderstandInvalidSeconds;

	[Header("アウトプット\nアニメーション変更Delay")]
	[SerializeField]
	private float OutputDelaySecondsMin;

	[SerializeField]
	private float OutputDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float OutputSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float OutputInvalidSeconds;

	[Header("Loop2\nアニメーション変更Delay")]
	[SerializeField]
	private float Loop2DelaySecondsMin;

	[SerializeField]
	private float Loop2DelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float Loop2SubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float Loop2InvalidSeconds;

	[SerializeField]
	[Header("何秒ごとにページをめくるか: 最低値")]
	public float Loop2FlipPageDelaySecondsMin;

	[SerializeField]
	[Header("何秒ごとにページをめくるか: 最大値")]
	public float Loop2FlipPageDelaySecondsMax;

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

	[Space(20f)]
	[Header("調整確認用に表示しているパラメータ\n値が高いものに関連するモーションが再生されやすくなる")]
	[SerializeField]
	[Header("基本\u3000本を読む")]
	private float _baseAmount;

	[SerializeField]
	[Header("問題\u3000考える")]
	private float _problemAmount;

	[SerializeField]
	[Header("理解\u3000ページをめくる")]
	private float _understandAmount;

	[SerializeField]
	[Header("アウトプットタイピング")]
	private float _outputAmount;

	[SerializeField]
	[Header("ループ2（体勢を変えて本を読む）")]
	private float _loop2Amount;

	[SerializeField]
	[Header("飲み物を飲む")]
	private float _drinkTeaAmount;

	private MyStopWatch _animUpdateStopWatch = new MyStopWatch();

	private MyStopWatch _problemInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _understandInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _outputInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _loop2InvalidStopWatch = new MyStopWatch();

	private MyStopWatch _drinkTeaInvalidStopWatch = new MyStopWatch();

	private float _nextFlipPageDelaySeconds;

	private DateTime _lastFlipPageDateTime = DateTime.MinValue;

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.WorkBase002);
		_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
		_animUpdateStopWatch.Watch.Restart();
		_baseAmount = InitBaseAmount;
		_problemAmount = 0f;
		_understandAmount = 0f;
		_outputAmount = 0f;
		_loop2Amount = 0f;
		_drinkTeaAmount = 0f;
		_nextFlipPageDelaySeconds = 0f;
		_lastFlipPageDateTime = DateTime.MinValue;
	}

	protected override void OnUpdate()
	{
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		switch (_mainState)
		{
		case MainState.Idle:
			if (base.Owner.HeroineService.GetCurrentAnimationType() == HeroineService.AnimationType.WorkBase002_Loop2 && (DateTime.Now - _lastFlipPageDateTime).TotalSeconds >= (double)_nextFlipPageDelaySeconds)
			{
				base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.WorkBase002_Loop2_PageFlip);
				_lastFlipPageDateTime = DateTime.Now;
				_nextFlipPageDelaySeconds = UnityEngine.Random.Range(Loop2FlipPageDelaySecondsMin, Loop2FlipPageDelaySecondsMax);
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
		}
	}

	public void UpdateAnimation()
	{
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		if (base.Owner.HeroineService.IsEndAnimation())
		{
			HeroineService.AnimationType animationType = HeroineService.AnimationType.WorkBase002;
			if ((uint)(currentAnimationType - 250) <= 6u)
			{
				animationType = GetNextAnimation();
			}
			else
			{
				animationType = HeroineService.AnimationType.WorkBase002;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			base.Owner.HeroineService.ChangeAnimation(animationType);
			_animUpdateStopWatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			float maxInclusive = _baseAmount + _problemAmount + _understandAmount + _outputAmount + _loop2Amount + _drinkTeaAmount;
			float num = UnityEngine.Random.Range(0f, maxInclusive);
			HeroineService.AnimationType result;
			if (num < _baseAmount)
			{
				result = HeroineService.AnimationType.WorkBase002;
				float num2 = (_problemInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * ProblemChangingAmount) : 0f);
				float num3 = (_understandInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * UnderstandChangingAmount) : 0f);
				float num4 = (_outputInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * OutputChangingAmount) : 0f);
				float num5 = (_loop2InvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * Loop2ChangingAmount) : 0f);
				float num6 = (_drinkTeaInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * DrinkTeaChangingAmount) : 0f);
				_baseAmount -= num2 + num3 + num4;
				_problemAmount += num2;
				_understandAmount += num3;
				_outputAmount += num4;
				_loop2Amount += num5;
				_drinkTeaAmount += num6;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			else if (num < _baseAmount + _problemAmount)
			{
				result = HeroineService.AnimationType.WorkBase002_Thinking;
				CommonUpdateParameter(ref _problemAmount, ref _problemInvalidStopWatch, ProblemSubtractRatio, ProblemDelaySecondsMin, ProblemDelaySecondsMax, ProblemInvalidSeconds);
			}
			else if (num < _baseAmount + _problemAmount + _understandAmount)
			{
				result = HeroineService.AnimationType.WorkBase002_PageFlip;
				CommonUpdateParameter(ref _understandAmount, ref _understandInvalidStopWatch, UnderstandSubtractRatio, UnderstandDelaySecondsMin, UnderstandDelaySecondsMax, UnderstandInvalidSeconds);
			}
			else if (num < _baseAmount + _problemAmount + _understandAmount + _outputAmount)
			{
				result = HeroineService.AnimationType.WorkBase002_KeyType;
				CommonUpdateParameter(ref _outputAmount, ref _outputInvalidStopWatch, OutputSubtractRatio, OutputDelaySecondsMin, OutputDelaySecondsMax, OutputInvalidSeconds);
			}
			else if (num < _baseAmount + _problemAmount + _understandAmount + _outputAmount + _loop2Amount)
			{
				result = HeroineService.AnimationType.WorkBase002_Loop2;
				CommonUpdateParameter(ref _loop2Amount, ref _loop2InvalidStopWatch, Loop2SubtractRatio, Loop2DelaySecondsMin, Loop2DelaySecondsMax, Loop2InvalidSeconds);
				_lastFlipPageDateTime = DateTime.Now;
				_nextFlipPageDelaySeconds = UnityEngine.Random.Range(Loop2FlipPageDelaySecondsMin, Loop2FlipPageDelaySecondsMax);
			}
			else
			{
				result = HeroineService.AnimationType.WorkBase002_DrinkTea;
				CommonUpdateParameter(ref _drinkTeaAmount, ref _drinkTeaInvalidStopWatch, DrinkTeaSubtractRatio, DrinkTeaDelaySecondsMin, DrinkTeaDelaySecondsMax, DrinkTeaInvalidSeconds);
			}
			return result;
		}
	}

	private void CommonUpdateParameter(ref float targetAmount, ref MyStopWatch invalidStopWatch, float targetSubtractRatio, float animDelayMin, float animDelayMax, float invalidSeconds)
	{
		float num = targetAmount * targetSubtractRatio;
		targetAmount -= num;
		_baseAmount += num;
		_animUpdateStopWatch.ChangeTargetSecondsForRandom(animDelayMin, animDelayMax);
		invalidStopWatch.SetTargetSeconds(invalidSeconds);
		invalidStopWatch.Watch.Restart();
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		_problemInvalidStopWatch.Watch.Stop();
		_problemInvalidStopWatch.SetTargetSeconds(0f);
		_understandInvalidStopWatch.Watch.Stop();
		_understandInvalidStopWatch.SetTargetSeconds(0f);
		_outputInvalidStopWatch.Watch.Stop();
		_outputInvalidStopWatch.SetTargetSeconds(0f);
		_loop2InvalidStopWatch.Watch.Stop();
		_loop2InvalidStopWatch.SetTargetSeconds(0f);
		_drinkTeaInvalidStopWatch.Watch.Stop();
		_drinkTeaInvalidStopWatch.SetTargetSeconds(0f);
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
