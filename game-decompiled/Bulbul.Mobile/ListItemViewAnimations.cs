using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Bulbul.Mobile;

public static class ListItemViewAnimations
{
	public enum RemoveAnimationDirection
	{
		Left,
		Right
	}

	public static async UniTask PlayRemovingAnimation(IAnimationView view, CancellationToken cancellationToken, TweenCancelBehaviour tweenCancelBehavior, RemoveAnimationDirection direction = RemoveAnimationDirection.Left)
	{
		Sequence s = DOTween.Sequence();
		view.SetActiveRaycastBlocker(isActive: true);
		float endValue = ((direction != RemoveAnimationDirection.Left) ? 800f : (-800f));
		try
		{
			await s.Join(view.AnimationRectTransform.DOAnchorPosX(endValue, 0.2f)).Join(view.AnimationCanvasGroup.DOFade(0f, 0.2f)).ToUniTask(tweenCancelBehavior, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
		}
		finally
		{
			view.SetActiveRaycastBlocker(isActive: false);
		}
	}
}
