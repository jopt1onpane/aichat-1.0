using System;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace Bulbul;

public class AddExpValueUI : MonoBehaviour, IPooledObject<AddExpValueUI>
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private TextMeshProUGUI _expText;

	[SerializeField]
	[Header("フェード秒数")]
	private float _fadeDuration;

	[SerializeField]
	[Header("フェードEaseType")]
	private Ease _fadeEaseType;

	[SerializeField]
	[Header("スライド秒数")]
	private float _slideDuration;

	[SerializeField]
	[Header("スライドEaseType")]
	private Ease _slideEaseType;

	[SerializeField]
	private float _initPosY;

	[SerializeField]
	private float _toPosY;

	private bool _isPlaying;

	private Tween _fadeTween;

	private Tween _slideTween;

	private Action _onEndAnimation;

	private IPlayerLevelUIService _playerLevelUIService;

	private IObjectPool<AddExpValueUI> _objectPool;

	private bool _isReleased;

	public IObjectPool<AddExpValueUI> ObjectPool
	{
		set
		{
			_objectPool = value;
		}
	}

	public void Initialize()
	{
		_isReleased = false;
	}

	public void Deactivate()
	{
		if (_objectPool != null && !_isReleased)
		{
			_isReleased = true;
			_fadeTween?.Kill();
			_slideTween?.Kill();
			_objectPool.Release(this);
		}
	}

	private void OnDestroy()
	{
		_fadeTween?.Kill();
		_slideTween?.Kill();
	}

	public void Setup(float toPosY, IPlayerLevelUIService levelUIService)
	{
		if (_playerLevelUIService == null)
		{
			_playerLevelUIService = levelUIService;
			ObservableSubscribeExtensions.Subscribe(_playerLevelUIService.OnEndShowExpValue, delegate
			{
				if (base.gameObject.activeSelf)
				{
					EndAnim();
				}
			}).AddTo(this);
		}
		_toPosY = toPosY;
	}

	public void StartAnim(float exp, Action onEndAnimation)
	{
		if (!_isPlaying)
		{
			_isPlaying = true;
			_expText.text = ((int)exp).ToString();
			Vector2 anchoredPosition = base.transform.GetComponent<RectTransform>().anchoredPosition;
			anchoredPosition.y = _initPosY;
			base.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			_fadeTween?.Kill();
			_fadeTween = _canvasGroup.DOFade(1f, _fadeDuration).SetEase(_fadeEaseType);
			_slideTween?.Kill();
			_slideTween = base.transform.GetComponent<RectTransform>().DOAnchorPosY(_toPosY, _slideDuration).SetEase(_slideEaseType);
			_onEndAnimation = onEndAnimation;
		}
	}

	public void EndAnim()
	{
		_fadeTween?.Kill();
		_fadeTween = _canvasGroup.DOFade(0f, _fadeDuration).SetEase(_fadeEaseType).OnComplete(delegate
		{
			_isPlaying = false;
			_onEndAnimation();
		});
		_slideTween?.Kill();
		_slideTween = base.transform.GetComponent<RectTransform>().DOAnchorPosY(_initPosY, _slideDuration).SetEase(_slideEaseType);
	}
}
