using Bulbul;
using MyUtil;
using UnityEngine;

public class HeroineStateWorkReport : HeroineBaseState
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
	[Header("小さい問題の変動量")]
	private float SmallProblemChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("大きい問題の変動量")]
	private float BigProblemChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("ループ2（レポート書く）の変動量")]
	private float Loop2ChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("飲み物を飲むの変動量")]
	private float DrinkTeaChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("消しカスを払うの変動量")]
	private float BrushAwayTrashChangingAmount;

	[SerializeField]
	[Header("ベース\n初期値")]
	public float InitBaseAmount;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	public float BaseDelaySecondsMin;

	[SerializeField]
	public float BaseDelaySecondsMax;

	[Header("小さい問題\nアニメーション変更Delay")]
	[SerializeField]
	private float SmallProblemDelaySecondsMin;

	[SerializeField]
	private float SmallProblemDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float SmallProblemSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float SmallProblemInvalidSeconds;

	[Header("大きい問題\nアニメーション変更Delay")]
	[SerializeField]
	private float BigProblemDelaySecondsMin;

	[SerializeField]
	private float BigProblemDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float BigProblemSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float BigProblemInvalidSeconds;

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

	[Header("消しカスを払う\nアニメーション変更Delay")]
	[SerializeField]
	private float BrushAwayTrashDelaySecondsMin;

	[SerializeField]
	private float BrushAwayTrashDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float BrushAwayTrashSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float BrushAwayTrashInvalidSeconds;

	[Space(20f)]
	[Header("調整確認用に表示しているパラメータ\n値が高いものに関連するモーションが再生されやすくなる")]
	[SerializeField]
	[Header("基本\u3000本を読む")]
	private float _baseAmount;

	[SerializeField]
	[Header("小さい問題\u3000少し考える")]
	private float _smallProblemAmount;

	[SerializeField]
	[Header("大きい問題\u3000深く考える")]
	private float _bigProblemAmount;

	[SerializeField]
	[Header("ループ2（レポート書く）")]
	private float _loop2Amount;

	[SerializeField]
	[Header("飲み物を飲む")]
	private float _drinkTeaAmount;

	[SerializeField]
	[Header("消しカスを払う")]
	private float _brushAwayTrashAmount;

	private MyStopWatch _animUpdateStopWatch = new MyStopWatch();

	private MyStopWatch _smallProblemInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _bigProblemInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _loop2InvalidStopWatch = new MyStopWatch();

	private MyStopWatch _drinkTeaInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _brushAwayTrashInvalidStopWatch = new MyStopWatch();

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.WorkBase003);
		_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
		_animUpdateStopWatch.Watch.Restart();
		_baseAmount = InitBaseAmount;
		_smallProblemAmount = 0f;
		_bigProblemAmount = 0f;
		_loop2Amount = 0f;
		_drinkTeaAmount = 0f;
		_brushAwayTrashAmount = 0f;
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
			HeroineService.AnimationType animationType = HeroineService.AnimationType.WorkBase003;
			if ((uint)(currentAnimationType - 300) <= 5u)
			{
				animationType = GetNextAnimation();
			}
			else
			{
				animationType = HeroineService.AnimationType.WorkBase003;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			base.Owner.HeroineService.ChangeAnimation(animationType);
			_animUpdateStopWatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			float maxInclusive = _baseAmount + _smallProblemAmount + _bigProblemAmount + _loop2Amount + _drinkTeaAmount + _brushAwayTrashAmount;
			float num = Random.Range(0f, maxInclusive);
			HeroineService.AnimationType nextAnimation = default(HeroineService.AnimationType);
			if (num < _baseAmount)
			{
				BaseUpdate();
			}
			else if (num < _baseAmount + _smallProblemAmount)
			{
				if (_smallProblemInvalidStopWatch.IsElapsedTargetTime())
				{
					SmallThinkingUpdate();
				}
				else
				{
					BaseUpdate();
				}
			}
			else if (num < _baseAmount + _smallProblemAmount + _bigProblemAmount)
			{
				if (_bigProblemInvalidStopWatch.IsElapsedTargetTime())
				{
					BigThinkingUpdate();
				}
				else
				{
					BaseUpdate();
				}
			}
			else if (num < _baseAmount + _smallProblemAmount + _bigProblemAmount + _loop2Amount)
			{
				Loop2Update();
			}
			else if (num < _baseAmount + _smallProblemAmount + _bigProblemAmount + _loop2Amount + _drinkTeaAmount)
			{
				DrinkUpdate();
			}
			else
			{
				BrushAwayTrashUpdate();
			}
			return nextAnimation;
			void BaseUpdate()
			{
				nextAnimation = HeroineService.AnimationType.WorkBase003;
				float num2 = (_smallProblemInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * SmallProblemChangingAmount) : 0f);
				float num3 = (_bigProblemInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * BigProblemChangingAmount) : 0f);
				float num4 = (_loop2InvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * Loop2ChangingAmount) : 0f);
				float num5 = (_drinkTeaInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * DrinkTeaChangingAmount) : 0f);
				float num6 = (_brushAwayTrashInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * BrushAwayTrashChangingAmount) : 0f);
				_baseAmount -= num2 + num3 + num4 + num5 + num6;
				_smallProblemAmount += num2;
				_bigProblemAmount += num3;
				_loop2Amount += num4;
				_drinkTeaAmount += num5;
				_brushAwayTrashAmount += num6;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			void BigThinkingUpdate()
			{
				nextAnimation = HeroineService.AnimationType.WorkBase003_BigThinking;
				CommonUpdateParameter(ref _bigProblemAmount, ref _bigProblemInvalidStopWatch, BigProblemSubtractRatio, BigProblemDelaySecondsMin, BigProblemDelaySecondsMax, BigProblemInvalidSeconds);
			}
			void BrushAwayTrashUpdate()
			{
				nextAnimation = HeroineService.AnimationType.WorkBase003_BrushAwayTrash;
				float num2 = _brushAwayTrashAmount * BrushAwayTrashSubtractRatio;
				_brushAwayTrashAmount -= num2;
				_baseAmount += num2;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BrushAwayTrashDelaySecondsMin, BrushAwayTrashDelaySecondsMax);
				_brushAwayTrashInvalidStopWatch.SetTargetSeconds(BrushAwayTrashInvalidSeconds);
				_brushAwayTrashInvalidStopWatch.Watch.Restart();
			}
			void DrinkUpdate()
			{
				nextAnimation = HeroineService.AnimationType.WorkBase003_DrinkTea;
				float num2 = _drinkTeaAmount * DrinkTeaSubtractRatio;
				_drinkTeaAmount -= num2;
				_baseAmount += num2;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(DrinkTeaDelaySecondsMin, DrinkTeaDelaySecondsMax);
				_drinkTeaInvalidStopWatch.SetTargetSeconds(DrinkTeaInvalidSeconds);
				_drinkTeaInvalidStopWatch.Watch.Restart();
			}
			void Loop2Update()
			{
				nextAnimation = HeroineService.AnimationType.WorkBase003_Loop2;
				float num2 = _loop2Amount * Loop2SubtractRatio;
				_loop2Amount -= num2;
				_baseAmount += num2;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(Loop2DelaySecondsMin, Loop2DelaySecondsMax);
				_loop2InvalidStopWatch.SetTargetSeconds(Loop2InvalidSeconds);
				_loop2InvalidStopWatch.Watch.Restart();
			}
			void SmallThinkingUpdate()
			{
				nextAnimation = HeroineService.AnimationType.WorkBase003_SmallThinking;
				CommonUpdateParameter(ref _smallProblemAmount, ref _smallProblemInvalidStopWatch, SmallProblemSubtractRatio, SmallProblemDelaySecondsMin, SmallProblemDelaySecondsMax, SmallProblemInvalidSeconds);
			}
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
		_smallProblemInvalidStopWatch.Watch.Stop();
		_smallProblemInvalidStopWatch.SetTargetSeconds(0f);
		_bigProblemInvalidStopWatch.Watch.Stop();
		_bigProblemInvalidStopWatch.SetTargetSeconds(0f);
		_loop2InvalidStopWatch.Watch.Stop();
		_loop2InvalidStopWatch.SetTargetSeconds(0f);
		_drinkTeaInvalidStopWatch.Watch.Stop();
		_drinkTeaInvalidStopWatch.SetTargetSeconds(0f);
		_brushAwayTrashInvalidStopWatch.Watch.Stop();
		_brushAwayTrashInvalidStopWatch.SetTargetSeconds(0f);
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
