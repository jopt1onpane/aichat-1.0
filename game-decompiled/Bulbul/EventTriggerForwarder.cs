using UnityEngine;
using UnityEngine.EventSystems;

namespace Bulbul;

public class EventTriggerForwarder : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler
{
	private EventTrigger _eventTrigger;

	private EventTrigger eventTrigger
	{
		get
		{
			if (!_eventTrigger)
			{
				return _eventTrigger = GetComponentInParent<EventTrigger>();
			}
			return _eventTrigger;
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnPointerEnter(eventData);
		}
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnPointerExit(eventData);
		}
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnPointerDown(eventData);
		}
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnPointerUp(eventData);
		}
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnPointerClick(eventData);
		}
	}

	void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnInitializePotentialDrag(eventData);
		}
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnBeginDrag(eventData);
		}
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnDrag(eventData);
		}
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnEndDrag(eventData);
		}
	}

	void IDropHandler.OnDrop(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnDrop(eventData);
		}
	}

	void IScrollHandler.OnScroll(PointerEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnScroll(eventData);
		}
	}

	void IUpdateSelectedHandler.OnUpdateSelected(BaseEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnUpdateSelected(eventData);
		}
	}

	void ISelectHandler.OnSelect(BaseEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnSelect(eventData);
		}
	}

	void IDeselectHandler.OnDeselect(BaseEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnDeselect(eventData);
		}
	}

	void IMoveHandler.OnMove(AxisEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnMove(eventData);
		}
	}

	void ISubmitHandler.OnSubmit(BaseEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnSubmit(eventData);
		}
	}

	void ICancelHandler.OnCancel(BaseEventData eventData)
	{
		if ((bool)eventTrigger)
		{
			eventTrigger.OnCancel(eventData);
		}
	}
}
