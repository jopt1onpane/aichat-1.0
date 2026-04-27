using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class InteractiveTabIconUI : InteractableUI
{
	[SerializeField]
	private Image _tabIcon;

	[SerializeField]
	private Sprite _mouseOverIconSprite;

	[SerializeField]
	private Sprite _usingOverIconSprite;

	[SerializeField]
	private HoldButtonAnimation _holdButtonAnimation;

	private Sprite _defaultIconSprite;

	protected override void SetupCore()
	{
		_defaultIconSprite = _tabIcon.sprite;
		base.SetupCore();
	}

	protected override void ActivateMouseOverImage(bool isUseDoComplete = false)
	{
		base.ActivateMouseOverImage(isUseDoComplete);
		_tabIcon.sprite = _mouseOverIconSprite;
	}

	protected override void DeactivateMouseOverImage(bool isUseDoComplete = false)
	{
		base.DeactivateMouseOverImage(isUseDoComplete);
		_tabIcon.sprite = _defaultIconSprite;
	}

	protected override void ActivateUsingImage(bool isUseDoComplete = false)
	{
		base.ActivateUsingImage(isUseDoComplete);
		_tabIcon.sprite = _usingOverIconSprite;
		if ((bool)_holdButtonAnimation)
		{
			_holdButtonAnimation.enabled = false;
		}
	}

	protected override void DeactivateUsingImage(bool isUseDoComplete = false)
	{
		base.DeactivateUsingImage(isUseDoComplete);
		_tabIcon.sprite = _defaultIconSprite;
		if ((bool)_holdButtonAnimation)
		{
			_holdButtonAnimation.enabled = true;
		}
	}
}
