using System;
using Bulbul;
using NestopiSystem;
using R3;
using UnityEngine;

public class SyncPositionAndSizeFromOptionalParentUI : MonoBehaviour
{
	[SerializeField]
	[Header("同期するオブジェクトを指定\u3000nullの場合所属するCanvasを探す")]
	private RectTransform _parent;

	private IDisposable _parentDisposable;

	private RectTransform _subscribedParent;

	private bool _isDirty;

	private RectTransform _rectTransform;

	private RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public void Awake()
	{
		if (_parent == null)
		{
			_parent = GetComponentInParent<Canvas>(includeInactive: true).transform as RectTransform;
		}
	}

	public void Start()
	{
		Sync();
		_parentDisposable = ObservableSubscribeExtensions.Subscribe(this.GetOrAddComponent<TransformHasChangedTrigger>().OnTransformChanged, delegate
		{
			_isDirty = true;
		}).AddTo(this);
	}

	private void Update()
	{
		if (_isDirty)
		{
			Sync();
		}
	}

	public void Sync()
	{
		_isDirty = false;
		if (_parent == null)
		{
			return;
		}
		if (_parent != _subscribedParent)
		{
			_parentDisposable?.Dispose();
			_subscribedParent = _parent;
			_parentDisposable = ObservableSubscribeExtensions.Subscribe(_parent.GetOrAddComponent<TransformHasChangedTrigger>().OnTransformChanged, delegate
			{
				_isDirty = true;
			}).AddTo(this);
		}
		RectTransform.anchorMax = _parent.anchorMax;
		RectTransform.anchorMin = _parent.anchorMin;
		RectTransform.position = _parent.position;
		Rect rect = _parent.rect;
		RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
		RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
	}
}
