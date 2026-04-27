using UnityEngine;

namespace NestopiSystem;

public static class RectExtensions
{
	public static Vector2 NormalizedToPointUnclamped(this Rect rectangle, Vector2 normalizedRectCoordinates)
	{
		return new Vector2(Mathf.LerpUnclamped(rectangle.x, rectangle.xMax, normalizedRectCoordinates.x), Mathf.LerpUnclamped(rectangle.y, rectangle.yMax, normalizedRectCoordinates.y));
	}

	public static bool TryGetIntersectRect(this Rect rect1, Rect rect2, out Rect result)
	{
		result = default(Rect);
		float num = Mathf.Max(rect1.xMin, rect2.xMin);
		float num2 = Mathf.Min(rect1.xMax, rect2.xMax);
		float num3 = Mathf.Max(rect1.yMin, rect2.yMin);
		float num4 = Mathf.Min(rect1.yMax, rect2.yMax);
		if (num2 > num && num4 > num3)
		{
			result = new Rect(num, num3, num2 - num, num4 - num3);
			return true;
		}
		return false;
	}

	public static bool ContainsScreenPoint(this RectTransform rectTransform, Vector2 screenPoint, Camera uiCamera = null)
	{
		return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, uiCamera);
	}

	public static bool ContainsScreenPoint(this RectTransform rectTransform, Vector2 screenPoint, float margin, MarginDirection marginDirection = MarginDirection.All, Camera uiCamera = null)
	{
		if (rectTransform == null)
		{
			return false;
		}
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, uiCamera, out var localPoint))
		{
			return false;
		}
		Rect rect = rectTransform.rect;
		if ((marginDirection & MarginDirection.Left) != MarginDirection.None)
		{
			rect.xMin -= margin;
		}
		if ((marginDirection & MarginDirection.Right) != MarginDirection.None)
		{
			rect.xMax += margin;
		}
		if ((marginDirection & MarginDirection.Bottom) != MarginDirection.None)
		{
			rect.yMin -= margin;
		}
		if ((marginDirection & MarginDirection.Top) != MarginDirection.None)
		{
			rect.yMax += margin;
		}
		return rect.Contains(localPoint);
	}
}
