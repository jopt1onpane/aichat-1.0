using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class FadeController : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	private bool _isComplete = true;

	private Tween _fadeTween;

	public void Setup()
	{
		_isComplete = true;
		_canvasGroup.alpha = 0f;
	}

	public async UniTask FadeOut(float toSecond = 2f, Ease ease = Ease.Unset)
	{
		_fadeTween?.Kill();
		_canvasGroup.alpha = 0f;
		_canvasGroup.gameObject.SetActive(value: true);
		_isComplete = false;
		_fadeTween = _canvasGroup.DOFade(1f, toSecond).SetEase(ease);
		await _fadeTween;
		_isComplete = true;
	}

	public async UniTask FadeIn(float toSecond = 1.2f, Ease ease = Ease.Unset)
	{
		_fadeTween?.Kill();
		_canvasGroup.alpha = 1f;
		_canvasGroup.gameObject.SetActive(value: true);
		_isComplete = false;
		_fadeTween = _canvasGroup.DOFade(0f, toSecond).SetEase(ease);
		await _fadeTween;
		_fadeTween = null;
		_isComplete = true;
		_canvasGroup.gameObject.SetActive(value: false);
	}

	public void ImmediateFadeIn()
	{
		_fadeTween?.Kill();
		_fadeTween = null;
		_isComplete = true;
		_canvasGroup.alpha = 0f;
		_canvasGroup.gameObject.SetActive(value: false);
	}

	public bool IsComplete()
	{
		return _isComplete;
	}
}
