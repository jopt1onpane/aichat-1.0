using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using MyUtil;
using UnityEngine;

public class HeroineStateBreakTeaTime : HeroineBaseState
{
	private enum MainState
	{
		Idle,
		UpdateMotion,
		ChangingMotion
	}

	private const string CoffeeMakerStartAnimName = "anim_break_tea_sub_maker_start";

	private const string CoffeeMakerLoopAnimName = "anim_break_tea_sub_maker_loop";

	private const string CoffeeMakerEndAnimName = "Wild001_Motion10_UseCoffeeMaker";

	private MainState _mainState;

	[Header("ベース再生時の変動量\u3000ベースの値から割合で各値に移す\n全部合わせて0～1の間になるようにする")]
	[SerializeField]
	[Range(0f, 1f)]
	[Header("飲み物を飲みたい")]
	public float WantDrinkAddRatio;

	[SerializeField]
	[Header("ベース\n初期値")]
	public float InitBaseAmount;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	public float BaseDelaySecondsMin;

	[SerializeField]
	public float BaseDelaySecondsMax;

	[SerializeField]
	[Header("お茶を飲む\n再生した時の減少割合")]
	[Range(0f, 1f)]
	public float DrinkSubtractRatio;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	public float DrinkDelaySecondsMin;

	[SerializeField]
	public float DrinkDelaySecondsMax;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float DrinkInvalidSeconds;

	[SerializeField]
	[Header("熱いお茶を飲む確率")]
	[Range(0f, 1f)]
	public float DrinkHotTeaProbability;

	[Header("ストレッチ\nお茶を飲むときの変動量\u3000お茶を飲むたびに何割ストレッチしたくなるか")]
	[SerializeField]
	[Range(0f, 1f)]
	[Header("ストレッチしたい")]
	public float StretchAddProbability;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	public float StretchDelaySecondsMin;

	[SerializeField]
	public float StretchDelaySecondsMax;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float StretchInvalidSeconds;

	[Space(20f)]
	[Header("調整確認用に表示しているパラメータ\n値が高いものに関連するモーションが再生されやすくなる")]
	[SerializeField]
	[Header("基本")]
	private float _baseAmount;

	[SerializeField]
	[Header("お茶を飲みたい")]
	private float _wantDrinkAmount;

	[SerializeField]
	[Header("ストレッチする確率0～1")]
	private float _stretchProbability;

	private MyStopWatch _animUpdateStopWatch = new MyStopWatch();

	private MyStopWatch _drinkInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _stretchInvalidStopWatch = new MyStopWatch();

