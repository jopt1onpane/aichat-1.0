using System;
using DG.Tweening;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul;

public class InteractableUI : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	private const float MouseOverActivateTime = 0.15f;

	private const float MouseOverDeactivateTime = 0.2f;

	private const Ease MouseOverActivateEaseType = Ease.Unset;

	private const Ease MouseOverDeactivateEaseType = Ease.Unset;

	private const float UsingActivateTime = 0.15f;

	private const float UsingDeactivateTime = 0.2f;

	private const Ease UsingActivateEaseType = Ease.Unset;

	private const Ease UsingDeactivateEaseType = Ease.Unset;

	private const float BaseActivateTime = 0.15f;

	private const float BaseDeactivateTime = 0.2f;

	private const Ease BaseActivateEaseType = Ease.Unset;

	private const Ease BaseDeactivateEaseType = Ease.Unset;

	[SerializeField]
	[Header("マウスオーバー画像")]
	private Image[] _mouseOverImage;

	[SerializeField]
	private float _mouseOverImageAlpha = 1f;

	private Tween[] _mouseOverImageTween;

	[SerializeField]
	[Header("使用中画像")]
	private Image[] _usingImage;

	[SerializeField]
	private float _usingImageAlpha = 1f;

	private Tween[] _usingImageTween;

	[SerializeField]
	[Header("マウスオーバー/使用中時に非表示にする画像")]
	private Image[] _baseImage;

	private Tween[] _baseImageTween;

	[SerializeField]
	private float _baseImageAlpha = 1f;

	[SerializeField]
	[Header("Using時にマウスオーバー画像を非表示にする")]
	private bool _hideMouseOverOnUse;

	[SerializeField]
	[Header("非アクティブ時に表示リセット")]
	private bool _deactivateOnDisable;

	protected Action _onPointerEnterAction;

	protected Action _onPointerExitAction;

	protected Action _onInteractAction;

	private bool _isMouseOver;

	private bool _isFinishSetup;

	private IDisposable _disposable;

	private RectTransform rectTransform;

	private Button __buttonCacheMobile;

	private bool _isTryGetButtonMobile;

	public bool IsUsing { get; private set; }

	public RectTransform RectTransform
	{
		get
		{
			if (!rectTransform)
			{
				return rectTransform = GetComponent<RectTransform>();
			}
			return rectTransform;
		}
	}

	private Button _buttonCacheMobile
	{
		get
		{
			if (__buttonCacheMobile == null && !_isTryGetButtonMobile)
			{
				TryGetComponent<Button>(out __buttonCacheMobile);
				_isTryGetButtonMobile = true;
			}
			return __buttonCacheMobile;
		}
	}

	public void SetPointerEnterAction(Action action)
	{
		_onPointerEnterAction = action;
	}

	public void SetPointerExitAction(Action action)
	{
		_onPointerExitAction = action;
	}

	public void Awake()
	{
		if (!_isFinishSetup)
		{
			Setup();
		}
		if (DevicePlatform.Steam.IsMobile())
		{
			return;
		}
		_disposable = ObservableSubscribeExtensions.Subscribe(Observable.Interval(TimeSpan.FromSeconds(0.20000000298023224)), delegate
		{
			if (InputController.Instance.CurrentFrameEventSystemRaycastHitObject?.GetComponentInParent<InteractableUI>() == this)
			{
				if (!_isMouseOver)
				{
					PointerEnter();
				}
			}
			else if (_isMouseOver)
			{
				PointerExit();
			}
		});
	}

	public void Setup()
	{
		if (!_isFinishSetup)
		{
			SetupCore();
		}
	}

	protected virtual void SetupCore()
	{
		_mouseOverImageTween = new Tween[_mouseOverImage.Length];
		_usingImageTween = new Tween[_usingImage.Length];
		_baseImageTween = new Tween[_baseImage.Length];
		DeactivateAllUI(isUseDoComplete: true);
		_isFinishSetup = true;
	}

	private void OnDisable()
	{
		if (_deactivateOnDisable)
		{
			DeactivateAllUI(isUseDoComplete: true);
		}
	}

	public void OnDestroy()
	{
		KillAllTweens();
		_disposable?.Dispose();
	}

	private void KillAllTweens()
	{
		if (_mouseOverImageTween != null)
		{
			for (int i = 0; i < _mouseOverImageTween.Length; i++)
			{
				_mouseOverImageTween[i]?.Kill();
			}
		}
		if (_usingImageTween != null)
		{
			for (int j = 0; j < _usingImageTween.Length; j++)
			{
				_usingImageTween[j]?.Kill();
			}
		}
		if (_baseImageTween != null)
		{
			for (int k = 0; k < _baseImageTween.Length; k++)
			{
				_baseImageTween[k]?.Kill();
			}
		}
	}

	public void ActivateUseUI(bool isUseDoComplete = false)
	{
		if (!IsUsing)
		{
			IsUsing = true;
			DeactivateBaseImage(isUseDoComplete);
			if (_isMouseOver && _hideMouseOverOnUse)
			{
				DeactivateMouseOverImage(isUseDoComplete);
			}
			ActivateUsingImage(isUseDoComplete);
			_onInteractAction?.Invoke();
		}
	}

	public void DeactivateUseUI(bool isUseDoComplete = false)
	{
		if (IsUsing)
		{
			IsUsing = false;
			DeactivateUsingImage(isUseDoComplete);
			if (_isMouseOver && _hideMouseOverOnUse)
			{
				ActivateMouseOverImage(isUseDoComplete);
			}
			else
			{
				ActivateBaseImage(isUseDoComplete);
			}
		}
	}

	public void SetUseUI(bool isUse, bool isDoComplete = false)
	{
		if (isUse)
		{
			ActivateUseUI(isDoComplete);
		}
		else
		{
			DeactivateUseUI(isDoComplete);
		}
	}

	public void DeactivateAllUI(bool isUseDoComplete = false)
	{
		IsUsing = false;
		_isMouseOver = false;
		DeactivateMouseOverImage(isUseDoComplete);
		DeactivateUsingImage(isUseDoComplete);
		ActivateBaseImage(isUseDoComplete);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		PointerEnter();
	}

	private void PointerEnter()
	{
		if (DevicePlatform.Steam.IsMobile() || !base.enabled)
		{
			return;
		}
		_isMouseOver = true;
		if (!IsUsing)
		{
			if (_mouseOverImage.Length != 0)
			{
				DeactivateBaseImage();
			}
			ActivateMouseOverImage();
			_onPointerEnterAction?.Invoke();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		PointerExit();
	}

	private void PointerExit()
	{
		_isMouseOver = false;
		if (!IsUsing)
		{
			DeactivateMouseOverImage();
			ActivateBaseImage();
			_onPointerExitAction?.Invoke();
		}
	}

	protected virtual void ActivateMouseOverImage(bool isUseDoComplete = false)
	{
		if (_mouseOverImageTween == null)
		{
			return;
		}
		for (int i = 0; i < _mouseOverImageTween.Length; i++)
		{
			if (isUseDoComplete)
			{
				_mouseOverImageTween[i]?.Kill();
				_mouseOverImage[i]?.SetAlpha(_mouseOverImageAlpha);
			}
			else
			{
				_mouseOverImageTween[i]?.Kill();
				_mouseOverImageTween[i] = _mouseOverImage[i]?.DOFade(_mouseOverImageAlpha, 0.15f).SetEase(Ease.Unset).SetLink(base.gameObject);
			}
		}
	}

	protected virtual void DeactivateMouseOverImage(bool isUseDoComplete = false)
	{
		if (_mouseOverImageTween == null)
		{
			return;
		}
		for (int i = 0; i < _mouseOverImage.Length; i++)
		{
			if (isUseDoComplete)
			{
				_mouseOverImageTween[i]?.Kill();
				_mouseOverImage[i]?.SetAlpha(0f);
			}
			else
			{
				_mouseOverImageTween[i]?.Kill();
				_mouseOverImageTween[i] = _mouseOverImage[i]?.DOFade(0f, 0.2f).SetEase(Ease.Unset).SetLink(base.gameObject);
			}
		}
	}

	protected virtual void ActivateUsingImage(bool isUseDoComplete = false)
	{
		if (_usingImageTween == null)
		{
			return;
		}
		for (int i = 0; i < _usingImageTween.Length; i++)
		{
			if (isUseDoComplete)
			{
				_usingImageTween[i]?.Kill();
				_usingImage[i]?.SetAlpha(_usingImageAlpha);
			}
			else
			{
				_usingImageTween[i]?.Kill();
				_usingImageTween[i] = _usingImage[i]?.DOFade(_usingImageAlpha, 0.15f).SetEase(Ease.Unset).SetLink(base.gameObject);
			}
		}
	}

	protected virtual void DeactivateUsingImage(bool isUseDoComplete = false)
	{
		if (_usingImageTween == null)
		{
			return;
		}
		for (int i = 0; i < _usingImage.Length; i++)
		{
			if (isUseDoComplete)
			{
				_usingImageTween[i]?.Kill();
				_usingImage[i]?.SetAlpha(0f);
			}
			else
			{
				_usingImageTween[i]?.Kill();
				_usingImageTween[i] = _usingImage[i]?.DOFade(0f, 0.2f).SetEase(Ease.Unset).SetLink(base.gameObject);
			}
		}
	}

	protected virtual void ActivateBaseImage(bool isUseDoComplete = false)
	{
		if (_baseImageTween == null)
		{
			return;
		}
		for (int i = 0; i < _baseImageTween.Length; i++)
		{
			if (isUseDoComplete)
			{
				_baseImageTween[i]?.Kill();
				_baseImage[i]?.SetAlpha(_baseImageAlpha);
			}
			else
			{
				_baseImageTween[i]?.Kill();
				_baseImageTween[i] = _baseImage[i]?.DOFade(_baseImageAlpha, 0.15f).SetEase(Ease.Unset).SetLink(base.gameObject);
			}
		}
	}

	protected virtual void DeactivateBaseImage(bool isUseDoComplete = false)
	{
		if (_baseImageTween == null)
		{
			return;
		}
		for (int i = 0; i < _baseImage.Length; i++)
		{
			if (isUseDoComplete)
			{
				_baseImageTween[i]?.Kill();
				_baseImage[i]?.SetAlpha(0f);
			}
			else
			{
				_baseImageTween[i]?.Kill();
				_baseImageTween[i] = _baseImage[i]?.DOFade(0f, 0.2f).SetEase(Ease.Unset).SetLink(base.gameObject);
			}
		}
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if (!DevicePlatform.Steam.IsMobile())
		{
			return;
		}
		if (_buttonCacheMobile != null)
		{
			Button buttonCacheMobile = _buttonCacheMobile;
			if (!buttonCacheMobile.enabled || !buttonCacheMobile.interactable)
			{
				PointerExit();
				return;
			}
		}
		_isMouseOver = true;
		if (!IsUsing)
		{
			if (_mouseOverImage.Length != 0)
			{
				DeactivateBaseImage();
			}
			ActivateMouseOverImage();
			_onPointerEnterAction?.Invoke();
		}
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if (DevicePlatform.Steam.IsMobile())
		{
			PointerExit();
		}
	}
}
