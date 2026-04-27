using Bulbul;
using UnityEngine;
using VContainer;

public class BreakAnimationBehavior : MonoBehaviour
{
	[Inject]
	private HeroineService _heroineService;

	[Header("アニメーションの管理で使用\u3000値が高いものに関連するモーションが再生されやすくなる")]
	[SerializeField]
	[Header("ベース")]
	private float _baseAmount;

	[SerializeField]
	[Header("楽しい感情")]
	private float _enjoyAmount;

	[Header("変動量\u3000全部合わせて0～1の間になるようにする")]
	[SerializeField]
	[Header("楽しい感情の変動量")]
	private float _enjoyChangingAmount;

	[Header("アニメーション変更Delay")]
	[SerializeField]
	[Header("ベース")]
	private float _baseDelaySecondsMin;

	[SerializeField]
	private float _baseDelaySecondsMax;

	[SerializeField]
	[Header("笑う(enjoy)")]
	private float _enjoyDelaySecondsMin;

	[SerializeField]
	private float _enjoyDelaySecondsMax;

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
		if (!_stopwatch.IsElapsedTargetTime())
		{
			return;
		}
		HeroineService.AnimationType currentAnimationType = _heroineService.GetCurrentAnimationType();
		if (_heroineService.IsEndAnimation(_currentAnimLoopCount))
		{
			HeroineService.AnimationType animationType = HeroineService.AnimationType.Base001;
			if ((uint)(currentAnimationType - 402) <= 2u || currentAnimationType == HeroineService.AnimationType.BreakBase001)
			{
				animationType = GetNextAnimation();
			}
			else
			{
				animationType = HeroineService.AnimationType.BreakBase001;
				_stopwatch.ChangeTargetSecondsForRandom(_baseDelaySecondsMin, _baseDelaySecondsMax);
			}
			ChangeAnimation(currentAnimationType, animationType);
			_stopwatch.Watch.Restart();
		}
		HeroineService.AnimationType GetNextAnimation()
		{
			float maxInclusive = _baseAmount + _enjoyAmount;
			float num = Random.Range(0f, maxInclusive);
			HeroineService.AnimationType result = HeroineService.AnimationType.BreakBase001;
			_currentAnimLoopCount = 0;
			if (num < _baseAmount)
			{
				result = HeroineService.AnimationType.BreakBase001;
				float num2 = _baseAmount * _enjoyChangingAmount;
				_baseAmount -= num2;
				_enjoyAmount += num2;
				_stopwatch.ChangeTargetSecondsForRandom(_baseDelaySecondsMin, _baseDelaySecondsMax);
			}
			else if (num < _baseAmount + _enjoyAmount)
			{
				result = (_beforeProbremAnimation = ((_beforeProbremAnimation == HeroineService.AnimationType.BreakBase001_Laugh) ? HeroineService.AnimationType.BreakBase001_Laugh : HeroineService.AnimationType.BreakBase001_Laugh));
				float enjoyAmount = _enjoyAmount;
				_baseAmount += _enjoyAmount;
				_enjoyAmount -= enjoyAmount;
				_stopwatch.ChangeTargetSecondsForRandom(_enjoyDelaySecondsMin, _enjoyDelaySecondsMax);
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
