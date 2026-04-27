using UnityEngine;
using UnityEngine.UI;

namespace NestopiSystem;

public static class UIExtensions
{
	public static void SetAlpha(this Graphic target, float alpha)
	{
		Color color = target.color;
		target.color = new Color(color.r, color.g, color.b, alpha);
	}

	public static void SetAlpha(this SpriteRenderer target, float alpha)
	{
		Color color = target.color;
		target.color = new Color(color.r, color.g, color.b, alpha);
	}

	public static RectTransform RectTransform(this Canvas self)
	{
		return (RectTransform)self.transform;
	}

	public static void SetSprite(this Image image, Sprite sprite, float alpha = 1f)
	{
		if ((bool)sprite)
		{
			image.sprite = sprite;
			image.SetAlpha(alpha);
		}
		else
		{
			image.SetAlpha(0f);
			image.sprite = null;
		}
	}
}
