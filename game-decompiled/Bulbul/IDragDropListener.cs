using UnityEngine.EventSystems;

namespace Bulbul;

public interface IDragDropListener
{
	void OnBeginDragItem(IDragReorderableItemVH item, PointerEventData eventData);

	void OnDragItem(IDragReorderableItemVH item, PointerEventData eventData);

	void OnDroppedItem(IDragReorderableItemVH item, PointerEventData eventData);

	void OnPointerDownItem(IDragReorderableItemVH item, PointerEventData eventData);

	void OnPointerUpItem(IDragReorderableItemVH item, PointerEventData eventData);

	void OnPointerExitItem(IDragReorderableItemVH item, PointerEventData eventData);
}
