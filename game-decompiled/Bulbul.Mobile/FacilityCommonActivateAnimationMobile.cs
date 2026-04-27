using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class FacilityCommonActivateAnimationMobile : FacilityAnimationBase
{
	[SerializeField]
	private CanvasGroup _rootCanvasGroup;

	[SerializeField]
	private CanvasGroup _bgCanvasGroup;

	private float _fromPosY;

	private float _toPosY;

	private Sequence sequence;

	private RectTransform _rootRectTransform;

	public override void Setup()
	{
		_rootRectTransform = _rootCanvasGroup.GetComponent<RectTransform>();
		_fromPosY = _rootRectTransform.anchoredPosition.y + -30f;
		_toPosY = _rootRectTransform.anchoredPosition.y;
		_rootCanvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
		Vector2 anchoredPosition = _rootRectTransform.anchoredPosition;
		anchoredPosition.y = _fromPosY;
		_rootRectTransform.anchoredPosition = anchoredPosition;
	}

	public override UniTask Activate()
	{
		base.gameObject.SetActive(value: true);
		sequence?.Complete();
		sequence = DOTween.Sequence();
		sequence.Join(_rootRectTransform.DOAnchorPosY(_toPosY, 0.2f));
		sequence.Join(_rootCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuart));
		sequence.Join(_bgCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuart));
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
		sequence.Join(_rootRectTransform.DOAnchorPosY(_fromPosY, 0.2f));
		sequence.Join(_rootCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutQuart));
		sequence.Join(_bgCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutQuart));
		sequence.OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
			onCompleteDeactivate?.OnNext(Unit.Default);
		});
		return sequence.ToUniTask();
	}
}
