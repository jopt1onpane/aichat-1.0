using System;

namespace Bulbul.Mobile;

public class TodoTaskListCompletePullDownViewHolder : TodoTaskListBaseViewHolder
{
	private TodoTaskListCompletePullDownView _view;

	public TodoTaskListCompletePullDownView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<TodoTaskListCompletePullDownView>();
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
		return modelType == typeof(TodoTaskListCompletePullDownModel);
	}

	public override void UpdateView(TodoTaskListItemBaseModel model, bool isPlaceHolder = false)
	{
		View.UpdateView(model as TodoTaskListCompletePullDownModel);
		base.UpdateView(model, isPlaceHolder);
	}
}
