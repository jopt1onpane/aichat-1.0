using DG.Tweening;
using NestopiSystem;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TweenImgeAlphaBlinking : TweenBase
{
	[SerializeField]
	[Tooltip("点滅なので1で1往復")]
	private int blinkCount;

	[SerializeField]
	[Tooltip("回数制限無し")]
	private bool isInf;

	private Image image;

	private Image Image
	{
		get
		{
			if (image == null)
			{
				image = GetComponent<Image>();
			}
			return image;
		}
	}

	public override void PlayNormalSetting()
	{
		OnSettingPlayNormal();
		Image.SetAlpha(0f);
		Image.enabled = true;
		tween = Image.DOFade(1f, normalParam.toTime).SetLoops(isInf ? (-1) : (blinkCount + 1), LoopType.Yoyo).SetDelay(normalParam.delaySeocnds)
			.SetEase(normalParam.easeType)
			.OnComplete(delegate
			{
				Image.enabled = false;
				OnCompletePlayNormal();
			});
		Pause();
	}

	public override void PlayReverseSetting()
	{
		OnSettingPlayReverse();
		Image.SetAlpha(1f);
		Image.enabled = true;
		tween = Image.DOFade(0f, normalParam.toTime).SetLoops(isInf ? (-1) : (blinkCount + 1), LoopType.Yoyo).SetDelay(normalParam.delaySeocnds)
			.SetEase(normalParam.easeType)
			.OnComplete(delegate
			{
				Image.enabled = true;
				OnCompletePlayNormal();
			});
		Pause();
	}
}
