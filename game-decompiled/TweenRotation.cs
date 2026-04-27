using DG.Tweening;
using UnityEngine;

public class TweenRotation : TweenBase
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
		target.localEulerAngles = initValue;
		tween = target.DOLocalRotate(toValue, normalParam.toTime).SetDelay(normalParam.delaySeocnds).SetEase(normalParam.easeType)
			.OnComplete(delegate
			{
				OnCompletePlayNormal();
			});
		Pause();
	}

	public override void PlayReverseSetting()
	{
		OnSettingPlayReverse();
		target.localEulerAngles = toValue;
		tween = target.DOLocalRotate(initValue, reverseParam.toTime).SetDelay(reverseParam.delaySeocnds).SetEase(reverseParam.easeType)
			.OnComplete(delegate
			{
				OnCompletePlaReverse();
			});
		Pause();
	}
}
