using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class ToggleSwitch : MonoBehaviour
{
	private enum ToggleState
	{
		OFF,
		ON
	}

	private class SwitchTransition : IToggleStyleButtonTransition
	{
		private RectTransform maskRect;

		private float maskMaxWidth;

		private RectTransform handleRect;

		private Vector3 handleOff;

		private Vector3 handleOn;

		private float duration;

		private Ease easing;

		private bool isTransitioning;

		private Tween handleTween;

		private Tween maskTween;

		bool IToggleStyleButtonTransition.IsTrantioning => isTransitioning;

		public SwitchTransition(RectTransform handleRect, Vector3 handleOn, Vector3 handleOff, RectTransform maskRect, float maskMaxWidth, float duration, Ease easing)
		{
			this.maskRect = maskRect;
			this.maskMaxWidth = maskMaxWidth;
			this.handleRect = handleRect;
			this.handleOff = handleOff;
			this.handleOn = handleOn;
			this.duration = duration;
			this.easing = easing;
			isTransitioning = false;
		}

		void IToggleStyleButtonTransition.Transition(bool isOn, bool isImmediate)
		{
			maskTween?.Kill();
			handleTween?.Kill();
			if (isOn)
			{
				Vector2 sizeDelta = maskRect.sizeDelta;
				sizeDelta.x = maskMaxWidth;
				if (isImmediate)
				{
					maskRect.sizeDelta = sizeDelta;
					handleRect.anchoredPosition = handleOn;
					isTransitioning = false;
					return;
				}
				isTransitioning = true;
				maskTween = maskRect.DOSizeDelta(sizeDelta, duration);
				handleTween = handleRect.DOAnchorPos(handleOn, duration).SetEase(easing).OnComplete(delegate
				{
					isTransitioning = false;
				});
				return;
			}
			Vector2 sizeDelta2 = maskRect.sizeDelta;
			sizeDelta2.x = 0f;
			if (isImmediate)
			{
				maskRect.sizeDelta = sizeDelta2;
				handleRect.anchoredPosition = handleOff;
				isTransitioning = false;
				return;
			}
			isTransitioning = true;
			maskTween = maskRect.DOSizeDelta(sizeDelta2, duration);
			handleTween = handleRect.DOAnchorPos(handleOff, duration).SetEase(easing).OnComplete(delegate
			{
				isTransitioning = false;
			});
		}
	}

	[SerializeField]
	private Button toggleButton;

	[SerializeField]
	private RectTransform maskTargetRect;

	[SerializeField]
	private RectTransform maskRect;

	[SerializeField]
	private Image toggleHandle;

	[SerializeField]
	private RectTransform handleAnchor_OFF;

	[SerializeField]
	private RectTransform handleAnchor_ON;

	[SerializeField]
	private float duration;

	[SerializeField]
	private Ease easing = Ease.Linear;

	[SerializeField]
	private bool isAnimationDebug;

	private Subject<bool> _onValueChanged = new Subject<bool>();

	private ToggleState _state;

	private IToggleStyleButtonTransition _transition;

	public Observable<bool> OnValueChanged => _onValueChanged;

	private void OnDestroy()
	{
		_onValueChanged?.Dispose();
	}

	public void Setup()
	{
		if (_transition == null)
		{
			_transition = new SwitchTransition(toggleHandle.rectTransform, handleAnchor_ON.anchoredPosition, handleAnchor_OFF.anchoredPosition, maskRect, maskTargetRect.sizeDelta.x, duration, easing);
		}
		ObservableSubscribeExtensions.Subscribe(toggleButton.OnClickAsObservable(), delegate
		{
			OnClickToggle(_state != ToggleState.ON);
		}).AddTo(this);
	}

	private void OnClickToggle(bool isOn)
	{
		if (!_transition.IsTrantioning)
		{
			_onValueChanged?.OnNext(isOn);
		}
	}

	public void SetToggle(bool value, bool isImmediate = false, bool isNotify = false)
	{
		_state = (value ? ToggleState.ON : ToggleState.OFF);
		if (isAnimationDebug)
		{
			_transition = new SwitchTransition(toggleHandle.rectTransform, handleAnchor_ON.anchoredPosition, handleAnchor_OFF.anchoredPosition, maskRect, maskTargetRect.sizeDelta.x, duration, easing);
		}
		_transition?.Transition(value, isImmediate);
		if (isNotify)
		{
			_onValueChanged?.OnNext(value);
		}
	}
}
