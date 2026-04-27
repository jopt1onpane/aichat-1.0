using System;
using System.Collections.Generic;
using Com.ForbiddenByte.OSA.Core;
using frame8.Logic.Misc.Other;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bulbul;

public class DragReorderManipulator<TParams, TItemViewsHolder, TItemModel> : MonoBehaviour, IDragDropListener, ICancelHandler, IEventSystemHandler, IDisposable where TParams : BaseParams where TItemViewsHolder : BaseItemViewsHolder
{
	public class EmptyModel
	{
		public int placeholderForIndex;
	}

	private class DragStateManager
	{
		public TItemViewsHolder Dragged { get; private set; }

		public TItemModel ModelOfDragged { get; private set; }

		public bool IsDragging { get; private set; }

		public EmptyModel PlaceholderModel { get; private set; }

		private int LastSwappedItemAtIndex { get; set; }

		private Vector2 LastSwapDragEventDelta { get; set; }

		private bool PreventSwapWithLastSwappedItem { get; set; }

		public void EnterState_None()
		{
			Dragged = null;
			ModelOfDragged = default(TItemModel);
			PlaceholderModel = null;
			LastSwappedItemAtIndex = -1;
			PreventSwapWithLastSwappedItem = false;
			LastSwapDragEventDelta = Vector2.zero;
			IsDragging = false;
		}

		public void EnterState_Dragging(TItemViewsHolder dragged, TItemModel modelOfDragged, EmptyModel placeholderModel, PointerEventData eventData)
		{
			Dragged = dragged;
			ModelOfDragged = modelOfDragged;
			PlaceholderModel = placeholderModel;
			IsDragging = true;
		}

		public void RegisterSwap(int placeholderIndexBeforeSwap, Vector2 dragEventDelta)
		{
			LastSwappedItemAtIndex = placeholderIndexBeforeSwap;
			LastSwapDragEventDelta = dragEventDelta;
			PreventSwapWithLastSwappedItem = true;
		}

		public bool TryPrepareSwap(int closestVHIndex, Vector2 dragEventDelta)
		{
			if (closestVHIndex == PlaceholderModel.placeholderForIndex)
			{
				return false;
			}
			if (Vector2.Dot(LastSwapDragEventDelta, dragEventDelta) < 0f)
			{
				PreventSwapWithLastSwappedItem = false;
				return true;
			}
			if (PreventSwapWithLastSwappedItem && closestVHIndex == LastSwappedItemAtIndex)
			{
				return false;
			}
			return true;
		}
	}

	[Range(0f, 1f)]
	[Tooltip("This is normalized (0-1) relative to the dragSpace's size")]
	[SerializeField]
	private float _minDistFromEdgeToBeginScroll01 = 0.2f;

	[SerializeField]
	private float _maxScrollSpeedOnBoundary = 3000f;

	[SerializeField]
	private float _dragSpaceMaxOverflowRange;

	private OSA<TParams, TItemViewsHolder> _list;

	private OSAListDataHelper<TItemModel> _data;

	private DragStateManager _dragState;

	private Canvas _canvas;

	private RectTransform _canvasRT;

	private RectTransform _dragSpace;

	private Vector2 _currentOnDragEventPosition;

	private Camera _currentPressEventCamera;

	private Vector2 _distancePointerToDraggedInCanvasSpace;

	private Subject<(int movedIdx, TItemModel draggedModel)> _onChangeOrder = new Subject<(int, TItemModel)>();

	private Subject<TItemModel> _onBeginDrag = new Subject<TItemModel>();

	private Subject<Unit> _onEndDrag = new Subject<Unit>();

	private int _topOrderLimit;

	private int _bottomOrderLimit;

	private bool _isLimitOrder;

	public Observable<(int movedIdx, TItemModel draggedModel)> OnChangeOrder => _onChangeOrder;

	public Observable<TItemModel> OnBeginDrag => _onBeginDrag;

	public Observable<Unit> OnEndDrag => _onEndDrag;

	protected TItemViewsHolder DraggedHolder => _dragState.Dragged;

	protected TItemModel DraggedModel => _dragState.ModelOfDragged;

	public bool IsDragging => _dragState.IsDragging;

	public void Init(OSA<TParams, TItemViewsHolder> list, OSAListDataHelper<TItemModel> data, bool isLimitOrder = false)
	{
		_list = list;
		_data = data;
		_dragState = new DragStateManager();
		_canvas = GetComponentInParent<Canvas>();
		_canvasRT = _canvas.transform as RectTransform;
		_dragSpace = list.Viewport;
		_isLimitOrder = isLimitOrder;
	}

