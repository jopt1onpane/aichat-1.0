using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Bulbul;

public class ClickOutsideDetector : MonoBehaviour
{
	public enum TriggerType
	{
		MouseDown,
		MouseUp
	}

	[SerializeField]
	private TriggerType _triggerType;

	[SerializeField]
	[Tooltip("外側クリック判定から除外するもの")]
	private List<RectTransform> _excludeTransforms = new List<RectTransform>();

	[SerializeField]
	[Tooltip("この範囲でドラッグ開始したら判定から除外する")]
	private List<RectTransform> _dragExcludeTransforms = new List<RectTransform>();

	private readonly Subject<Unit> _onClickOutside = new Subject<Unit>();

	private bool _isInsideOnPointerDown;

	private bool _isDragFromInside;

	public Subject<Unit> OnClickOutside => _onClickOutside;

	private void Start()
	{
		RectTransform item = base.transform as RectTransform;
		if (!_excludeTransforms.Contains(item))
		{
			_excludeTransforms.Add(item);
		}
	}

	private void Update()
	{
		if (CheckClickOutside())
		{
			_onClickOutside.OnNext(Unit.Default);
		}
	}

	private bool CheckClickOutside()
	{
		switch (_triggerType)
		{
		case TriggerType.MouseDown:
			if (InputController.Instance.GetClickDown())
			{
				return !IsPointerInside(_excludeTransforms);
			}
			return false;
		case TriggerType.MouseUp:
			if (InputController.Instance.GetClickDown())
			{
				_isInsideOnPointerDown = IsPointerInside(_excludeTransforms);
				_isDragFromInside = IsPointerInside(_dragExcludeTransforms);
			}
			if (InputController.Instance.GetClickUp())
			{
				if (_isInsideOnPointerDown)
				{
					return false;
				}
				if (InputController.Instance.Dragged && _isDragFromInside)
				{
					return false;
				}
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	public void AddExcludeTransform(RectTransform trans)
	{
		_excludeTransforms.Add(trans);
	}

	private bool IsPointerInside(List<RectTransform> insides)
	{
		foreach (RectTransform inside in insides)
		{
			Vector2 screenPoint = Input.mousePosition;
			if (RectTransformUtility.RectangleContainsScreenPoint(inside, screenPoint))
			{
				return true;
			}
		}
		return false;
	}
}
