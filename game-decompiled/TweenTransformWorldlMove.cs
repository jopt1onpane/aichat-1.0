using DG.Tweening;
using UnityEngine;

public class TweenTransformWorldlMove : TweenBase
{
	[SerializeField]
	private Transform target;

	[SerializeField]
	private Vector3 initValue;

	[SerializeField]
	private Vector3 toValue;

	public override void PlayNormalSetting()
	{
		OnSettingPlayNormal();
		target.transform.position = initValue;
		tween = target.DOMove(toValue, normalParam.toTime).SetDelay(normalParam.delaySeocnds).SetEase(normalParam.easeType)
			.OnComplete(delegate
			{
				OnCompletePlayNormal();
			});
		Pause();
	}

	public override void PlayReverseSetting()
	{
		OnSettingPlayReverse();
		target.transform.position = toValue;
		tween = target.DOMove(initValue, reverseParam.toTime).SetDelay(reverseParam.delaySeocnds).SetEase(reverseParam.easeType)
			.OnComplete(delegate
			{
				OnCompletePlaReverse();
			});
		Pause();
	}

	public void SetInitValue(Vector3 initValue)
	{
		this.initValue = initValue;
	}

	public void SetToValue(Vector3 toValue)
	{
		this.toValue = toValue;
	}

	public void SetToValueY(float toY)
	{
		toValue.y = toY;
	}
}
