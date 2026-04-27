using Cysharp.Threading.Tasks;
using DG.Tweening;
using NestopiSystem;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class FacilityBurgerActivateAnimationMobile : FacilityAnimationBase
{
	[SerializeField]
	private CanvasGroup _upperCanvasGroup;

	[SerializeField]
	private CanvasGroup _lowerCanvasGroup;

	private float _upperFromPosY;

	private float _upperToPosY;

	private float _lowerFromPosY;

	private float _lowerToPosY;

	private Sequence sequence;

	private RectTransform _upperRectTransform;

	private RectTransform _lowerRectTransform;

	public override void Setup()
	{
		_upperRectTransform = _upperCanvasGroup.GetComponent<RectTransform>();
		_lowerRectTransform = _lowerCanvasGroup.GetComponent<RectTransform>();
		_upperFromPosY = _upperRectTransform.anchoredPosition.y + Mathf.Abs(270f);
		_upperToPosY = _upperRectTransform.anchoredPosition.y;
		_lowerFromPosY = _lowerRectTransform.anchoredPosition.y - Mathf.Abs(270f);
		_lowerToPosY = _lowerRectTransform.anchoredPosition.y;
		_upperCanvasGroup.alpha = 0f;
		_lowerCanvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
		_upperRectTransform.anchoredPosition = _upperRectTransform.anchoredPosition.WithY(_upperFromPosY);
		_lowerRectTransform.anchoredPosition = _lowerRectTransform.anchoredPosition.WithY(_lowerFromPosY);
	}

	public override UniTask Activate()
	{
		base.gameObject.SetActive(value: true);
		sequence?.Complete();
		sequence = DOTween.Sequence();
		sequence.Join(_upperRectTransform.DOAnchorPosY(_upperToPosY, 0.3f));
		sequence.Join(_lowerRectTransform.DOAnchorPosY(_lowerToPosY, 0.3f));
		sequence.Join(_upperCanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuart));
		sequence.Join(_lowerCanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuart));
		sequence.OnComplete(delegate
		{
			onCompleteActivate?.OnNext(Unit.Default);
		});
		return sequence.ToUniTask();
	}

	public override UniTask Deactivate()
	{
		sequence?.Complete();
		sequence = DOTween.Sequence();
		sequence.Join(_upperRectTransform.DOAnchorPosY(_upperFromPosY, 0.3f));
		sequence.Join(_lowerRectTransform.DOAnchorPosY(_lowerFromPosY, 0.3f));
		sequence.Join(_upperCanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.OutQuart));
		sequence.Join(_lowerCanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.OutQuart));
		sequence.OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
			onCompleteDeactivate?.OnNext(Unit.Default);
		});
		return sequence.ToUniTask();
	}
}
