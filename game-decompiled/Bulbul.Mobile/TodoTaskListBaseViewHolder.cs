using System;
using Com.ForbiddenByte.OSA.Core;
using UnityEngine;

namespace Bulbul.Mobile;

public abstract class TodoTaskListBaseViewHolder : BaseItemViewsHolder
{
	protected float _itemHeight;

	public virtual void UpdateItemHeight(float itemHeight)
	{
		_itemHeight = itemHeight;
	}

	public virtual bool CanPresentModelType(Type modelType)
	{
		if (!(modelType == null))
		{
			return true;
		}
		return false;
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

	public virtual void UpdateView(TodoTaskListItemBaseModel model, bool isPlaceHolder = false)
	{
		if (model == null)
		{
			_itemHeight = 190f;
		}
		else
		{
			_itemHeight = model.Height;
		}
	}
}
