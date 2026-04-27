using System;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class AddExpValueUIPool : PoolManager<AddExpValueUI>
{
	[Inject]
	private IPlayerLevelUIService _levelUIService;

	[SerializeField]
	[Header("経験値取得量UIプレハブ")]
	private AddExpValueUI _addExpValueUIPrefab;

	[SerializeField]
	[Header("UIの幅")]
	private float _uiHeight;

	[SerializeField]
	[Header("UI同士の隙間")]
	private float _uiSpace;

	[SerializeField]
	private float _toPosY;

	private int _expValueUICount;

	public void Setup()
	{
		NewObjectPool();
		ObservableSubscribeExtensions.Subscribe(_levelUIService.OnEndShowExpValue, delegate
		{
			_expValueUICount = 0;
		}).AddTo(this);
	}

	public void AddExpValueUI(float exp, Action onEndAnimation)
	{
		float num = (_uiHeight + _uiSpace) * (float)_expValueUICount;
		num *= -1f;
		AddExpValueUI expValueUI = _objectPool.Get();
		_expValueUICount++;
		float toPosY = _toPosY + num;
		expValueUI.Initialize();
		expValueUI.Setup(toPosY, _levelUIService);
		expValueUI.StartAnim(exp, delegate
		{
			onEndAnimation();
			expValueUI.Deactivate();
		});
	}
}
