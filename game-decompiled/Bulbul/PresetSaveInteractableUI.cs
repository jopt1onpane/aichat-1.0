using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class PresetSaveInteractableUI : InteractableUI
{
	[SerializeField]
	private Button Button;

	[SerializeField]
	private RectTransform _backRect;

	[SerializeField]
	private HoldButtonAnimation _holdButtonAnimation;

	private Tween _tween;

	private Vector2 _defaultSize;

	private Vector2 _usingBackSize;

	public Observable<Unit> OnClickAsObservable()
	{
		return Button.OnClickAsObservable();
	}

	protected override void SetupCore()
	{
		_defaultSize = _backRect.sizeDelta;
		base.SetupCore();
	}

	public void ActivateUseUI(Vector2 usingSize)
	{
		_usingBackSize = usingSize;
		ActivateUseUI();
	}

	protected override void ActivateUsingImage(bool isUseDoComplete = false)
	{
		base.ActivateUsingImage(isUseDoComplete);
		_tween?.Kill();
		_tween = _backRect.DOSizeDelta(_usingBackSize, 0.2f);
		base.enabled = false;
		_holdButtonAnimation.enabled = false;
		Button.interactable = false;
	}

	protected override void DeactivateUsingImage(bool isUseDoComplete = false)
	{
		base.DeactivateUsingImage(isUseDoComplete);
		_tween?.Kill();
		_tween = _backRect.DOSizeDelta(_defaultSize, 0.2f);
		base.enabled = true;
		_holdButtonAnimation.enabled = true;
		Button.interactable = true;
	}
}
