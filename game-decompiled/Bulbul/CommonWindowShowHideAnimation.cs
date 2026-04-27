using DG.Tweening;
using NestopiSystem;
using UnityEngine;

namespace Bulbul;

public class CommonWindowShowHideAnimation : MonoBehaviour
{
	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private float _moveY;

	public bool DeactivateOnHide;

	private float? _basePosY;

	private Tween _tween;

	public bool IsShowing { get; private set; }

	public bool IsAnimating
	{
		get
		{
			if (_tween != null && _tween.active)
			{
				return _tween.IsPlaying();
			}
			return false;
		}
	}

	public void Show()
	{
		IsShowing = true;
		float valueOrDefault = _basePosY.GetValueOrDefault();
		if (!_basePosY.HasValue)
		{
			valueOrDefault = _rectTransform.anchoredPosition.y;
			_basePosY = valueOrDefault;
		}
		_tween?.Kill();
		_rectTransform.anchoredPosition = _rectTransform.anchoredPosition.WithY(_basePosY.Value - _moveY);
		_canvasGroup.alpha = 0f;
		if (DeactivateOnHide)
		{
			_rectTransform.gameObject.SetActive(value: true);
		}
		_tween = DOTween.Sequence().Append(_rectTransform.DOAnchorPosY(_basePosY.Value, 0.2f)).Join(_canvasGroup.DOFade(1f, 0.2f));
	}

	public void Hide(bool immediate = false)
	{
		IsShowing = false;
		float valueOrDefault = _basePosY.GetValueOrDefault();
		if (!_basePosY.HasValue)
		{
			valueOrDefault = _rectTransform.anchoredPosition.y;
			_basePosY = valueOrDefault;
		}
		_tween?.Kill();
		_tween = DOTween.Sequence().Append(_rectTransform.DOAnchorPosY(_basePosY.Value - _moveY, 0.2f)).Join(_canvasGroup.DOFade(0f, 0.2f));
		if (DeactivateOnHide)
		{
			_tween.OnComplete(delegate
			{
				_rectTransform.gameObject.SetActive(value: false);
			});
		}
		if (immediate)
		{
			_tween.Complete();
		}
	}

	private void OnDestroy()
	{
		_tween?.Kill();
		_tween = null;
	}
}
