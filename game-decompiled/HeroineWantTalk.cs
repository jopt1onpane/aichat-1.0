using Bulbul;
using MyUtil;
using R3;
using UnityEngine;
using VContainer;

public class HeroineWantTalk : HeroineBaseState
{
	private enum MainState
	{
		Idle,
		UpdateMotion,
		ChangingMotion
	}

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private IWantTalkUI _wantTalkUI;

	[Inject]
	private IPlayerLevelDirectionState _playerLevelDirectionState;

	private MainState _mainState;

	[SerializeField]
	[Header("ベース\n初期値")]
	private float _initBaseAmount;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	private float _baseDelaySecondsMin;

	[SerializeField]
	private float _baseDelaySecondsMax;

	[SerializeField]
	[Header("手を振る行動のパラメータ")]
	private StateActionSelectParameter _waveSelectParam;

	[SerializeField]
	[Header("前のめりになる行動のパラメータ")]
	private StateActionSelectParameter _leaningSelectParam;

	[SerializeField]
	[Header("前のめり再生秒数最低値")]
	public float _playLeaningSecondsMin;

	[SerializeField]
	[Header("前のめり再生秒数最大値")]
	public float _playLeaningSecondsMax;

	[Space(20f)]
	[Header("調整確認用")]
	[SerializeField]
	[Header("基本モーションの選択される可能性")]
	private float _baseAmount;

	private MyStopWatch _animUpdateStopWatch = new MyStopWatch();

	private Subject<Unit> _onStartWantTalk = new Subject<Unit>();

	private Subject<Unit> _onEndWantTalk = new Subject<Unit>();

	public Observable<Unit> OnStartWantTalk => _onStartWantTalk;

	public Observable<Unit> OnEndWantTalk => _onEndWantTalk;

	public override void OnRegisterToStateMachine()
	{
		_wantTalkUI.Setup();
		_baseAmount = _initBaseAmount;
		_waveSelectParam.InitAll();
		_leaningSelectParam.InitAll();
	}

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		UpdateAnimation();
		UpdateShowWantTalkUI();
		_onStartWantTalk.OnNext(Unit.Default);
	}

	protected override void OnUpdate()
	{
		UpdateShowWantTalkUI();
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		if (currentAnimationType != HeroineService.AnimationType.WantTalk_Base_001 && currentAnimationType != HeroineService.AnimationType.WantTalk_Base_001_WaveHandShortTime && currentAnimationType != HeroineService.AnimationType.WantTalk_Base_001_LeaningForward)
		{
			base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.WantTalk_Base_001);
			UpdateAnimation();
		}
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

	public void UpdateShowWantTalkUI()
	{
		if (_wantTalkUI.IsActive())
		{
			return;
		}
		if (_playerLevelDirectionState.IsCurrentLevelUpDirection && !_directionService.SlideFadeAnnounce.IsPossibleWantTalkAnnounce)
		{
			if (base.Owner.HeroineService.IsCurrentAnimation(HeroineService.AnimationType.WantTalk_Base_001.ToString()) || base.Owner.HeroineService.IsCurrentAnimation(HeroineService.AnimationType.WantTalk_Base_001_WaveHandShortTime.ToString()) || base.Owner.HeroineService.IsCurrentAnimation(HeroineService.AnimationType.WantTalk_Base_001_LeaningForward.ToString()))
			{
				_wantTalkUI.Activate();
			}
		}
		else if (!_playerLevelDirectionState.IsCurrentLevelUpDirection && (base.Owner.HeroineService.IsCurrentAnimation(HeroineService.AnimationType.WantTalk_Base_001.ToString()) || base.Owner.HeroineService.IsCurrentAnimation(HeroineService.AnimationType.WantTalk_Base_001_WaveHandShortTime.ToString()) || base.Owner.HeroineService.IsCurrentAnimation(HeroineService.AnimationType.WantTalk_Base_001_LeaningForward.ToString())))
		{
			_wantTalkUI.Activate();
		}
	}

	public void UpdateAnimation()
	{
		base.Owner.HeroineService.GetCurrentAnimationType();
		if (base.Owner.HeroineService.IsEndAnimation())
		{
			HeroineService.AnimationType animationType = GetNextAnimation();
			switch (animationType)
			{
			case HeroineService.AnimationType.WantTalk_Base_001:
			case HeroineService.AnimationType.WantTalk_Base_001_WaveHandShortTime:
				base.Owner.HeroineService.ChangeLookScaleAnimation(1f, 1f);
				break;
			case HeroineService.AnimationType.WantTalk_Base_001_LeaningForward:
				base.Owner.HeroineService.ChangeLookScaleAnimation(0f, 1f);
				break;
			}
			base.Owner.HeroineService.ChangeAnimation(animationType);
			_animUpdateStopWatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			float maxInclusive = _baseAmount + _waveSelectParam._possibilityAmount + _leaningSelectParam._possibilityAmount;
			float num = Random.Range(0f, maxInclusive);
			HeroineService.AnimationType animationType2 = HeroineService.AnimationType.WantTalk_Base_001;
			if (num < _baseAmount)
			{
				animationType2 = HeroineService.AnimationType.WantTalk_Base_001;
				float num2 = _waveSelectParam.UpdatePossibilityAmount(_baseAmount);
				float num3 = _leaningSelectParam.UpdatePossibilityAmount(_baseAmount);
				_baseAmount -= num2 + num3;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(_baseDelaySecondsMin, _baseDelaySecondsMax);
			}
			else if (num < _baseAmount + _waveSelectParam._possibilityAmount)
			{
				animationType2 = HeroineService.AnimationType.WantTalk_Base_001_WaveHandShortTime;
				float num4 = _waveSelectParam.UseAction();
				_baseAmount += num4;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(_baseDelaySecondsMin, _baseDelaySecondsMax);
			}
			else
			{
				animationType2 = HeroineService.AnimationType.WantTalk_Base_001_LeaningForward;
				float num5 = _leaningSelectParam.UseAction();
				_baseAmount += num5;
				_animUpdateStopWatch.ChangeTargetSecondsForRandom(_playLeaningSecondsMin, _playLeaningSecondsMax);
			}
			return animationType2;
		}
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		_onEndWantTalk.OnNext(Unit.Default);
		_wantTalkUI.Deactivate();
		base.Owner.HeroineService.ChangeLookScaleAnimation(0f, 1f);
		_waveSelectParam.InitStopWatch();
		_leaningSelectParam.InitStopWatch();
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
