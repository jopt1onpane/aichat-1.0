using DG.Tweening;
using UnityEngine;

public class SequenceFadeInOut : SequenceBase
{
	[SerializeField]
	private TweenCanvasGroupFade tweenFade;

	public override void PlayNormalSetting()
	{
		OnSettingPlayNormal();
		tweenFade.PlayNormalSetting();
		sequence = DOTween.Sequence();
		sequence.Append(tweenFade.GetTween()).OnComplete(delegate
		{
			FadeOut();
		});
		Pause();
	}

	public void FadeOut()
	{
		tweenFade.PlayReverseSetting();
		sequence = DOTween.Sequence();
		sequence.Append(tweenFade.GetTween()).OnComplete(delegate
		{
			OnCompletePlayNormal();
		});
		BasePlay();
	}

	public override void PlayReverseSetting()
	{
		PlayNormalSetting();
	}
}
