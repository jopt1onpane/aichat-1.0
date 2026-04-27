using System;
using Bulbul;
using DG.Tweening;
using NestopiSystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PulldownListUI : MonoBehaviour
{
	[SerializeField]
	[Header("現在選択中の要素のテキスト表示")]
	private TMP_Text _currentSelectContentText;

	[SerializeField]
	[Header("プルダウンの親")]
	private RectTransform _pullDownParentRect;

	[SerializeField]
	[Header("プルダウンする為のHeight変更目標")]
	private float _openPullDownSizeDeltaY;

	[SerializeField]
	[Header("プルダウン開閉秒数")]
	private float _pullDownOpenCloseSeconds = 0.3f;

	[SerializeField]
	[Header("プルダウン開閉Ease")]
	private Ease _pullDownOpenCloseEase = Ease.OutCubic;

	[SerializeField]
	[Header("プルダウン開閉ボタン")]
	private Button _pullDownOpenButton;

	[SerializeField]
	[Header("プルダウン開閉ボタンのRectTransform")]
	private RectTransform _pullDownButtonRect;

	private float _closePullDownSizeDeltaY;

	private bool _isOpen;

	private Tween _openCloseTween;

	private Canvas _canvas;

	private SystemSeService _systemSeService;

	private Subject<Unit> _onClosePulldownComplete;

	public TMP_Text CurrentSelectContentText => _currentSelectContentText;

	public Observable<Unit> OnClickPullDownButton => _pullDownOpenButton.OnClickAsObservable();

	public Observable<Unit> OnClosePulldownComplete => _onClosePulldownComplete;

	public bool IsOpen => _isOpen;

	public void Setup()
	{
		if (_systemSeService == null)
		{
			_systemSeService = RoomLifetimeScope.Resolve<SystemSeService>();
		}
		_closePullDownSizeDeltaY = _pullDownParentRect.rect.height;
		_canvas = GetComponentInParent<Canvas>();
		_onClosePulldownComplete = new Subject<Unit>();
		if (_pullDownOpenButton != null)
		{
			ObservableSubscribeExtensions.Subscribe(_pullDownOpenButton.OnClickAsObservable(), delegate
			{
				TogglePullDown();
			}).AddTo(this);
		}
	}

	private void Update()
	{
		if (_isOpen)
		{
			CheckMousePosition();
		}
	}

	public void TogglePullDown()
	{
		if (_isOpen)
		{
			ClosePullDown();
		}
		else
		{
			OpenPullDown();
		}
	}

	public void OpenPullDown()
	{
		_isOpen = true;
		_openCloseTween?.Kill();
		_openCloseTween = DOTweenModuleUI.DOSizeDelta(endValue: new Vector2(_pullDownParentRect.sizeDelta.x, _openPullDownSizeDeltaY), target: _pullDownParentRect, duration: _pullDownOpenCloseSeconds).SetEase(_pullDownOpenCloseEase);
		_pullDownButtonRect.localScale = new Vector3(1f, -1f, 1f);
		_systemSeService.PlayPulldownOpen();
	}

	public void ClosePullDown(bool immediate = false)
	{
		_isOpen = false;
		_openCloseTween?.Kill();
		_openCloseTween = null;
		_pullDownButtonRect.localScale = new Vector3(1f, 1f, 1f);
		Vector2 vector = new Vector2(_pullDownParentRect.sizeDelta.x, _closePullDownSizeDeltaY);
		if (immediate)
		{
			_pullDownParentRect.sizeDelta = vector;
			_onClosePulldownComplete?.OnNext(Unit.Default);
			return;
		}
		_openCloseTween = _pullDownParentRect.DOSizeDelta(vector, _pullDownOpenCloseSeconds).SetEase(_pullDownOpenCloseEase).OnComplete(delegate
		{
			_onClosePulldownComplete?.OnNext(Unit.Default);
		});
		_systemSeService.PlayPulldownClose();
	}

	public void ChangeSelectContentText(string text)
	{
		_currentSelectContentText.text = text;
	}

	public void ChangeSelectContentTextByLocalizeId(string localizeId)
	{
		_currentSelectContentText.GetOrAddComponent<TextLocalizationBehaviour>().Set(localizeId);
	}

	public void ChangeSelectContentTextByLocalizeId(string localizeId, Func<string, string> formatter)
	{
		_currentSelectContentText.GetOrAddComponent<TextLocalizationBehaviour>().Set(localizeId, formatter);
	}

	public void SetTargetPullDownSizeDelta(float targetHeight)
	{
		_openPullDownSizeDeltaY = targetHeight;
	}

	public void SetPullDownButtonInteractable(bool interactable)
	{
		_pullDownOpenButton.interactable = interactable;
	}

	private void CheckMousePosition()
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_pullDownParentRect, Input.mousePosition, (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : _canvas.worldCamera, out var localPoint);
		if (!_pullDownParentRect.rect.Contains(localPoint))
		{
			ClosePullDown();
		}
	}

	private void OnDestroy()
	{
		_openCloseTween?.Kill();
		_openCloseTween = null;
		_onClosePulldownComplete?.Dispose();
	}
}
