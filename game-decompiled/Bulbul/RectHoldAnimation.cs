using DG.Tweening;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul;

[DisallowMultipleComponent]
public class RectHoldAnimation : MonoBehaviour
{
	[SerializeField]
	private EventTrigger eventTrigger;

	[SerializeField]
	private float hoverScale = 1.05f;

	[SerializeField]
	private float clickScale = 0.95f;

	private Vector3 defaultScale;

	private bool isPressed;

	private bool isMouseOvered;

	private bool clickScaled;

	private bool hoverScaled;

	private void Start()
	{
		defaultScale = base.transform.localScale;
		ObservableSubscribeExtensions.Subscribe(eventTrigger.OnPointerDownAsObservable(), delegate
		{
			isPressed = true;
			ClickDown();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(eventTrigger.OnPointerUpAsObservable(), delegate
		{
			isPressed = false;
			ClickUp();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(eventTrigger.OnPointerEnterAsObservable(), delegate
		{
			isMouseOvered = true;
			MouseOverEnter();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(eventTrigger.OnPointerExitAsObservable(), delegate
		{
			isMouseOvered = false;
			MouseOverExit();
		}).AddTo(this);
	}

	private void OnDisable()
	{
		if (clickScaled || hoverScaled)
		{
			base.transform.DOScale(defaultScale, 0.1f).SetEase(Ease.Linear);
			clickScaled = false;
			hoverScaled = false;
		}
	}

	private void ClickUp()
	{
		if (!eventTrigger.TryGetComponent<Selectable>(out var component) || component.interactable)
		{
			Vector3 endValue = (isMouseOvered ? (hoverScale * defaultScale) : defaultScale);
			base.transform.DOScale(endValue, 0.1f).SetEase(Ease.Linear);
			clickScaled = false;
		}
	}

	private void ClickDown()
	{
		if (!eventTrigger.TryGetComponent<Selectable>(out var component) || component.interactable)
		{
			base.transform.DOScale(clickScale * defaultScale, 0.1f).SetEase(Ease.Linear);
			clickScaled = true;
		}
	}

	private void MouseOverEnter()
	{
		if (!eventTrigger.TryGetComponent<Selectable>(out var component) || component.interactable)
		{
			base.transform.DOScale(hoverScale * defaultScale, 0.1f).SetEase(Ease.Linear);
			hoverScaled = true;
		}
	}

	private void MouseOverExit()
	{
		if (!eventTrigger.TryGetComponent<Selectable>(out var component) || component.interactable)
		{
			Vector3 endValue = (isPressed ? (clickScale * defaultScale) : defaultScale);
			base.transform.DOScale(endValue, 0.1f).SetEase(Ease.Linear);
			hoverScaled = false;
		}
	}
}
