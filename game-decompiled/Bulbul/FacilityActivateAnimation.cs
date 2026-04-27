using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Bulbul;

public class FacilityActivateAnimation : FacilityAnimationBase
{
	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _fromPosY;

	private float _toPosY;

	public override void Setup()
	{
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		_canvasGroup.alpha = 0f;
		Vector2 anchoredPosition = _rectTransform.anchoredPosition;
		anchoredPosition.y = _fromPosY;
		_rectTransform.anchoredPosition = anchoredPosition;
	}

	public override UniTask Activate()
	{
		_moveTween?.Kill();
		_fadeTween?.Kill();
		base.gameObject.SetActive(value: true);
		_moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
		return _fadeTween.ToUniTask();
	}

	public override UniTask Deactivate()
	{
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		return _fadeTween.ToUniTask();
	}

	protected override void OnDestroy()
	{
		_moveTween?.Kill();
		_fadeTween?.Kill();
	}
}
