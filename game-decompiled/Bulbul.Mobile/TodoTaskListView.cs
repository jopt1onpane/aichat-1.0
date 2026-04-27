using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com.ForbiddenByte.OSA.Core;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class TodoTaskListView : OSA<TodoTaskListParam, TodoTaskListBaseViewHolder>, IInfomationProviderForOSAMyAnimation, IOSAModelIndexGetter<ulong>
{
	private class CompletedAndUnCompletedAnimationHelper
	{
		private CancellationTokenSource _cancellationTokenSource;

		public HashSet<ulong> PendingExecutionRemove = new HashSet<ulong>();

		public HashSet<TodoData> PendingExecutionAddCompleted = new HashSet<TodoData>();

		public HashSet<TodoData> PendingExecutionAddUnCompleted = new HashSet<TodoData>();

		public HashSet<ulong> RemoveScheduleds = new HashSet<ulong>();

		public HashSet<TodoData> AddCompletedScheduleds = new HashSet<TodoData>();

		public HashSet<TodoData> AddUnCompletedScheduleds = new HashSet<TodoData>();

		private ItemRemoveAnimationState<ulong> _removingAnimation;

		private Subject<ulong> _onRequestedRemove = new Subject<ulong>();

		private Subject<ulong> _onRequestedRemoveFromCanceled = new Subject<ulong>();

		private float _baseHeight;

		public bool IsPlaying => PendingExecutionRemove.Count + PendingExecutionAddCompleted.Count + PendingExecutionAddUnCompleted.Count + RemoveScheduleds.Count + AddCompletedScheduleds.Count + AddUnCompletedScheduleds.Count != 0;

		public Observable<ulong> OnRequestedRemove => _onRequestedRemove;

		public Observable<ulong> OnRequestedRemoveFromCanceled => _onRequestedRemoveFromCanceled;

		public void Clear()
		{
			RemoveScheduleds.Clear();
			AddUnCompletedScheduleds.Clear();
			AddCompletedScheduleds.Clear();
			PendingExecutionAddUnCompleted.Clear();
			PendingExecutionAddCompleted.Clear();
			PendingExecutionRemove.Clear();
		}

		public CompletedAndUnCompletedAnimationHelper(IInfomationProviderForOSAMyAnimation animProvider, IOSAModelIndexGetter<ulong> getter, float baseHeight)
		{
			_removingAnimation = new ItemRemoveAnimationState<ulong>(animProvider, getter);
			_baseHeight = baseHeight;
		}

		public void Cancel()
		{
			if (_cancellationTokenSource != null)
			{
				_cancellationTokenSource.Cancel();
				_removingAnimation.Cancel();
				_cancellationTokenSource.Dispose();
				_cancellationTokenSource = null;
			}
		}

		private void CreateCancellationToken()
		{
			if (_cancellationTokenSource == null)
			{
				_cancellationTokenSource = new CancellationTokenSource();
			}
		}

		public async UniTask PlayMovingCompletedAsync(ulong uuid, TodoTaskListItemViewHolder todoTaskHolder, TodoData todoData)
		{
			if (RemoveScheduleds.Contains(uuid) || AddCompletedScheduleds.Contains(todoData))
			{
				return;
			}
			RemoveScheduleds.Add(uuid);
			AddCompletedScheduleds.Add(todoData);
			CreateCancellationToken();
			CancellationTokenSource cts = _cancellationTokenSource;
			try
			{
				int idx = todoTaskHolder.ItemIndex;
				await todoTaskHolder.View.View.ChangeUIForCompleteAsync(cts.Token);
				int itemIndex = todoTaskHolder.ItemIndex;
				if (todoTaskHolder.View.View.gameObject.activeInHierarchy && idx == itemIndex)
				{
					await _removingAnimation.Play(todoTaskHolder.View.View, uuid, _baseHeight, 0.1f, ListItemViewAnimations.RemoveAnimationDirection.Right);
				}
			}
			finally
			{
				if (!cts.IsCancellationRequested)
				{
					PendingExecutionRemove.Add(uuid);
					PendingExecutionAddCompleted.Add(todoData);
					RemoveScheduleds.Remove(uuid);
					AddCompletedScheduleds.Remove(todoData);
				}
			}
		}

		public async UniTask PlayMovingUnCompletedAsync(ulong uuid, TodoTaskListCompleteItemViewHolder todoCompletedHolder, TodoData todoData)
		{
			if (RemoveScheduleds.Contains(uuid) || AddUnCompletedScheduleds.Contains(todoData))
			{
				return;
			}
			RemoveScheduleds.Add(uuid);
			AddUnCompletedScheduleds.Add(todoData);
			CreateCancellationToken();
			CancellationTokenSource cts = _cancellationTokenSource;
			try
			{
				int idx = todoCompletedHolder.ItemIndex;
				await todoCompletedHolder.View.View.ChangeUIForUncompleteAsync(cts.Token);
				int itemIndex = todoCompletedHolder.ItemIndex;
				if (todoCompletedHolder.View.View.gameObject.activeInHierarchy && idx == itemIndex)
				{
					await _removingAnimation.Play(todoCompletedHolder.View.View, uuid, _baseHeight, 0.1f);
				}
			}
			finally
			{
				if (!cts.IsCancellationRequested)
				{
					PendingExecutionRemove.Add(uuid);
					PendingExecutionAddUnCompleted.Add(todoData);
					RemoveScheduleds.Remove(uuid);
					AddUnCompletedScheduleds.Remove(todoData);
				}
			}
		}
	}

	[SerializeField]
	[Header("並び替え制御")]
	private TodoTaskListDragReorderManipulator _dragManipulator;

	[SerializeField]
	[Header("画面全体をブロックできるオブジェクト")]
	private Image _raycastBlocker;

	private static readonly int WorkingTaskPullDownModelIdx = 0;

	private static readonly int WorkingTaskModelStartIdx = WorkingTaskPullDownModelIdx + 1;

	private Subject<ulong> onRemoveTask = new Subject<ulong>();

	private Subject<ulong> onCompleteTask = new Subject<ulong>();

	private Subject<ulong> onUncompleteTask = new Subject<ulong>();

	private Subject<(ulong uuid, string title)> onChangeTodoTitle = new Subject<(ulong, string)>();

	private Subject<(ulong uuid, DateTime? datetime)> onSelectExpireCalendar = new Subject<(ulong, DateTime?)>();

	private Subject<(ulong uuid, DateTime? datetime)> onSelectCompleteCalender = new Subject<(ulong, DateTime?)>();

	private Subject<(ulong target, ulong origin)> onSwapTodoTask = new Subject<(ulong, ulong)>();

	private OSAListDataHelper<TodoTaskListItemBaseModel> Data;

	private ReactiveProperty<bool> _dragHandleEnableChecker = new ReactiveProperty<bool>();

	private TodoListUIModel _requestData;

	private bool _requestPullDownOpen;

	private List<ulong> _removeRequestedUuid = new List<ulong>();

	private ItemRemoveAnimationState<ulong> _itemRemoveAnimation;

	private ItemInsertAnimationState<ulong> _itemInsertAnimation;

	private PullDownAnimationState _workingPullDownAnimationState;

	private PullDownAnimationStateHelper<TodoTaskListItemBaseModel, TodoTaskListItemViewHolder, TodoTaskListBaseViewHolder> _workingPullDownStateHelper = new PullDownAnimationStateHelper<TodoTaskListItemBaseModel, TodoTaskListItemViewHolder, TodoTaskListBaseViewHolder>();

	private PullDownAnimationState _completePullDownAnimationState;

	private PullDownAnimationStateHelper<TodoTaskListItemBaseModel, TodoTaskListCompleteItemViewHolder, TodoTaskListBaseViewHolder> _completePullDownStateHelper = new PullDownAnimationStateHelper<TodoTaskListItemBaseModel, TodoTaskListCompleteItemViewHolder, TodoTaskListBaseViewHolder>();

	private CompletedAndUnCompletedAnimationHelper _compAndUnCompAnimhelper;

	public Observable<ulong> OnRemoveTask => onRemoveTask;

	public Observable<ulong> OnCompleteTask => onCompleteTask;

	public Observable<ulong> OnUncompleteTask => onUncompleteTask;

	public Observable<(ulong uuid, string title)> OnChangeTodoTitle => onChangeTodoTitle;

	public Observable<(ulong uuid, DateTime? datetime)> OnSelectExpireCalendar => onSelectExpireCalendar;

	public Observable<(ulong uuid, DateTime? datetime)> OnSelectCompleteCalender => onSelectCompleteCalender;

	public Observable<(ulong target, ulong origin)> OnSwapTodoTask => onSwapTodoTask;

	int IOSAModelIndexGetter<ulong>.GetModelIndex(ulong equatable)
	{
		return GetIndexedTodoModelData(equatable).index;
	}

	void IInfomationProviderForOSAMyAnimation.RequestChangeItemSizeAndUpdateLayout(int idx, float size, bool endEdgeStationary = false)
	{
		if (Data.Count != 0 && idx < Data.Count && idx >= 0)
		{
			if (GetVirtualAbstractNormalizedScrollPosition() > 0.1)
			{
				endEdgeStationary = false;
			}
			RequestChangeItemSizeAndUpdateLayout(idx, size, endEdgeStationary, computeVisibility: true, correctItemPosition: true);
			Data[idx].Height = size;
		}
	}

	void IInfomationProviderForOSAMyAnimation.RequestChangeItemSize(int idx, float size)
	{
		if (Data.Count != 0 && idx < Data.Count && idx >= 0)
		{
			Data[idx].Height = size;
		}
	}

	private void SetActiveRaycastBlocker(bool active)
	{
		_raycastBlocker.enabled = active;
	}

	protected override void OnDestroy()
	{
		CancelAnimationsAndRequests();
		onRemoveTask?.Dispose();
		onCompleteTask?.Dispose();
		onUncompleteTask.Dispose();
		onChangeTodoTitle?.Dispose();
		onSelectExpireCalendar?.Dispose();
		onSelectCompleteCalender?.Dispose();
		_workingPullDownAnimationState?.Dispose();
		_completePullDownAnimationState?.Dispose();
		_dragHandleEnableChecker?.Dispose();
		base.OnDestroy();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		CancelAnimationsAndRequests();
	}

	protected override void Start()
	{
		Data = new OSAListDataHelper<TodoTaskListItemBaseModel>(this);
		_itemRemoveAnimation = new ItemRemoveAnimationState<ulong>(this, this);
		_itemInsertAnimation = new ItemInsertAnimationState<ulong>(this, this);
		_workingPullDownAnimationState = new PullDownAnimationState(this);
		_completePullDownAnimationState = new PullDownAnimationState(this);
		_compAndUnCompAnimhelper = new CompletedAndUnCompletedAnimationHelper(this, this, _Params.DefaultItemSize);
		_itemRemoveAnimation.OnComplete.Subscribe(delegate(ulong uuid)
		{
			_removeRequestedUuid.Add(uuid);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_workingPullDownAnimationState.OnStartAnimation, delegate
		{
			SetActiveRaycastBlocker(active: true);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_workingPullDownAnimationState.OnFinishAnimation, delegate
		{
			SetActiveRaycastBlocker(active: false);
			_workingPullDownStateHelper.MargeTempExclusionsIntoKeeps();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_workingPullDownAnimationState.OnRequestedInsertionModelFromListBottom, delegate
		{
			TodoTaskListItemBaseModel todoTaskListItemBaseModel = _workingPullDownStateHelper.TakeModelOutAtLast();
			todoTaskListItemBaseModel.Height = 0f;
			todoTaskListItemBaseModel.HasPendingSizeChange = true;
			Data.InsertOne(WorkingTaskModelStartIdx, todoTaskListItemBaseModel);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_workingPullDownAnimationState.OnRequestedRemovalModelFromListTop, delegate
		{
			TodoTaskListItemBaseModel todoTaskListItemBaseModel = Data[WorkingTaskModelStartIdx];
			todoTaskListItemBaseModel.Height = 0f;
			Data.RemoveOne(WorkingTaskModelStartIdx);
			_workingPullDownStateHelper.Add(todoTaskListItemBaseModel);
		}).AddTo(this);
		_workingPullDownAnimationState.OnSkipAnimation.Subscribe(delegate(bool isOpen)
		{
			int workingTaskModelStartIdx = WorkingTaskModelStartIdx;
			int modelStartIndex = GetModelStartIndex(typeof(TodoTaskSeparatorModel));
			if (isOpen)
			{
				foreach (TodoTaskListItemBaseModel keep in _workingPullDownStateHelper.Keeps)
				{
					keep.Height = _Params._todoTaskCellHeight;
					keep.HasPendingSizeChange = true;
				}
				_workingPullDownStateHelper.InsertModelsKeepsToArg(Data, workingTaskModelStartIdx);
				_workingPullDownStateHelper.Clear();
			}
			else
			{
				for (int i = workingTaskModelStartIdx; i < modelStartIndex; i++)
				{
					_workingPullDownStateHelper.Add(Data[i]);
				}
				Data.RemoveItems(workingTaskModelStartIdx, modelStartIndex - workingTaskModelStartIdx);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_completePullDownAnimationState.OnStartAnimation, delegate
		{
			SetActiveRaycastBlocker(active: true);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_completePullDownAnimationState.OnFinishAnimation, delegate
		{
			SetActiveRaycastBlocker(active: false);
			_completePullDownStateHelper.MargeTempExclusionsIntoKeeps();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_completePullDownAnimationState.OnRequestedInsertionModelFromListBottom, delegate
		{
			int index = GetModelStartIndex(typeof(TodoTaskListCompletePullDownModel)) + 1;
			TodoTaskListItemBaseModel todoTaskListItemBaseModel = _completePullDownStateHelper.TakeModelOutAtLast();
			todoTaskListItemBaseModel.Height = 0f;
			todoTaskListItemBaseModel.HasPendingSizeChange = true;
			Data.InsertOne(index, todoTaskListItemBaseModel);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_completePullDownAnimationState.OnRequestedRemovalModelFromListTop, delegate
		{
			int index = GetModelStartIndex(typeof(TodoTaskListCompletePullDownModel)) + 1;
			TodoTaskListItemBaseModel todoTaskListItemBaseModel = Data[index];
			todoTaskListItemBaseModel.Height = 0f;
			Data.RemoveOne(index);
			_completePullDownStateHelper.Add(todoTaskListItemBaseModel);
		}).AddTo(this);
		_completePullDownAnimationState.OnSkipAnimation.Subscribe(delegate(bool isOpen)
		{
			int num = GetModelStartIndex(typeof(TodoTaskListCompletePullDownModel)) + 1;
			int count = Data.Count;
			if (isOpen)
			{
				foreach (TodoTaskListItemBaseModel keep2 in _completePullDownStateHelper.Keeps)
				{
					keep2.Height = _Params._completeTaskCellHeight;
					keep2.HasPendingSizeChange = true;
				}
				_completePullDownStateHelper.InsertModelsKeepsToArg(Data, num);
				_completePullDownStateHelper.Clear();
			}
			else
			{
				for (int i = num; i < count; i++)
				{
					_completePullDownStateHelper.Add(Data[i]);
				}
				Data.RemoveItems(num, count - num);
			}
		}).AddTo(this);
		_dragManipulator.Init(this, Data, isLimitOrder: true);
		_dragManipulator.OnChangeOrder.Subscribe(delegate((int movedIdx, TodoTaskListItemBaseModel draggedModel) info)
		{
			if (info.draggedModel.CachedType == typeof(TodoTaskListItemModel))
			{
				TodoTaskListItemModel todoTaskListItemModel = info.draggedModel as TodoTaskListItemModel;
				TodoTaskListItemModel todoTaskListItemModel2 = ((info.movedIdx == 0) ? null : (Data[info.movedIdx - 1] as TodoTaskListItemModel));
				if (todoTaskListItemModel2 == null)
				{
					Debug.LogWarning($"TodoTaskListView:範囲外指定です movedIdx:{info.movedIdx}");
				}
				else
				{
					ulong item = todoTaskListItemModel?.UniqueID ?? 0;
					ulong item2 = todoTaskListItemModel2?.UniqueID ?? 0;
					onSwapTodoTask?.OnNext((item, item2));
				}
			}
			else if (info.draggedModel.CachedType == typeof(TodoTaskListCompleteItemModel))
			{
				TodoTaskListCompleteItemModel todoTaskListCompleteItemModel = info.draggedModel as TodoTaskListCompleteItemModel;
				TodoTaskListCompleteItemModel todoTaskListCompleteItemModel2 = ((info.movedIdx == 0) ? null : (Data[info.movedIdx - 1] as TodoTaskListCompleteItemModel));
				if (todoTaskListCompleteItemModel2 == null)
				{
					Debug.LogWarning($"TodoTaskListView:範囲外指定です movedIdx:{info.movedIdx}");
				}
				else
				{
					ulong item3 = todoTaskListCompleteItemModel?.UniqueID ?? 0;
					ulong item4 = todoTaskListCompleteItemModel2?.UniqueID ?? 0;
					onSwapTodoTask?.OnNext((item3, item4));
				}
			}
		}).AddTo(this);
		_dragManipulator.OnBeginDrag.Subscribe(delegate(TodoTaskListItemBaseModel data)
		{
			_Params.ScrollEnabled = false;
			if (data.CachedType == typeof(TodoTaskListItemModel))
			{
				int workingTaskModelStartIdx = WorkingTaskModelStartIdx;
				int bottom = GetModelStartIndex(typeof(TodoTaskSeparatorModel)) - 1;
				_dragManipulator.SetOrderLimit(workingTaskModelStartIdx, bottom);
			}
			else if (data.CachedType == typeof(TodoTaskListCompleteItemModel))
			{
				int top = GetModelStartIndex(typeof(TodoTaskListCompletePullDownModel)) + 1;
				int count = Data.Count;
				_dragManipulator.SetOrderLimit(top, count);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_dragManipulator.OnEndDrag, delegate
		{
			_Params.ScrollEnabled = true;
		}).AddTo(this);
		_Params._todoTaskPullDown.gameObject.SetActive(value: false);
		_Params._todoTaskCell.gameObject.SetActive(value: false);
		_Params._taskListSeparator.gameObject.SetActive(value: false);
		_Params._completeTaskPullDown.gameObject.SetActive(value: false);
		_Params._completeTaskCell.gameObject.SetActive(value: false);
		base.Start();
		Data.InsertOne(WorkingTaskPullDownModelIdx, new TodoTaskListPullDownModel
		{
			Height = _Params._todoTaskPullDownHeight,
			IsOpened = true
		});
		Data.InsertOne(WorkingTaskPullDownModelIdx + 1, new TodoTaskSeparatorModel
		{
			Height = _Params._taskListSeparatorHeight
		});
		Data.InsertOne(Data.Count, new TodoTaskListCompletePullDownModel
		{
			Height = _Params._completeTaskPullDownHeight,
			IsOpened = true
		});
		if (_requestData != null)
		{
			EnterSettings(_requestData, _requestPullDownOpen);
			_requestData = null;
			_requestPullDownOpen = false;
		}
	}

	protected override void Update()
	{
		base.Update();
		UpdateRemoveRequest();
		UpdatePendingExecutionRemove();
		UpdatePullDown();
		_dragHandleEnableChecker.Value = !IsItemAnimationChanging();
	}

	private void UpdateRemoveRequest()
	{
		foreach (ulong item in _removeRequestedUuid)
		{
			(TodoTaskListItemBaseModel, int) indexedTodoModelData = GetIndexedTodoModelData(item);
			if (indexedTodoModelData.Item1 != null)
			{
				Data.RemoveOne(indexedTodoModelData.Item2, freezeEndEdge: true);
			}
		}
		_removeRequestedUuid.Clear();
	}

	private void UpdatePendingExecutionRemove()
	{
		foreach (ulong item in _compAndUnCompAnimhelper.PendingExecutionRemove)
		{
			(TodoTaskListItemBaseModel, int) indexedTodoModelData = GetIndexedTodoModelData(item);
			if (indexedTodoModelData.Item1 != null)
			{
				Data.RemoveOne(indexedTodoModelData.Item2, freezeEndEdge: true);
			}
		}
		_compAndUnCompAnimhelper.PendingExecutionRemove.Clear();
		foreach (TodoData item2 in _compAndUnCompAnimhelper.PendingExecutionAddCompleted)
		{
			AddCompleteTodoTask(item2);
		}
		_compAndUnCompAnimhelper.PendingExecutionAddCompleted.Clear();
		foreach (TodoData item3 in _compAndUnCompAnimhelper.PendingExecutionAddUnCompleted)
		{
			TodoTaskListItemModel todoTaskListItemModel = AddTodoTask(item3, createNew: false);
			todoTaskListItemModel.Height = 0f;
			todoTaskListItemModel.HasPendingSizeChange = true;
			_itemInsertAnimation.Play(todoTaskListItemModel.UniqueID, _Params.DefaultItemSize, 0.1f).Forget();
		}
		_compAndUnCompAnimhelper.PendingExecutionAddUnCompleted.Clear();
	}

	private bool IsItemAnimationChanging()
	{
		if (!_compAndUnCompAnimhelper.IsPlaying && !_itemInsertAnimation.IsPlayingAnimation && !_itemRemoveAnimation.IsPlayingAnimation)
		{
			return _removeRequestedUuid.Count != 0;
		}
		return true;
	}

	private void UpdatePullDown()
	{
		TodoTaskListPullDownModel pullDownModel = GetPullDownModel();
		TodoTaskListCompletePullDownModel completePullDownModel = GetCompletePullDownModel();
		completePullDownModel.IsTodoStateChanging = (pullDownModel.IsTodoStateChanging = IsItemAnimationChanging());
		UpdatePullDown(typeof(TodoTaskListPullDownModel));
		UpdatePullDown(typeof(TodoTaskListCompletePullDownModel));
	}

	public void CancelAnimationsAndRequests()
	{
		_itemRemoveAnimation.Cancel();
		_itemInsertAnimation.Cancel();
		_workingPullDownAnimationState.Cancel();
		_completePullDownAnimationState.Cancel();
		_compAndUnCompAnimhelper.Cancel();
		_compAndUnCompAnimhelper.Clear();
		_removeRequestedUuid.Clear();
	}

	public void EnterSettings(TodoListUIModel model, bool isPullDownOpen)
	{
		if (!base.IsInitialized)
		{
			_requestData = model;
			_requestPullDownOpen = isPullDownOpen;
			return;
		}
		SetActiveRaycastBlocker(active: false);
		CancelAnimationsAndRequests();
		_ = model.CurrentTodoList;
		List<TodoData> sortedWorkingTodoList = model.SortedWorkingTodoList;
		List<TodoData> sortedCompleteTodoList = model.SortedCompleteTodoList;
		double virtualAbstractNormalizedScrollPosition = GetVirtualAbstractNormalizedScrollPosition();
		TodoTaskListPullDownModel pullDownModel = GetPullDownModel();
		pullDownModel.IsOpened = true;
		pullDownModel.IsRemoving = false;
		pullDownModel.ExistTodo = sortedWorkingTodoList.Count() > 0;
		UpdatePullDown(typeof(TodoTaskListPullDownModel));
		TodoTaskListCompletePullDownModel completePullDownModel = GetCompletePullDownModel();
		completePullDownModel.IsOpened = true;
		completePullDownModel.IsRemoving = false;
		completePullDownModel.ExistTodo = sortedCompleteTodoList.Count() > 0;
		UpdatePullDown(typeof(TodoTaskListCompletePullDownModel));
		RemoveAllWorkingTodoTasks();
		if (pullDownModel.IsOpened)
		{
			AddWorkingTodoTasks(sortedWorkingTodoList);
		}
		else
		{
			foreach (TodoData item in sortedWorkingTodoList)
			{
				TodoTaskListItemModel model2 = new TodoTaskListItemModel
				{
					UniqueID = item.UniqueID,
					Title = item.TodoText,
					Height = _Params._todoTaskCellHeight,
					Expire = item.Expire,
					IsHabit = false,
					IsCreateNew = false
				};
				_workingPullDownStateHelper.Add(model2);
			}
		}
		RemoveAllCompleteTodoTasks();
		if (completePullDownModel.IsOpened)
		{
			AddCompleteTodoTasks(sortedCompleteTodoList);
		}
		else
		{
			foreach (TodoData item2 in sortedCompleteTodoList)
			{
				TodoTaskListCompleteItemModel model3 = new TodoTaskListCompleteItemModel
				{
					UniqueID = item2.UniqueID,
					Title = item2.TodoText,
					Height = _Params._completeTaskCellHeight,
					Expire = item2.Expire,
					CompletedDay = item2.Completed,
					IsHabit = false
				};
				_completePullDownStateHelper.Add(model3);
			}
		}
		SetVirtualAbstractNormalizedScrollPosition(virtualAbstractNormalizedScrollPosition, computeVisibilityNow: true, out var _);
	}

	public TodoTaskListItemModel AddTodoTask(TodoData data, bool createNew = true)
	{
		int modelStartIndex = GetModelStartIndex(typeof(TodoTaskSeparatorModel));
		TodoTaskListItemModel todoTaskListItemModel = new TodoTaskListItemModel
		{
			Height = _Params._todoTaskCellHeight,
			UniqueID = data.UniqueID,
			Title = data.TodoText,
			Expire = data.Expire,
			IsHabit = false,
			IsCreateNew = createNew
		};
		TodoTaskListPullDownModel pullDownModel = GetPullDownModel();
		if (!pullDownModel.IsOpened)
		{
			_workingPullDownStateHelper.Add(todoTaskListItemModel);
			if (todoTaskListItemModel.IsCreateNew)
			{
				pullDownModel.IsOpened = true;
				UpdatePullDown(typeof(TodoTaskListPullDownModel));
				foreach (TodoTaskListItemBaseModel keep in _workingPullDownStateHelper.Keeps)
				{
					keep.Height = _Params._todoTaskCellHeight;
					keep.HasPendingSizeChange = true;
				}
				_workingPullDownStateHelper.InsertModelsKeepsToArg(Data, WorkingTaskModelStartIdx);
				SmoothScrollTo(modelStartIndex + _workingPullDownStateHelper.Count, 0.3f, 0.2f, 0.5f);
				_workingPullDownStateHelper.Clear();
			}
		}
		else if (createNew)
		{
			Data.InsertOne(modelStartIndex, todoTaskListItemModel);
			ScrollTo(modelStartIndex, 0.5f, 0.5f);
		}
		else
		{
			Data.InsertOne(modelStartIndex, todoTaskListItemModel);
		}
		return todoTaskListItemModel;
	}

	public void AddCompleteTodoTask(TodoData data)
	{
		int count = Data.Count;
		TodoTaskListCompleteItemModel model = new TodoTaskListCompleteItemModel
		{
			Height = _Params._completeTaskCellHeight,
			UniqueID = data.UniqueID,
			Title = data.TodoText,
			Expire = data.Expire,
			CompletedDay = data.Completed,
			IsHabit = false
		};
		if (!GetCompletePullDownModel().IsOpened)
		{
			_completePullDownStateHelper.Add(model);
		}
		else
		{
			Data.InsertOne(count, model);
		}
	}

	public void RemoveTodoTask(ulong uuid)
	{
		(TodoTaskListItemBaseModel, int) indexedTodoModelData = GetIndexedTodoModelData(uuid);
		if (indexedTodoModelData.Item1 == null)
		{
			return;
		}
		if (indexedTodoModelData.Item1.CachedType == typeof(TodoTaskListItemModel))
		{
			TodoTaskListItemViewHolder todoTaskListItemViewHolder = null;
			foreach (TodoTaskListBaseViewHolder visibleItem in _VisibleItems)
			{
				if (visibleItem != null)
				{
					TodoTaskListItemBaseModel todoTaskListItemBaseModel = Data[visibleItem.ItemIndex];
					if (todoTaskListItemBaseModel != null && !(todoTaskListItemBaseModel.CachedType != typeof(TodoTaskListItemModel)) && (todoTaskListItemBaseModel as TodoTaskListItemModel).UniqueID == uuid)
					{
						todoTaskListItemViewHolder = visibleItem as TodoTaskListItemViewHolder;
						break;
					}
				}
			}
			if (todoTaskListItemViewHolder == null)
			{
				_removeRequestedUuid.Add(uuid);
			}
			else
			{
				_itemRemoveAnimation.Play(todoTaskListItemViewHolder.View.View, uuid, indexedTodoModelData.Item1.Height, 0.1f).Forget();
			}
		}
		else
		{
			if (!(indexedTodoModelData.Item1.CachedType == typeof(TodoTaskListCompleteItemModel)))
			{
				return;
			}
			TodoTaskListCompleteItemViewHolder todoTaskListCompleteItemViewHolder = null;
			foreach (TodoTaskListBaseViewHolder visibleItem2 in _VisibleItems)
			{
				if (visibleItem2 != null)
				{
					TodoTaskListItemBaseModel todoTaskListItemBaseModel2 = Data[visibleItem2.ItemIndex];
					if (todoTaskListItemBaseModel2 != null && !(todoTaskListItemBaseModel2.CachedType != typeof(TodoTaskListCompleteItemModel)) && (todoTaskListItemBaseModel2 as TodoTaskListCompleteItemModel).UniqueID == uuid)
					{
						todoTaskListCompleteItemViewHolder = visibleItem2 as TodoTaskListCompleteItemViewHolder;
						break;
					}
				}
			}
			if (todoTaskListCompleteItemViewHolder == null)
			{
				_removeRequestedUuid.Add(uuid);
			}
			else
			{
				_itemRemoveAnimation.Play(todoTaskListCompleteItemViewHolder.View.View, uuid, indexedTodoModelData.Item1.Height, 0.1f).Forget();
			}
		}
	}

	public void ChangeTodoTitle(ulong uuid, string title)
	{
		(TodoTaskListItemBaseModel, int) indexedTodoModelData = GetIndexedTodoModelData(uuid);
		if (indexedTodoModelData.Item1 == null)
		{
			return;
		}
		if (indexedTodoModelData.Item1.CachedType == typeof(TodoTaskListItemModel))
		{
			(indexedTodoModelData.Item1 as TodoTaskListItemModel).Title = title;
		}
		else if (indexedTodoModelData.Item1.CachedType == typeof(TodoTaskListCompleteItemModel))
		{
			(indexedTodoModelData.Item1 as TodoTaskListCompleteItemModel).Title = title;
		}
		foreach (TodoTaskListBaseViewHolder visibleItem in _VisibleItems)
		{
			visibleItem?.UpdateView(Data[visibleItem.ItemIndex]);
		}
	}

	public void SetTodoExpireDate(ulong uuid, DateTime? date)
	{
		(TodoTaskListItemBaseModel, int) indexedTodoModelData = GetIndexedTodoModelData(uuid);
		if (indexedTodoModelData.Item2 < 0)
		{
			return;
		}
		if (indexedTodoModelData.Item1.CachedType == typeof(TodoTaskListItemModel))
		{
			(indexedTodoModelData.Item1 as TodoTaskListItemModel).Expire = date;
		}
		else if (indexedTodoModelData.Item1.CachedType == typeof(TodoTaskListCompleteItemModel))
		{
			(indexedTodoModelData.Item1 as TodoTaskListCompleteItemModel).Expire = date;
		}
		foreach (TodoTaskListBaseViewHolder visibleItem in _VisibleItems)
		{
			visibleItem?.UpdateView(Data[visibleItem.ItemIndex]);
		}
	}

	public void SetTodoCompleteDate(ulong uuid, DateTime? date)
	{
		(TodoTaskListItemBaseModel, int) indexedTodoModelData = GetIndexedTodoModelData(uuid);
		if (indexedTodoModelData.Item2 < 0 || !(indexedTodoModelData.Item1.CachedType == typeof(TodoTaskListCompleteItemModel)))
		{
			return;
		}
		(indexedTodoModelData.Item1 as TodoTaskListCompleteItemModel).CompletedDay = date;
		foreach (TodoTaskListBaseViewHolder visibleItem in _VisibleItems)
		{
			visibleItem?.UpdateView(Data[visibleItem.ItemIndex]);
		}
	}

	public void SetTodoComplete(TodoData todoData)
	{
		(TodoTaskListItemBaseModel, int) indexedTodoModelData = GetIndexedTodoModelData(todoData.UniqueID);
		if (indexedTodoModelData.Item2 < 0 || !(indexedTodoModelData.Item1.CachedType == typeof(TodoTaskListItemModel)))
		{
			return;
		}
		TodoTaskListItemModel todoTaskListItemModel = indexedTodoModelData.Item1 as TodoTaskListItemModel;
		TodoTaskListItemViewHolder todoTaskListItemViewHolder = null;
		foreach (TodoTaskListBaseViewHolder visibleItem in _VisibleItems)
		{
			if (visibleItem != null)
			{
				TodoTaskListItemBaseModel todoTaskListItemBaseModel = Data[visibleItem.ItemIndex];
				if (todoTaskListItemBaseModel != null && !(todoTaskListItemBaseModel.CachedType != typeof(TodoTaskListItemModel)) && (todoTaskListItemBaseModel as TodoTaskListItemModel).UniqueID == todoData.UniqueID)
				{
					todoTaskListItemViewHolder = visibleItem as TodoTaskListItemViewHolder;
					break;
				}
			}
		}
		if (todoTaskListItemViewHolder == null)
		{
			_compAndUnCompAnimhelper.PendingExecutionRemove.Add(todoTaskListItemModel.UniqueID);
			_compAndUnCompAnimhelper.PendingExecutionAddCompleted.Add(todoData);
		}
		else
		{
			_compAndUnCompAnimhelper.PlayMovingCompletedAsync(todoTaskListItemModel.UniqueID, todoTaskListItemViewHolder, todoData).Forget();
		}
	}

	private async UniTask CompleteAnimation(TodoTaskListItemViewHolder vh, ulong uuid, TodoData addData)
	{
	}

	public void SetTodoUncomplete(TodoData todoData)
	{
		(TodoTaskListItemBaseModel, int) indexedTodoModelData = GetIndexedTodoModelData(todoData.UniqueID);
		if (indexedTodoModelData.Item2 < 0 || !(indexedTodoModelData.Item1.CachedType == typeof(TodoTaskListCompleteItemModel)))
		{
			return;
		}
		TodoTaskListCompleteItemModel todoTaskListCompleteItemModel = indexedTodoModelData.Item1 as TodoTaskListCompleteItemModel;
		TodoTaskListCompleteItemViewHolder todoTaskListCompleteItemViewHolder = null;
		foreach (TodoTaskListBaseViewHolder visibleItem in _VisibleItems)
		{
			if (visibleItem != null)
			{
				TodoTaskListItemBaseModel todoTaskListItemBaseModel = Data[visibleItem.ItemIndex];
				if (todoTaskListItemBaseModel != null && !(todoTaskListItemBaseModel.CachedType != typeof(TodoTaskListCompleteItemModel)) && (todoTaskListItemBaseModel as TodoTaskListCompleteItemModel).UniqueID == todoData.UniqueID)
				{
					todoTaskListCompleteItemViewHolder = visibleItem as TodoTaskListCompleteItemViewHolder;
					break;
				}
			}
		}
		if (todoTaskListCompleteItemViewHolder == null)
		{
			_compAndUnCompAnimhelper.PendingExecutionRemove.Add(todoTaskListCompleteItemModel.UniqueID);
			_compAndUnCompAnimhelper.PendingExecutionAddUnCompleted.Add(todoData);
		}
		else
		{
			_compAndUnCompAnimhelper.PlayMovingUnCompletedAsync(todoTaskListCompleteItemModel.UniqueID, todoTaskListCompleteItemViewHolder, todoData).Forget();
		}
	}

	public void SetChangeRemoveMode(bool isRemoveMode)
	{
		foreach (TodoTaskListBaseViewHolder visibleItem in _VisibleItems)
		{
			if (visibleItem != null)
			{
				TodoTaskListItemBaseModel todoTaskListItemBaseModel = Data[visibleItem.ItemIndex];
				if (todoTaskListItemBaseModel.CachedType == typeof(TodoTaskListItemModel))
				{
					(visibleItem as TodoTaskListItemViewHolder).View.ChangeRemovingMode(isRemoveMode);
				}
				else if (todoTaskListItemBaseModel.CachedType == typeof(TodoTaskListCompleteItemModel))
				{
					(visibleItem as TodoTaskListCompleteItemViewHolder).View.ChangeRemovingMode(isRemoveMode);
				}
			}
		}
	}

	private void UpdatePullDown(Type checkType)
	{
		if (!(checkType != typeof(TodoTaskListPullDownModel)) || !(checkType != typeof(TodoTaskListCompletePullDownModel)))
		{
			int modelStartIndex = GetModelStartIndex(checkType);
			GetItemViewsHolderIfVisible(modelStartIndex)?.UpdateView(Data[modelStartIndex]);
		}
	}

	private bool PlayWorkingPullDownAnimation(bool isOpen)
	{
		int workingTaskModelStartIdx = WorkingTaskModelStartIdx;
		int modelStartIndex = GetModelStartIndex(typeof(TodoTaskSeparatorModel));
		int num = modelStartIndex - workingTaskModelStartIdx;
		if (num <= 0 && !_workingPullDownStateHelper.IsKeep)
		{
			return false;
		}
		if (isOpen)
		{
			num = _workingPullDownStateHelper.Count;
		}
		else
		{
			(int, int) tuple = _workingPullDownStateHelper.AddTempExclusions(_VisibleItems, Data, modelStartIndex - 1);
			if (tuple.Item2 > 0)
			{
				Data.RemoveItems(tuple.Item1, tuple.Item2);
			}
			modelStartIndex = GetModelStartIndex(typeof(TodoTaskSeparatorModel));
			num = modelStartIndex - workingTaskModelStartIdx;
			if (num <= 0)
			{
				_workingPullDownStateHelper.MargeTempExclusionsIntoKeeps();
				return false;
			}
		}
		_workingPullDownAnimationState.PlayAnimation(isOpen, workingTaskModelStartIdx, num, 15, _Params._todoTaskCellHeight, 0.2f);
		return true;
	}

	private bool PlayCompletePullDownAnimation(bool isOpen)
	{
		int num = GetModelStartIndex(typeof(TodoTaskListCompletePullDownModel)) + 1;
		int count = Data.Count;
		int num2 = count - num;
		if (num2 <= 0 && !_completePullDownStateHelper.IsKeep)
		{
			return false;
		}
		if (isOpen)
		{
			num2 = _completePullDownStateHelper.Count;
		}
		else
		{
			(int, int) tuple = _completePullDownStateHelper.AddTempExclusions(_VisibleItems, Data, count - 1);
			if (tuple.Item2 > 0)
			{
				Data.RemoveItems(tuple.Item1, tuple.Item2);
			}
			count = Data.Count;
			num2 = count - num;
			if (num2 <= 0)
			{
				_completePullDownStateHelper.MargeTempExclusionsIntoKeeps();
				return false;
			}
		}
		_completePullDownAnimationState.PlayAnimation(isOpen, num, num2, 15, _Params._completeTaskCellHeight, 0.2f);
		return true;
	}

	private TodoTaskListPullDownModel GetPullDownModel()
	{
		return Data[WorkingTaskPullDownModelIdx] as TodoTaskListPullDownModel;
	}

	private TodoTaskListCompletePullDownModel GetCompletePullDownModel()
	{
		return Data.FirstOrDefault((TodoTaskListItemBaseModel x) => x != null && x.CachedType == typeof(TodoTaskListCompletePullDownModel)) as TodoTaskListCompletePullDownModel;
	}

	private (TodoTaskListItemBaseModel model, int index) GetIndexedTodoModelData(ulong uuid)
	{
		return Data.Indexed().FirstOrDefault(((TodoTaskListItemBaseModel item, int index) x) => (x.item is TodoTaskListItemModel && (x.item as TodoTaskListItemModel).UniqueID == uuid) || (x.item is TodoTaskListCompleteItemModel && (x.item as TodoTaskListCompleteItemModel).UniqueID == uuid), (null, -1));
	}

	private int GetModelStartIndex(Type targetType)
	{
		return Data.Indexed().FirstOrDefault(((TodoTaskListItemBaseModel item, int index) x) => x.item != null && x.item.CachedType == targetType, (null, -1)).index;
	}

	private void AddWorkingTodoTasks(IEnumerable<TodoData> datas)
	{
		List<TodoTaskListItemBaseModel> list = new List<TodoTaskListItemBaseModel>();
		foreach (TodoData data in datas)
		{
			if (data != null)
			{
				TodoTaskListItemModel item = new TodoTaskListItemModel
				{
					Height = _Params._todoTaskCellHeight,
					UniqueID = data.UniqueID,
					Title = data.TodoText,
					Expire = data.Expire,
					IsHabit = false,
					IsCreateNew = false
				};
				list.Add(item);
			}
		}
		if (list.Count != 0)
		{
			Data.InsertItems(WorkingTaskModelStartIdx, list);
		}
	}

	private void AddCompleteTodoTasks(IEnumerable<TodoData> datas)
	{
		int modelStartIndex = GetModelStartIndex(typeof(TodoTaskListCompletePullDownModel));
		List<TodoTaskListItemBaseModel> list = new List<TodoTaskListItemBaseModel>();
		foreach (TodoData data in datas)
		{
			if (data != null)
			{
				TodoTaskListCompleteItemModel item = new TodoTaskListCompleteItemModel
				{
					Height = _Params._completeTaskCellHeight,
					UniqueID = data.UniqueID,
					Title = data.TodoText,
					Expire = data.Expire,
					CompletedDay = data.Completed,
					IsHabit = false
				};
				list.Add(item);
			}
		}
		if (list.Count != 0)
		{
			Data.InsertItems(modelStartIndex + 1, list);
		}
	}

	private void RemoveAllWorkingTodoTasks()
	{
		_workingPullDownStateHelper.Clear();
		int modelStartIndex = GetModelStartIndex(typeof(TodoTaskSeparatorModel));
		int workingTaskModelStartIdx = WorkingTaskModelStartIdx;
		int num = modelStartIndex;
		if (workingTaskModelStartIdx < num)
		{
			int count = num - workingTaskModelStartIdx;
			Data.RemoveItems(workingTaskModelStartIdx, count);
		}
	}

	private void RemoveAllCompleteTodoTasks()
	{
		_completePullDownStateHelper.Clear();
		int num = GetModelStartIndex(typeof(TodoTaskListCompletePullDownModel)) + 1;
		int count = Data.Count;
		if (num < count)
		{
			int count2 = count - num;
			Data.RemoveItems(num, count2);
		}
	}

	protected override TodoTaskListBaseViewHolder CreateViewsHolder(int itemIndex)
	{
		if (Data[itemIndex] == null)
		{
			TodoTaskListReorderTempViewHolder todoTaskListReorderTempViewHolder = new TodoTaskListReorderTempViewHolder();
			todoTaskListReorderTempViewHolder.Init(_Params._todoTaskCell, _Params.Content, itemIndex);
			return todoTaskListReorderTempViewHolder;
		}
		Type cachedType = Data[itemIndex].CachedType;
		TodoTaskListBaseViewHolder todoTaskListBaseViewHolder = null;
		if (cachedType == typeof(TodoTaskListItemModel))
		{
			_ = Data[itemIndex];
			TodoTaskListItemViewHolder todoTaskListItemViewHolder = new TodoTaskListItemViewHolder();
			todoTaskListItemViewHolder.Init(_Params._todoTaskCell, _Params.Content, itemIndex);
			todoTaskListItemViewHolder.View.View.DragReorderHandle.Init(_dragManipulator, todoTaskListItemViewHolder);
			todoTaskListItemViewHolder.View.View.SetupDragHandleEnableControl(_dragHandleEnableChecker);
			todoTaskListItemViewHolder.View.View.OnRemoved.Subscribe(delegate(ulong uuid)
			{
				RemoveTaskNotify(uuid);
			}).AddTo(this);
			todoTaskListItemViewHolder.View.View.OnSwitchCompleted.Subscribe(delegate(ulong uuid)
			{
				CompleteTaskNotify(uuid);
			}).AddTo(this);
			todoTaskListItemViewHolder.View.View.OnSettingDateTime.Subscribe(delegate((ulong uniqueId, DateTime? datetime) uuid)
			{
				OpenExpireDateCalendarNotify(uuid);
			}).AddTo(this);
			todoTaskListItemViewHolder.View.View.OnChangedEndTodoText.Subscribe(delegate((ulong, string) info)
			{
				ChangeTodoTitleNotify(info.Item1, info.Item2);
			}).AddTo(this);
			todoTaskListBaseViewHolder = todoTaskListItemViewHolder;
		}
		else if (cachedType == typeof(TodoTaskListCompleteItemModel))
		{
			_ = Data[itemIndex];
			TodoTaskListCompleteItemViewHolder todoTaskListCompleteItemViewHolder = new TodoTaskListCompleteItemViewHolder();
			todoTaskListCompleteItemViewHolder.Init(_Params._completeTaskCell, _Params.Content, itemIndex);
			todoTaskListCompleteItemViewHolder.View.View.DragReorderHandle.Init(_dragManipulator, todoTaskListCompleteItemViewHolder);
			todoTaskListCompleteItemViewHolder.View.View.OnRemoved.Subscribe(delegate(ulong uuid)
			{
				RemoveTaskNotify(uuid);
			}).AddTo(this);
			todoTaskListCompleteItemViewHolder.View.View.OnSwitchCompleted.Subscribe(delegate(ulong uuid)
			{
				UncompleteTaskNotify(uuid);
			}).AddTo(this);
			todoTaskListCompleteItemViewHolder.View.View.OnSettingDateTime.Subscribe(delegate((ulong uniqueId, DateTime? datetime) uuid)
			{
				OpenCompleteDateCalendarNotify(uuid);
			}).AddTo(this);
			todoTaskListCompleteItemViewHolder.View.View.OnChangedEndTodoText.Subscribe(delegate((ulong, string) info)
			{
				ChangeTodoTitleNotify(info.Item1, info.Item2);
			}).AddTo(this);
			todoTaskListBaseViewHolder = todoTaskListCompleteItemViewHolder;
		}
		else if (cachedType == typeof(TodoTaskListPullDownModel))
		{
			TodoTaskListPullDownModel model = Data[itemIndex] as TodoTaskListPullDownModel;
			TodoTaskListPullDownViewHolder todoTaskListPullDownViewHolder = new TodoTaskListPullDownViewHolder();
			todoTaskListPullDownViewHolder.Init(_Params._todoTaskPullDown, _Params.Content, itemIndex);
			todoTaskListPullDownViewHolder.View.PullDownButton.OnValueChanged.Subscribe(delegate(bool flag)
			{
				if (IsItemAnimationChanging())
				{
					UpdatePullDown();
				}
				else
				{
					model.IsOpened = flag;
					UpdatePullDown(typeof(TodoTaskListPullDownModel));
					PlayWorkingPullDownAnimation(model.IsOpened);
				}
			}).AddTo(this);
			todoTaskListBaseViewHolder = todoTaskListPullDownViewHolder;
		}
		else if (cachedType == typeof(TodoTaskListCompletePullDownModel))
		{
			TodoTaskListCompletePullDownModel model2 = Data[itemIndex] as TodoTaskListCompletePullDownModel;
			TodoTaskListCompletePullDownViewHolder todoTaskListCompletePullDownViewHolder = new TodoTaskListCompletePullDownViewHolder();
			todoTaskListCompletePullDownViewHolder.Init(_Params._completeTaskPullDown, _Params.Content, itemIndex);
			todoTaskListCompletePullDownViewHolder.View.PullDownButton.OnValueChanged.Subscribe(delegate(bool flag)
			{
				if (IsItemAnimationChanging())
				{
					UpdatePullDown();
				}
				else
				{
					model2.IsOpened = flag;
					UpdatePullDown(typeof(TodoTaskListCompletePullDownModel));
					PlayCompletePullDownAnimation(model2.IsOpened);
				}
			}).AddTo(this);
			todoTaskListBaseViewHolder = todoTaskListCompletePullDownViewHolder;
		}
		else if (cachedType == typeof(TodoTaskSeparatorModel))
		{
			todoTaskListBaseViewHolder = new TodoTaskListSeparatorViewHolder();
			todoTaskListBaseViewHolder.Init(_Params._taskListSeparator, _Params.Content, itemIndex);
		}
		return todoTaskListBaseViewHolder;
	}

	protected override void UpdateViewsHolder(TodoTaskListBaseViewHolder newOrRecycled)
	{
		TodoTaskListItemBaseModel todoTaskListItemBaseModel = Data[newOrRecycled.ItemIndex];
		bool flag = _dragManipulator.IsPlaceHolderModel(todoTaskListItemBaseModel);
		newOrRecycled.UpdateView(todoTaskListItemBaseModel, flag);
		if (!flag && todoTaskListItemBaseModel.HasPendingSizeChange)
		{
			ScheduleComputeVisibilityTwinPass();
		}
	}

	protected override void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
	{
		base.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, itemsDesc);
	}

	protected override float UpdateItemSizeOnTwinPass(TodoTaskListBaseViewHolder viewsHolder)
	{
		TodoTaskListItemBaseModel todoTaskListItemBaseModel = Data[viewsHolder.ItemIndex];
		if (todoTaskListItemBaseModel == null)
		{
			return _Params.DefaultItemSize;
		}
		viewsHolder.UpdateItemHeight(todoTaskListItemBaseModel.Height);
		return base.UpdateItemSizeOnTwinPass(viewsHolder);
	}

	protected override void OnItemHeightChangedPreTwinPass(TodoTaskListBaseViewHolder viewsHolder)
	{
		base.OnItemHeightChangedPreTwinPass(viewsHolder);
		if (viewsHolder != null && Data != null && Data[viewsHolder.ItemIndex] != null)
		{
			viewsHolder.UpdateItemHeight(Data[viewsHolder.ItemIndex].Height);
			Data[viewsHolder.ItemIndex].HasPendingSizeChange = false;
		}
	}

	protected override bool IsRecyclable(TodoTaskListBaseViewHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
	{
		return potentiallyRecyclable.CanPresentModelType(Data[indexOfItemThatWillBecomeVisible]?.CachedType);
	}

	protected override void OnItemIndexChangedDueInsertOrRemove(TodoTaskListBaseViewHolder shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
	{
		base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);
	}

	protected override void RebuildLayoutDueToScrollViewSizeChange()
	{
		foreach (TodoTaskListItemBaseModel item in (IEnumerable<TodoTaskListItemBaseModel>)Data)
		{
			item.HasPendingSizeChange = true;
		}
		base.RebuildLayoutDueToScrollViewSizeChange();
	}

	private void RemoveTaskNotify(ulong uuid)
	{
		onRemoveTask?.OnNext(uuid);
	}

	private void CompleteTaskNotify(ulong uuid)
	{
		onCompleteTask?.OnNext(uuid);
	}

	private void UncompleteTaskNotify(ulong uuid)
	{
		onUncompleteTask?.OnNext(uuid);
	}

	private void ChangeTodoTitleNotify(ulong uuid, string title)
	{
		onChangeTodoTitle?.OnNext((uuid, title));
	}

	private void OpenExpireDateCalendarNotify((ulong uuid, DateTime? datetime) settingData)
	{
		onSelectExpireCalendar?.OnNext(settingData);
	}

	private void OpenCompleteDateCalendarNotify((ulong uuid, DateTime? datetime) settingData)
	{
		onSelectCompleteCalender?.OnNext(settingData);
	}
}