	public void SetOrderLimit(int top, int bottom)
	{
		_topOrderLimit = top;
		_bottomOrderLimit = bottom;
	}

	public void Update()
	{
		if (_dragState.IsDragging)
		{
			DoAutoScrolling();
		}
	}

	private void DoAutoScrolling()
	{
		if (_list.GetContentSizeToViewportRatio() < 1.0)
		{
			return;
		}
		if (_isLimitOrder)
		{
			bool isPointInDragSpace;
			TItemViewsHolder closestVHAtScreenPoint = GetClosestVHAtScreenPoint(_currentOnDragEventPosition, _currentPressEventCamera, out isPointInDragSpace);
			if (closestVHAtScreenPoint != null)
			{
				int itemIndex = closestVHAtScreenPoint.ItemIndex;
				int num = _topOrderLimit - 1;
				if (num < 0)
				{
					num = 0;
				}
				int num2 = _bottomOrderLimit + 1;
				if (num2 >= _data.Count)
				{
					num2 = _data.Count - 1;
				}
				if (itemIndex <= num || itemIndex >= num2)
				{
					return;
				}
			}
		}
		if (!GetLocalPointInDragSpace(_currentOnDragEventPosition, _currentPressEventCamera, out var localPoint))
		{
			return;
		}
		float value = _list.ConvertLocalPointToLongitudinalPointStart0End1(_dragSpace, localPoint);
		value = Mathf.Clamp01(value);
		float num3 = _maxScrollSpeedOnBoundary * _list.DeltaTime;
		float minDistFromEdgeToBeginScroll = _minDistFromEdgeToBeginScroll01;
		float num4 = 1f - _minDistFromEdgeToBeginScroll01;
		float num5 = 0f;
		if (value < minDistFromEdgeToBeginScroll)
		{
			num5 = num3 * (minDistFromEdgeToBeginScroll - value) / _minDistFromEdgeToBeginScroll01;
		}
		else if (value > num4)
		{
			num5 = (0f - num3) * (value - num4) / _minDistFromEdgeToBeginScroll01;
		}
		if (num5 == 0f)
		{
			return;
		}
		_list.ScrollByAbstractDelta(num5);
		Vector2 dragDelta = (_list.IsVertical ? new Vector2(0f, num5) : new Vector2(num5, 0f));
		ReorderItemByDrag(dragDelta);
		if (_isLimitOrder && _dragState.Dragged is IDragReorderableItemVH dragReorderableItemVH)
		{
			Vector3 stoppedPos2;
			if (_list.IsVertical && CheckOrderLimitDragPosVertical(dragReorderableItemVH, out var stoppedPos))
			{
				dragReorderableItemVH.RectTransform.position = stoppedPos;
			}
			else if (_list.IsHorizontal && CheckOrderLimitDragPosHorizontal(dragReorderableItemVH, out stoppedPos2))
			{
				dragReorderableItemVH.RectTransform.position = stoppedPos2;
			}
		}
	}

	public bool IsPlaceHolderModel(TItemModel model)
	{
		if (_dragState.IsDragging)
		{
			return EqualityComparer<TItemModel>.Default.Equals(model, default(TItemModel));
		}
		return false;
	}

	void IDragDropListener.OnBeginDragItem(IDragReorderableItemVH item, PointerEventData eventData)
	{
		_currentPressEventCamera = eventData.pressEventCamera;
		Vector2 vector = UIUtils8.Instance.WorldToScreenPointForCanvas(_canvas, eventData.pressEventCamera, item.RectTransform.position);
		_distancePointerToDraggedInCanvasSpace = vector - eventData.position;
		item.RectTransform.SetParent(base.transform, worldPositionStays: true);
		item.OnBeginDrag();
		TItemViewsHolder val = item as TItemViewsHolder;
		int itemIndex = val.ItemIndex;
		TItemModel val2 = _data[itemIndex];
		_onBeginDrag.OnNext(val2);
		_data.List.RemoveAt(itemIndex);
		double normalizedPosition = _list.GetNormalizedPosition();
		_list.RemoveItemWithViewsHolder(val, stealViewsHolderInsteadOfRecycle: true, contentPanelEndEdgeStationary: false);
		EmptyModel placeholderModel = new EmptyModel();
		_dragState.EnterState_Dragging(val, val2, placeholderModel, eventData);
		InsertPlaceholderAtNewIndex(itemIndex);
		_list.SetNormalizedPosition(normalizedPosition);
	}

