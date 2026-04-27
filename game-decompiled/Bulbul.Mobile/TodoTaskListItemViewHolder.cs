using System;
using UnityEngine;

namespace Bulbul.Mobile;

public class TodoTaskListItemViewHolder : TodoTaskListBaseViewHolder, IDragReorderableItemVH
{
	private TodoTaskListTaskItemView _view;

	public TodoTaskListTaskItemView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<TodoTaskListTaskItemView>();
			}
			return _view;
		}
	}

	ItemDragReorderHandle IDragReorderableItemVH.DragHandle => View.View.DragReorderHandle;

	RectTransform IDragReorderableItemVH.RectTransform => View.RectTransform;

	public override bool CanPresentModelType(Type modelType)
	{
		return modelType == typeof(TodoTaskListItemModel);
	}

	public override void UpdateView(TodoTaskListItemBaseModel model, bool isPlaceHolder = false)
	{
		View.UpdateView(model as TodoTaskListItemModel, TodoListUIModel.IsTaskRemoveMode);
		base.UpdateView(model, isPlaceHolder);
	}

	void IDragReorderableItemVH.OnBeginDrag()
	{
		View.View.ActivateDraggingImages();
	}

	void IDragReorderableItemVH.OnEndDrag()
	{
		View.View.DeactivateDraggingImages();
	}

	void IDragReorderableItemVH.OnPointerDownIfNotDragged()
	{
		View.View.ActivateDraggingImages();
	}

	void IDragReorderableItemVH.OnPointerExitOrUpIfNotDragged()
	{
		View.View.DeactivateDraggingImages();
	}
}
