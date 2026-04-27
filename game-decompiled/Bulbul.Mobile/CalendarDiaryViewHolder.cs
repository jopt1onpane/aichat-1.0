using System;
using UnityEngine;

namespace Bulbul.Mobile;

public class CalendarDiaryViewHolder : CalendarContentsBaseViewHolder
{
	private CalendarContentsListItemDiaryView _view;

	public CalendarContentsListItemDiaryView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<CalendarContentsListItemDiaryView>();
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
		return modelType == typeof(CalendarDiaryViewModel);
	}

	public override void UpdateViews(CalendarContentsListBaseModel model)
	{
		View.UpdateView(model as CalendarDiaryViewModel);
		base.UpdateViews(model);
	}

	public override void MarkForRebuild()
	{
		base.MarkForRebuild();
		Vector2 sizeDelta = root.sizeDelta;
		sizeDelta.y = View.TotalHeight;
		root.sizeDelta = sizeDelta;
	}

	public override void UnmarkForRebuild()
	{
		base.UnmarkForRebuild();
	}
}
