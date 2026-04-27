using System;

namespace Bulbul.Mobile;

public class TodoTaskListPullDownViewHolder : TodoTaskListBaseViewHolder
{
	private TodoTaskListPullDownView _view;

	public TodoTaskListPullDownView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<TodoTaskListPullDownView>();
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
		return modelType == typeof(TodoTaskListPullDownModel);
	}

	public override void UpdateView(TodoTaskListItemBaseModel model, bool isPlaceHolder = false)
	{
		View.UpdateView(model as TodoTaskListPullDownModel);
		base.UpdateView(model, isPlaceHolder);
	}
}
