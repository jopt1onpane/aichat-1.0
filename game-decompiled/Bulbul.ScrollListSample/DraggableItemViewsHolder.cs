using Com.ForbiddenByte.OSA.Core;
using UnityEngine;

namespace Bulbul.ScrollListSample;

public class DraggableItemViewsHolder : BaseItemViewsHolder, IDragReorderableItemVH
{
	private DraggableItemView _view;

	public ItemDragReorderHandle DragHandle => _view.DragHandle;

	public RectTransform RectTransform => _view.transform as RectTransform;

	public override void CollectViews()
	{
		base.CollectViews();
		_view = root.GetComponent<DraggableItemView>();
	}

	public void SetModel(DraggableItemModel model, bool isPlaceholder)
	{
		_view.Content.SetActive(!isPlaceholder);
		if (!isPlaceholder)
		{
			_view.TitleText.text = model.Title;
		}
	}

	public void UnsetModel()
	{
	}

	public void OnBeginDrag()
	{
		_view.Content.transform.localScale = new Vector3(_view.DraggingScale, _view.DraggingScale, _view.DraggingScale);
	}

	public void OnEndDrag()
	{
		_view.Content.transform.localScale = Vector3.one;
	}

	public void OnPointerDownIfNotDragged()
	{
	}

	public void OnPointerExitOrUpIfNotDragged()
	{
	}
}
