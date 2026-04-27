using System;
using UnityEngine;

namespace Bulbul.Mobile;

public class CalendarAndWorkViewHolder : CalendarContentsBaseViewHolder
{
	private CalendarContentsListItemCalendarAndWorkTimeView _view;

	public CalendarContentsListItemCalendarAndWorkTimeView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<CalendarContentsListItemCalendarAndWorkTimeView>();
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
		return modelType == typeof(CalendarAndWorkViewModel);
	}

	public override void UpdateViews(CalendarContentsListBaseModel model)
	{
		View.UpdateView(model as CalendarAndWorkViewModel);
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
