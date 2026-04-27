using System;
using Bulbul;
using UnityEngine;
using VContainer;

public class HeroineNormalAnimationController : MonoBehaviour
{
	[Serializable]
	private enum MainState
	{
		Idle,
		StartChange,
		Changing
	}

	private MainState _mainState;

	[Inject]
	private HeroineService _heroineService;

	[SerializeField]
	private bool _isUse;

	[SerializeField]
	private BreakAnimationBehavior _breakAnimationBehavior;

	private HeroineService.AnimationType _beforeChangeAnimation;

	private HeroineService.AnimationType _nextAnimation;

	private int _currentAnimLoopCount;

	private HeroineService.AnimationType _beforeProbremAnimation;

	public void Setup()
	{
		_mainState = MainState.Idle;
	}

	public void UpdateAnimation(FacilityPomodoro pomodoro)
	{
		if (!_isUse)
		{
			return;
		}
		HeroineService.AnimationType currentAnimationType = _heroineService.GetCurrentAnimationType();
		switch (_mainState)
		{
		case MainState.Idle:
			if (_heroineService.IsEndAnimation(_currentAnimLoopCount))
			{
				_mainState = MainState.StartChange;
			}
			break;
		case MainState.StartChange:
			if (!pomodoro.IsCurrentWorking() && !pomodoro.IsCurrentResting())
			{
				UpdateNormalAnimation(currentAnimationType);
			}
			_mainState = MainState.Changing;
			break;
		case MainState.Changing:
			if (currentAnimationType == _nextAnimation && !_heroineService.IsEndAnimation(_currentAnimLoopCount))
			{
				_mainState = MainState.Idle;
			}
			else if (currentAnimationType != _beforeChangeAnimation && _heroineService.IsEndAnimation(_currentAnimLoopCount))
			{
				_mainState = MainState.Idle;
			}
			break;
		}
	}

	public void UpdateNormalAnimation(HeroineService.AnimationType currentAnimationType)
	{
		_breakAnimationBehavior.UpdateAnimation();
	}
}
