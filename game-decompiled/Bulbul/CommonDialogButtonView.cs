using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class CommonDialogButtonView : ButtonEventObservable
{
	public TextLocalizationBehaviour Text;

	[SerializeField]
	private Image mainImage;

	[SerializeField]
	private Sprite normalStyle;

	[SerializeField]
	private Sprite submitStyle;

	public void SetStyle(CommonButtonStyle style)
	{
		Sprite sprite = style switch
		{
			CommonButtonStyle.Normal => normalStyle, 
			CommonButtonStyle.Submit => submitStyle, 
			_ => throw new ArgumentOutOfRangeException("style", style, null), 
		};
		mainImage.sprite = sprite;
	}
}
