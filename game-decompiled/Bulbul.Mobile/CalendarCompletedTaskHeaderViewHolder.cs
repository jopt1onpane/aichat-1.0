using System;
using UnityEngine;

namespace Bulbul.Mobile;

public class CalendarCompletedTaskHeaderViewHolder : CalendarContentsBaseViewHolder
{
	private CalendarContentsListItemCompletedTaskHeaderView _view;

	public CalendarContentsListItemCompletedTaskHeaderView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<CalendarContentsListItemCompletedTaskHeaderView>();
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
		return modelType == typeof(CalendarCompletedTaskHeaderViewModel);
	}

	public override void UpdateViews(CalendarContentsListBaseModel model)
	{
		View.UpdateView(model as CalendarCompletedTaskHeaderViewModel);
		base.UpdateViews(model);
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
