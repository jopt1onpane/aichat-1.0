using System;
using UnityEngine;

namespace Bulbul.Mobile;

public class TodoTaskListCompleteItemViewHolder : TodoTaskListBaseViewHolder, IDragReorderableItemVH
{
	private TodoTaskListCompleteItemView _view;

	public TodoTaskListCompleteItemView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<TodoTaskListCompleteItemView>();
			}
			return _view;
		}
	}

	ItemDragReorderHandle IDragReorderableItemVH.DragHandle => View.View.DragReorderHandle;

	RectTransform IDragReorderableItemVH.RectTransform => View.RectTransform;

	public override bool CanPresentModelType(Type modelType)
	{
		return modelType == typeof(TodoTaskListCompleteItemModel);
	}

	public override void UpdateView(TodoTaskListItemBaseModel model, bool isPlaceHolder = false)
	{
		View.UpdateView(model as TodoTaskListCompleteItemModel, TodoListUIModel.IsTaskRemoveMode);
		base.UpdateView(model, isPlaceHolder);
	}

	void IDragReorderableItemVH.OnBeginDrag()
	{
	}

	void IDragReorderableItemVH.OnEndDrag()
	{
	}

	void IDragReorderableItemVH.OnPointerDownIfNotDragged()
	{
	}

	void IDragReorderableItemVH.OnPointerExitOrUpIfNotDragged()
	{
	}
}
