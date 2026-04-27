using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using MyUtil;
using UnityEngine;

public class HeroineStateBreakReadBook : HeroineBaseState
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
	[Header("次のページに進みたい")]
	public float WantNextPageAddRatio;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("前のページに戻りたい")]
	public float WantPreviousPageAddRatio;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("関心")]
	public float InterestAddRatio;

	[SerializeField]
	[Header("ベース\n初期値")]
	public float InitBaseAmount;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	public float BaseDelaySecondsMin;

	[SerializeField]
	public float BaseDelaySecondsMax;

	[SerializeField]
	[Header("ページ変更\nアニメーション変更Delay")]
	public float PageChangedDelaySeconds;

	[SerializeField]
	[Header("次のページに進む\n再生した時の減少割合")]
	[Range(0f, 1f)]
	public float NextPageSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float NextPageInvalidSeconds;

	[SerializeField]
	[Header("前のページに戻る\n再生した時の減少割合")]
	[Range(0f, 1f)]
	public float PreviousPageSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float PreviousInvalidSeconds;

	[SerializeField]
	[Header("関心\nアニメーション変更Delay")]
	public float InterestDelaySecondsMin;

	[SerializeField]
	public float InterestDelaySecondsMax;

	[SerializeField]
	[Header("再生した時の減少割合")]
	[Range(0f, 1f)]
	public float InterestSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	public float InterestInvalidSeconds;

	[Space(20f)]
	[Header("調整確認用に表示しているパラメータ\n値が高いものに関連するモーションが再生されやすくなる")]
	[SerializeField]
	[Header("基本")]
	private float _baseAmount;

	[SerializeField]
	[Header("次のページに進みたい")]
	private float _wantNextPageAmount;

	[SerializeField]
	[Header("前のページに戻りたい")]
	private float _wantPreviousPageAmount;

	[SerializeField]
	[Header("関心")]
	private float _interestAmount;

	private MyStopWatch _animUpdateStopWatch = new MyStopWatch();

	private MyStopWatch _nextPageInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _previousPageInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _interestInvalidStopWatch = new MyStopWatch();

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.BreakBase002);
		_baseAmount = InitBaseAmount;
		_wantNextPageAmount = 0f;
		_wantPreviousPageAmount = 0f;
		_interestAmount = 0f;
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
			HeroineService.AnimationType animationType = HeroineService.AnimationType.BreakBase002;
			if ((uint)(currentAnimationType - 650) <= 3u)
			{
				animationType = GetNextAnimation();
			}
			else
			{
				animationType = HeroineService.AnimationType.BreakBase002;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			base.Owner.HeroineService.ChangeAnimation(animationType);
			_animUpdateStopWatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			float maxInclusive = _baseAmount + _wantNextPageAmount + _wantPreviousPageAmount + _interestAmount;
			float num = Random.Range(0f, maxInclusive);
			HeroineService.AnimationType nextAnimation = HeroineService.AnimationType.BreakBase002;
			if (num < _baseAmount)
			{
				BaseUpdate();
			}
			else if (num < _baseAmount + _wantNextPageAmount)
			{
				if (_nextPageInvalidStopWatch.IsElapsedTargetTime())
				{
					NextPageUpdate();
				}
				else
				{
					BaseUpdate();
				}
			}
			else if (num < _baseAmount + _wantNextPageAmount + _wantPreviousPageAmount)
			{
				if (_previousPageInvalidStopWatch.IsElapsedTargetTime())
				{
					PreviousPageUpdate();
				}
				else
				{
					BaseUpdate();
				}
			}
			else if (_interestInvalidStopWatch.IsElapsedTargetTime())
			{
				InterestUpdate();
			}
			else
			{
				BaseUpdate();
			}
			return nextAnimation;
			void BaseUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase002;
				float num2 = (_nextPageInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * WantNextPageAddRatio) : 0f);
				float num3 = (_previousPageInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * WantPreviousPageAddRatio) : 0f);
				float num4 = (_interestInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * InterestAddRatio) : 0f);
				_baseAmount -= num2 + num3 + num4;
				_wantNextPageAmount += num2;
				_wantPreviousPageAmount += num3;
				_interestAmount += num4;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(BaseDelaySecondsMin, BaseDelaySecondsMax);
			}
			void InterestUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase002_Interest;
				float num2 = _interestAmount * InterestSubtractRatio;
				_interestAmount -= num2;
				_baseAmount += num2;
				_interestInvalidStopWatch.ChangeTargetSecondsForRandom(InterestDelaySecondsMin, InterestDelaySecondsMax);
				_interestInvalidStopWatch.Watch.Restart();
			}
			void NextPageUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase002_NextPage;
				float num2 = _wantNextPageAmount * NextPageSubtractRatio;
				_baseAmount += num2;
				_wantNextPageAmount -= num2;
				_animUpdateStopWatch.SetTargetSeconds(PageChangedDelaySeconds);
				_nextPageInvalidStopWatch.SetTargetSeconds(NextPageInvalidSeconds);
				_nextPageInvalidStopWatch.Watch.Restart();
			}
			void PreviousPageUpdate()
			{
				nextAnimation = HeroineService.AnimationType.BreakBase002_PreviousPage;
				float num2 = _wantPreviousPageAmount * PreviousPageSubtractRatio;
				_wantPreviousPageAmount -= num2;
				_baseAmount += num2;
				_animUpdateStopWatch.SetTargetSeconds(PageChangedDelaySeconds);
				_previousPageInvalidStopWatch.SetTargetSeconds(PreviousInvalidSeconds);
				_previousPageInvalidStopWatch.Watch.Restart();
			}
		}
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		_nextPageInvalidStopWatch.Watch.Stop();
		_nextPageInvalidStopWatch.SetTargetSeconds(0f);
		_previousPageInvalidStopWatch.Watch.Stop();
		_previousPageInvalidStopWatch.SetTargetSeconds(0f);
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
