using Bulbul;
using UnityEngine;
using VContainer;

public class WorkAnimationBehavior : MonoBehaviour
{
	[Inject]
	private HeroineService _heroineService;

	[Header("アニメーションの管理で使用\u3000値が高いものに関連するモーションが再生されやすくなる")]
	[SerializeField]
	[Header("ベース")]
	private float _baseAmount;

	[SerializeField]
	[Header("問題の量")]
	private float _problemAmount;

	[SerializeField]
	[Header("疲労の量")]
	private float _gatigueAmount;

	[Header("変動量\u3000全部合わせて0～1の間になるようにする")]
	[SerializeField]
	[Header("問題の変動量")]
	private float _problemChangingAmount;

	[SerializeField]
	[Header("疲労の変動量")]
	private float _gatigueChangingAmount;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	[Header("ベース")]
	private float _baseDelaySecondsMin;

	[SerializeField]
	private float _baseDelaySecondsMax;

	[SerializeField]
	[Header("問題")]
	private float _problemDelaySecondsMin;

	[SerializeField]
	private float _problemDelaySecondsMax;

	[SerializeField]
	[Header("疲労")]
	private float _gatigueDelaySecondsMin;

	[SerializeField]
	private float _gatigueDelaySecondsMax;

	private HeroineService.AnimationType _beforeChangeAnimation;

	private HeroineService.AnimationType _nextAnimation;

	private int _currentAnimLoopCount;

	private HeroineService.AnimationType _beforeProbremAnimation;

	private float _delayTotalSeconds;

	private MyStopWatch _stopwatch = new MyStopWatch();

	public void Setup()
	{
		_stopwatch.Watch.Start();
	}

	public void UpdateAnimation()
	{
		HeroineService.AnimationType currentAnimationType = _heroineService.GetCurrentAnimationType();
		if (_heroineService.IsEndAnimation(_currentAnimLoopCount))
		{
			HeroineService.AnimationType animationType = HeroineService.AnimationType.Base001;
			if (currentAnimationType == HeroineService.AnimationType.WorkBase001 || (uint)(currentAnimationType - 402) <= 2u)
			{
				animationType = GetNextAnimation();
			}
			else
			{
				animationType = HeroineService.AnimationType.WorkBase001;
				_stopwatch.ChangeTargetSecondsForRandom(_baseDelaySecondsMin, _baseDelaySecondsMax);
			}
			ChangeAnimation(currentAnimationType, animationType);
			_stopwatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			float maxInclusive = _baseAmount + _problemAmount + _gatigueAmount;
			float num = Random.Range(0f, maxInclusive);
			_currentAnimLoopCount = 0;
			HeroineService.AnimationType result;
			if (num < _baseAmount)
			{
				result = HeroineService.AnimationType.WorkBase001;
				float num2 = _baseAmount * _problemChangingAmount;
				float num3 = _baseAmount * _gatigueChangingAmount;
				_baseAmount -= num2 + num3;
				_problemAmount += num2;
				_gatigueAmount += num3;
				_stopwatch.ChangeTargetSecondsForRandom(_baseDelaySecondsMin, _baseDelaySecondsMax);
			}
			else if (num < _baseAmount + _problemAmount)
			{
				result = (_beforeProbremAnimation = ((_beforeProbremAnimation == HeroineService.AnimationType.WorkBase001_Thinking) ? HeroineService.AnimationType.WorkBase001_Stop : HeroineService.AnimationType.WorkBase001_Thinking));
				float num4 = _problemAmount * 0.5f;
				_problemAmount -= num4;
				_baseAmount += num4;
				_stopwatch.ChangeTargetSecondsForRandom(_problemDelaySecondsMin, _problemDelaySecondsMax);
			}
			else
			{
				result = HeroineService.AnimationType.WorkBase001_Stop;
				float gatigueAmount = _gatigueAmount;
				_gatigueAmount -= gatigueAmount;
				_baseAmount += gatigueAmount;
				_stopwatch.ChangeTargetSecondsForRandom(_gatigueDelaySecondsMin, _gatigueDelaySecondsMax);
			}
			return result;
		}
	}

	private void ChangeAnimation(HeroineService.AnimationType currentAnimation, HeroineService.AnimationType nextAnim)
	{
		_beforeChangeAnimation = currentAnimation;
		_nextAnimation = nextAnim;
		_heroineService.ChangeHeroineAnimationForInteger((int)nextAnim);
	}
}
