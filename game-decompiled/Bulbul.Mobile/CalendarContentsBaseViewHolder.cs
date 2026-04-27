using System;
using Com.ForbiddenByte.OSA.Core;

namespace Bulbul.Mobile;

public abstract class CalendarContentsBaseViewHolder : BaseItemViewsHolder
{
	protected float _itemHeight;

	public override void CollectViews()
	{
		base.CollectViews();
	}

	public virtual void UpdateItemHeight(float itemHeight)
	{
		_itemHeight = itemHeight;
	}

	public abstract bool CanPresentModelType(Type modelType);

	public virtual void UpdateViews(CalendarContentsListBaseModel model)
	{
		_itemHeight = model.Height;
	}
}
