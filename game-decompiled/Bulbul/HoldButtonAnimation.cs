using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class HoldButtonAnimation : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private float hoverScale = 1.05f;

	[SerializeField]
	private float clickScale = 0.95f;

	[SerializeField]
	private float activateScale = 1.05f;

	private Button button;

	private Vector3 defaultScale;

	private bool isPressed;

	private bool isMouseOvered;

	private bool clickScaled;

	private bool hoverScaled;

	private bool isActivated;

	private bool _isFinishSetup;

	private Tween _tween;

	public void Setup()
	{
		button = GetComponent<Button>();
		defaultScale = base.transform.localScale;
	}

	private void Start()
	{
		if (!_isFinishSetup)
		{
			Setup();
		}
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		isPressed = true;
		ClickDown();
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		isPressed = false;
		ClickUp();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!DevicePlatform.Steam.IsMobile())
		{
			isMouseOvered = true;
			MouseOverEnter();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isMouseOvered = false;
		MouseOverExit();
	}

	private void OnDisable()
	{
		if (clickScaled || hoverScaled || isActivated)
		{
			_tween?.Kill();
			_tween = base.transform.DOScale(defaultScale, 0.1f).SetEase(Ease.Linear);
			clickScaled = false;
			hoverScaled = false;
			isActivated = false;
		}
	}

	private void ClickUp()
	{
		if (button.interactable && !isActivated)
		{
			_tween?.Kill();
			Vector3 endValue = (isMouseOvered ? (hoverScale * defaultScale) : defaultScale);
			_tween = base.transform.DOScale(endValue, 0.1f).SetEase(Ease.Linear);
			clickScaled = false;
		}
	}

	private void ClickDown()
	{
		if (button.interactable && !isActivated)
		{
			_tween?.Kill();
			_tween = base.transform.DOScale(clickScale * defaultScale, 0.1f).SetEase(Ease.Linear);
			clickScaled = true;
		}
	}

	private void MouseOverEnter()
	{
		if (button.interactable && !isActivated)
		{
			_tween?.Kill();
			_tween = base.transform.DOScale(hoverScale * defaultScale, 0.1f).SetEase(Ease.Linear);
			hoverScaled = true;
		}
	}

	private void MouseOverExit()
	{
		if (button.interactable && !isActivated)
		{
			_tween?.Kill();
			Vector3 endValue = (isPressed ? (clickScale * defaultScale) : defaultScale);
			_tween = base.transform.DOScale(endValue, 0.1f).SetEase(Ease.Linear);
			hoverScaled = false;
		}
	}

	public void ActivateUseUI()
	{
		if (!isActivated)
		{
			isActivated = true;
			_tween?.Kill();
			_tween = base.transform.DOScale(activateScale * defaultScale, 0.1f).SetEase(Ease.Linear);
		}
	}

	public void DeactivateUseUI()
	{
		if (isActivated)
		{
			isActivated = false;
			_tween?.Kill();
			Vector3 endValue = (isMouseOvered ? (hoverScale * defaultScale) : defaultScale);
			_tween = base.transform.DOScale(endValue, 0.1f).SetEase(Ease.Linear);
		}
	}

	private void OnDestroy()
	{
		_tween?.Kill();
		_tween = null;
	}
}
