using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul;

[RequireComponent(typeof(Graphic))]
public class ItemDragReorderHandle : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	private bool isPointerDown;

	public IDragDropListener DragDropListener { get; private set; }

	public IDragReorderableItemVH Item { get; private set; }

	public void Init(IDragDropListener dragDropListener, IDragReorderableItemVH item)
	{
		DragDropListener = dragDropListener;
		Item = item;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		isPointerDown = true;
		DragDropListener?.OnPointerDownItem(Item, eventData);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (isPointerDown)
		{
			isPointerDown = false;
			DragDropListener?.OnPointerExitItem(Item, eventData);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isPointerDown = false;
		DragDropListener?.OnPointerUpItem(Item, eventData);
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		DragDropListener?.OnBeginDragItem(Item, eventData);
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		DragDropListener?.OnDragItem(Item, eventData);
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		DragDropListener?.OnDroppedItem(Item, eventData);
	}
}
