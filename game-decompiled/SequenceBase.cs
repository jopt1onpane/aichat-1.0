using DG.Tweening;

public abstract class SequenceBase : AnimationBase
{
	protected Sequence sequence;

	public Sequence GetSequence()
	{
		return sequence;
	}

	public override void BasePlay()
	{
		sequence.Play();
	}

	public override void Play(MyTweenPlayType playType)
	{
		sequence.Kill();
		switch (playType)
		{
		case MyTweenPlayType.Normal:
			PlayNormalSetting();
			break;
		case MyTweenPlayType.Reverse:
			PlayReverseSetting();
			break;
		}
		sequence.Play();
	}

	public override void PlayComplete(MyTweenPlayType playType)
	{
		Play(playType);
		sequence.Complete();
	}

	public override void Restart()
	{
		sequence.Restart();
	}

	public override void Pause()
	{
		sequence.Pause();
	}

	public override void Kill()
	{
		sequence.Kill();
	}
}
