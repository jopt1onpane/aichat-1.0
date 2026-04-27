using DG.Tweening;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class ItemSkinSelectorShowAnim : MonoBehaviour
{
	[SerializeField]
	private RectTransform _spaceTransform;

	[SerializeField]
	private float _spaceShowSize = 100f;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private ClickOutsideDetector _clickOutsideDetector;

	public ItemSkinSelectorUI SelectorUI;

	private EnableCounter _clickOutsideDisableCounter;

	private Tween _tween;

	private RectTransform _layoutTransform;

	private int _showFrame = -1;

	private ScrollRect _parentScrollRect;

	private bool _isSkipAdjustment = true;

	public bool IsShowing { get; private set; }

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		Vector2 sizeDelta = _spaceTransform.sizeDelta;
		sizeDelta.y = 0f;
		_spaceTransform.sizeDelta = sizeDelta;
		_canvasGroup.alpha = 0f;
		_clickOutsideDisableCounter = new EnableCounter();
		_clickOutsideDisableCounter.Enabled.Subscribe(delegate(bool disable)
		{
			_clickOutsideDetector.enabled = !disable;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_clickOutsideDetector.OnClickOutside, delegate
		{
			if (Time.frameCount != _showFrame)
			{
				Hide();
			}
		}).AddTo(this);
		_layoutTransform = _spaceTransform.GetComponentInParent<LayoutGroup>(includeInactive: true).transform as RectTransform;
		_parentScrollRect = _spaceTransform.GetComponentInParent<ScrollRect>(includeInactive: true);
	}

	public void Show(DecorationService.DecorationModelType model)
	{
		IsShowing = true;
		_showFrame = Time.frameCount;
		_isSkipAdjustment = true;
		_tween?.Kill();
		Vector2 sizeDelta = _spaceTransform.sizeDelta;
		_spaceTransform.sizeDelta = sizeDelta.WithY(Mathf.Min(_spaceShowSize * 0.85f, sizeDelta.y));
		_canvasGroup.alpha = Mathf.Min(0.5f, _canvasGroup.alpha);
		base.gameObject.SetActive(value: true);
		Vector2 endValue = _spaceTransform.sizeDelta.WithY(_spaceShowSize);
		_tween = DOTween.Sequence().Append(_spaceTransform.DOSizeDelta(endValue, 0.2f).SetEase(Ease.InOutSine)).Join(_canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.InOutSine))
			.OnUpdate(delegate
			{
				LayoutRebuilder.MarkLayoutForRebuild(_layoutTransform);
				AdjustScrollPosition();
				_isSkipAdjustment = false;
			})
			.OnComplete(delegate
			{
				AdjustScrollPosition();
			});
	}

	private void AdjustScrollPosition()
	{
		if (_isSkipAdjustment)
		{
			return;
		}
		float height = _parentScrollRect.viewport.rect.height;
		float num = _spaceTransform.localPosition.y - _spaceTransform.sizeDelta.y;
		float y = _parentScrollRect.content.transform.localPosition.y;
		float num2 = height + (num + y);
		if (num2 < 0f)
		{
			float num3 = _parentScrollRect.content.rect.height - height;
			if (num3 != 0f)
			{
				float num4 = num2 / num3;
				Vector2 normalizedPosition = _parentScrollRect.normalizedPosition;
				normalizedPosition.y += num4;
				_parentScrollRect.normalizedPosition = normalizedPosition;
			}
		}
	}

	public void Hide()
	{
		IsShowing = false;
		_tween?.Kill();
		Vector2 endValue = _spaceTransform.sizeDelta.WithY(0f);
		_tween = DOTween.Sequence().Append(_spaceTransform.DOSizeDelta(endValue, 0.2f).SetEase(Ease.InOutSine)).Join(_canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InOutSine))
			.OnUpdate(delegate
			{
				LayoutRebuilder.MarkLayoutForRebuild(_layoutTransform);
			})
			.OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
			});
	}

	public EnableCounter.EnableScope DisableCloseOnClickOutsideScope()
	{
		return _clickOutsideDisableCounter.CreateEnableScope();
	}

	private void OnDestroy()
	{
		_tween?.Kill();
		_tween = null;
	}
}
