using System.Collections.Generic;
using Com.ForbiddenByte.OSA.Core;
using Com.ForbiddenByte.OSA.DataHelpers;
using UnityEngine;

namespace Bulbul.ScrollListSample;

public class SampleListView : OSA<SampleListParams, SampleListItemViewsHolder>
{
	private SimpleDataHelper<SampleItemModel> _data;

	protected override void Start()
	{
		_data = new SimpleDataHelper<SampleItemModel>(this);
		_Params.NormalPrefab.gameObject.SetActive(value: false);
		_Params.RichPrefab.gameObject.SetActive(value: false);
		base.Start();
		List<SampleItemModel> list = new List<SampleItemModel>();
		for (int i = 0; i < 100; i++)
		{
			list.Add(new SampleItemModel
			{
				Title = "Item " + i,
				IsRich = (i % 2 == 0)
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

	protected override SampleListItemViewsHolder CreateViewsHolder(int itemIndex)
	{
		SampleListItemViewsHolder sampleListItemViewsHolder = new SampleListItemViewsHolder();
		RectTransform rootPrefab = (_data[itemIndex].IsRich ? _Params.RichPrefab : _Params.NormalPrefab);
		sampleListItemViewsHolder.Init(rootPrefab, _Params.Content, itemIndex);
		return sampleListItemViewsHolder;
	}

	protected override void UpdateViewsHolder(SampleListItemViewsHolder item)
	{
		SampleItemModel model = _data[item.ItemIndex];
		item.SetModel(model);
	}

	protected override void OnBeforeRecycleOrDisableViewsHolder(SampleListItemViewsHolder item, int newItemIndex)
	{
		item.UnsetModel();
		base.OnBeforeRecycleOrDisableViewsHolder(item, newItemIndex);
	}
}
