using System.Collections.Generic;
using Bulbul.Web;
using Com.ForbiddenByte.OSA.Core;
using R3;

namespace Bulbul.Mobile;

public class NewsListView : OSA<NewsListParam, NewsListViewHolder>
{
	private OSAListDataHelper<NewsListItemModel> _data;

	private List<NewsListItemModel> _requestDatas;

	private Subject<NewsData> onClickNewsCell = new Subject<NewsData>();

	private int _selectedIdx;

	public Observable<NewsData> OnClickNewsCell => onClickNewsCell;

	protected override void Start()
	{
		_data = new OSAListDataHelper<NewsListItemModel>(this);
		base.Start();
		if (_requestDatas != null)
		{
			_data.InsertItemsAtStart(_requestDatas);
			_requestDatas = null;
		}
	}

	public void ResetNewsData()
	{
		if (base.IsInitialized)
		{
			_data.List.Clear();
			_data.NotifyListChangedExternally();
		}
	}

	public void SetNewsData(NewsData[] newsData)
	{
		_selectedIdx = 0;
		List<NewsListItemModel> list = new List<NewsListItemModel>();
		for (int i = 0; i < newsData.Length; i++)
		{
			NewsListItemModel item = new NewsListItemModel(newsData[i]);
			list.Add(item);
		}
		if (!base.IsInitialized)
		{
			_requestDatas = list;
		}
		else if (_data.Count > 0)
		{
			_data.List.Clear();
			_data.List.AddRange(list);
			_data.NotifyListChangedExternally();
		}
		else
		{
			_data.InsertItemsAtStart(list);
		}
	}

	public void SelectTop()
	{
		if (_data.List.Count != 0)
		{
			Select(0);
		}
	}

	private void Select(int idx)
	{
		int selectedIdx = _selectedIdx;
		_selectedIdx = idx;
		_data.List[selectedIdx].IsSelected = false;
		_data.List[_selectedIdx].IsSelected = true;
		onClickNewsCell.OnNext(_data.List[_selectedIdx].NewsData);
		GetItemViewsHolderIfVisible(selectedIdx)?.SetModel(_data.List[selectedIdx]);
		GetItemViewsHolderIfVisible(_selectedIdx)?.SetModel(_data.List[_selectedIdx]);
	}

	protected override NewsListViewHolder CreateViewsHolder(int itemIndex)
	{
		NewsListViewHolder holder = new NewsListViewHolder();
		holder.Init(_Params._prefab, _Params.Content, itemIndex);
		ObservableSubscribeExtensions.Subscribe(holder.View.OnClickNotice, delegate
		{
			Select(holder.ItemIndex);
		}).AddTo(this);
		return holder;
	}

	protected override void OnItemHeightChangedPreTwinPass(NewsListViewHolder viewsHolder)
	{
		base.OnItemHeightChangedPreTwinPass(viewsHolder);
	}

	protected override void UpdateViewsHolder(NewsListViewHolder newOrRecycled)
	{
		NewsListItemModel model = _data[newOrRecycled.ItemIndex];
		newOrRecycled.SetModel(model);
		ScheduleComputeVisibilityTwinPass();
	}

	protected override void OnItemIndexChangedDueInsertOrRemove(NewsListViewHolder shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
	{
		base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);
		ScheduleComputeVisibilityTwinPass();
	}

	protected override void RebuildLayoutDueToScrollViewSizeChange()
	{
		base.RebuildLayoutDueToScrollViewSizeChange();
	}
}
