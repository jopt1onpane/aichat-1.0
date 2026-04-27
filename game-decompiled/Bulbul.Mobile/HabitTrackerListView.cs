using System;
using System.Collections.Generic;
using System.Linq;
using Com.ForbiddenByte.OSA.Core;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class HabitTrackerListView : OSA<HabitTrackerListParam, HabitTrackerListViewHolder>, IOSAModelIndexGetter<string>, IInfomationProviderForOSAMyAnimation
{
	[SerializeField]
	private HabitTrackerListDragReorderManipulator _dragManipulator;

	private OSAListDataHelper<HabitTrackerListItemModel> Data;

	private HabitTrackerUIModel model;

	private IEnumerable<string> _requestHabit;

	private ItemRemoveAnimationState<string> itemRemoveAnimation;

	private List<string> removeRequestedItemIds = new List<string>();

	private bool _isRemoveMode;

	private bool _isSettingMode;

	private Subject<int> onChangeRemoveButtonState = new Subject<int>();

	private Subject<string> onClickRemoveHabit = new Subject<string>();

	private Subject<(string uuid, string title)> onChangeHabitTitle = new Subject<(string, string)>();

	private Subject<(string uuid, bool enable)> onChangeHabitPeriod = new Subject<(string, bool)>();

	private Subject<(string uuid, DateTime date, bool isComplete)> onChangeHabitComplete = new Subject<(string, DateTime, bool)>();

	private Subject<(string uuid, DayOfWeek date, bool enable)> onChangeHabitEnable = new Subject<(string, DayOfWeek, bool)>();

	private Subject<(string from, string to)> onChangeReorder = new Subject<(string, string)>();

	public Observable<int> OnChangeRemoveButtonState => onChangeRemoveButtonState;

	public Observable<string> OnClickRemoveHabit => onClickRemoveHabit;

	public Observable<(string uuid, string title)> OnChangeHabitTitle => onChangeHabitTitle;

	public Observable<(string uuid, bool enable)> OnChangeHabitPeriod => onChangeHabitPeriod;

	public Observable<(string uuid, DateTime date, bool isComplete)> OnChangeHabitComplete => onChangeHabitComplete;

	public Observable<(string uuid, DayOfWeek date, bool enable)> OnChangeHabitEnable => onChangeHabitEnable;

	public Observable<(string from, string to)> OnChangeReorder => onChangeReorder;

	protected override void OnDestroy()
	{
		onClickRemoveHabit?.Dispose();
		onChangeHabitTitle?.Dispose();
		onChangeHabitPeriod?.Dispose();
		onChangeHabitComplete?.Dispose();
		onChangeHabitEnable?.Dispose();
		if (itemRemoveAnimation != null)
		{
			itemRemoveAnimation.Cancel();
		}
		base.OnDestroy();
	}

	protected override void OnDisable()
	{
		if (itemRemoveAnimation != null)
		{
			itemRemoveAnimation.Cancel();
		}
		base.OnDisable();
	}

	protected override void Start()
	{
		Data = new OSAListDataHelper<HabitTrackerListItemModel>(this);
		_dragManipulator.Init(this, Data);
		_dragManipulator.OnChangeOrder.Subscribe(delegate((int movedIdx, HabitTrackerListItemModel draggedModel) info)
		{
			HabitTrackerListItemModel item = info.draggedModel;
			HabitTrackerListItemModel habitTrackerListItemModel = ((info.movedIdx == 0) ? null : Data[info.movedIdx - 1]);
			string uniqueId = item.UniqueId;
			string item2 = habitTrackerListItemModel?.UniqueId;
			onChangeReorder?.OnNext((uniqueId, item2));
		}).AddTo(this);
		itemRemoveAnimation = new ItemRemoveAnimationState<string>(this, this);
		itemRemoveAnimation.OnComplete.Subscribe(delegate(string uuid)
		{
			removeRequestedItemIds.Add(uuid);
		}).AddTo(this);
		base.Start();
		_Params.itemPrefab.gameObject.SetActive(value: false);
		if (_requestHabit != null)
		{
			UpdateHabitData(_requestHabit);
			_requestHabit = null;
		}
		UpdateRemoveButtonState();
	}

	protected override void Update()
	{
		base.Update();
		if (!base.IsInitialized)
		{
			return;
		}
		_dragManipulator.Update();
		bool flag = false;
		foreach (string removeRequestedItemId in removeRequestedItemIds)
		{
			(HabitTrackerListItemModel, int) indexedModel = GetIndexedModel(removeRequestedItemId);
			if (indexedModel.Item1 != null)
			{
				Data.RemoveOne(indexedModel.Item2);
				flag = true;
			}
		}
		if (flag)
		{
			UpdateRemoveButtonState();
		}
		removeRequestedItemIds.Clear();
	}

	public void EnterSetting(HabitTrackerUIModel model)
	{
		this.model = model;
		if (!base.IsInitialized)
		{
			_requestHabit = this.model.GetAllHabitIds(includeDeleted: false);
			return;
		}
		_isRemoveMode = model.IsRemoveMode;
		_isSettingMode = model.IsSettingMode;
		UpdateHabitData(this.model.GetAllHabitIds(includeDeleted: false));
	}

	public void UpdateHabitData(IEnumerable<string> habitIds)
	{
		if (Data.Count > 0)
		{
			Data.RemoveItems(0, Data.Count);
		}
		List<HabitTrackerListItemModel> list = new List<HabitTrackerListItemModel>();
		foreach (string habitId in habitIds)
		{
			HabitTrackerListItemModel itemModel = new HabitTrackerListItemModel
			{
				UniqueId = habitId,
				Title = model.GetHabitTitle(habitId),
				IsAlivePeriod = model.IsDateInAlivePeriod(habitId, DateTime.Today)
			};
			itemModel.dayInfos = Enumerable.Range(0, 7).Select(delegate(int i)
			{
				HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = new HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel();
				DateTime date = (habitTrackerListItemDayInfoModel.date = model.CurrentStartDate.AddDays(i));
				habitTrackerListItemDayInfoModel.enableState = model.GetHabitDayEnableState(itemModel.UniqueId, date);
				habitTrackerListItemDayInfoModel.isChecked = model.IsHabitDayCompleted(itemModel.UniqueId, date);
				return habitTrackerListItemDayInfoModel;
			}).ToArray();
			list.Add(itemModel);
		}
		Data.InsertItemsAtStart(list);
	}

	public void AddHabit(string uuid)
	{
		HabitTrackerListItemModel itemModel = new HabitTrackerListItemModel
		{
			UniqueId = uuid,
			Title = model.GetHabitTitle(uuid),
			IsAlivePeriod = model.IsDateInAlivePeriod(uuid, DateTime.Today),
			IsCreateNew = true
		};
		itemModel.dayInfos = Enumerable.Range(0, 7).Select(delegate(int i)
		{
			HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = new HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel();
			DateTime date = (habitTrackerListItemDayInfoModel.date = model.CurrentStartDate.AddDays(i));
			if (!_isSettingMode)
			{
				habitTrackerListItemDayInfoModel.enableState = model.GetHabitDayEnableState(itemModel.UniqueId, date);
			}
			else
			{
				bool flag = model.IsHabitDayOfWeekEnabled(itemModel.UniqueId, date.DayOfWeek);
				habitTrackerListItemDayInfoModel.enableState = ((!flag) ? HabitDateEnableState.Disabled : HabitDateEnableState.Enabled);
			}
			habitTrackerListItemDayInfoModel.isChecked = model.IsHabitDayCompleted(itemModel.UniqueId, date);
			return habitTrackerListItemDayInfoModel;
		}).ToArray();
		Data.InsertOneAtEnd(itemModel);
		SmoothScrollTo(Data.Count - 1, 0.25f, 0.5f, 0.5f);
		UpdateRemoveButtonState();
	}

	public void RemoveHabit(string uuid)
	{
		(HabitTrackerListItemModel, int) indexedModel = GetIndexedModel(uuid);
		if (indexedModel.Item2 != -1)
		{
			HabitTrackerListViewHolder itemViewsHolderIfVisible = GetItemViewsHolderIfVisible(indexedModel.Item2);
			if (itemViewsHolderIfVisible != null)
			{
				itemRemoveAnimation.Play(itemViewsHolderIfVisible.View, uuid, _Params.DefaultItemSize, 0.2f).Forget();
			}
			else
			{
				Data.RemoveOne(indexedModel.Item2);
			}
		}
	}

	public void ChangeHabitTitle(string uuid, string title)
	{
		(HabitTrackerListItemModel, int) indexedModel = GetIndexedModel(uuid);
		if (indexedModel.Item1 == null)
		{
			return;
		}
		indexedModel.Item1.Title = title;
		foreach (HabitTrackerListViewHolder visibleItem in _VisibleItems)
		{
			if (visibleItem != null && Data[visibleItem.ItemIndex].UniqueId == uuid)
			{
				visibleItem.ChangeTitle(title);
				break;
			}
		}
	}

	public void ChangeHabitPeriod(string uuid)
	{
		(HabitTrackerListItemModel model, int index) data = GetIndexedModel(uuid);
		if (data.model == null)
		{
			return;
		}
		data.model.IsAlivePeriod = model.IsDateInAlivePeriod(uuid, DateTime.Today);
		data.model.dayInfos = Enumerable.Range(0, 7).Select(delegate(int i)
		{
			HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = new HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel();
			DateTime date = (habitTrackerListItemDayInfoModel.date = model.CurrentStartDate.AddDays(i));
			if (!_isSettingMode)
			{
				habitTrackerListItemDayInfoModel.enableState = model.GetHabitDayEnableState(data.model.UniqueId, date);
			}
			else
			{
				HabitDateEnableState habitDateEnableState = model.GetHabitDayEnableState(data.model.UniqueId, date, _isSettingMode);
				if (habitDateEnableState == HabitDateEnableState.DeadPeriod)
				{
					habitDateEnableState = ((!model.IsHabitDayOfWeekEnabled(data.model.UniqueId, date.DayOfWeek)) ? HabitDateEnableState.Disabled : HabitDateEnableState.Enabled);
				}
				habitTrackerListItemDayInfoModel.enableState = habitDateEnableState;
			}
			habitTrackerListItemDayInfoModel.isChecked = model.IsHabitDayCompleted(data.model.UniqueId, date);
			return habitTrackerListItemDayInfoModel;
		}).ToArray();
		foreach (HabitTrackerListViewHolder visibleItem in _VisibleItems)
		{
			if (visibleItem != null && Data[visibleItem.ItemIndex].UniqueId == uuid)
			{
				visibleItem.ChangeHabitPeriod();
				break;
			}
		}
	}

	public void ChangeHabitComplete(string uuid, DateTime date, bool isComplete)
	{
		(HabitTrackerListItemModel, int) indexedModel = GetIndexedModel(uuid);
		if (indexedModel.Item1 == null)
		{
			return;
		}
		HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = Enumerable.FirstOrDefault(indexedModel.Item1.dayInfos, (HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel x) => x.date == date);
		HabitDateEnableState enableState = habitTrackerListItemDayInfoModel.enableState;
		habitTrackerListItemDayInfoModel.isChecked = isComplete;
		habitTrackerListItemDayInfoModel.enableState = model.GetHabitDayEnableState(uuid, date);
		foreach (HabitTrackerListViewHolder visibleItem in _VisibleItems)
		{
			if (visibleItem != null && Data[visibleItem.ItemIndex].UniqueId == uuid)
			{
				visibleItem.ChangeHabitComplete(date, isComplete, enableState);
				break;
			}
		}
	}

	public void ChangeHabitEnable(string uuid, DayOfWeek date, bool enable)
	{
		(HabitTrackerListItemModel, int) indexedModel = GetIndexedModel(uuid);
		if (indexedModel.Item1 == null)
		{
			return;
		}
		Enumerable.FirstOrDefault(indexedModel.Item1.dayInfos, (HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel x) => x.date.DayOfWeek == date).enableState = ((!enable) ? HabitDateEnableState.Disabled : HabitDateEnableState.Enabled);
		foreach (HabitTrackerListViewHolder visibleItem in _VisibleItems)
		{
			if (visibleItem != null && Data[visibleItem.ItemIndex].UniqueId == uuid)
			{
				visibleItem.ChangeHabitEnable(date, enable);
				break;
			}
		}
	}

	public void UpdateHabitDayOfWeekEnableComplete(bool isApply)
	{
		foreach (HabitTrackerListItemModel itemModel in (IEnumerable<HabitTrackerListItemModel>)Data)
		{
			if (itemModel != null)
			{
				itemModel.dayInfos = Enumerable.Range(0, 7).Select(delegate(int i)
				{
					HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = new HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel();
					DateTime date = (habitTrackerListItemDayInfoModel.date = model.CurrentStartDate.AddDays(i));
					habitTrackerListItemDayInfoModel.enableState = model.GetHabitDayEnableState(itemModel.UniqueId, date);
					habitTrackerListItemDayInfoModel.isChecked = model.IsHabitDayCompleted(itemModel.UniqueId, date);
					return habitTrackerListItemDayInfoModel;
				}).ToArray();
			}
		}
		foreach (HabitTrackerListViewHolder visibleItem in _VisibleItems)
		{
			visibleItem?.UpdateView(Data[visibleItem.ItemIndex], isPlaceHolder: false, _isRemoveMode, _isSettingMode);
		}
	}

	public void ChangeWeek(bool isNext)
	{
		foreach (HabitTrackerListItemModel itemModel in (IEnumerable<HabitTrackerListItemModel>)Data)
		{
			if (itemModel == null)
			{
				continue;
			}
			itemModel.dayInfos = Enumerable.Range(0, 7).Select(delegate(int i)
			{
				HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = new HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel();
				DateTime date = (habitTrackerListItemDayInfoModel.date = model.CurrentStartDate.AddDays(i));
				if (!_isSettingMode)
				{
					habitTrackerListItemDayInfoModel.enableState = model.GetHabitDayEnableState(itemModel.UniqueId, date);
				}
				else
				{
					HabitDateEnableState habitDateEnableState = model.GetHabitDayEnableState(itemModel.UniqueId, date, _isSettingMode);
					if (habitDateEnableState == HabitDateEnableState.DeadPeriod)
					{
						habitDateEnableState = ((!model.IsHabitDayOfWeekEnabled(itemModel.UniqueId, date.DayOfWeek)) ? HabitDateEnableState.Disabled : HabitDateEnableState.Enabled);
					}
					habitTrackerListItemDayInfoModel.enableState = habitDateEnableState;
				}
				habitTrackerListItemDayInfoModel.isChecked = model.IsHabitDayCompleted(itemModel.UniqueId, date);
				return habitTrackerListItemDayInfoModel;
			}).ToArray();
		}
		foreach (HabitTrackerListViewHolder visibleItem in _VisibleItems)
		{
			visibleItem?.UpdateView(Data[visibleItem.ItemIndex], isPlaceHolder: false, _isRemoveMode, _isSettingMode);
		}
	}

	public void ChangeRemoveMode(bool isRemoveMode)
	{
		_isRemoveMode = isRemoveMode;
		foreach (HabitTrackerListViewHolder visibleItem in _VisibleItems)
		{
			visibleItem?.ChangeRemoveMode(_isRemoveMode);
		}
	}

	public void ChangeSettingMode(bool isSettingMode)
	{
		_isSettingMode = isSettingMode;
		if (_isSettingMode)
		{
			foreach (HabitTrackerListItemModel itemModel in (IEnumerable<HabitTrackerListItemModel>)Data)
			{
				if (itemModel != null)
				{
					itemModel.dayInfos = Enumerable.Range(0, 7).Select(delegate(int i)
					{
						HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = new HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel();
						DateTime date = (habitTrackerListItemDayInfoModel.date = model.CurrentStartDate.AddDays(i));
						bool flag = model.IsHabitDayOfWeekEnabled(itemModel.UniqueId, date.DayOfWeek);
						habitTrackerListItemDayInfoModel.enableState = ((!flag) ? HabitDateEnableState.Disabled : HabitDateEnableState.Enabled);
						habitTrackerListItemDayInfoModel.isChecked = model.IsHabitDayCompleted(itemModel.UniqueId, date);
						return habitTrackerListItemDayInfoModel;
					}).ToArray();
				}
			}
		}
		else
		{
			foreach (HabitTrackerListItemModel itemModel2 in (IEnumerable<HabitTrackerListItemModel>)Data)
			{
				if (itemModel2 != null)
				{
					itemModel2.dayInfos = Enumerable.Range(0, 7).Select(delegate(int i)
					{
						HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = new HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel();
						DateTime date = (habitTrackerListItemDayInfoModel.date = model.CurrentStartDate.AddDays(i));
						habitTrackerListItemDayInfoModel.enableState = model.GetHabitDayEnableState(itemModel2.UniqueId, date);
						habitTrackerListItemDayInfoModel.isChecked = model.IsHabitDayCompleted(itemModel2.UniqueId, date);
						return habitTrackerListItemDayInfoModel;
					}).ToArray();
				}
			}
		}
		foreach (HabitTrackerListViewHolder visibleItem in _VisibleItems)
		{
			visibleItem?.ChangeSettingMode(_isSettingMode);
		}
	}

	protected override HabitTrackerListViewHolder CreateViewsHolder(int itemIndex)
	{
		HabitTrackerListViewHolder habitTrackerListViewHolder = new HabitTrackerListViewHolder();
		habitTrackerListViewHolder.Init(_Params.itemPrefab, _Params.Content, itemIndex);
		((IDragReorderableItemVH)habitTrackerListViewHolder)?.DragHandle.Init(_dragManipulator, habitTrackerListViewHolder);
		habitTrackerListViewHolder.View.OnRemoveHabit.Subscribe(delegate(string uuid)
		{
			onClickRemoveHabit?.OnNext(uuid);
		}).AddTo(this);
		habitTrackerListViewHolder.View.OnChangeHabitTitle.Subscribe(delegate((string uuid, string title) info)
		{
			onChangeHabitTitle?.OnNext(info);
		}).AddTo(this);
		habitTrackerListViewHolder.View.OnChangeHabitPeriod.Subscribe(delegate((string uuid, bool enable) info)
		{
			onChangeHabitPeriod?.OnNext(info);
		}).AddTo(this);
		habitTrackerListViewHolder.View.OnChangeHabitCompleted.Subscribe(delegate((string uuid, DateTime date, bool isCompleted) info)
		{
			onChangeHabitComplete?.OnNext(info);
		}).AddTo(this);
		habitTrackerListViewHolder.View.OnChangeHabitEnable.Subscribe(delegate((string uuid, DayOfWeek date, bool enable) info)
		{
			onChangeHabitEnable?.OnNext(info);
		}).AddTo(this);
		return habitTrackerListViewHolder;
	}

	protected override void UpdateViewsHolder(HabitTrackerListViewHolder newOrRecycled)
	{
		HabitTrackerListItemModel habitTrackerListItemModel = Data[newOrRecycled.ItemIndex];
		bool isPlaceHolder = _dragManipulator.IsPlaceHolderModel(habitTrackerListItemModel);
		newOrRecycled.UpdateView(habitTrackerListItemModel, isPlaceHolder, _isRemoveMode, _isSettingMode);
	}

	private (HabitTrackerListItemModel model, int index) GetIndexedModel(string uuid)
	{
		return Data.Indexed().FirstOrDefault(((HabitTrackerListItemModel item, int index) x) => x.item != null && x.item.UniqueId == uuid, (null, -1));
	}

	private int GetModelIndex(string uuid)
	{
		foreach (var item in Data.Indexed())
		{
			if (item.item != null && item.item.UniqueId == uuid)
			{
				return item.index;
			}
		}
		return -1;
	}

	int IOSAModelIndexGetter<string>.GetModelIndex(string equatable)
	{
		return GetModelIndex(equatable);
	}

	void IInfomationProviderForOSAMyAnimation.RequestChangeItemSizeAndUpdateLayout(int idx, float size, bool endEdgeStationary = false)
	{
		RequestChangeItemSizeAndUpdateLayout(idx, size, endEdgeStationary);
	}

	void IInfomationProviderForOSAMyAnimation.RequestChangeItemSize(int idx, float size)
	{
	}

	private void UpdateRemoveButtonState()
	{
		int count = Data.Count;
		onChangeRemoveButtonState?.OnNext(count);
		if (_isRemoveMode && count <= 0)
		{
			ChangeRemoveMode(isRemoveMode: false);
		}
	}
}
