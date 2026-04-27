using System;
using Com.ForbiddenByte.OSA.Core;
using UnityEngine;

namespace Bulbul.Mobile;

public class HabitTrackerListViewHolder : BaseItemViewsHolder, IDragReorderableItemVH
{
	private HabitTrackerListItemView _view;

	public HabitTrackerListItemView View
	{
		get
		{
			if (_view == null)
			{
				_view = root.GetComponent<HabitTrackerListItemView>();
			}
			return _view;
		}
	}

	ItemDragReorderHandle IDragReorderableItemVH.DragHandle => View.DragHandle;

	RectTransform IDragReorderableItemVH.RectTransform => View.RectTransform;

	public override void CollectViews()
	{
		base.CollectViews();
		View.Initialize();
	}

	public void UpdateView(HabitTrackerListItemModel model, bool isPlaceHolder, bool isRemovingMode, bool isSettingMode)
	{
		View.SetActive(!isPlaceHolder);
		if (!isPlaceHolder && model != null)
		{
			View.UpdateModel(model, isRemovingMode, isSettingMode);
		}
	}

	public void ChangeRemoveMode(bool isRemoveMode)
	{
		View.ChangeRemoveMode(isRemoveMode);
	}

	public void ChangeSettingMode(bool isSettingMode)
	{
		View.ChangeSettingMode(isSettingMode);
	}

	public void ChangeTitle(string title)
	{
		View.ChangeTitle(title);
	}

	public void ChangeHabitComplete(DateTime date, bool isComplete, HabitDateEnableState prevState)
	{
		View.ChangeHabitComplete(date, isComplete, prevState);
	}

	public void ChangeHabitEnable(DayOfWeek date, bool enable)
	{
		View.ChangeHabitEnable(date, enable);
	}

	public void ChangeHabitPeriod()
	{
		View.ChangeHabitPeriod();
	}

	void IDragReorderableItemVH.OnBeginDrag()
	{
		View.ActivateDraggingImages();
	}

	void IDragReorderableItemVH.OnEndDrag()
	{
		View.DeactivateDraggingImages();
	}

	void IDragReorderableItemVH.OnPointerDownIfNotDragged()
	{
		View.ActivateDraggingImages();
	}

	void IDragReorderableItemVH.OnPointerExitOrUpIfNotDragged()
	{
		View.DeactivateDraggingImages();
	}
}
