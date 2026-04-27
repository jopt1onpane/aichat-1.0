using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class PurchasePopover : MonoBehaviour
{
	[SerializeField]
	private Image _itemImage;

	[SerializeField]
	private TMP_Text _priceText;

	[SerializeField]
	private Button _purchaseButton;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private ClickOutsideDetector _clickOutsideDetector;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private CanvasGroup _buttonCanvasGroup;

	[SerializeField]
	private Animator _purchaseCompletedEffectAnimator;

	private UniTaskCompletionSource _hideSource;

	private CancellationTokenSource _purchaseCompletedEffectAnimationCancelletionTokenSource;

	private const float _showMoveY = -10f;

	private bool _isShowing;

	private Tween _showTween;

	private bool _isPlayingPurchaseCompletedAnimation;

	private static readonly Vector2 baseScreenSize = new Vector2(1920f, 1080f);

	private bool _canParchase;

	private ScrollRect _scrollRectForParentVerificationWithinViewport;

	private float _parentHeight;

	private Transform _parent;

	private Vector3 _offset;

	private RectTransform __rectTransform;

	public bool IsShowing => _isShowing;

	private bool _enableFollowLimited => _scrollRectForParentVerificationWithinViewport != null;

	private RectTransform _rectTransform
	{
		get
		{
			if (__rectTransform == null)
			{
				__rectTransform = base.transform as RectTransform;
			}
			return __rectTransform;
		}
	}

	public void Setup(ScrollRect scrollRectForParentVerificationWithinViewport)
	{
		ObservableSubscribeExtensions.Subscribe(_closeButton.OnClickAsObservable(), delegate
		{
			Hide();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_clickOutsideDetector.OnClickOutside, delegate
		{
			Hide();
		}).AddTo(this);
		base.gameObject.SetActive(value: false);
		_scrollRectForParentVerificationWithinViewport = scrollRectForParentVerificationWithinViewport;
	}

	public void Show(Sprite itemSprite, int price, bool canPurchase, Func<bool> purchaseFunc, Transform parent, float parentHeight, Vector2 offset)
	{
		_parentHeight = parentHeight;
		_showTween?.Kill();
		_hideSource?.TrySetResult();
		_purchaseCompletedEffectAnimationCancelletionTokenSource?.Cancel();
		_purchaseCompletedEffectAnimationCancelletionTokenSource?.Dispose();
		_purchaseCompletedEffectAnimationCancelletionTokenSource = null;
		RectTransform rectTransform = _rectTransform;
		_isShowing = true;
		_isPlayingPurchaseCompletedAnimation = false;
		_itemImage.sprite = itemSprite;
		_priceText.text = price.ToString();
		_priceText.color = (canPurchase ? Color.white : MyDefine.CommonRed);
		_purchaseButton.onClick.RemoveAllListeners();
		_purchaseButton.onClick.AddListener(delegate
		{
			purchaseFunc();
		});
		_buttonCanvasGroup.interactable = false;
		_buttonCanvasGroup.blocksRaycasts = false;
		_canParchase = canPurchase;
		_offset = offset;
		_parent = parent;
		Vector3 vector = parent.position + CalcOffset();
		rectTransform.position = vector - new Vector3(0f, -10f);
		base.gameObject.SetActive(value: true);
		_showTween = DOTween.Sequence().Append(rectTransform.DOMoveY(-10f, 0.2f).SetRelative()).Join(_canvasGroup.DOFade(1f, 0.2f))
			.Join(_purchaseButton.transform.DOScale(new Vector3(1.2f, 1.2f, 1f), 0.1f).SetLoops(2, LoopType.Yoyo))
			.OnComplete(delegate
			{
				_buttonCanvasGroup.interactable = _canParchase;
				_buttonCanvasGroup.blocksRaycasts = _canParchase;
			});
		_hideSource = new UniTaskCompletionSource();
	}

	private Vector3 CalcOffset()
	{
		float num = (float)Screen.width / baseScreenSize.x;
		float num2 = (float)Screen.height / baseScreenSize.y;
		return new Vector3(_offset.x * num, _offset.y * num2);
	}

	private void Update()
	{
		if (!_isShowing || (_showTween != null && _showTween.IsPlaying()) || _isPlayingPurchaseCompletedAnimation)
		{
			return;
		}
		Vector3 position = _parent.position + CalcOffset();
		base.transform.position = position;
		if (!_enableFollowLimited)
		{
			return;
		}
		RectTransform viewport = _scrollRectForParentVerificationWithinViewport.viewport;
		if (viewport.pivot.y >= 1f)
		{
			float y = viewport.position.y;
			float num = viewport.position.y - viewport.rect.height * viewport.transform.lossyScale.y;
			float num2 = _parentHeight * _parent.lossyScale.y * 0.5f;
			float num3 = _parent.position.y + num2;
			if (_parent.position.y - num2 > y)
			{
				Hide();
			}
			else if (num3 < num)
			{
				Hide();
			}
		}
	}

	private void OnDestroy()
	{
		if (_purchaseCompletedEffectAnimationCancelletionTokenSource != null)
		{
			_purchaseCompletedEffectAnimationCancelletionTokenSource.Cancel();
			_purchaseCompletedEffectAnimationCancelletionTokenSource.Dispose();
			_purchaseCompletedEffectAnimationCancelletionTokenSource = null;
		}
	}

	public UniTask WaitHide()
	{
		return _hideSource.Task;
	}

	public void Hide()
	{
		if (_isShowing)
		{
			_isShowing = false;
			RectTransform target = base.transform as RectTransform;
			_showTween?.Kill();
			_showTween = DOTween.Sequence().Append(target.DOLocalMoveY(10f, 0.2f).SetRelative()).Join(_canvasGroup.DOFade(0f, 0.2f))
				.OnComplete(delegate
				{
					base.gameObject.SetActive(value: false);
				});
			_hideSource.TrySetResult();
		}
	}

	public void HideWithPurchaseAnim()
	{
		_purchaseCompletedEffectAnimationCancelletionTokenSource = new CancellationTokenSource();
		PlayPurchaseCompletedAnimation(_purchaseCompletedEffectAnimationCancelletionTokenSource.Token, Hide).Forget();
	}

	private async UniTask PlayPurchaseCompletedAnimation(CancellationToken cancellationToken, Action completedAction = null)
	{
		_isPlayingPurchaseCompletedAnimation = true;
		GameObject effectObj = _purchaseCompletedEffectAnimator.gameObject;
		effectObj.SetActive(value: true);
		bool num = await UniTask.WaitUntil(() => _purchaseCompletedEffectAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
		effectObj.SetActive(value: false);
		_isPlayingPurchaseCompletedAnimation = false;
		if (!num)
		{
			completedAction?.Invoke();
		}
	}
}
