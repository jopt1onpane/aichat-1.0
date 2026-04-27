using System;

namespace Bulbul.Mobile;

public class TodoTaskListSeparatorViewHolder : TodoTaskListBaseViewHolder
{
	private TodoTaskListSeparatorView _view;

	public TodoTaskListSeparatorView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<TodoTaskListSeparatorView>();
			}
			return _view;
		}
	}

	public override bool CanPresentModelType(Type modelType)
	{
		if (modelType == null)
		{
			return false;
		}
		return modelType == typeof(TodoTaskSeparatorModel);
	}

	public override void UpdateView(TodoTaskListItemBaseModel model, bool isPlaceHolder = false)
	{
		View.UpdateModel();
		base.UpdateView(model, isPlaceHolder);
	}
}
