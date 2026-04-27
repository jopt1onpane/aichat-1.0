using UnityEngine;
using UnityEngine.UI;

namespace NestopiSystem;

public static class ScrollRectExtensions
{
	public static float ScrollToTop(this ScrollRect scrollRect, RectTransform target)
	{
		return scrollRect.ScrollTo(target, 1f);
	}

	public static float ScrollToCenter(this ScrollRect scrollRect, RectTransform target)
	{
		return scrollRect.ScrollTo(target, 0.5f);
	}

	public static float ScrollToBottom(this ScrollRect scrollRect, RectTransform target)
	{
		return scrollRect.ScrollTo(target, 0f);
	}

	public static float ScrollTo(this ScrollRect scrollRect, RectTransform target, float align)
	{
		float height = scrollRect.content.rect.height;
		float height2 = scrollRect.viewport.rect.height;
		if (height < height2)
		{
			return 0f;
		}
		float num = height + GetPosY(target) + target.rect.height * align;
		float num2 = height2 * align;
		float value = (num - num2) / (height - height2);
		return scrollRect.verticalNormalizedPosition = Mathf.Clamp01(value);
	}

	public static float ScrollToHorizontal(this ScrollRect scrollRect, RectTransform target, float align)
	{
		float width = scrollRect.content.rect.width;
		float width2 = scrollRect.viewport.rect.width;
		if (width < width2)
		{
			return 0f;
		}
		float num = GetPosX(target) + target.rect.width * align;
		float num2 = width2 * align;
		float value = (num - num2) / (width - width2);
		return scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(value);
	}

	private static float GetPosY(RectTransform transform)
	{
		return transform.localPosition.y + transform.rect.y;
	}

	private static float GetPosX(RectTransform transform)
	{
		return transform.localPosition.x + transform.rect.x;
	}
}
