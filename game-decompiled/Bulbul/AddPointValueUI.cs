using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Bulbul;

public class AddPointValueUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private TextMeshProUGUI _pointText;

	[SerializeField]
	[Header("フェード秒数")]
	private float _fadeDuration;

	[SerializeField]
	[Header("フェードEaseType")]
	private Ease _fadeEaseType;

	[SerializeField]
	[Header("スライド秒数")]
	private float _slideDuration;

	[SerializeField]
	[Header("スライドEaseType")]
	private Ease _slideEaseType;

	private float _fromPosY;

	private Tween _tween;

	public void Initialize()
	{
		Reset();
	}

	public void Reset()
	{
		_canvasGroup.alpha = 0f;
		_tween?.Kill();
	}

	private void OnDestroy()
	{
		_tween?.Kill();
	}

	public void StartAnim(int point, float fromPosY, float toPosY)
	{
		TextMeshProUGUI pointText = _pointText;
		int num = point;
		pointText.text = num.ToString();
		_fromPosY = fromPosY;
		RectTransform rectTransform = base.transform as RectTransform;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.y = _fromPosY;
		rectTransform.anchoredPosition = anchoredPosition;
		_tween?.Kill();
		_tween = DOTween.Sequence().Append(_canvasGroup.DOFade(1f, _fadeDuration).SetEase(_fadeEaseType)).Join(rectTransform.DOAnchorPosY(toPosY, _slideDuration).SetEase(_slideEaseType));
	}

	public void EndAnim(Action onComplete)
	{
		_tween?.Kill();
		RectTransform target = base.transform as RectTransform;
		_tween = DOTween.Sequence().Append(_canvasGroup.DOFade(0f, _fadeDuration).SetEase(_fadeEaseType)).Join(target.DOAnchorPosY(_fromPosY, _slideDuration).SetEase(_slideEaseType))
			.OnComplete(delegate
			{
				onComplete?.Invoke();
			});
	}
}
