using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtil;
using UnityEngine;

namespace Bulbul;

public class HeroineStateLeaveChairGoSofa : HeroineBaseState
{
	private enum Phase
	{
		None,
		GoToSofa,
		Stay,
		Comeback
	}

	private const string LoopAnimName = "anim_wild_leavechair_sofa_loop";

	[Header("ベース再生時の変動量\u3000ベースの値から割合で各値に移す\n全部合わせて0～1の間になるようにする")]
	[SerializeField]
	[Range(0f, 1f)]
	[Header("ベースから思考へ移す割合")]
	private float _thinkingChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("ベースから外を見るへ移す割合")]
	private float _lookOutsideChangingAmount;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("ベースからヘッドホンへ移す割合")]
	private float _controlHeadphoneChangingAmount;

	[Header("ソファ滞在中モーション切り替えのベース設定")]
	[SerializeField]
	[Header("初期ベース量")]
	private float _initBaseAmount;

	[SerializeField]
	[Header("モーション更新の待ち時間（秒）")]
	private float _baseDelaySecondsMin;

	[SerializeField]
	private float _baseDelaySecondsMax;

	[Header("思考（少し考える）")]
	[SerializeField]
	[Header("モーション更新の待ち時間最小（秒）")]
	private float _thinkingDelaySecondsMin;

	[SerializeField]
	[Header("モーション更新の待ち時間最大（秒）")]
	private float _thinkingDelaySecondsMax;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("再生した時の減少割合")]
	private float _thinkingSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	private float _thinkingInvalidSeconds;

	[Header("外を見る")]
	[SerializeField]
	[Header("モーション更新の待ち時間最小（秒）")]
	private float _lookOutsideDelaySecondsMin;

	[SerializeField]
	[Header("モーション更新の待ち時間最大（秒）")]
	private float _lookOutsideDelaySecondsMax;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("再生した時の減少割合")]
	private float _lookOutsideSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	private float _lookOutsideInvalidSeconds;

	[Header("ヘッドホンを触る")]
	[SerializeField]
	[Header("モーション更新の待ち時間最小（秒）")]
	private float _controlHeadphoneDelaySecondsMin;

	[SerializeField]
	[Header("モーション更新の待ち時間最大（秒）")]
	private float _controlHeadphoneDelaySecondsMax;

	[SerializeField]
	[Range(0f, 1f)]
	[Header("再生した時の減少割合")]
	private float _controlHeadphoneSubtractRatio;

	[SerializeField]
	[Header("再生した後何秒間無効化するか")]
	private float _controlHeadphoneInvalidSeconds;

	[Space(20f)]
	[Header("調整確認用に表示しているパラメータ\n値が高いものに関連するモーションが再生されやすくなる")]
	[SerializeField]
	[Header("基本")]
	private float _baseAmount;

	[SerializeField]
	[Header("思考量")]
	private float _thinkingAmount;

	[SerializeField]
	[Header("外を見る量")]
	private float _lookOutsideAmount;

	[SerializeField]
	[Header("ヘッドホンを触る量")]
	private float _controlHeadphoneAmount;

	private Phase _phase;

	private float _startSeconds;

	private float _stayMaxSeconds;

	private MyStopWatch _animUpdateStopWatch = new MyStopWatch();

	private MyStopWatch _thinkingInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _lookOutsideInvalidStopWatch = new MyStopWatch();

	private MyStopWatch _controlHeadphoneInvalidStopWatch = new MyStopWatch();

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		_phase = Phase.GoToSofa;
		_startSeconds = Time.time;
		LeaveChairData leaveChairData = base.Owner.MasterDataLoader.HeroineAIMasterData.LeaveChairData;
		_stayMaxSeconds = Random.Range(leaveChairData.SofaStaySecondsMin, leaveChairData.SofaStaySecondsMax);
		_baseAmount = _initBaseAmount;
		_thinkingAmount = 0f;
		_lookOutsideAmount = 0f;
		_controlHeadphoneAmount = 0f;
		_animUpdateStopWatch.ChangeTargetSecondsForRandom(_baseDelaySecondsMin, _baseDelaySecondsMax);
		_animUpdateStopWatch.Watch.Restart();
		_thinkingInvalidStopWatch.SetTargetSeconds(0f);
		_lookOutsideInvalidStopWatch.SetTargetSeconds(0f);
		_controlHeadphoneInvalidStopWatch.SetTargetSeconds(0f);
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Wild003_LeaveChair_Sofa);
	}

	protected override void OnUpdate()
	{
		switch (_phase)
		{
		case Phase.GoToSofa:
			if (base.Owner.HeroineService.IsCurrentAnimation("anim_wild_leavechair_sofa_loop"))
			{
				_phase = Phase.Stay;
			}
			break;
		case Phase.Stay:
			if (Time.time - _startSeconds >= _stayMaxSeconds)
			{
				StartComeback();
				_phase = Phase.Comeback;
			}
			else
			{
				UpdateStayAnimation();
			}
			break;
		case Phase.Comeback:
			if (base.Owner.HeroineService.IsEndAnimation(HeroineService.AnimationType.Base001.ToString()))
			{
				base.Owner.OnLeaveChairFinished();
			}
			break;
		}
	}

	private void StartComeback()
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Base001);
	}

	private void UpdateStayAnimation()
	{
		if (_animUpdateStopWatch.IsElapsedTargetTime() && base.Owner.HeroineService.IsEndAnimation())
		{
			HeroineService.AnimationType nextStayAnimation = GetNextStayAnimation();
			base.Owner.HeroineService.ChangeAnimation(nextStayAnimation);
			_animUpdateStopWatch.Watch.Restart();
		}
	}

	private HeroineService.AnimationType GetNextStayAnimation()
	{
		HeroineService.AnimationType currentAnimationType = base.Owner.HeroineService.GetCurrentAnimationType();
		HeroineService.AnimationType animationType = HeroineService.AnimationType.Wild003_LeaveChair_Sofa;
		if ((uint)(currentAnimationType - 151) <= 3u)
		{
			animationType = GetNextStayAnimationFromParameter();
		}
		else
		{
			animationType = HeroineService.AnimationType.Wild003_LeaveChair_Sofa;
			_animUpdateStopWatch.ChangeTargetSecondsForRandom(_baseDelaySecondsMin, _baseDelaySecondsMax);
		}
		return animationType;
	}

	private HeroineService.AnimationType GetNextStayAnimationFromParameter()
	{
		float maxInclusive = _baseAmount + _thinkingAmount + _lookOutsideAmount + _controlHeadphoneAmount;
		float num = Random.Range(0f, maxInclusive);
		HeroineService.AnimationType nextAnimation = default(HeroineService.AnimationType);
		if (num < _baseAmount)
		{
			BaseUpdate();
		}
		else if (num < _baseAmount + _thinkingAmount)
		{
			if (_thinkingInvalidStopWatch.IsElapsedTargetTime())
			{
				ThinkingUpdate();
			}
			else
			{
				BaseUpdate();
			}
		}
		else if (num < _baseAmount + _thinkingAmount + _lookOutsideAmount)
		{
			if (_lookOutsideInvalidStopWatch.IsElapsedTargetTime())
			{
				LookOutsideUpdate();
			}
			else
			{
				BaseUpdate();
			}
		}
		else if (_controlHeadphoneInvalidStopWatch.IsElapsedTargetTime())
		{
			ControlHeadphoneUpdate();
		}
		else
		{
			BaseUpdate();
		}
		return nextAnimation;
		void BaseUpdate()
		{
			nextAnimation = HeroineService.AnimationType.Wild003_LeaveChair_Sofa;
			float num2 = (_thinkingInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * _thinkingChangingAmount) : 0f);
			float num3 = (_lookOutsideInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * _lookOutsideChangingAmount) : 0f);
			float num4 = (_controlHeadphoneInvalidStopWatch.IsElapsedTargetTime() ? (_baseAmount * _controlHeadphoneChangingAmount) : 0f);
			_baseAmount -= num2 + num3 + num4;
			_thinkingAmount += num2;
			_lookOutsideAmount += num3;
			_controlHeadphoneAmount += num4;
			_animUpdateStopWatch.ChangeTargetSecondsForRandom(_baseDelaySecondsMin, _baseDelaySecondsMax);
		}
		void ControlHeadphoneUpdate()
		{
			nextAnimation = HeroineService.AnimationType.Wild003_LeaveChair_Sofa_Control_Headphone;
			CommonUpdateParameter(ref _controlHeadphoneAmount, ref _controlHeadphoneInvalidStopWatch, _controlHeadphoneSubtractRatio, _controlHeadphoneDelaySecondsMin, _controlHeadphoneDelaySecondsMax, _controlHeadphoneInvalidSeconds);
		}
		void LookOutsideUpdate()
		{
			nextAnimation = HeroineService.AnimationType.Wild003_LeaveChair_Sofa_Look_Outside;
			CommonUpdateParameter(ref _lookOutsideAmount, ref _lookOutsideInvalidStopWatch, _lookOutsideSubtractRatio, _lookOutsideDelaySecondsMin, _lookOutsideDelaySecondsMax, _lookOutsideInvalidSeconds);
		}
		void ThinkingUpdate()
		{
			nextAnimation = HeroineService.AnimationType.Wild003_LeaveChair_Sofa_Thinking;
			CommonUpdateParameter(ref _thinkingAmount, ref _thinkingInvalidStopWatch, _thinkingSubtractRatio, _thinkingDelaySecondsMin, _thinkingDelaySecondsMax, _thinkingInvalidSeconds);
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
		LeaveChairData data = base.Owner.MasterDataLoader.HeroineAIMasterData.LeaveChairData;
		await UniTask.WaitUntil(() => Time.time - _startSeconds > data.SofaForceStaySeconds, PlayerLoopTiming.Update, token);
		StartComeback();
		await UniTask.WaitUntil(() => base.Owner.HeroineService.IsCurrentAnimation(HeroineService.AnimationType.Base001.ToString()), PlayerLoopTiming.Update, token);
	}
}