	void IDragDropListener.OnDragItem(IDragReorderableItemVH item, PointerEventData eventData)
	{
		if (!_dragState.IsDragging || item != _dragState.Dragged)
		{
			return;
		}
		_currentOnDragEventPosition = eventData.position;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRT, _currentOnDragEventPosition + _distancePointerToDraggedInCanvasSpace, eventData.pressEventCamera, out var worldPoint);
		if (GetLocalPointInDragSpace(worldPoint, eventData.pressEventCamera, out var localPoint))
		{
			if (_list.IsVertical)
			{
				localPoint.x = 0f;
				float num = item.RectTransform.sizeDelta.y * _canvas.rootCanvas.scaleFactor;
				float y = item.RectTransform.pivot.y;
				Rect rect = _dragSpace.rect;
				float min = rect.min.y - _dragSpaceMaxOverflowRange + num * y;
				float max = rect.max.y + _dragSpaceMaxOverflowRange - num * (1f - y);
				localPoint.y = Mathf.Clamp(localPoint.y, min, max);
			}
			else
			{
				localPoint.y = 0f;
				float num2 = item.RectTransform.sizeDelta.x * _canvas.rootCanvas.scaleFactor;
				float x = item.RectTransform.pivot.x;
				Rect rect2 = _dragSpace.rect;
				float min2 = rect2.min.x - _dragSpaceMaxOverflowRange + num2 * x;
				float max2 = rect2.max.x + _dragSpaceMaxOverflowRange - num2 * (1f - x);
				localPoint.x = Mathf.Clamp(localPoint.x, min2, max2);
			}
			worldPoint = _dragSpace.TransformPoint(localPoint);
		}
		item.RectTransform.position = worldPoint;
		_currentOnDragEventPosition = RectTransformUtility.WorldToScreenPoint(_currentPressEventCamera, worldPoint);
		ReorderItemByDrag(eventData.delta);
		if (_isLimitOrder)
		{
			Vector3 stoppedPos2;
			if (_list.IsVertical && CheckOrderLimitDragPosVertical(item, out var stoppedPos))
			{
				item.RectTransform.position = stoppedPos;
			}
			else if (_list.IsHorizontal && CheckOrderLimitDragPosHorizontal(item, out stoppedPos2))
			{
				item.RectTransform.position = stoppedPos2;
			}
		}
	}

	private bool CheckOrderLimitDragPosVertical(IDragReorderableItemVH item, out Vector3 stoppedPos)
	{
		stoppedPos = Vector3.zero;
		bool isPointInDragSpace;
		TItemViewsHolder closestVHAtScreenPoint = GetClosestVHAtScreenPoint(_currentOnDragEventPosition, _currentPressEventCamera, out isPointInDragSpace);
		if (closestVHAtScreenPoint == null)
		{
			return false;
		}
		if (closestVHAtScreenPoint.ItemIndex <= _topOrderLimit)
		{
			TItemViewsHolder itemViewsHolderIfVisible = _list.GetItemViewsHolderIfVisible(_topOrderLimit - 1);
			if (itemViewsHolderIfVisible == null)
			{
				return false;
			}
			float scaleFactor = _canvas.rootCanvas.scaleFactor;
			float num = itemViewsHolderIfVisible.root.position.y - itemViewsHolderIfVisible.root.sizeDelta.y * scaleFactor * itemViewsHolderIfVisible.root.pivot.y;
			if (item.RectTransform.position.y + item.RectTransform.sizeDelta.y * scaleFactor * (1f - item.RectTransform.pivot.y) >= num)
			{
				stoppedPos = item.RectTransform.position;
				stoppedPos.y = num - item.RectTransform.sizeDelta.y * scaleFactor * (1f - item.RectTransform.pivot.y);
				return true;
			}
		}
		else if (closestVHAtScreenPoint.ItemIndex >= _bottomOrderLimit)
		{
			TItemViewsHolder itemViewsHolderIfVisible2 = _list.GetItemViewsHolderIfVisible(_bottomOrderLimit + 1);
			if (itemViewsHolderIfVisible2 == null)
			{
				return false;
			}
			float scaleFactor2 = _canvas.rootCanvas.scaleFactor;
			float num2 = itemViewsHolderIfVisible2.root.position.y + itemViewsHolderIfVisible2.root.sizeDelta.y * scaleFactor2 * (1f - itemViewsHolderIfVisible2.root.pivot.y);
			if (item.RectTransform.position.y - item.RectTransform.sizeDelta.y * scaleFactor2 * item.RectTransform.pivot.y <= num2)
			{
				stoppedPos = item.RectTransform.position;
				stoppedPos.y = num2 + item.RectTransform.sizeDelta.y * scaleFactor2 * item.RectTransform.pivot.y;
				return true;
			}
		}
		return false;
	}

	private bool CheckOrderLimitDragPosHorizontal(IDragReorderableItemVH item, out Vector3 stoppedPos)
	{
		stoppedPos = Vector3.zero;
		bool isPointInDragSpace;
		TItemViewsHolder closestVHAtScreenPoint = GetClosestVHAtScreenPoint(_currentOnDragEventPosition, _currentPressEventCamera, out isPointInDragSpace);
		if (closestVHAtScreenPoint == null)
		{
			return false;
		}
		if (closestVHAtScreenPoint.ItemIndex <= _topOrderLimit)
		{
			TItemViewsHolder itemViewsHolderIfVisible = _list.GetItemViewsHolderIfVisible(_topOrderLimit - 1);
			if (itemViewsHolderIfVisible == null)
			{
				return false;
			}
			float scaleFactor = _canvas.rootCanvas.scaleFactor;
			float num = itemViewsHolderIfVisible.root.position.x + itemViewsHolderIfVisible.root.sizeDelta.x * scaleFactor * (1f - itemViewsHolderIfVisible.root.pivot.x);
			if (item.RectTransform.position.x - item.RectTransform.sizeDelta.x * scaleFactor * item.RectTransform.pivot.x <= num)
			{
				stoppedPos = item.RectTransform.position;
				stoppedPos.x = num + item.RectTransform.sizeDelta.x * scaleFactor * item.RectTransform.pivot.x;
				return true;
			}
		}
		else if (closestVHAtScreenPoint.ItemIndex >= _bottomOrderLimit)
		{
			TItemViewsHolder itemViewsHolderIfVisible2 = _list.GetItemViewsHolderIfVisible(_bottomOrderLimit + 1);
			if (itemViewsHolderIfVisible2 == null)
			{
				return false;
			}
			float scaleFactor2 = _canvas.rootCanvas.scaleFactor;
			float num2 = itemViewsHolderIfVisible2.root.position.x - itemViewsHolderIfVisible2.root.sizeDelta.x * scaleFactor2 * itemViewsHolderIfVisible2.root.pivot.x;
			if (item.RectTransform.position.x + item.RectTransform.sizeDelta.x * scaleFactor2 * (1f - item.RectTransform.pivot.x) >= num2)
			{
				stoppedPos = item.RectTransform.position;
				stoppedPos.x = num2 - item.RectTransform.sizeDelta.x * scaleFactor2 * (1f - item.RectTransform.pivot.x);
				return true;
			}
		}
		return false;
	}

	private void ReorderItemByDrag(Vector2 dragDelta)
	{
		bool isPointInDragSpace;
		TItemViewsHolder closestVHAtScreenPoint = GetClosestVHAtScreenPoint(_currentOnDragEventPosition, _currentPressEventCamera, out isPointInDragSpace);
		if (closestVHAtScreenPoint != null)
		{
			int itemIndex = closestVHAtScreenPoint.ItemIndex;
			int placeholderForIndex = _dragState.PlaceholderModel.placeholderForIndex;
			if (_dragState.TryPrepareSwap(itemIndex, dragDelta) && (!_isLimitOrder || (itemIndex >= _topOrderLimit && itemIndex <= _bottomOrderLimit)))
			{
				double normalizedPosition = _list.GetNormalizedPosition();
				_data.RemoveOne(placeholderForIndex);
				InsertPlaceholderAtNewIndex(itemIndex);
				_dragState.RegisterSwap(placeholderForIndex, dragDelta);
				_list.SetNormalizedPosition(normalizedPosition);
			}
		}
	}

	void IDragDropListener.OnDroppedItem(IDragReorderableItemVH item, PointerEventData eventData)
	{
		if (_dragState.IsDragging && item == _dragState.Dragged)
		{
			item.RectTransform.SetParent(_list.Content);
			item.OnEndDrag();
			_onEndDrag.OnNext(Unit.Default);
			DropDraggedVHAndEnterNoneState(eventData);
		}
	}

	void IDragDropListener.OnPointerDownItem(IDragReorderableItemVH item, PointerEventData eventData)
	{
		if (!_dragState.IsDragging && item != _dragState.Dragged)
		{
			item.OnPointerDownIfNotDragged();
		}
	}

	void IDragDropListener.OnPointerUpItem(IDragReorderableItemVH item, PointerEventData eventData)
	{
		if (!_dragState.IsDragging && item != _dragState.Dragged)
		{
			item.OnPointerExitOrUpIfNotDragged();
		}
	}

	void IDragDropListener.OnPointerExitItem(IDragReorderableItemVH item, PointerEventData eventData)
	{
		if (!_dragState.IsDragging && item != _dragState.Dragged)
		{
			item.OnPointerExitOrUpIfNotDragged();
		}
	}

	void ICancelHandler.OnCancel(BaseEventData eventData)
	{
		if (_dragState.IsDragging)
		{
			_onEndDrag.OnNext(Unit.Default);
			DropDraggedVHAndEnterNoneState(null);
		}
	}

	private void InsertPlaceholderAtNewIndex(int index)
	{
		_dragState.PlaceholderModel.placeholderForIndex = index;
		_data.InsertOne(index, default(TItemModel));
		_list.RequestChangeItemSizeAndUpdateLayout(index, _dragState.Dragged.root.rect.height, itemEndEdgeStationary: false, computeVisibility: true, correctItemPosition: true);
	}

	private void DropDraggedVHAndEnterNoneState(PointerEventData eventData)
	{
		TItemViewsHolder dragged = _dragState.Dragged;
		TItemModel modelOfDragged = _dragState.ModelOfDragged;
		int placeholderForIndex = _dragState.PlaceholderModel.placeholderForIndex;
		int num;
		if (eventData == null)
		{
			num = dragged.ItemIndex;
			_data.RemoveOne(placeholderForIndex);
			placeholderForIndex = -1;
		}
		else
		{
			num = placeholderForIndex;
			placeholderForIndex++;
		}
		_data.List.Insert(num, modelOfDragged);
		bool num2 = eventData != null && num != dragged.ItemIndex;
		_dragState.EnterState_None();
		Rect rect = dragged.root.rect;
		float requestedSize = (_list.IsVertical ? rect.height : rect.width);
		_list.InsertItemWithViewsHolder(dragged, num, contentPanelEndEdgeStationary: false);
		_list.RequestChangeItemSizeAndUpdateLayout(num, requestedSize);
		if (placeholderForIndex != -1)
		{
			_data.RemoveOne(placeholderForIndex);
		}
		if (num2)
		{
			_onChangeOrder?.OnNext((num, modelOfDragged));
		}
	}

	private TItemViewsHolder GetClosestVHAtScreenPoint(PointerEventData eventData, out bool isPointInDragSpace)
	{
		return GetClosestVHAtScreenPoint(eventData.position, eventData.pressEventCamera, out isPointInDragSpace);
	}

	private TItemViewsHolder GetClosestVHAtScreenPoint(Vector2 position, Camera camera, out bool isPointInDragSpace)
	{
		isPointInDragSpace = false;
		if (!GetLocalPointInDragSpace(position, camera, out var localPoint))
		{
			return null;
		}
		isPointInDragSpace = true;
		float distance;
		TItemViewsHolder viewsHolderClosestToViewportPoint = _list.GetViewsHolderClosestToViewportPoint(_canvas, _canvasRT, localPoint, 0.5f, out distance);
		if (viewsHolderClosestToViewportPoint == null)
		{
			return null;
		}
		return viewsHolderClosestToViewportPoint;
	}

	private bool GetLocalPointInDragSpace(Vector2 screenPoint, Camera camera, out Vector2 localPoint)
	{
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragSpace, screenPoint, camera, out localPoint);
	}

	private bool GetLocalPointInDragSpaceIfWithinBounds(Vector2 screenPoint, Camera camera, out Vector2 localPoint)
	{
		if (GetLocalPointInDragSpace(screenPoint, camera, out localPoint))
		{
			return _dragSpace.IsLocalPointInRect(localPoint);
		}
		return false;
	}

	public void Dispose()
	{
		_onChangeOrder.Dispose();
		_onChangeOrder = null;
	}
}
