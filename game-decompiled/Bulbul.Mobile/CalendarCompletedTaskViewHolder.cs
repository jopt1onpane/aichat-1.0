using System;
using UnityEngine;

namespace Bulbul.Mobile;

public class CalendarCompletedTaskViewHolder : CalendarContentsBaseViewHolder
{
	public static bool IsRemovingMode;

	private CalendarContentsListItemCompletedTaskView _view;

	public CalendarContentsListItemCompletedTaskView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<CalendarContentsListItemCompletedTaskView>();
			}
			return _view;
		}
	}

	public override void CollectViews()
	{
		base.CollectViews();
	}

	public override bool CanPresentModelType(Type modelType)
	{
		return modelType == typeof(CalendarCompletedTaskViewModel);
	}

	public override void UpdateViews(CalendarContentsListBaseModel model)
	{
		View.UpdateView(model as CalendarCompletedTaskViewModel, IsRemovingMode);
		base.UpdateViews(model);
	}

	public void ChangeRemovingMode()
	{
		View.ChangeRemovingMode(IsRemovingMode);
	}

	public override void MarkForRebuild()
	{
		base.MarkForRebuild();
		Vector2 sizeDelta = root.sizeDelta;
		sizeDelta.y = _itemHeight;
		root.sizeDelta = sizeDelta;
	}

	public override void UnmarkForRebuild()
	{
		base.UnmarkForRebuild();
	}
}
