using System;
using System.Collections.Generic;
using System.Linq;
using Com.ForbiddenByte.OSA.Core;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class CalendarContentsListView : OSA<CalendarContentParams, CalendarContentsBaseViewHolder>, IInfomationProviderForOSAMyAnimation, IOSAModelIndexGetter<ulong>, IOSAModelIndexGetter<string>
{
	[SerializeField]
	[Header("画面全体をブロックできるオブジェクト")]
	private GameObject _raycastBlocker;

	private static readonly int _calendarAndWorkTimeModelIdx = 0;

	private static readonly int _completedTaskHeaderModelIdx = 1;

	private static readonly int _completedTaskModelStartIdx = _completedTaskHeaderModelIdx + 1;

	private Subject<DateTime> _onSelectedDay = new Subject<DateTime>();

	private Subject<(int, int)> _onValueChangedMonth = new Subject<(int, int)>();

	private Subject<TimeSpan> _onSettingWorkTime = new Subject<TimeSpan>();

	private Subject<string> _onEndEditDiary = new Subject<string>();

	private Subject<(ulong, string)> _onChangedTodoTitle = new Subject<(ulong, string)>();

	private Subject<ulong> _onRemovedTodo = new Subject<ulong>();

	private Subject<(ulong uuid, DateTime? datetime)> _onSettingCompletedDateTime = new Subject<(ulong, DateTime?)>();

	private Subject<(string, string)> _onChangedHabitTrackerTaskTitle = new Subject<(string, string)>();

	private Subject<string> _onRemovedHabitTrackerTask = new Subject<string>();

	private Subject<bool> _onChangedActiveCalendar = new Subject<bool>();

	private bool _isRequestedBeforeInit;

	private CalendarAndWorkViewModel _requestedCalendarAndWorkDataBeforeInit = new CalendarAndWorkViewModel();

	private string _requestedDiaryBeforeInit;

	private List<CalendarContentsListBaseModel> _requestedTodoDataBeforeInit;

	private bool _requestedPullDownForceOpenBeforeInit;

	private bool _IsPrevCalendarInsideViewport = true;

	private bool _isCompleteTodoPullDownOpened = true;

	private int _prevFrameDataLen;

	private PullDownAnimationState _pullDownAnimationState;

	private PullDownAnimationStateHelper<CalendarContentsListBaseModel, CalendarCompletedTaskViewHolder, CalendarContentsBaseViewHolder> _pullDownStateHelper = new PullDownAnimationStateHelper<CalendarContentsListBaseModel, CalendarCompletedTaskViewHolder, CalendarContentsBaseViewHolder>();

	private ItemRemoveAnimationState<ulong> _removeAnimationState;

	private ItemRemoveAnimationState<string> _removeAnimationHabitTrackerState;

	private List<ulong> _removedRequests = new List<ulong>();

	private List<string> _removedHabitTrackerRequests = new List<string>();

	private int _lastDataIdx => Data.Count - 1;

	private bool ExistCompletedTask => Data.Count > 3;

	public OSAListDataHelper<CalendarContentsListBaseModel> Data { get; private set; }

	public Observable<DateTime> OnSelectedDay => _onSelectedDay;

	public Observable<(int, int)> OnValueChangedMonth => _onValueChangedMonth;

	public Observable<TimeSpan> OnSettingWorkTime => _onSettingWorkTime;

	public Observable<string> OnEndEditDiary => _onEndEditDiary;

	public Observable<(ulong, string)> OnChangedTodoTitle => _onChangedTodoTitle;

	public Observable<ulong> OnRemovedTodo => _onRemovedTodo;

	public Observable<(ulong uuid, DateTime? datetime)> OnSettingCompletedDateTime => _onSettingCompletedDateTime;

	public Observable<(string, string)> OnChangedHabitTrackerTaskTitle => _onChangedHabitTrackerTaskTitle;

	public Observable<string> OnRemovedHabitTrackerTask => _onRemovedHabitTrackerTask;

	public Observable<bool> OnChangedActiveCalendar => _onChangedActiveCalendar;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_onValueChangedMonth.Dispose();
		_onSelectedDay.Dispose();
		_onSettingWorkTime.Dispose();
		_onEndEditDiary.Dispose();
		_onRemovedTodo.Dispose();
		_onSettingCompletedDateTime.Dispose();
		_pullDownAnimationState.Dispose();
	}

	protected override void Start()
	{
		Data = new OSAListDataHelper<CalendarContentsListBaseModel>(this);
		_pullDownAnimationState = new PullDownAnimationState(this);
		_removeAnimationState = new ItemRemoveAnimationState<ulong>(this, this);
		_removeAnimationHabitTrackerState = new ItemRemoveAnimationState<string>(this, this);
		ObservableSubscribeExtensions.Subscribe(_removeAnimationState.OnStart, delegate
		{
			GetCompletedTaskHeaderModel().IsRemoving = true;
			UpdateCompletedTaskHeaderView();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_removeAnimationHabitTrackerState.OnStart, delegate
		{
			GetCompletedTaskHeaderModel().IsRemoving = true;
			UpdateCompletedTaskHeaderView();
		}).AddTo(this);
		_removeAnimationState.OnComplete.Subscribe(delegate(ulong uniqueID)
		{
			if (!_removeAnimationState.IsPlayingAnimation && !_removeAnimationHabitTrackerState.IsPlayingAnimation)
			{
				GetCompletedTaskHeaderModel().IsRemoving = false;
				UpdateCompletedTaskHeaderView();
			}
			_removedRequests.Add(uniqueID);
		}).AddTo(this);
		_removeAnimationHabitTrackerState.OnComplete.Subscribe(delegate(string habitID)
		{
			if (!_removeAnimationState.IsPlayingAnimation && !_removeAnimationHabitTrackerState.IsPlayingAnimation)
			{
				GetCompletedTaskHeaderModel().IsRemoving = false;
				UpdateCompletedTaskHeaderView();
			}
			_removedHabitTrackerRequests.Add(habitID);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pullDownAnimationState.OnStartAnimation, delegate
		{
			SetActiveRaycastBlocker(active: true);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pullDownAnimationState.OnFinishAnimation, delegate
		{
			SetActiveRaycastBlocker(active: false);
			_pullDownStateHelper.MargeTempExclusionsIntoKeeps();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pullDownAnimationState.OnRequestedInsertionModelFromListBottom, delegate
		{
			CalendarContentsListBaseModel calendarContentsListBaseModel = _pullDownStateHelper.TakeModelOutAtLast();
			calendarContentsListBaseModel.Height = 0f;
			calendarContentsListBaseModel.HasPendingSizeChange = true;
			Data.InsertOne(_completedTaskModelStartIdx, calendarContentsListBaseModel);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_pullDownAnimationState.OnRequestedRemovalModelFromListTop, delegate
		{
			CalendarContentsListBaseModel calendarContentsListBaseModel = Data[_completedTaskModelStartIdx];
			calendarContentsListBaseModel.Height = 0f;
			Data.RemoveOne(_completedTaskModelStartIdx);
			_pullDownStateHelper.Add(calendarContentsListBaseModel);
		}).AddTo(this);
		_pullDownAnimationState.OnSkipAnimation.Subscribe(delegate(bool isOpen)
		{
			if (isOpen)
			{
				foreach (CalendarContentsListBaseModel keep in _pullDownStateHelper.Keeps)
				{
					keep.Height = _Params.CompletedTaskPrefabHeight;
					keep.HasPendingSizeChange = true;
				}
				_pullDownStateHelper.InsertModelsKeepsToArg(Data, _completedTaskModelStartIdx);
				_pullDownStateHelper.Clear();
			}
			else
			{
				for (int i = _completedTaskModelStartIdx; i < _lastDataIdx; i++)
				{
					_pullDownStateHelper.Add(Data[i]);
				}
				Data.RemoveItems(_completedTaskModelStartIdx, _lastDataIdx - _completedTaskModelStartIdx);
			}
		}).AddTo(this);
		base.Start();
		Data.InsertOne(_calendarAndWorkTimeModelIdx, new CalendarAndWorkViewModel
		{
			Height = _Params.CalendarAndWorkPrefabHeight
		});
		Data.InsertOne(_completedTaskHeaderModelIdx, new CalendarCompletedTaskHeaderViewModel
		{
			Height = _Params.CompletedTaskHeaderPrefabHeight
		});
		Data.InsertOne(_lastDataIdx + 1, new CalendarDiaryViewModel
		{
			Height = _Params.DiaryPrefabHeight
		});
		_prevFrameDataLen = Data.Count;
		if (_isRequestedBeforeInit)
		{
			EnterSettingCalendarContentsView(_requestedCalendarAndWorkDataBeforeInit.CalendarMonthlyData, _requestedCalendarAndWorkDataBeforeInit.SelectedYear, _requestedCalendarAndWorkDataBeforeInit.SelectedMonth, _requestedCalendarAndWorkDataBeforeInit.SelectedDay, _requestedCalendarAndWorkDataBeforeInit.WorkTime, _requestedDiaryBeforeInit, _requestedTodoDataBeforeInit, _requestedPullDownForceOpenBeforeInit);
			_isRequestedBeforeInit = false;
		}
	}

	public List<CalendarContentsListBaseModel> CreateModelList(List<TodoData> todos, IEnumerable<string> habitTrackers, DateTime habitTrackerCompletedDateTime, Func<string, string> habitTrackerTitleGetter)
	{
		List<CalendarContentsListBaseModel> list = new List<CalendarContentsListBaseModel>();
		foreach (TodoData todo in todos)
		{
			CalendarCompletedTaskViewModel calendarCompletedTaskViewModel = new CalendarCompletedTaskViewModel();
			calendarCompletedTaskViewModel.UniqueID = todo.UniqueID;
			calendarCompletedTaskViewModel.Title = todo.TodoText;
			calendarCompletedTaskViewModel.CompletedDateTime = todo.Completed.Value;
			calendarCompletedTaskViewModel.IsHabitTrackerTask = false;
			calendarCompletedTaskViewModel.Height = _Params.CompletedTaskPrefabHeight;
			list.Add(calendarCompletedTaskViewModel);
		}
		foreach (string habitTracker in habitTrackers)
		{
			CalendarCompletedTaskViewModel calendarCompletedTaskViewModel2 = new CalendarCompletedTaskViewModel();
			calendarCompletedTaskViewModel2.HabitTrackerID = habitTracker;
			calendarCompletedTaskViewModel2.Title = habitTrackerTitleGetter?.Invoke(habitTracker);
			calendarCompletedTaskViewModel2.CompletedDateTime = habitTrackerCompletedDateTime;
			calendarCompletedTaskViewModel2.IsHabitTrackerTask = true;
			calendarCompletedTaskViewModel2.Height = _Params.CompletedTaskPrefabHeight;
			list.Add(calendarCompletedTaskViewModel2);
		}
		return list;
	}

	protected override void RebuildLayoutDueToScrollViewSizeChange()
	{
		foreach (CalendarContentsListBaseModel item in (IEnumerable<CalendarContentsListBaseModel>)Data)
		{
			item.HasPendingSizeChange = true;
		}
		base.RebuildLayoutDueToScrollViewSizeChange();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_pullDownAnimationState.Cancel();
	}

	protected override void Update()
	{
		base.Update();
		if (!base.IsInitialized)
		{
			return;
		}
		if (_prevFrameDataLen != Data.Count)
		{
			_prevFrameDataLen = Data.Count;
			CalendarCompletedTaskHeaderViewModel completedTaskHeaderModel = GetCompletedTaskHeaderModel();
			bool flag = _prevFrameDataLen > 3;
			bool num = flag != completedTaskHeaderModel.ExistTodo || !_isCompleteTodoPullDownOpened;
			completedTaskHeaderModel.ExistTodo = flag;
			if (num)
			{
				UpdateCompletedTaskHeaderView();
			}
		}
		UpdateDiaryHeight();
		UpdateCalendarInsideViewport();
		UpdateRequest();
	}

	private void UpdateRequest()
	{
		foreach (ulong request in _removedRequests)
		{
			(CalendarContentsListBaseModel, int) tuple = Data.Indexed().FirstOrDefault(((CalendarContentsListBaseModel item, int index) x) => (x.item is CalendarCompletedTaskViewModel calendarCompletedTaskViewModel && calendarCompletedTaskViewModel.UniqueID == request) ? true : false, (null, -1));
			if (tuple.Item2 != -1)
			{
				Data.RemoveOne(tuple.Item2);
			}
		}
		_removedRequests.Clear();
		foreach (string request2 in _removedHabitTrackerRequests)
		{
			(CalendarContentsListBaseModel, int) tuple2 = Data.Indexed().FirstOrDefault(((CalendarContentsListBaseModel item, int index) x) => (x.item is CalendarCompletedTaskViewModel { IsHabitTrackerTask: not false } calendarCompletedTaskViewModel && calendarCompletedTaskViewModel.HabitTrackerID.Contains(request2)) ? true : false, (null, -1));
			if (tuple2.Item2 != -1)
			{
				Data.RemoveOne(tuple2.Item2);
			}
		}
		_removedHabitTrackerRequests.Clear();
	}

	private void UpdateCalendarInsideViewport()
	{
		if (!(base.Velocity.y > 0.01f) && !(base.Velocity.y < -0.01f))
		{
			bool flag = false;
			CalendarContentsBaseViewHolder itemViewsHolderIfVisible = GetItemViewsHolderIfVisible(_calendarAndWorkTimeModelIdx);
			if (itemViewsHolderIfVisible != null)
			{
				flag = (itemViewsHolderIfVisible as CalendarAndWorkViewHolder)?.View.CheckCalendarInsideViewport(base.Viewport) ?? false;
			}
			if (_IsPrevCalendarInsideViewport != flag)
			{
				_onChangedActiveCalendar.OnNext(flag);
				_IsPrevCalendarInsideViewport = flag;
			}
		}
	}

	private void UpdateDiaryHeight()
	{
		if (Data[_lastDataIdx] is CalendarDiaryViewModel { HasPendingSizeChange: not false } calendarDiaryViewModel)
		{
			RequestChangeItemSizeAndUpdateLayout(_lastDataIdx, calendarDiaryViewModel.Height, calendarDiaryViewModel.IsChangedHeightStationary);
			calendarDiaryViewModel.HasPendingSizeChange = false;
		}
	}

	public void ResetScroll()
	{
		if (base.IsInitialized)
		{
			SetVirtualAbstractNormalizedScrollPosition(1.0, computeVisibilityNow: true, out var _);
		}
	}

	protected override void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
	{
		base.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, itemsDesc);
	}

	protected override CalendarContentsBaseViewHolder CreateViewsHolder(int itemIndex)
	{
		Type cachedType = Data[itemIndex].CachedType;
		if (cachedType == typeof(CalendarAndWorkViewModel))
		{
			CalendarAndWorkViewHolder calendarAndWorkViewHolder = new CalendarAndWorkViewHolder();
			calendarAndWorkViewHolder.Init(_Params.CalendarAndWorkPrefab, _Params.Content, itemIndex);
			CalendarContentsListItemCalendarAndWorkTimeView view = calendarAndWorkViewHolder.View;
			view.CalendarCoreUI.OnChangedMonth.Subscribe(delegate((int, int) _)
			{
				_onValueChangedMonth.OnNext(_);
			}).AddTo(this);
			view.CalendarCoreUI.OnSelectedDay.Subscribe(delegate(SelectCalendarDayUI ui)
			{
				_onSelectedDay.OnNext(ui.DateTime);
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(view.WorkTimeView.OnClickEditButton, delegate
			{
				CalendarAndWorkViewModel calendarAndWorkModel = GetCalendarAndWorkModel();
				_onSettingWorkTime.OnNext(calendarAndWorkModel.WorkTime);
			}).AddTo(this);
			return calendarAndWorkViewHolder;
		}
		if (cachedType == typeof(CalendarCompletedTaskHeaderViewModel))
		{
			CalendarCompletedTaskHeaderViewHolder calendarCompletedTaskHeaderViewHolder = new CalendarCompletedTaskHeaderViewHolder();
			calendarCompletedTaskHeaderViewHolder.Init(_Params.CompletedTaskHeaderPrefab, _Params.Content, itemIndex);
			CalendarContentsListItemCompletedTaskHeaderView view2 = calendarCompletedTaskHeaderViewHolder.View;
			view2.RemovingToggle.OnValueChanged.Subscribe(delegate(bool flag)
			{
				CalendarCompletedTaskViewHolder.IsRemovingMode = flag;
				(Data[_completedTaskHeaderModelIdx] as CalendarCompletedTaskHeaderViewModel).IsRemovingMode = flag;
				foreach (CalendarContentsBaseViewHolder visibleItem in _VisibleItems)
				{
					if (visibleItem is CalendarCompletedTaskViewHolder calendarCompletedTaskViewHolder2)
					{
						calendarCompletedTaskViewHolder2.ChangeRemovingMode();
					}
				}
			}).AddTo(this);
			view2.PullDown.OnValueChanged.Subscribe(delegate(bool flag)
			{
				_isCompleteTodoPullDownOpened = flag;
				(Data[_completedTaskHeaderModelIdx] as CalendarCompletedTaskHeaderViewModel).IsOpened = flag;
				UpdateCompletedTaskHeaderView();
				if (_isCompleteTodoPullDownOpened)
				{
					PlayPullDownAnimation(isOpen: true);
				}
				else
				{
					PlayPullDownAnimation(isOpen: false);
				}
			}).AddTo(this);
			return calendarCompletedTaskHeaderViewHolder;
		}
		if (cachedType == typeof(CalendarCompletedTaskViewModel))
		{
			CalendarCompletedTaskViewHolder calendarCompletedTaskViewHolder = new CalendarCompletedTaskViewHolder();
			calendarCompletedTaskViewHolder.Init(_Params.CompletedTaskPrefab, _Params.Content, itemIndex);
			CalendarContentsListItemCompletedTaskView view3 = calendarCompletedTaskViewHolder.View;
			view3.TodoListItemView.OnChangedEndTodoText.Subscribe(delegate((ulong, string) data)
			{
				SearchTodoTaskModel(data.Item1).Title = data.Item2;
				_onChangedTodoTitle.OnNext(data);
			}).AddTo(this);
			view3.TodoListItemView.OnSettingDateTime.Subscribe(delegate((ulong uniqueId, DateTime? datetime) settingData)
			{
				SearchTodoTaskModel(settingData.uniqueId);
				_onSettingCompletedDateTime.OnNext(settingData);
			}).AddTo(this);
			view3.TodoListItemView.OnRemoved.Subscribe(delegate(ulong uniqueID)
			{
				_removeAnimationState.Play(view3.TodoListItemView, uniqueID, _Params.CompletedTaskPrefabHeight, 0.1f).Forget();
				_onRemovedTodo.OnNext(uniqueID);
			}).AddTo(this);
			view3.TodoListItemView.OnRemovedHabitTrackerTask.Subscribe(delegate(string habitID)
			{
				_removeAnimationHabitTrackerState.Play(view3.TodoListItemView, habitID, _Params.CompletedTaskPrefabHeight, 0.1f).Forget();
				_onRemovedHabitTrackerTask.OnNext(habitID);
			}).AddTo(this);
			view3.TodoListItemView._OnChangedEndTodoTextHabitTrackerTask.Subscribe(delegate((string, string) data)
			{
				SearchHabitTrackerTaskModel(data.Item1).Title = data.Item2;
				_onChangedHabitTrackerTaskTitle.OnNext(data);
			}).AddTo(this);
			return calendarCompletedTaskViewHolder;
		}
		if (cachedType == typeof(CalendarDiaryViewModel))
		{
			CalendarDiaryViewHolder calendarDiaryViewHolder = new CalendarDiaryViewHolder();
			calendarDiaryViewHolder.Init(_Params.DiaryPrefab, _Params.Content, itemIndex);
			CalendarContentsListItemDiaryView view4 = calendarDiaryViewHolder.View;
			view4.AutoSizingHeightInputFieldView.OnEndEdit.Subscribe(delegate(string _)
			{
				(Data[_lastDataIdx] as CalendarDiaryViewModel).DiaryText = _;
				_onEndEditDiary.OnNext(_);
			}).AddTo(this);
			view4.OnValueChangedHeight.Subscribe(delegate((float, bool) height)
			{
				CalendarDiaryViewModel obj = Data[_lastDataIdx] as CalendarDiaryViewModel;
				obj.HasPendingSizeChange = true;
				obj.Height = height.Item1;
				obj.IsChangedHeightStationary = !height.Item2;
			}).AddTo(this);
			return calendarDiaryViewHolder;
		}
		throw new InvalidOperationException("Unrecognized model type: " + cachedType.Name);
	}

	protected override void UpdateViewsHolder(CalendarContentsBaseViewHolder newOrRecycled)
	{
		CalendarContentsListBaseModel calendarContentsListBaseModel = Data[newOrRecycled.ItemIndex];
		newOrRecycled.UpdateViews(calendarContentsListBaseModel);
		if (calendarContentsListBaseModel.HasPendingSizeChange)
		{
			ScheduleComputeVisibilityTwinPass();
		}
	}

	protected override float UpdateItemSizeOnTwinPass(CalendarContentsBaseViewHolder viewsHolder)
	{
		viewsHolder.UpdateItemHeight(Data[viewsHolder.ItemIndex].Height);
		return base.UpdateItemSizeOnTwinPass(viewsHolder);
	}

	protected override void OnItemHeightChangedPreTwinPass(CalendarContentsBaseViewHolder viewsHolder)
	{
		base.OnItemHeightChangedPreTwinPass(viewsHolder);
		viewsHolder.UpdateItemHeight(Data[viewsHolder.ItemIndex].Height);
		Data[viewsHolder.ItemIndex].HasPendingSizeChange = false;
	}

	protected override void OnItemIndexChangedDueInsertOrRemove(CalendarContentsBaseViewHolder shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
	{
		base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);
	}

	protected override bool IsRecyclable(CalendarContentsBaseViewHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
	{
		CalendarContentsListBaseModel calendarContentsListBaseModel = Data[indexOfItemThatWillBecomeVisible];
		return potentiallyRecyclable.CanPresentModelType(calendarContentsListBaseModel.CachedType);
	}

	private CalendarCompletedTaskHeaderViewModel GetCompletedTaskHeaderModel()
	{
		return Data[_completedTaskHeaderModelIdx] as CalendarCompletedTaskHeaderViewModel;
	}

	private void UpdateCompletedTaskHeaderView()
	{
		GetItemViewsHolderIfVisible(_completedTaskHeaderModelIdx)?.UpdateViews(Data[_completedTaskHeaderModelIdx]);
	}

	private CalendarAndWorkViewModel GetCalendarAndWorkModel()
	{
		return Data[_calendarAndWorkTimeModelIdx] as CalendarAndWorkViewModel;
	}

	private void UpdateCalendarAndWorkView()
	{
		GetItemViewsHolderIfVisible(_calendarAndWorkTimeModelIdx)?.UpdateViews(Data[_calendarAndWorkTimeModelIdx]);
	}

	private CalendarDiaryViewModel GetCalendarDiaryModel()
	{
		return Data[_lastDataIdx] as CalendarDiaryViewModel;
	}

	private void UpdateCalendarDiaryView()
	{
		GetItemViewsHolderIfVisible(_lastDataIdx)?.UpdateViews(Data[_lastDataIdx]);
	}

	public void EnterSettingCalendarContentsView(CalenderMonthlyData calenderMonthlyData, int selectedYear, int selectedMonth, int selectedDay, TimeSpan workTime, string diaryText, List<CalendarContentsListBaseModel> todos, bool isPullDownForceOpen)
	{
		if (!base.IsInitialized)
		{
			_isRequestedBeforeInit = true;
			_requestedCalendarAndWorkDataBeforeInit.CalendarMonthlyData = calenderMonthlyData;
			_requestedCalendarAndWorkDataBeforeInit.SelectedYear = selectedYear;
			_requestedCalendarAndWorkDataBeforeInit.SelectedMonth = selectedMonth;
			_requestedCalendarAndWorkDataBeforeInit.SelectedDay = selectedDay;
			_requestedCalendarAndWorkDataBeforeInit.WorkTime = workTime;
			_requestedDiaryBeforeInit = diaryText;
			_requestedTodoDataBeforeInit = todos;
			_requestedPullDownForceOpenBeforeInit = isPullDownForceOpen;
			return;
		}
		_pullDownAnimationState.Cancel();
		double virtualAbstractNormalizedScrollPosition = GetVirtualAbstractNormalizedScrollPosition();
		SetActiveRaycastBlocker(active: false);
		CalendarCompletedTaskViewHolder.IsRemovingMode = false;
		_isCompleteTodoPullDownOpened = isPullDownForceOpen || _isCompleteTodoPullDownOpened;
		CalendarAndWorkViewModel calendarAndWorkModel = GetCalendarAndWorkModel();
		calendarAndWorkModel.CalendarMonthlyData = calenderMonthlyData;
		calendarAndWorkModel.SelectedYear = selectedYear;
		calendarAndWorkModel.SelectedMonth = selectedMonth;
		calendarAndWorkModel.SelectedDay = selectedDay;
		calendarAndWorkModel.WorkTime = workTime;
		UpdateCalendarAndWorkView();
		CalendarCompletedTaskHeaderViewModel completedTaskHeaderModel = GetCompletedTaskHeaderModel();
		completedTaskHeaderModel.IsRemovingMode = CalendarCompletedTaskViewHolder.IsRemovingMode;
		completedTaskHeaderModel.IsOpened = _isCompleteTodoPullDownOpened;
		completedTaskHeaderModel.ExistTodo = todos.Count > 0;
		UpdateCompletedTaskHeaderView();
		RemoveAllCompletedTodo();
		if (_isCompleteTodoPullDownOpened)
		{
			Data.InsertItems(_lastDataIdx, todos);
		}
		else
		{
			foreach (CalendarContentsListBaseModel todo in todos)
			{
				todo.Height = _Params.CompletedTaskPrefabHeight;
				_pullDownStateHelper.Add(todo);
			}
		}
		GetCalendarDiaryModel().DiaryText = diaryText;
		UpdateCalendarDiaryView();
		SetVirtualAbstractNormalizedScrollPosition(virtualAbstractNormalizedScrollPosition, computeVisibilityNow: true, out var _);
	}

	public void UpdateUIFromSelectedDay(int selectedYear, int selectedMonth, int selectedDay, TimeSpan workTime, string diaryText, List<CalendarContentsListBaseModel> todos)
	{
		_pullDownAnimationState.Cancel();
		CalendarAndWorkViewModel calendarAndWorkModel = GetCalendarAndWorkModel();
		calendarAndWorkModel.WorkTime = workTime;
		calendarAndWorkModel.SelectedYear = selectedYear;
		calendarAndWorkModel.SelectedMonth = selectedMonth;
		calendarAndWorkModel.SelectedDay = selectedDay;
		UpdateCalendarAndWorkView();
		CalendarCompletedTaskViewHolder.IsRemovingMode = false;
		CalendarCompletedTaskHeaderViewModel completedTaskHeaderModel = GetCompletedTaskHeaderModel();
		completedTaskHeaderModel.IsRemovingMode = CalendarCompletedTaskViewHolder.IsRemovingMode;
		completedTaskHeaderModel.IsOpened = _isCompleteTodoPullDownOpened;
		completedTaskHeaderModel.ExistTodo = todos.Count > 0;
		UpdateCompletedTaskHeaderView();
		GetCalendarDiaryModel().DiaryText = diaryText;
		UpdateCalendarDiaryView();
		RemoveAllCompletedTodo();
		if (_isCompleteTodoPullDownOpened)
		{
			Data.InsertItems(_lastDataIdx, todos);
			return;
		}
		foreach (CalendarContentsListBaseModel todo in todos)
		{
			todo.Height = _Params.CompletedTaskPrefabHeight;
			_pullDownStateHelper.Add(todo);
		}
	}

	public void SetMonthlyDataOnlyWithoutUpdateUI(CalenderMonthlyData calendarMonthlyData)
	{
		GetCalendarAndWorkModel().CalendarMonthlyData = calendarMonthlyData;
	}

	private void RemoveAllCompletedTodo()
	{
		if (ExistCompletedTask)
		{
			Data.RemoveItems(_completedTaskModelStartIdx, _lastDataIdx - _completedTaskModelStartIdx);
		}
		_pullDownStateHelper.Clear();
	}

	public void RemoveCompletedTodoTaskWithAnimation(ulong uniqueID)
	{
		int num = -1;
		for (int i = 0; i < Data.Count; i++)
		{
			if (Data[i] is CalendarCompletedTaskViewModel calendarCompletedTaskViewModel && calendarCompletedTaskViewModel.UniqueID == uniqueID)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			if (!(GetItemViewsHolderIfVisible(num) is CalendarCompletedTaskViewHolder calendarCompletedTaskViewHolder))
			{
				Data.RemoveOne(num);
			}
			else
			{
				_removeAnimationState.Play(calendarCompletedTaskViewHolder.View.TodoListItemView, uniqueID, _Params.CompletedTaskHeaderPrefabHeight, 0.1f).Forget();
			}
		}
	}

	public void RefleshCalendarView(CalenderMonthlyData data)
	{
		GetCalendarAndWorkModel().CalendarMonthlyData = data;
		UpdateCalendarAndWorkView();
	}

	public void MoveNextSelectedDay()
	{
		CalendarAndWorkViewModel calendarAndWorkModel = GetCalendarAndWorkModel();
		int selectedDay = calendarAndWorkModel.SelectedDay;
		int selectedYear = calendarAndWorkModel.SelectedYear;
		int selectedMonth = calendarAndWorkModel.SelectedMonth;
		int num = selectedDay + 1;
		if (num > DateTime.DaysInMonth(selectedYear, selectedMonth))
		{
			int num2 = selectedMonth + 1;
			int num3 = ((num2 >= 12) ? (selectedYear + 1) : selectedYear);
			if (num3 <= 2999)
			{
				if (num2 > 12)
				{
					num2 = 1;
				}
				_onValueChangedMonth.OnNext((num3, num2));
				num = 1;
				_onSelectedDay.OnNext(new DateTime(num3, num2, num));
			}
		}
		else
		{
			_onSelectedDay.OnNext(new DateTime(selectedYear, selectedMonth, num));
		}
	}

	public void MovePrevSelectedDay()
	{
		CalendarAndWorkViewModel calendarAndWorkModel = GetCalendarAndWorkModel();
		int selectedDay = calendarAndWorkModel.SelectedDay;
		int selectedYear = calendarAndWorkModel.SelectedYear;
		int selectedMonth = calendarAndWorkModel.SelectedMonth;
		int num = selectedDay - 1;
		if (num < 1)
		{
			int num2 = selectedMonth - 1;
			int num3 = ((num2 < 1) ? (selectedYear - 1) : selectedYear);
			if (num3 >= 2025)
			{
				if (num2 < 1)
				{
					num2 = 12;
				}
				_onValueChangedMonth.OnNext((num3, num2));
				num = DateTime.DaysInMonth(num3, num2);
				_onSelectedDay.OnNext(new DateTime(num3, num2, num));
			}
		}
		else
		{
			_onSelectedDay.OnNext(new DateTime(selectedYear, selectedMonth, num));
		}
	}

	public void UpdateWorkTime(TimeSpan workTime)
	{
		CalendarAndWorkViewModel calendarAndWorkModel = GetCalendarAndWorkModel();
		calendarAndWorkModel.WorkTime = workTime;
		if (base.gameObject.activeInHierarchy)
		{
			GetItemViewsHolderIfVisible(_calendarAndWorkTimeModelIdx)?.UpdateViews(calendarAndWorkModel);
		}
	}

	private void PlayPullDownAnimation(bool isOpen)
	{
		if (!ExistCompletedTask && !_pullDownStateHelper.IsKeep)
		{
			return;
		}
		int num = 0;
		if (isOpen)
		{
			num = _pullDownStateHelper.Count;
		}
		else
		{
			(int, int) tuple = _pullDownStateHelper.AddTempExclusions(_VisibleItems, Data, _lastDataIdx - 1);
			if (tuple.Item2 > 0)
			{
				Data.RemoveItems(tuple.Item1, tuple.Item2);
			}
			if (!ExistCompletedTask)
			{
				_pullDownStateHelper.MargeTempExclusionsIntoKeeps();
				return;
			}
			num = _lastDataIdx - _completedTaskModelStartIdx;
		}
		_pullDownAnimationState.PlayAnimation(isOpen, _completedTaskModelStartIdx, num, 15, _Params.CompletedTaskPrefabHeight, 0.2f);
	}

	private void SetActiveRaycastBlocker(bool active)
	{
		if (_raycastBlocker.activeSelf != active)
		{
			_raycastBlocker.SetActive(active);
		}
	}

	private CalendarCompletedTaskViewModel SearchTodoTaskModel(ulong uniqueID)
	{
		return (from x in Data
			where x is CalendarCompletedTaskViewModel
			select x as CalendarCompletedTaskViewModel).FirstOrDefault((CalendarCompletedTaskViewModel x) => x.UniqueID == uniqueID);
	}

	private CalendarCompletedTaskViewModel SearchHabitTrackerTaskModel(string habitID)
	{
		return (from x in Data
			where x is CalendarCompletedTaskViewModel
			select x as CalendarCompletedTaskViewModel).FirstOrDefault((CalendarCompletedTaskViewModel x) => x.HabitTrackerID == habitID);
	}

	void IInfomationProviderForOSAMyAnimation.RequestChangeItemSizeAndUpdateLayout(int idx, float size, bool endEdgeStationary)
	{
		RequestChangeItemSizeAndUpdateLayout(idx, size);
		Data[idx].Height = size;
	}

	void IInfomationProviderForOSAMyAnimation.RequestChangeItemSize(int idx, float size)
	{
		Data[idx].Height = size;
	}

	int IOSAModelIndexGetter<ulong>.GetModelIndex(ulong equatable)
	{
		for (int i = 0; i < Data.Count; i++)
		{
			if (Data[i] is CalendarCompletedTaskViewModel calendarCompletedTaskViewModel && calendarCompletedTaskViewModel.UniqueID == equatable)
			{
				return i;
			}
		}
		return -1;
	}

	int IOSAModelIndexGetter<string>.GetModelIndex(string equatable)
	{
		for (int i = 0; i < Data.Count; i++)
		{
			if (Data[i] is CalendarCompletedTaskViewModel { IsHabitTrackerTask: not false } calendarCompletedTaskViewModel && calendarCompletedTaskViewModel.HabitTrackerID == equatable)
			{
				return i;
			}
		}
		return -1;
	}
}
