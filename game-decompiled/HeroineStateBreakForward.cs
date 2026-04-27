using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using MyUtil;
using UnityEngine;

public class HeroineStateBreakForward : HeroineBaseState
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
	[Header("次へ行く変動割合")]
	public float NextAddRatio;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("ハラハラの変動量")]
	public float SuspensefulAddRatio;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("ペン回しの変動量")]
	public float PlayPenAddRatio;

	[SerializeField]
	[Header("ベース\n初期値")]
	public float InitBaseAmount;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	public float BaseDelaySecondsMin;

	[SerializeField]
	public float BaseDelaySecondsMax;

	[Header("次へ行く\nアニメーション変更Delay")]
	public float ToNextDelaySecondsMin;

	[SerializeField]
	public float NextDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float NextSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float NextInvalidSeconds;

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

	[Header("ペン回し\nアニメーション変更Delay")]
	[SerializeField]
	public float PlayPenDelaySecondsMin;

	[SerializeField]
	public float PlayPenDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float PlayPenSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float PlayPenInvalidSeconds;

	[SerializeField]
	[Header("ペン回し失敗確率")]
	[Range(0f, 1f)]
	public float PlayPenFailProbability;

	[SerializeField]
	[Header("ペン回しに入った後、続けて行う回数（最小）")]
	public int PlayPenLoopCountMin = 10;

	[SerializeField]
	[Header("ペン回しに入った後、続けて行う回数（最大）")]
	public int PlayPenLoopCountMax = 20;

	[Space(20f)]
	[Header("調整確認用に表示しているパラメータ\n値が高いものに関連するモーションが再生されやすくなる")]
	[SerializeField]
	[Header("基本")]
	private float _baseAmount;

	[SerializeField]
	[Header("次へ進む")]
	private float _nextAmount;

	[SerializeField]
	[Header("驚き")]
	private float _suspensefulAmount;

	[SerializeField]
	[Header("ペン回し")]
	private float _playPenAmount;

	private MyStopWatch _animUpdateStopWatch = new MyStopWatch();

	private MyStopWatch _nextInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _suspensefulInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _playPenInvalidStopWatch = new MyStopWatch();

	private int _playPenLoopRemainCount;

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.BreakBase006);
		_baseAmount = InitBaseAmount;
		_nextAmount = 0f;
		_suspensefulAmount = 0f;
		_playPenAmount = 0f;
		_playPenLoopRemainCount = 0;
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
			HeroineService.AnimationType animationType = HeroineService.AnimationType.BreakBase006;
			if ((uint)(currentAnimationType - 850) <= 4u)
			{
				animationType = GetNextAnimation();
			}
			else
			{
				animationType = HeroineService.AnimationType.BreakBase006;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			base.Owner.HeroineService.ChangeAnimation(animationType);
			_animUpdateStopWatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			HeroineService.AnimationType nextAnimation = HeroineService.AnimationType.BreakBase006;
			if (_playPenLoopRemainCount > 0)
			{
				PlayPenUpdate();
				_playPenLoopRemainCount--;
				if (nextAnimation == HeroineService.AnimationType.BreakBase006_DropPen)
				{
					_playPenLoopRemainCount = 0;
				}
				return nextAnimation;
			}
			float maxInclusive = _baseAmount + _nextAmount + _suspensefulAmount + _playPenAmount;
			float num = Random.Range(0f, maxInclusive);
			if (num < _baseAmount)
			{
				BaseUpdate();
			}
			else if (num < _baseAmount + _nextAmount)
			{
				if (_nextInvalidStopWatch.IsElapsedTargetTime())
				{
					NextMotionUpdate();
				}
				else
				{
					BaseUpdate();
				}
			}
			else if (num < _baseAmount + _nextAmount + _suspensefulAmount)
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
			else if (num < _baseAmount + _nextAmount + _suspensefulAmount + _playPenAmount)
			{
				if (_playPenInvalidStopWatch.IsElapsedTargetTime())
				{
					PlayPenUpdate();
					if (nextAnimation != HeroineService.AnimationType.BreakBase006_DropPen)
					{
						int num2 = Random.Range(PlayPenLoopCountMin, PlayPenLoopCountMax + 1);
						_playPenLoopRemainCount = num2 - 1;
					}
				}
				else
				{
					BaseUpdate();
				}
			}
			else
			{
				BaseUpdate();
			}
			return nextAnimation;
			void BaseUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase006;
				float num3 = (_nextInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * NextAddRatio) : 0f);
				float num4 = (_suspensefulInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * SuspensefulAddRatio) : 0f);
				float num5 = (_playPenInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * PlayPenAddRatio) : 0f);
				_baseAmount -= num3 + num4 + num5;
				_nextAmount += num3;
				_suspensefulAmount += num4;
				_playPenAmount += num5;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			void NextMotionUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase006_Keyboard;
				float num3 = _nextAmount * NextSubtractRatio;
				_baseAmount += num3;
				_nextAmount -= num3;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(ToNextDelaySecondsMin, NextDelaySecondsMax);
				_nextInvalidStopWatch.SetTargetSeconds(NextInvalidSeconds);
				_nextInvalidStopWatch.Watch.Restart();
			}
			void PlayPenUpdate()
			{
				if (Random.value < PlayPenFailProbability)
				{
					nextAnimation = HeroineService.AnimationType.BreakBase006_DropPen;
				}
				else
				{
					nextAnimation = HeroineService.AnimationType.BreakBase006_PlayPenLoop;
				}
				float num3 = _playPenAmount * PlayPenSubtractRatio;
				_playPenAmount -= num3;
				_baseAmount += num3;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(PlayPenDelaySecondsMin, PlayPenDelaySecondsMax);
				_playPenInvalidStopWatch.SetTargetSeconds(PlayPenInvalidSeconds);
				_playPenInvalidStopWatch.Watch.Restart();
			}
			void SuspensefulUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase006_Interest;
				float num3 = _suspensefulAmount * SuspensefulSubtractRatio;
				_suspensefulAmount -= num3;
				_baseAmount += num3;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(SuspensefulDelaySecondsMin, SuspensefulDelaySecondsMax);
				_suspensefulInvalidStopWatch.SetTargetSeconds(SuspensefulInvalidSeconds);
				_suspensefulInvalidStopWatch.Watch.Restart();
			}
		}
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		_nextInvalidStopWatch.Watch.Stop();
		_nextInvalidStopWatch.SetTargetSeconds(0f);
		_suspensefulInvalidStopWatch.Watch.Stop();
		_suspensefulInvalidStopWatch.SetTargetSeconds(0f);
		_playPenInvalidStopWatch.Watch.Stop();
		_playPenInvalidStopWatch.SetTargetSeconds(0f);
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
