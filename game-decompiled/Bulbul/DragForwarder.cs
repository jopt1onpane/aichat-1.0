using NestopiSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bulbul;

public class DragForwarder : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	private IBeginDragHandler _beginDragHandler;

	private IDragHandler _dragHandler;

	private IEndDragHandler _endDragHandler;

	private IInitializePotentialDragHandler _initializePotentialDragHandler;

	private IBeginDragHandler beginDragHandler
	{
		get
		{
			if (_beginDragHandler.IsNullOrDestroy())
			{
				_beginDragHandler = base.transform.parent.GetComponentInParent<IBeginDragHandler>();
			}
			return _beginDragHandler;
		}
	}

	private IDragHandler dragHandler
	{
		get
		{
			if (_dragHandler.IsNullOrDestroy())
			{
				_dragHandler = base.transform.parent.GetComponentInParent<IDragHandler>();
			}
			return _dragHandler;
		}
	}

	private IEndDragHandler endDragHandler
	{
		get
		{
			if (_endDragHandler.IsNullOrDestroy())
			{
				_endDragHandler = base.transform.parent.GetComponentInParent<IEndDragHandler>();
			}
			return _endDragHandler;
		}
	}

	private IInitializePotentialDragHandler initializePotentialDragHandler
	{
		get
		{
			if (_initializePotentialDragHandler.IsNullOrDestroy())
			{
				_initializePotentialDragHandler = base.transform.parent.GetComponentInParent<IInitializePotentialDragHandler>();
			}
			return _initializePotentialDragHandler;
		}
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		if (!initializePotentialDragHandler.IsNullOrDestroy())
		{
			initializePotentialDragHandler.OnInitializePotentialDrag(eventData);
		}
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		if (!beginDragHandler.IsNullOrDestroy())
		{
			beginDragHandler.OnBeginDrag(eventData);
		}
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		if (!dragHandler.IsNullOrDestroy())
		{
			dragHandler.OnDrag(eventData);
		}
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		if (!endDragHandler.IsNullOrDestroy())
		{
			endDragHandler.OnEndDrag(eventData);
		}
	}
}
