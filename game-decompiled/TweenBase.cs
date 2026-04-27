using DG.Tweening;
using UnityEngine;

public abstract class TweenBase : AnimationBase
{
	[SerializeField]
	protected TweenParam normalParam;

	[SerializeField]
	protected TweenParam reverseParam;

	protected Tween tween;

	public TweenParam NormalParam => normalParam;

	public TweenParam ReverseParam => reverseParam;

	public Tween GetTween()
	{
		return tween;
	}

	public override void BasePlay()
	{
		tween.Play();
	}

	public override void Play(MyTweenPlayType playType)
	{
		tween.Kill();
		switch (playType)
		{
		case MyTweenPlayType.Normal:
			PlayNormalSetting();
			break;
		case MyTweenPlayType.Reverse:
			PlayReverseSetting();
			break;
		}
		tween.Play();
	}

	public override void PlayComplete(MyTweenPlayType playType)
	{
		Play(playType);
		tween.Complete();
	}

	public override void Restart()
	{
		tween.Restart();
	}

	public override void Pause()
	{
		tween.Pause();
	}

	public override void Kill()
	{
		tween.Kill();
	}
}