	private bool _isUseStretchByNextDrink;

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.BreakBase004);
		_baseAmount = InitBaseAmount;
		_wantDrinkAmount = 0f;
		_stretchProbability = 0f;
	}

	protected override void OnUpdate()
	{
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		switch (_mainState)
		{
		case MainState.Idle:
			if (base.Owner.HeroineService.GetCurrentAnimationType() == HeroineService.AnimationType.BreakBase004_UseCoffeeMaker && (base.Owner.HeroineService.IsCurrentAnimation("anim_break_tea_sub_maker_start") || base.Owner.HeroineService.IsCurrentAnimation("anim_break_tea_sub_maker_loop") || base.Owner.HeroineService.IsCurrentAnimation("Wild001_Motion10_UseCoffeeMaker")))
			{
				base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.BreakBase004);
			}
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
		if (!_animUpdateStopWatch.IsElapsedTargetTime())
		{
			return;
		}
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		if (base.Owner.HeroineService.IsEndAnimation())
		{
			HeroineService.AnimationType animationType = HeroineService.AnimationType.BreakBase004;
			if ((uint)(currentAnimationType - 750) <= 3u || currentAnimationType == HeroineService.AnimationType.BreakBase004_CoolingDrinkTea)
			{
				animationType = GetNextAnimation();
			}
			else
			{
				animationType = HeroineService.AnimationType.BreakBase004;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			base.Owner.HeroineService.ChangeAnimation(animationType);
			_animUpdateStopWatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			HeroineService.AnimationType nextAnimation = HeroineService.AnimationType.BreakBase002;
			if (_isUseStretchByNextDrink)
			{
				StretchUpdate();
				return nextAnimation;
			}
			float maxInclusive = _baseAmount + _wantDrinkAmount;
			float num = Random.Range(0f, maxInclusive);
			if (num < _baseAmount)
			{
				BaseUpdate();
			}
			else if (num < _baseAmount + _wantDrinkAmount)
			{
				if (_drinkInvalidStopWatch.IsElapsedTargetTime())
				{
					DrinkTeaUpdate();
				}
				else
				{
					BaseUpdate();
				}
			}
			else if (_stretchInvalidStopWatch.IsElapsedTargetTime())
			{
				StretchUpdate();
			}
			else
			{
				BaseUpdate();
			}
			return nextAnimation;
			void BaseUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase004;
				float num2 = (_drinkInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * WantDrinkAddRatio) : 0f);
				_baseAmount -= num2;
				_wantDrinkAmount += num2;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			void DrinkTeaUpdate()
			{
				if (base.Owner.HeroineService.HeroineUseObjectController.Drinks.IsNeedPourDrinks())
				{
					nextAnimation = HeroineService.AnimationType.BreakBase004_UseCoffeeMaker;
				}
				else
				{
					if (base.Owner.HeroineService.HeroineUseObjectController.Drinks.IsHot())
					{
						if (Random.value <= DrinkHotTeaProbability)
						{
							nextAnimation = HeroineService.AnimationType.BreakBase004_DrinkHot;
						}
						else
						{
							nextAnimation = HeroineService.AnimationType.BreakBase004_CoolingDrinkTea;
						}
					}
					else
					{
						nextAnimation = HeroineService.AnimationType.BreakBase004_DrinkTea;
					}
					if (_stretchInvalidStopWatch.IsElapsedTargetTime())
					{
						_stretchProbability += StretchAddProbability;
						if (Random.value <= _stretchProbability)
						{
							_isUseStretchByNextDrink = true;
						}
					}
					float num2 = _wantDrinkAmount * DrinkSubtractRatio;
					_baseAmount += num2;
					_wantDrinkAmount -= num2;
					_drinkInvalidStopWatch.SetTargetSeconds(DrinkInvalidSeconds);
					_drinkInvalidStopWatch.Watch.Restart();
					_animUpdateStopWatch.ChangeTargetSecondsForRandom(DrinkDelaySecondsMin, DrinkDelaySecondsMax);
					base.Owner.HeroineService.HeroineUseObjectController.Drinks.Drink();
				}
			}
			void StretchUpdate()
			{
				_isUseStretchByNextDrink = false;
				nextAnimation = HeroineService.AnimationType.BreakBase004_Stretch;
				_stretchProbability = 0f;
				_stretchInvalidStopWatch.SetTargetSeconds(StretchInvalidSeconds);
				_stretchInvalidStopWatch.Watch.Restart();
			}
		}
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		_drinkInvalidStopWatch.Watch.Stop();
		_drinkInvalidStopWatch.SetTargetSeconds(0f);
		_stretchInvalidStopWatch.Watch.Stop();
		_stretchInvalidStopWatch.SetTargetSeconds(0f);
	}

	public override bool IsPossibleClickReaction()
	{
		if (IsNotDrinkTea())
		{
			return !base.Owner.HeroineService.HeroineUseObjectController.Drinks.IsPouringDrinks();
		}
		return false;
	}

	public override bool IsPossibleTalk()
	{
		if (IsNotDrinkTea())
		{
			return !base.Owner.HeroineService.HeroineUseObjectController.Drinks.IsPouringDrinks();
		}
		return false;
	}

	public override void ToPossibleClickReaction()
	{
	}

	public override async UniTask ReadyFinishState(CancellationToken token)
	{
		await UniTask.WaitUntil(() => IsNotDrinkTea() && !base.Owner.HeroineService.HeroineUseObjectController.Drinks.IsPouringDrinks());
	}

	private bool IsNotDrinkTea()
	{
		switch (base.Owner.HeroineService.GetCurrentAnimationType())
		{
		case HeroineService.AnimationType.BreakBase004_DrinkTea:
		case HeroineService.AnimationType.BreakBase004_DrinkHot:
		case HeroineService.AnimationType.BreakBase004_CoolingDrinkTea:
			return false;
		default:
			if (base.Owner.HeroineService.IsCurrentAnimation("anim_break_tea_sub_drink_start") || base.Owner.HeroineService.IsCurrentAnimation("anim_break_tea_sub_drink_loop") || base.Owner.HeroineService.IsCurrentAnimation("anim_break_tea_sub_hot") || base.Owner.HeroineService.IsCurrentAnimation("anim_break_tea_sub_drink_loop 1") || base.Owner.HeroineService.IsCurrentAnimation("anim_break_tea_sub_drink_end 1"))
			{
				return false;
			}
			return true;
		}
	}
}
