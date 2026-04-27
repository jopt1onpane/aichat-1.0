using TMPro;
using UnityEngine;

namespace Bulbul;

public class InteractableUISettingTab : InteractableUI
{
	[Header("テキスト")]
	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private Color _mouseOverColor;

	[SerializeField]
	private Color _usingOverColor;

	[SerializeField]
	private bool _isUseManualDefaultColor;

	[SerializeField]
	private Color _manualDefaultColor;

	private Color _defaultColor;

	[SerializeField]
	private HoldButtonAnimation _holdButtonAnimation;

	protected override void SetupCore()
	{
		_defaultColor = _text.color;
		base.SetupCore();
	}

	protected override void ActivateMouseOverImage(bool isUseDoComplete = false)
	{
		base.ActivateMouseOverImage(isUseDoComplete);
		_text.color = _mouseOverColor;
	}

	protected override void DeactivateMouseOverImage(bool isUseDoComplete = false)
	{
		base.DeactivateMouseOverImage(isUseDoComplete);
		if (_isUseManualDefaultColor)
		{
			_text.color = _manualDefaultColor;
		}
		else
		{
			_text.color = _defaultColor;
		}
	}

	protected override void ActivateUsingImage(bool isUseDoComplete = false)
	{
		base.ActivateUsingImage(isUseDoComplete);
		_text.color = _usingOverColor;
		if ((bool)_holdButtonAnimation)
		{
			_holdButtonAnimation.enabled = false;
		}
	}

	protected override void DeactivateUsingImage(bool isUseDoComplete = false)
	{
		base.DeactivateUsingImage(isUseDoComplete);
		if (_isUseManualDefaultColor)
		{
			_text.color = _manualDefaultColor;
		}
		else
		{
			_text.color = _defaultColor;
		}
		if ((bool)_holdButtonAnimation)
		{
			_holdButtonAnimation.enabled = true;
		}
	}
}
