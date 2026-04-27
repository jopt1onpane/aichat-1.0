using System.Collections.Generic;
using System.Linq;
using Com.ForbiddenByte.OSA.Core;
using NestopiSystem;
using R3;

namespace Bulbul.Mobile;

public class TodoListSelectorListView : OSA<TodoListSelectorListViewParam, TodoListSelectorListItemViewHolder>
{
	private OSAListDataHelper<TodoListSelectorListItemModel> Data;

	private List<TodoListSelectorListItemModel> requestDatas;

	private ulong currentSelectUuid;

	private Subject<ulong> onClickSelect = new Subject<ulong>();

	private Subject<ulong> onClickRemove = new Subject<ulong>();

	private Subject<(ulong, string)> onValueChangeTitle = new Subject<(ulong, string)>();

	private Subject<(ulong, string)> onEndEditTitle = new Subject<(ulong, string)>();

	public readonly ReactiveProperty<bool> Gate = new ReactiveProperty<bool>(value: true);

	public Observable<ulong> OnClickSelect => onClickSelect;

	public Observable<ulong> OnClickRemove => onClickRemove;

	public Observable<(ulong uuid, string title)> OnValueChangeTitle => onValueChangeTitle;

	public Observable<(ulong uuid, string title)> OnEndEditTitle => onEndEditTitle;

	public int DataCount
	{
		get
		{
			if (Data == null)
			{
				if (requestDatas == null)
				{
					return 0;
				}
				return requestDatas.Count;
			}
			return Data.Count;
		}
	}

	protected override void Start()
	{
		if (Data == null)
		{
			Data = new OSAListDataHelper<TodoListSelectorListItemModel>(this);
		}
		_Params?.itemPrefab.gameObject.SetActive(value: false);
		base.Start();
		if (requestDatas != null)
		{
			Data.InsertItemsAtEnd(requestDatas);
			requestDatas = null;
		}
	}

	public void EnterSettings(TodoListUIModel model)
	{
		currentSelectUuid = model.CurrentTodoListUuid;
		IReadOnlyDictionary<ulong, TodoListData> todoListDic = model.TodoListDic;
		List<TodoListSelectorListItemModel> list = new List<TodoListSelectorListItemModel>();
		foreach (TodoListData value in todoListDic.Values)
		{
			if (value != null)
			{
				TodoListSelectorListItemModel item = new TodoListSelectorListItemModel(currentSelectUuid, value);
				list.Add(item);
			}
		}
		if (!base.IsInitialized)
		{
			requestDatas = list;
			return;
		}
		if (Data.Count > 0)
		{
			Data.RemoveItems(0, Data.Count);
		}
		Data.InsertItemsAtEnd(list);
	}

	protected override TodoListSelectorListItemViewHolder CreateViewsHolder(int itemIndex)
	{
		TodoListSelectorListItemViewHolder todoListSelectorListItemViewHolder = new TodoListSelectorListItemViewHolder(Gate);
		todoListSelectorListItemViewHolder.Init(_Params.itemPrefab, _Params.Content, itemIndex);
		todoListSelectorListItemViewHolder.View.OnClickSelect.Subscribe(delegate(ulong uuid)
		{
			OnClickSelectViewHolder(uuid);
		}).AddTo(this);
		todoListSelectorListItemViewHolder.View.OnClickRemove.Subscribe(delegate(ulong uuid)
		{
			OnClickRemoveViewHolder(uuid);
		}).AddTo(this);
		todoListSelectorListItemViewHolder.View.OnValueChangeTitle.Subscribe(delegate((ulong uuid, string title) info)
		{
			OnValueChangeViewHolderTitle(info.uuid, info.title);
		}).AddTo(this);
		todoListSelectorListItemViewHolder.View.OnEndEditTitle.Subscribe(delegate((ulong uuid, string title) info)
		{
			OnEndEditViewHolderTitle(info.uuid, info.title);
		}).AddTo(this);
		return todoListSelectorListItemViewHolder;
	}

	protected override void UpdateViewsHolder(TodoListSelectorListItemViewHolder newOrRecycled)
	{
		TodoListSelectorListItemModel model = Data[newOrRecycled.ItemIndex];
		newOrRecycled.UpdateModel(model, TodoListUIModel.IsListRemoveMode);
	}

	public void SelectedTodoListItem(ulong uuid)
	{
		currentSelectUuid = uuid;
		foreach (TodoListSelectorListItemModel item in (IEnumerable<TodoListSelectorListItemModel>)Data)
		{
			if (item == null)
			{
				return;
			}
			item.IsCurrentSelect = currentSelectUuid == item.UniqueId;
		}
		bool isListRemoveMode = TodoListUIModel.IsListRemoveMode;
		foreach (TodoListSelectorListItemViewHolder visibleItem in _VisibleItems)
		{
			if (visibleItem == null)
			{
				break;
			}
			visibleItem.UpdateModel(Data[visibleItem.ItemIndex], isListRemoveMode);
		}
	}

	public void AddTodoListItem(TodoListData data)
	{
		TodoListSelectorListItemModel model = new TodoListSelectorListItemModel(currentSelectUuid, data, isCreateNew: true);
		Data.InsertOneAtEnd(model);
		ScrollTo(Data.Count - 1, 0.5f, 0.5f);
		bool isListRemoveMode = TodoListUIModel.IsListRemoveMode;
		foreach (TodoListSelectorListItemViewHolder visibleItem in _VisibleItems)
		{
			visibleItem?.View.SetRemoveMode(isListRemoveMode);
		}
	}

	public void RemoveTodoListItem(ulong uuid)
	{
		(TodoListSelectorListItemModel, int) tuple = Data.Indexed().FirstOrDefault(((TodoListSelectorListItemModel item, int index) x) => x.item.UniqueId == uuid);
		if (tuple.Item1 == null)
		{
			return;
		}
		Data.RemoveOne(tuple.Item2);
		bool isListRemoveMode = TodoListUIModel.IsListRemoveMode;
		foreach (TodoListSelectorListItemViewHolder visibleItem in _VisibleItems)
		{
			visibleItem?.View.SetRemoveMode(isListRemoveMode);
		}
	}

	public void ChangeTodoListTitle(ulong uuid, string title)
	{
		(TodoListSelectorListItemModel, int) tuple = Data.Indexed().FirstOrDefault(((TodoListSelectorListItemModel item, int index) x) => x.item.UniqueId == uuid);
		tuple.Item1.Title = title;
		GetItemViewsHolder(tuple.Item2)?.UpdateModel(tuple.Item1, TodoListUIModel.IsListRemoveMode);
	}

	private void OnClickSelectViewHolder(ulong uuid)
	{
		onClickSelect?.OnNext(uuid);
	}

	private void OnClickRemoveViewHolder(ulong uuid)
	{
		onClickRemove?.OnNext(uuid);
	}

	private void OnValueChangeViewHolderTitle(ulong uuid, string title)
	{
		onValueChangeTitle?.OnNext((uuid, title));
	}

	private void OnEndEditViewHolderTitle(ulong uuid, string title)
	{
		onEndEditTitle?.OnNext((uuid, title));
	}
}
