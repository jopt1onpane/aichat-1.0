using DG.Tweening;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class UIFadeAnimation : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private float fadeDuration = 0.2f;

	[SerializeField]
	private float activeAlpha = 1f;

	[SerializeField]
	private float deactiveAlpha;

	[SerializeField]
	[Tooltip("CanvasGroupのblockRaycastをフェードイン開始時に有効にする")]
	private bool blockRaycastActiveImmediate = true;

	[SerializeField]
	[Tooltip("CanvasGroupのblockRaycastをフェードアウト開始時に無効にする")]
	private bool blockRaycastDeactiveImmediate;

	private Subject<bool> onFadeComplete = new Subject<bool>();

	private Tween fadeTween;

	public Observable<bool> OnFadeComplete => onFadeComplete;

	public void Setup()
	{
		base.gameObject.SetActive(value: false);
		_canvasGroup.alpha = deactiveAlpha;
		_canvasGroup.blocksRaycasts = false;
	}

	public void Activate()
	{
		fadeTween?.Kill();
		base.gameObject.SetActive(value: true);
		if (blockRaycastActiveImmediate)
		{
			_canvasGroup.blocksRaycasts = true;
		}
		fadeTween = _canvasGroup.DOFade(activeAlpha, fadeDuration).OnComplete(delegate
		{
			if (!blockRaycastActiveImmediate)
			{
				_canvasGroup.blocksRaycasts = true;
			}
			onFadeComplete.OnNext(value: true);
		});
	}

	public void Deactivate()
	{
		fadeTween?.Kill();
		if (blockRaycastDeactiveImmediate)
		{
			_canvasGroup.blocksRaycasts = false;
		}
		fadeTween = _canvasGroup.DOFade(deactiveAlpha, fadeDuration).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
			if (!blockRaycastActiveImmediate)
			{
				_canvasGroup.blocksRaycasts = false;
			}
			onFadeComplete.OnNext(value: false);
		});
	}
}
