using UnityEngine;

namespace Bulbul.Mobile;

public interface IAnimationView
{
	CanvasGroup AnimationCanvasGroup { get; }

	RectTransform AnimationRectTransform { get; }

	void SetActiveRaycastBlocker(bool isActive);
}
