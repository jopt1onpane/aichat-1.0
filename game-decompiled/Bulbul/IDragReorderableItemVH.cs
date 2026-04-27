using UnityEngine;

namespace Bulbul;

public interface IDragReorderableItemVH
{
	ItemDragReorderHandle DragHandle { get; }

	RectTransform RectTransform { get; }

	void OnBeginDrag();

	void OnEndDrag();

	void OnPointerDownIfNotDragged();

	void OnPointerExitOrUpIfNotDragged();
}
