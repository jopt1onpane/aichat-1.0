using Com.ForbiddenByte.OSA.Core;
using UnityEngine;

namespace Bulbul.Mobile;

public class MusicPlayListItemViewsHolder : BaseItemViewsHolder, IDragReorderableItemVH
{
	public MusicPlayListItemView View { get; set; }

	public ItemDragReorderHandle DragHandle => View.DragHandle;

	public RectTransform RectTransform => View.RectTransform;

	public override void CollectViews()
	{
		base.CollectViews();
		View = root.GetComponent<MusicPlayListItemView>();
		View.Initialize();
	}

	public void SetModel(MusicPlayListItemModel model, bool isPlaceHolder, bool isRemovingMode)
	{
		View.SetActive(!isPlaceHolder);
		if (isPlaceHolder)
		{
			View.ResetItemModel();
			return;
		}
		View.SetModel(model, isPlaceHolder, isRemovingMode);
		View.RemovingModeUI.TransitionImmediate(isRemovingMode);
	}

	public void UpdateModel(MusicPlayListItemModel model, bool isPlaceHolder, bool isRemovingMode)
	{
		View.SetActive(!isPlaceHolder);
		if (isPlaceHolder)
		{
			View.ResetItemModel();
			return;
		}
		View.UpdateModel(model, isPlaceHolder, isRemovingMode);
		View.RemovingModeUI.TransitionImmediate(isRemovingMode);
	}

	public void ChangeRemovingMode(bool isRemoving, bool isImmediate = false)
	{
		if (!isImmediate)
		{
			View.RemovingModeUI.Transition(isRemoving);
		}
		else
		{
			View.RemovingModeUI.TransitionImmediate(isRemoving);
		}
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
