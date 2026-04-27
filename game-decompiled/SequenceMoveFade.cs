using DG.Tweening;
using UnityEngine;

public class SequenceMoveFade : SequenceBase
{
	[SerializeField]
	private TweenCanvasGroupFade tweenFade;

	[SerializeField]
	private TweenTransformLocalMove tweenLocalMove;

	public TweenCanvasGroupFade FadeInTween => tweenFade;

	public TweenTransformLocalMove MoveOutTween => tweenLocalMove;

	public override void PlayNormalSetting()
	{
		OnSettingPlayNormal();
		tweenFade.PlayNormalSetting();
		tweenLocalMove.PlayNormalSetting();
		Sequence s = DOTween.Sequence();
		s.Append(tweenFade.GetTween()).Join(tweenLocalMove.GetTween()).OnComplete(delegate
		{
			OnCompletePlayNormal();
		});
		sequence = s;
		Pause();
	}

	public override void PlayReverseSetting()
	{
		OnSettingPlayReverse();
		tweenFade.PlayReverseSetting();
		tweenLocalMove.PlayReverseSetting();
		Sequence s = DOTween.Sequence();
		s.Append(tweenFade.GetTween()).Join(tweenLocalMove.GetTween()).OnComplete(delegate
		{
			OnCompletePlaReverse();
		});
		sequence = s;
		Pause();
	}

	public void SetMoveToPos(float toY)
	{
		tweenLocalMove.SetToValueY(toY);
	}
}
