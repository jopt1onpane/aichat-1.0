using Com.ForbiddenByte.OSA.Core;
using UnityEngine;

namespace Bulbul.Mobile;

public class NoteContentListItemViewHolder : BaseItemViewsHolder, IDragReorderableItemVH
{
	private NoteContentListItemView _view;

	public NoteContentListItemView View => _view;

	public ItemDragReorderHandle DragHandle => _view.DragHandle;

	public RectTransform RectTransform => root;

	public override void CollectViews()
	{
		base.CollectViews();
		_view = root.GetComponentInChildren<NoteContentListItemView>();
	}

	public void SetModel(NoteContentListItemModel model, bool isPlaceholder, bool isRemovingMode = true)
	{
		_view.SetActive(!isPlaceholder);
		if (!isPlaceholder)
		{
			_view.SetTitleText(model.Title);
			_view.PageUniqueId = model.pageUniqueID;
			_view.ButtonSetup();
			_view.ChangeRemovingMode(isRemovingMode, isImmediate: true);
			_view.SetActiveRaycastBlocker(isActive: false);
			_view.InitAlpha();
			_view.InitPos();
		}
	}

	public void ChangeRemovingMode(bool isRemoving, bool isImmediate = false)
	{
		_view.ChangeRemovingMode(isRemoving, isImmediate);
	}

	public bool CheckSamePageUniqueID(ulong pageUniqueID)
	{
		return _view.PageUniqueId == pageUniqueID;
	}

	public void UnsetModel()
	{
	}

	void IDragReorderableItemVH.OnBeginDrag()
	{
		_view.SetDraggingScale(isDragging: true);
		_view.SetActiveDraggingObjs(active: true);
	}

	void IDragReorderableItemVH.OnEndDrag()
	{
		_view.SetDraggingScale(isDragging: false);
		_view.SetActiveDraggingObjs(active: false);
	}

	void IDragReorderableItemVH.OnPointerDownIfNotDragged()
	{
		_view.SetDraggingScale(isDragging: true);
		_view.SetActiveDraggingObjs(active: true);
	}

	void IDragReorderableItemVH.OnPointerExitOrUpIfNotDragged()
	{
		_view.SetDraggingScale(isDragging: false);
		_view.SetActiveDraggingObjs(active: false);
	}
}
