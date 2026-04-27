using DG.Tweening;
using UnityEngine;

public class TweenCanvasGroupFade : TweenBase
{
	[SerializeField]
	private GameObject targetObj;

	[SerializeField]
	private CanvasGroup targetCanvasGroup;

	[SerializeField]
	private float initValue;

	[SerializeField]
	private int toValue;

	public override void PlayNormalSetting()
	{
		OnSettingPlayNormal();
		targetObj.SetActive(value: true);
		targetCanvasGroup.alpha = initValue;
		tween = DOTween.To(() => initValue, delegate(float value)
		{
			targetCanvasGroup.alpha = value;
		}, toValue, normalParam.toTime).SetDelay(normalParam.delaySeocnds).SetEase(normalParam.easeType)
			.OnComplete(delegate
			{
				OnCompletePlayNormal();
			});
		Pause();
	}

	public override void PlayReverseSetting()
	{
		OnSettingPlayReverse();
		targetObj.SetActive(value: true);
		targetCanvasGroup.alpha = toValue;
		tween = DOTween.To(() => targetCanvasGroup.alpha, delegate(float value)
		{
			targetCanvasGroup.alpha = value;
		}, initValue, reverseParam.toTime).SetDelay(reverseParam.delaySeocnds).SetEase(reverseParam.easeType)
			.OnComplete(delegate
			{
				OnCompletePlaReverse();
				targetObj.SetActive(value: false);
				targetCanvasGroup.alpha = toValue;
			});
		Pause();
	}
}
