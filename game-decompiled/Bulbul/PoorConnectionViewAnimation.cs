using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NestopiSystem;
using TMPro;
using UnityEngine;

namespace Bulbul;

public class PoorConnectionViewAnimation : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private RectTransform _iconRotationRectTransform;

	[SerializeField]
	private RectTransform _slideRectTransform;

	[SerializeField]
	private float _slideInFromX = -20f;

	[SerializeField]
	private float _slideInToX;

	[SerializeField]
	private bool _enableCautionTextSlideOut;

	[SerializeField]
	private RectTransform _cautionTextRectTransform;

	[SerializeField]
	private TMP_Text _cautionText;

	[SerializeField]
	private float _slideOutCautionFromX;

	[SerializeField]
	private float _slideOutCautionToX = -200f;

	[SerializeField]
	private CanvasGroup _bgCanvasGroup;

	[SerializeField]
	private GameObject _bgImage;

	private Tween _rotationTweem;

	private Tween _fadeTween;

	private Tween _slideInTweem;

	private Tween _textSlideOutTween;

	private Tween _textSlideOutFadeTween;

	private Tween _bgFadeTween;

	private CancellationTokenSource _cts;

	private void OnDestroy()
	{
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
	}

	private void OnDisable()
	{
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
		_rotationTweem?.Kill();
		_fadeTween?.Kill();
		_slideInTweem?.Kill();
		_textSlideOutTween?.Kill();
		_textSlideOutFadeTween?.Kill();
		_bgFadeTween?.Kill();
		_canvasGroup.alpha = 1f;
		_iconRotationRectTransform.localRotation = Quaternion.AngleAxis(0f, new Vector3(0f, 0f, 1f));
		_slideRectTransform.anchoredPosition = new Vector2(_slideOutCautionFromX, _slideRectTransform.anchoredPosition.y);
		_cautionTextRectTransform.gameObject.SetActive(!_enableCautionTextSlideOut);
		_cautionText.SetAlpha(1f);
		_cautionTextRectTransform.anchoredPosition = new Vector2(_slideOutCautionFromX, _cautionTextRectTransform.anchoredPosition.y);
		_bgCanvasGroup.alpha = 1f;
		_bgImage.gameObject.SetActive(!_enableCautionTextSlideOut);
	}

	public async UniTask PlayActivationAnimation()
	{
		_rotationTweem?.Kill();
		_fadeTween?.Kill();
		_slideInTweem?.Kill();
		_textSlideOutTween?.Kill();
		_textSlideOutFadeTween?.Kill();
		_bgFadeTween?.Kill();
		_canvasGroup.alpha = 0f;
		_iconRotationRectTransform.localRotation = Quaternion.AngleAxis(0f, new Vector3(0f, 0f, 1f));
		_cautionTextRectTransform.gameObject.SetActive(value: true);
		_cautionText.SetAlpha(1f);
		_cautionTextRectTransform.anchoredPosition = new Vector2(_slideOutCautionFromX, _cautionTextRectTransform.anchoredPosition.y);
		_slideRectTransform.anchoredPosition = new Vector2(_slideInFromX, _slideRectTransform.anchoredPosition.y);
		_bgCanvasGroup.alpha = 0f;
		_bgImage.SetActive(value: true);
		_rotationTweem = _iconRotationRectTransform.DOLocalRotate(new Vector3(0f, 0f, 360f), 4.9f, RotateMode.FastBeyond360);
		_fadeTween = _canvasGroup.DOFade(1f, 0.7f).SetLoops(7, LoopType.Yoyo);
		_slideInTweem = _slideRectTransform.DOAnchorPosX(_slideInToX, 0.25f);
		_bgFadeTween = _bgCanvasGroup.DOFade(1f, 0.25f);
		if (_enableCautionTextSlideOut)
		{
			_cts?.Cancel();
			_cts?.Dispose();
			_cts = new CancellationTokenSource();
			await _fadeTween.AwaitForComplete(TweenCancelBehaviour.Kill, _cts.Token);
			await UniTask.WaitForSeconds(4f, ignoreTimeScale: false, PlayerLoopTiming.Update, _cts.Token);
			PlaySludeOutCautionTextAnimation(_cts.Token).Forget();
		}
	}

	private async UniTask PlaySludeOutCautionTextAnimation(CancellationToken cancellationToken)
	{
		_textSlideOutTween?.Kill();
		_textSlideOutFadeTween?.Kill();
		_bgFadeTween?.Complete();
		_textSlideOutTween = _cautionTextRectTransform.DOAnchorPosX(_slideOutCautionToX, 0.3f).SetEase(Ease.Linear);
		_textSlideOutFadeTween = _cautionText.DOFade(0f, 0.2f).SetEase(Ease.InOutQuint);
		_bgFadeTween = _bgCanvasGroup.DOFade(0f, 0.25f);
		await _textSlideOutTween.AwaitForComplete(TweenCancelBehaviour.Kill, cancellationToken);
		_textSlideOutFadeTween?.Complete();
		_bgFadeTween?.Complete();
		_cautionText.gameObject.SetActive(value: false);
		_bgImage.SetActive(value: false);
	}
}
