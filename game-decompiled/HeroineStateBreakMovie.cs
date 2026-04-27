using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using MyUtil;
using UnityEngine;

public class HeroineStateBreakMovie : HeroineBaseState
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
	[Header("楽しい感情の変動割合")]
	public float EnjoyAddRatio;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("ハラハラの変動量")]
	public float SuspensefulAddRatio;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("ゴミが気になる変動量")]
	public float ToMindTrashAddRatio;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("ループ2（椅子を動かす）の変動量")]
	private float Loop2ChangingAmount;

	[SerializeField]
	[Header("ベース\n初期値")]
	public float InitBaseAmount;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	public float BaseDelaySecondsMin;

	[SerializeField]
	public float BaseDelaySecondsMax;

	[Header("笑う\nアニメーション変更Delay")]
	public float EnjoyDelaySecondsMin;

	[SerializeField]
	public float EnjoyDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float EnjoySubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float EnjoyInvalidSeconds;

	[SerializeField]
	[Header("ハラハラ(suspenseful)\nアニメーション変更Delay")]
	public float SuspensefulDelaySecondsMin;

	[SerializeField]
	public float SuspensefulDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float SuspensefulSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float SuspensefulInvalidSeconds;

	[SerializeField]
	[Header("ゴミが気になる\n再生した時の減少割合")]
	[Range(0f, 1f)]
	public float ToMindTrashSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float ToMindTrashInvalidSeconds;

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

	[Space(20f)]
	[Header("調整確認用に表示しているパラメータ\n値が高いものに関連するモーションが再生されやすくなる")]
	[SerializeField]
	[Header("基本")]
	private float _baseAmount;

	[SerializeField]
	[Header("楽しさ")]
	private float _enjoyAmount;

	[SerializeField]
	[Header("驚き")]
	private float _suspensefulAmount;

	[SerializeField]
	[Header("ゴミが気になる")]
	private float _toMindTrashAmount;

	[SerializeField]
	[Header("ループ2（体勢を変えて本を読む）")]
	private float _loop2Amount;

	private MyStopWatch _animUpdateStopWatch = new MyStopWatch();

	private MyStopWatch _enjoyInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _suspensefulInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _toMindTrashInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _loop2InvalidStopWatch = new MyStopWatch();

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.BreakBase001);
		_baseAmount = InitBaseAmount;
		_enjoyAmount = 0f;
		_suspensefulAmount = 0f;
		_loop2Amount = 0f;
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
		if (!_animUpdateStopWatch.IsElapsedTargetTime())
		{
			return;
		}
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		if (base.Owner.HeroineService.IsEndAnimation())
		{
			HeroineService.AnimationType animationType = HeroineService.AnimationType.BreakBase001;
			if ((uint)(currentAnimationType - 600) <= 4u)
			{
				animationType = GetNextAnimation();
			}
			else
			{
				animationType = HeroineService.AnimationType.BreakBase001;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			base.Owner.HeroineService.ChangeAnimation(animationType);
			_animUpdateStopWatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			float maxInclusive = _baseAmount + _enjoyAmount + _suspensefulAmount + _toMindTrashAmount + _loop2Amount;
			float num = Random.Range(0f, maxInclusive);
			HeroineService.AnimationType nextAnimation = HeroineService.AnimationType.BreakBase001;
			if (num < _baseAmount)
			{
				BaseUpdate();
			}
			else if (num < _baseAmount + _enjoyAmount)
			{
				if (_enjoyInvalidStopWatch.IsElapsedTargetTime())
				{
					EnjoyUpdate();
				}
				else
				{
					BaseUpdate();
				}
			}
			else if (num < _baseAmount + _enjoyAmount + _suspensefulAmount)
			{
				if (_suspensefulInvalidStopWatch.IsElapsedTargetTime())
				{
					SuspensefulUpdate();
				}
				else
				{
					BaseUpdate();
				}
			}
			else if (num < _baseAmount + _enjoyAmount + _suspensefulAmount + _toMindTrashAmount)
			{
				if (_toMindTrashInvalidStopWatch.IsElapsedTargetTime())
				{
					ToMindTrashUpdate();
				}
				else
				{
					BaseUpdate();
				}
			}
			else
			{
				Loop2Update();
			}
			return nextAnimation;
			void BaseUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase001;
				float num2 = (_enjoyInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * EnjoyAddRatio) : 0f);
				float num3 = (_suspensefulInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * SuspensefulAddRatio) : 0f);
				float num4 = (_toMindTrashInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * ToMindTrashAddRatio) : 0f);
				float num5 = (_loop2InvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * Loop2ChangingAmount) : 0f);
				_baseAmount -= num2 + num3 + num4;
				_enjoyAmount += num2;
				_suspensefulAmount += num3;
				_toMindTrashAmount += num4;
				_loop2Amount += num5;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			void EnjoyUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase001_Laugh;
				float num2 = _enjoyAmount * EnjoySubtractRatio;
				_baseAmount += num2;
				_enjoyAmount -= num2;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(EnjoyDelaySecondsMin, EnjoyDelaySecondsMax);
				_enjoyInvalidStopWatch.SetTargetSeconds(EnjoyInvalidSeconds);
				_enjoyInvalidStopWatch.Watch.Restart();
			}
			void Loop2Update()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase001_Loop2;
				float num2 = _loop2Amount * Loop2SubtractRatio;
				_loop2Amount -= num2;
				_baseAmount += num2;
				_loop2InvalidStopWatch.SetTargetSeconds(Loop2InvalidSeconds);
				_loop2InvalidStopWatch.Watch.Restart();
			}
			void SuspensefulUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase001_Suspenseful;
				float num2 = _suspensefulAmount * SuspensefulSubtractRatio;
				_suspensefulAmount -= num2;
				_baseAmount += num2;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(SuspensefulDelaySecondsMin, SuspensefulDelaySecondsMax);
				_suspensefulInvalidStopWatch.SetTargetSeconds(SuspensefulInvalidSeconds);
				_suspensefulInvalidStopWatch.Watch.Restart();
			}
			void ToMindTrashUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase001_OutTrash;
				float num2 = _toMindTrashAmount * ToMindTrashSubtractRatio;
				_toMindTrashAmount -= num2;
				_baseAmount += num2;
				_toMindTrashInvalidStopWatch.SetTargetSeconds(ToMindTrashInvalidSeconds);
				_toMindTrashInvalidStopWatch.Watch.Restart();
			}
		}
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		_enjoyInvalidStopWatch.Watch.Stop();
		_enjoyInvalidStopWatch.SetTargetSeconds(0f);
		_suspensefulInvalidStopWatch.Watch.Stop();
		_suspensefulInvalidStopWatch.SetTargetSeconds(0f);
	}

	public override bool IsPossibleClickReaction()
	{
		return true;
	}

	public override void ToPossibleClickReaction()
	{
	}

	public override bool IsPossibleTalk()
	{
		return true;
	}

	public override UniTask ReadyFinishState(CancellationToken token)
	{
		return UniTask.CompletedTask;
	}
}
