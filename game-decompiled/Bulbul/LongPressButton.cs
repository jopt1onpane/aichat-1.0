using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul;

[RequireComponent(typeof(Button))]
public class LongPressButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerExitHandler
{
	[SerializeField]
	[Header("初回実行までの遅延時間（秒）")]
	private float _initialDelay = 0.5f;

	[SerializeField]
	[Header("初期インターバル（秒）")]
	private float _initialInterval = 0.3f;

	[SerializeField]
	[Header("最小インターバル（秒）")]
	private float _minInterval = 0.05f;

	[SerializeField]
	[Header("加速度（秒ごとに減少する時間）")]
	private float _acceleration = 0.02f;

	[SerializeField]
	[Header("アニメーション：対象")]
	private Transform _targetTransform;

	[SerializeField]
	[Header("アニメーション：押下時のスケール")]
	private float _pressedScale = 0.9f;

	[SerializeField]
	[Header("アニメーション：アニメーション時間")]
	private float _animationDuration = 0.1f;

	private Button _button;

	private bool _isPressed;

	private float _pressedTime;

	private float _nextTriggerTime;

	private float _currentInterval;

	private Vector3 _originalScale;

	private Tween _currentTween;

	private readonly Subject<Unit> _onLongPress = new Subject<Unit>();

	private readonly Subject<Unit> _onClick = new Subject<Unit>();

	public Observable<Unit> OnLongPress => _onLongPress;

	public Observable<Unit> OnClick => _onClick;

	private void Awake()
	{
		_button = GetComponent<Button>();
		_currentInterval = _initialInterval;
		_originalScale = _targetTransform.localScale;
	}

	private void Update()
	{
		if (_isPressed && _button.interactable)
		{
			_pressedTime += Time.deltaTime;
			if (_pressedTime >= _nextTriggerTime)
			{
				PlayHoldAnimation();
				_onLongPress.OnNext(Unit.Default);
				_currentInterval = Mathf.Max(_minInterval, _currentInterval - _acceleration);
				_nextTriggerTime = _pressedTime + _currentInterval;
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_button.interactable)
		{
			_isPressed = true;
			_pressedTime = 0f;
			_currentInterval = _initialInterval;
			_nextTriggerTime = _initialDelay;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (_button.interactable)
		{
			if (_pressedTime < _initialDelay && _isPressed)
			{
				_onClick.OnNext(Unit.Default);
				_onLongPress.OnNext(Unit.Default);
			}
			_isPressed = false;
			_pressedTime = 0f;
			_currentInterval = _initialInterval;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isPressed = false;
		_pressedTime = 0f;
		_currentInterval = _initialInterval;
	}

	private void PlayHoldAnimation()
	{
		_currentTween?.Kill();
		_currentTween = _targetTransform.DOScale(_originalScale * _pressedScale, _animationDuration * 0.5f).SetEase(Ease.OutQuad).OnComplete(delegate
		{
			_targetTransform.DOScale(_originalScale, _animationDuration * 0.5f).SetEase(Ease.OutQuad);
		});
	}

	private void OnDestroy()
	{
		_currentTween?.Kill();
		_onLongPress?.Dispose();
		_onClick?.Dispose();
	}
}
