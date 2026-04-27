using System;
using UnityEngine;

public abstract class AnimationBase : MonoBehaviour, IAnimation
{
	protected bool isComplete;

	protected MyTweenPlayType beforePlayType;

	protected MyTweenPlayType currentPlayType;

	public Action OnComplete;

	public bool IsComplete => isComplete;

	public MyTweenPlayType BeforePlayType => beforePlayType;

	public MyTweenPlayType CurrentPlayType => currentPlayType;

	public abstract void Play(MyTweenPlayType type);

	public abstract void PlayComplete(MyTweenPlayType type);

	public abstract void BasePlay();

	public abstract void Restart();

	public abstract void Pause();

	public abstract void Kill();

	protected void PlaySetting(MyTweenPlayType playType)
	{
		switch (playType)
		{
		case MyTweenPlayType.Normal:
			PlayNormalSetting();
			break;
		case MyTweenPlayType.Reverse:
			PlayReverseSetting();
			break;
		}
	}

	public abstract void PlayNormalSetting();

	public abstract void PlayReverseSetting();

	protected void OnSettingPlayNormal()
	{
		isComplete = false;
		currentPlayType = MyTweenPlayType.Normal;
	}

	protected void OnSettingPlayReverse()
	{
		isComplete = false;
		currentPlayType = MyTweenPlayType.Reverse;
	}

	protected void OnCompletePlayNormal()
	{
		beforePlayType = MyTweenPlayType.Normal;
		isComplete = true;
		OnComplete?.Invoke();
	}

	protected void OnCompletePlaReverse()
	{
		beforePlayType = MyTweenPlayType.Reverse;
		isComplete = true;
		OnComplete?.Invoke();
	}

	public void UpdateNormalReverseLoopPlay()
	{
		if (isComplete)
		{
			if (BeforePlayType == MyTweenPlayType.Normal)
			{
				Play(MyTweenPlayType.Reverse);
			}
			else if (BeforePlayType == MyTweenPlayType.Reverse)
			{
				Play(MyTweenPlayType.Normal);
			}
		}
	}
}
