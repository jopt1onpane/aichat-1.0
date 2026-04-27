using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class FailedReactionIconEffectView : MonoBehaviour
{
	private static readonly float _duration = 0.5f;

	[SerializeField]
	private CanvasGroup _effectObj;

	[SerializeField]
	private Image _effectImage;

	[SerializeField]
	private float xShakeMove = 5f;

	[SerializeField]
	private float yFloatMove = 20f;

	private Camera _mainCamera;

	private RectTransform _rectTransform;

	private int _testIdx;

	private Sequence _sequence;

	private Tween _shakeTween;

	public void Setup()
	{
		_mainCamera = Camera.main;
		_rectTransform = _effectObj.transform as RectTransform;
	}

	public void Activate(Vector2 screenPosition)
	{
		if (_mainCamera == null)
		{
			Debug.LogWarning("FailedReactionIconEffectView:メインカメラがnullです");
			return;
		}
		_mainCamera.ScreenToWorldPoint(screenPosition);
		_shakeTween?.Kill();
		_sequence?.Kill();
		_sequence = DOTween.Sequence();
		_effectObj.gameObject.SetActive(value: true);
		_effectObj.alpha = 0f;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform.parent as RectTransform, screenPosition, null, out var localPoint);
		_rectTransform.anchoredPosition = localPoint;
		_sequence.Join(_effectObj.DOFade(1f, _duration).SetLoops(2, LoopType.Yoyo));
		_sequence.Join(_rectTransform.DOAnchorPosY(_rectTransform.anchoredPosition.y + yFloatMove, _duration));
		_shakeTween = _rectTransform.DOAnchorPosX(_rectTransform.anchoredPosition.x + xShakeMove, 0.05f).SetLoops(20, LoopType.Yoyo);
		_sequence.OnComplete(delegate
		{
			_effectObj.gameObject.SetActive(value: false);
			_shakeTween?.Kill();
			_shakeTween = null;
		});
		_testIdx++;
	}

	private void OnDisable()
	{
		_shakeTween?.Kill();
		_sequence?.Kill();
		_sequence = null;
		_shakeTween = null;
		_effectObj.gameObject.SetActive(value: false);
	}
}
