using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class FacilitySettingActivateAnimationMobile : FacilityAnimationBase
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	private Tween moveTween;

	private Tween fadeTween;

	private RectTransform _rectTransform;

	private float _fromPosY;

	private float _toPosY;

	public override void Setup()
	{
		base.gameObject.SetActive(value: false);
		_canvasGroup.alpha = 0f;
		_canvasGroup.blocksRaycasts = false;
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -30f;
		_toPosY = _rectTransform.anchoredPosition.y;
	}

	public override UniTask Activate()
	{
		moveTween?.Kill();
		fadeTween?.Kill();
		base.gameObject.SetActive(value: true);
		_canvasGroup.blocksRaycasts = true;
		moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f).SetEase(Ease.OutQuart);
		fadeTween = _canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuart).OnComplete(delegate
		{
			onCompleteActivate.OnNext(Unit.Default);
		});
		return fadeTween.ToUniTask();
	}

	public override UniTask Deactivate()
	{
		moveTween?.Kill();
		fadeTween?.Kill();
		moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f).SetEase(Ease.OutQuart);
		fadeTween = _canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutQuart).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
			_canvasGroup.blocksRaycasts = false;
			onCompleteDeactivate.OnNext(Unit.Default);
		});
		return fadeTween.ToUniTask();
	}
}
