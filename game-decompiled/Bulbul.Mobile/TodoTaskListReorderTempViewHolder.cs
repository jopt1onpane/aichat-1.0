using System;

namespace Bulbul.Mobile;

public class TodoTaskListReorderTempViewHolder : TodoTaskListBaseViewHolder
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

	public override bool CanPresentModelType(Type modelType)
	{
		return modelType == null;
	}

	public override void UpdateView(TodoTaskListItemBaseModel model, bool isPlaceHolder = false)
	{
		View.UpdateView(null, isRemovingMode: false, isPlaceHolder);
		base.UpdateView(model, isPlaceHolder);
	}
}
