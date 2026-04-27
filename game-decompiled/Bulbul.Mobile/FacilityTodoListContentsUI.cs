using System;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityTodoListContentsUI : MonoBehaviour
{
	[Inject]
	private SystemSeService _seService;

	[SerializeField]
	private TodoListUIViewMobile _uiView;

	private TodoListUIModel _todoListUIModel;

	private Subject<Unit> _onAchievementNoticeOnCompleteTodo = new Subject<Unit>();

	public Observable<Unit> OnAchievementNoticeOnCompleteTodo => _onAchievementNoticeOnCompleteTodo;

	public void Setup()
	{
		if (_todoListUIModel == null)
		{
			_todoListUIModel = new TodoListUIModel();
			_todoListUIModel.Initialize();
		}
		_uiView.Setup(_todoListUIModel);
		_uiView.OnPrepareContent.Subscribe(delegate(bool isFromTab)
		{
			EnterTodoListContents(!isFromTab);
		}).AddTo(this);
		_uiView.TodoListContentView.OnChangeRemoveMode.Subscribe(delegate(bool isRemoveMode)
		{
			_todoListUIModel.ChangeRemoveMode(isRemoveMode);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_uiView.TodoListContentView.OnAddTodoList, delegate
		{
			_todoListUIModel.AddTodoList();
			_seService.PlayClick();
		}).AddTo(this);
		_uiView.TodoListContentView.OnRemoveTodoList.Subscribe(delegate(ulong uuid)
		{
			_todoListUIModel.RemoveTodoList(uuid);
			_seService.PlayCancel();
		}).AddTo(this);
		_uiView.TodoListContentView.OnSelectTodoList.Subscribe(delegate(ulong uuid)
		{
			_todoListUIModel.SelectTodoList(uuid);
			_seService.PlayClick();
		}).AddTo(this);
		_uiView.TodoListContentView.OnChangeTodoListTitle.Subscribe(delegate((ulong uuid, string title) info)
		{
			_todoListUIModel.ChangeTodoListTitle(info.uuid, info.title);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_uiView.TodoListContentView.OnAddTask, delegate
		{
			_todoListUIModel.AddTodo();
			_seService.PlayClick();
		}).AddTo(this);
		_uiView.TodoTaskListView.OnRemoveTask.Subscribe(delegate(ulong uuid)
		{
			_todoListUIModel.RemoveTodo(uuid);
			_seService.PlayCancel();
		}).AddTo(this);
		_uiView.TodoTaskListView.OnCompleteTask.Subscribe(delegate(ulong uuid)
		{
			_todoListUIModel.CompleteTodo(uuid);
			_seService.PlayTaskComplete();
			_onAchievementNoticeOnCompleteTodo.OnNext(Unit.Default);
		}).AddTo(this);
		_uiView.TodoTaskListView.OnUncompleteTask.Subscribe(delegate(ulong uuid)
		{
			_todoListUIModel.UncompleteTodo(uuid);
			_seService.PlayCancel();
		}).AddTo(this);
		_uiView.TodoTaskListView.OnChangeTodoTitle.Subscribe(delegate((ulong uuid, string title) info)
		{
			_todoListUIModel.ChangeTodoTitle(info.uuid, info.title);
		}).AddTo(this);
		_uiView.TodoTaskListView.OnSelectExpireCalendar.Subscribe(delegate((ulong uuid, DateTime? datetime) settingData)
		{
			_todoListUIModel.DateTimeEditingTodoUuid = settingData.uuid;
			_todoListUIModel.isExpireEditing = true;
			_todoListUIModel.isCompleteEditing = false;
			_uiView.CalendarView.Open(settingData.datetime);
			_seService.PlaySelect();
		}).AddTo(this);
		_uiView.TodoTaskListView.OnSelectCompleteCalender.Subscribe(delegate((ulong uuid, DateTime? datetime) settingData)
		{
			_todoListUIModel.DateTimeEditingTodoUuid = settingData.uuid;
			_todoListUIModel.isExpireEditing = false;
			_todoListUIModel.isCompleteEditing = true;
			_uiView.CalendarView.Open(settingData.datetime);
			_seService.PlaySelect();
		}).AddTo(this);
		_uiView.TodoTaskListView.OnSwapTodoTask.Subscribe(delegate((ulong target, ulong origin) swapInfo)
		{
			_todoListUIModel.SwapAfter(swapInfo.target, swapInfo.origin);
		}).AddTo(this);
		_uiView.CalendarView.OnSelectedDay.Subscribe(delegate(SelectCalendarDayUI day)
		{
			if (_todoListUIModel.isExpireEditing)
			{
				_todoListUIModel.SetExpireDate(_todoListUIModel.DateTimeEditingTodoUuid, day.DateTime);
			}
			else if (_todoListUIModel.isCompleteEditing)
			{
				_todoListUIModel.SetCompleteDate(_todoListUIModel.DateTimeEditingTodoUuid, day.DateTime);
			}
			_todoListUIModel.isExpireEditing = false;
			_todoListUIModel.isCompleteEditing = false;
			_todoListUIModel.DateTimeEditingTodoUuid = 0uL;
			_seService.PlayClick();
		}).AddTo(this);
		_todoListUIModel.OnSelectTodoList.Subscribe(delegate(TodoListData todoList)
		{
			_uiView.TodoListContentView.SelectTodoList(todoList.UniqueID);
			_uiView.TodoTaskListView.EnterSettings(_todoListUIModel, isPullDownOpen: false);
		}).AddTo(this);
		_todoListUIModel.OnAddTodoList.Subscribe(delegate(TodoListData todoList)
		{
			_uiView.TodoListContentView.AddTodoList(todoList);
		}).AddTo(this);
		_todoListUIModel.OnRemoveTodoList.Subscribe(delegate(ulong uuid)
		{
			_uiView.TodoListContentView.RemoveTodoList(uuid);
		}).AddTo(this);
		_todoListUIModel.OnChangeTodoListTitle.Subscribe(delegate((ulong uuid, string title) data)
		{
			_uiView.TodoListContentView.ChangeTodoListTitle(data.uuid, data.title);
		}).AddTo(this);
		_todoListUIModel.OnAddTodoData.Subscribe(delegate((TodoData todoData, int compCount, int taskCount) data)
		{
			_uiView.TodoListContentView.OnUpdateTaskCount(data.compCount, data.taskCount);
			_uiView.TodoTaskListView.AddTodoTask(data.todoData);
		}).AddTo(this);
		_todoListUIModel.OnRemoveTodoData.Subscribe(delegate((ulong uuid, int compCount, int taskCount) data)
		{
			_uiView.TodoListContentView.OnUpdateTaskCount(data.compCount, data.taskCount);
			_uiView.TodoTaskListView.RemoveTodoTask(data.uuid);
		}).AddTo(this);
		_todoListUIModel.OnChangeTodoTitle.Subscribe(delegate((ulong uuid, string title) data)
		{
			_uiView.TodoTaskListView.ChangeTodoTitle(data.uuid, data.title);
		}).AddTo(this);
		_todoListUIModel.OnSetTodoExpireDate.Subscribe(delegate((ulong uuid, DateTime? expireDate) data)
		{
			_uiView.TodoTaskListView.SetTodoExpireDate(data.uuid, data.expireDate);
		}).AddTo(this);
		_todoListUIModel.OnSetTodoCompleteDate.Subscribe(delegate((ulong uuid, DateTime? completeDate) data)
		{
			_uiView.TodoTaskListView.SetTodoCompleteDate(data.uuid, data.completeDate);
		}).AddTo(this);
		_todoListUIModel.OnSetTodoComplete.Subscribe(delegate((TodoData todoData, int compCount, int taskCount) data)
		{
			_uiView.TodoListContentView.OnUpdateTaskCount(data.compCount, data.taskCount);
			_uiView.TodoTaskListView.SetTodoComplete(data.todoData);
		}).AddTo(this);
		_todoListUIModel.OnSetTodoUncomplete.Subscribe(delegate((TodoData todoData, int compCount, int taskCount) data)
		{
			_uiView.TodoListContentView.OnUpdateTaskCount(data.compCount, data.taskCount);
			_uiView.TodoTaskListView.SetTodoUncomplete(data.todoData);
		}).AddTo(this);
		_todoListUIModel.OnChangeRemoveMode.Subscribe(delegate(bool isRemove)
		{
			_uiView.TodoListContentView.ChangeTodoRemoveMode(isRemove);
			_uiView.TodoTaskListView.SetChangeRemoveMode(isRemove);
		}).AddTo(this);
	}

	private void EnterTodoListContents(bool isPullDownOpen)
	{
		_todoListUIModel.EnterSettings();
		_uiView.TodoListContentView.EnterSettings(_todoListUIModel);
		_uiView.TodoTaskListView.EnterSettings(_todoListUIModel, isPullDownOpen);
	}
}
