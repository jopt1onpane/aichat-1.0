using System.Collections.Generic;
using Com.ForbiddenByte.OSA.Core;
using UnityEngine;

namespace Bulbul.ScrollListSample;

public class DraggableListView : OSA<DraggableListParams, DraggableItemViewsHolder>
{
	[SerializeField]
	private DraggableListDragManipulator _dragManipulator;

	private OSAListDataHelper<DraggableItemModel> _data;

	protected override void Start()
	{
		_data = new OSAListDataHelper<DraggableItemModel>(this);
		_Params.ItemPrefab.gameObject.SetActive(value: false);
		_dragManipulator.Init(this, _data);
		base.Start();
		List<DraggableItemModel> list = new List<DraggableItemModel>();
		for (int i = 0; i < 100; i++)
		{
			list.Add(new DraggableItemModel
			{
				Title = "Item " + i
			});
		}
		_data.InsertItemsAtEnd(list);
	}

	protected override void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
	{
		base.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, itemsDesc);
		for (int i = 0; i < count; i++)
		{
			itemsDesc.BeginChangingItemsSizes(i);
			itemsDesc[i] = _Params.DefaultItemSize + (float)(i % 10 * 10);
			itemsDesc.EndChangingItemsSizes();
		}
	}

	protected override DraggableItemViewsHolder CreateViewsHolder(int itemIndex)
	{
		DraggableItemViewsHolder draggableItemViewsHolder = new DraggableItemViewsHolder();
		draggableItemViewsHolder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		draggableItemViewsHolder.DragHandle.Init(_dragManipulator, draggableItemViewsHolder);
		return draggableItemViewsHolder;
	}

	protected override void UpdateViewsHolder(DraggableItemViewsHolder item)
	{
		DraggableItemModel model = _data[item.ItemIndex];
		bool isPlaceholder = _dragManipulator.IsPlaceHolderModel(model);
		item.SetModel(model, isPlaceholder);
	}

	protected override void OnBeforeRecycleOrDisableViewsHolder(DraggableItemViewsHolder item, int newItemIndex)
	{
		item.UnsetModel();
		base.OnBeforeRecycleOrDisableViewsHolder(item, newItemIndex);
	}

	protected override void Update()
	{
		base.Update();
		if (base.IsInitialized)
		{
			_dragManipulator.Update();
		}
	}
}
