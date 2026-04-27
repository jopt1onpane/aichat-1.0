using UnityEngine;

public class MyAnimatorUtil
{
	public static bool IsEndAnimation(Animator animator, string animationName = null, int animationLoopCount = 0)
	{
		if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= (float)(1 + animationLoopCount))
		{
			if (animationName == null)
			{
				return true;
			}
			if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsCurrentAnimation(Animator animator, string animationName)
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
		{
			return true;
		}
		return false;
	}
}
