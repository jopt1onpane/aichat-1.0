using System;
using Bulbul;
using UnityEngine;

public class HeroineDrinks : MonoBehaviour
{
	private const string AnimNamePouringStart = "anim_break_tea_sub_maker_start";

	private const string AnimNamePouringLoop = "anim_break_tea_sub_maker_loop";

	private const string AnimNamePouringEnd = "Wild001_Motion10_UseCoffeeMaker";

	private const float DrinkHotTimeMinutes = 10f;

	[SerializeField]
	[Header("湯気：パーティクル")]
	private ParticleSystem _cupSmoke;

	[SerializeField]
	[Header("湯気：オブジェクト")]
	private GameObject _cupSmokeParentObject;

	private int _drinkRemainAmount = 5;

	private HeroineService _heroineService;

	private DateTime _lastPourDrinkDateTime;

	private Transform _cupSmokeToTarget;

	public void Setup(HeroineService heroineService)
	{
		_drinkRemainAmount = 4;
		_heroineService = heroineService;
		_lastPourDrinkDateTime = DateTime.MinValue;
	}

	public void Update()
	{
		UpdateCupSmokePosition();
	}

	private void UpdateCupSmokePosition()
	{
		if (!(_cupSmokeToTarget == null) && _cupSmokeParentObject.transform.position != _cupSmokeToTarget.position)
		{
			_cupSmokeParentObject.transform.position = _cupSmokeToTarget.position;
		}
	}

	public bool IsHot()
	{
		if (_drinkRemainAmount == 5 && (_lastPourDrinkDateTime - DateTime.Now).TotalMinutes < 10.0)
		{
			return true;
		}
		return false;
	}

	public void Drink()
	{
		_drinkRemainAmount--;
	}

	public bool IsNeedPourDrinks()
	{
		if (_drinkRemainAmount <= 0)
		{
			return !_cupSmoke.isPlaying;
		}
		return false;
	}

	public void PourDrinks()
	{
		if (!_cupSmoke.isPlaying)
		{
			_lastPourDrinkDateTime = DateTime.Now;
			_cupSmoke.Play();
			_drinkRemainAmount = 5;
		}
	}

	public bool IsPouringDrinks()
	{
		if (_heroineService.IsCurrentAnimation("anim_break_tea_sub_maker_start") || _heroineService.IsCurrentAnimation("anim_break_tea_sub_maker_loop") || _heroineService.IsCurrentAnimation("Wild001_Motion10_UseCoffeeMaker"))
		{
			return true;
		}
		return false;
	}

	public void SetCupSmokeToTarget(Transform targetTransform)
	{
		_cupSmokeToTarget = targetTransform;
	}
}
