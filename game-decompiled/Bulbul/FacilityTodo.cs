using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilityTodo : MonoBehaviour
{
	private enum MainState
	{
		Idle
	}

	private MainState _mainState;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private TodoListUI _todoListUI;

	private ulong _currentTodoListID;

	private TodoUI calenderTargetTodo;

	private readonly Subject<TodoData> onAddTodo = new Subject<TodoData>();

	private readonly Subject<TodoData> onRemoveTodo = new Subject<TodoData>();

	private readonly Subject<TodoData> onCompleteTodo = new Subject<TodoData>();

	private readonly Subject<TodoData> onUncompleteTodo = new Subject<TodoData>();

	private readonly Subject<(TodoData, DateTime)> onChangeCompleteDayTodo = new Subject<(TodoData, DateTime)>();

	private List<TodoData> temporatyCompletedTodoTaskList = new List<TodoData>(50);

	public Observable<TodoData> OnAddTodo => onAddTodo;

	public Observable<TodoData> OnRemoveTodo => onRemoveTodo;

	public Observable<TodoData> OnCompleteTodo => onCompleteTodo;

	public Observable<TodoData> OnUncompleteTodo => onUncompleteTodo;

	public Observable<(TodoData, DateTime)> OnChangeCompleteDayTodo => onChangeCompleteDayTodo;

	public TodoListData CurrentTodoListData => SaveDataManager.Instance.TodoAllData.TodoListDic[_currentTodoListID];

	public void Setup()
	{
		_todoListUI.Setup();
		if (SaveDataManager.Instance.TodoAllData.TodoListDic.Count > 0)
		{
			_currentTodoListID = SaveDataManager.Instance.TodoAllData.TodoListDic.FirstOrDefault().Value.UniqueID;
		}
		else
		{
			TodoListData todoListData = new TodoListData();
			SaveDataManager.Instance.TodoAllData.AddTodoList(todoListData);
			_currentTodoListID = todoListData.UniqueID;
		}
		foreach (KeyValuePair<ulong, TodoListData> item in SaveDataManager.Instance.TodoAllData.TodoListDic)
		{
			_todoListUI.AddTodoListUI(item.Value, OnClickButtonSelectTodoList, OnValueChangedTodoListTitleText, OnChangedTodoListTitleText, OnClickButtonRemoveTodoList);
		}
		if (_todoListUI.SelectedTodoListUIListCount == 0)
		{
			TodoListData todoListData2 = new TodoListData();
			if (!SaveDataManager.Instance.TodoAllData.AddTodoList(todoListData2))
			{
				return;
			}
			_todoListUI.AddTodoListUI(todoListData2, OnClickButtonSelectTodoList, OnValueChangedTodoListTitleText, OnChangedTodoListTitleText, OnClickButtonRemoveTodoList);
		}
		if (_todoListUI.SelectedTodoListUIListCount <= 1)
		{
			_todoListUI.DisableFirstListRemoveButton();
		}
		_todoListUI.SelectFirstTodoList();
		_todoListUI.UpdateTodoCountText(_currentTodoListID);
		ObservableSubscribeExtensions.Subscribe(_todoListUI.OnClickCalendarOutside.ObserveOn(UnityFrameProvider.PreLateUpdate), delegate
		{
			if (!(calenderTargetTodo == null))
			{
				_todoListUI.CloseCalender();
			}
		}).AddTo(this);
		_todoListUI.OnCalendarSelectDay.Subscribe(delegate(SelectCalendarDayUI ui)
		{
			if (!(calenderTargetTodo == null))
			{
				if (calenderTargetTodo.TodoData.CurrentState == TodoState.Working)
				{
					calenderTargetTodo.SetExpire(ui.DateTime);
				}
				else
				{
					onChangeCompleteDayTodo.OnNext((calenderTargetTodo.TodoData, ui.DateTime));
					calenderTargetTodo.SetComplete(ui.DateTime);
				}
				SaveDataManager.Instance.SaveTodoList(CurrentTodoListData);
				_todoListUI.CloseCalender();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_todoListUI.OnCalendarDisable, delegate
		{
			calenderTargetTodo = null;
		}).AddTo(this);
		_todoListUI.OnSwapAfter.Subscribe(delegate((ulong listID, ulong target, ulong origin) info)
		{
			SwapAfter(info.listID, info.target, info.origin);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_todoListUI.OnClickCloseButton, delegate
		{
			OnClickButtonDeactivateTodoList();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_todoListUI.OnClickAddTodoButton, delegate
		{
			OnClickButtonAddTodo();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_todoListUI.OnClickAddTodoListButton, delegate
		{
			OnClickButtonAddTodoList();
		}).AddTo(this);
		_todoListUI.SelectFirstList();
		Deactivate();
	}

	public void UpdateFacility()
	{
		if (_mainState == MainState.Idle)
		{
			_todoListUI.UpdateUI();
		}
	}

	public bool IsActive()
	{
		if (_todoListUI.IsActive())
		{
			return true;
		}
		return false;
	}

	public void Activate()
	{
		_todoListUI.Activate();
	}

	public void Deactivate()
	{
		_todoListUI.Deactivate();
	}

	public void InitializePosition()
	{
		_todoListUI.InitializePosition();
	}

	public void OnClickButtonAddTodoList()
	{
		TodoListData todoListData = new TodoListData();
		if (SaveDataManager.Instance.TodoAllData.AddTodoList(todoListData))
		{
			_todoListUI.AddTodoListUI(todoListData, OnClickButtonSelectTodoList, OnValueChangedTodoListTitleText, OnChangedTodoListTitleText, OnClickButtonRemoveTodoList);
			_todoListUI.EnableFirstListRemoveButton();
			_systemSeService.PlayClick();
		}
	}

	private void OnClickButtonRemoveTodoList(ulong todoListID)
	{
		SaveDataManager.Instance.TodoAllData.RemoveTodoList(todoListID);
		_todoListUI.RemoveSelectTodoListUI(todoListID);
		if (_currentTodoListID == todoListID)
		{
			_todoListUI.SelectFirstList();
		}
		if (_todoListUI.SelectedTodoListUIListCount <= 1)
		{
			_todoListUI.DisableFirstListRemoveButton();
		}
		_systemSeService.PlayCancel();
	}

	public void OnClickButtonSelectTodoList(ulong todoListID, InteractableUI interactableUI)
	{
		temporatyCompletedTodoTaskList.Clear();
		_currentTodoListID = todoListID;
		TodoListData todoListData = SaveDataManager.Instance.TodoAllData.TodoListDic[todoListID];
		_todoListUI.OnSelectTodoListUI(todoListData, interactableUI);
		foreach (ulong todoOrder in SaveDataManager.Instance.TodoAllData.TodoListDic[todoListID].TodoOrderList)
		{
			if (SaveDataManager.Instance.TodoAllData.TodoListDic[todoListID].TodoDic.TryGetValue(todoOrder, out var value))
			{
				if (value.CurrentState != TodoState.Complete)
				{
					AddTodo(value);
				}
				else
				{
					temporatyCompletedTodoTaskList.Add(value);
				}
			}
		}
		temporatyCompletedTodoTaskList.Sort(SortCompletedTodoTask);
		foreach (TodoData temporatyCompletedTodoTask in temporatyCompletedTodoTaskList)
		{
			AddTodo(temporatyCompletedTodoTask);
		}
		_todoListUI.UpdateTodoCountText(_currentTodoListID);
	}

	private int SortCompletedTodoTask(TodoData a, TodoData b)
	{
		DateTime? completed = a.Completed;
		DateTime? completed2 = b.Completed;
		if (!completed.HasValue && !completed2.HasValue)
		{
			return 0;
		}
		if (!completed.HasValue)
		{
			return 1;
		}
		if (!completed2.HasValue)
		{
			return -1;
		}
		int num = completed.Value.CompareTo(completed2.Value);
		if (num < 0)
		{
			return 1;
		}
		if (num > 0)
		{
			return -1;
		}
		return 0;
	}

	private void AddTodo(TodoData todoData)
	{
		TodoUI todoUI = _todoListUI.AddTodoUI(todoData, OnChangeTodoText, OnClickButtonDeleteTodo);
		todoUI.OnSwitchComplete.Subscribe(delegate(TodoUI ui)
		{
			if (SaveDataManager.Instance.TodoAllData.TodoListDic.TryGetValue(_currentTodoListID, out var value) && value.TodoDic.TryGetValue(todoData.UniqueID, out var value2) && IsTodoCompletable(value2))
			{
				if (calenderTargetTodo != null && calenderTargetTodo.TodoData == todoData)
				{
					_todoListUI.CloseCalender();
					calenderTargetTodo = null;
				}
				if (value2.CurrentState == TodoState.Working)
				{
					CompleteTodo(ui);
				}
				else
				{
					UncompleteTodo(ui);
				}
			}
		}).AddTo(todoUI.DestroyCancellationToken);
		todoUI.OnExpireClick.Subscribe(delegate(TodoUI ui)
		{
			if (calenderTargetTodo != null)
			{
				TodoUI todoUI2 = calenderTargetTodo;
				_todoListUI.CloseCalender();
				if (todoUI2 == ui)
				{
					return;
				}
			}
			DateTime? dateTime = ((ui.TodoData.CurrentState == TodoState.Working) ? ui.TodoData.Expire : ui.TodoData.Completed);
			_todoListUI.OpenCalender(dateTime);
			calenderTargetTodo = ui;
		}).AddTo(todoUI.DestroyCancellationToken);
	}

	private void OnValueChangedTodoListTitleText(ulong pageID, string inputText)
	{
		_todoListUI.OnValueChangedTitleText(pageID, inputText);
	}

	private void OnChangedTodoListTitleText(ulong pageID, string inputText)
	{
		SaveDataManager.Instance.TodoAllData.SetTitleText(pageID, inputText);
	}

	public void OnClickButtonAddTodo()
	{
		TodoData todoData = new TodoData();
		if (!SaveDataManager.Instance.TodoAllData.TodoListDic[_currentTodoListID].AddTodo(todoData))
		{
			Debug.LogError("すでに同じIDのTodoが登録されています！通常通らないはず");
			return;
		}
		AddTodo(todoData);
		_todoListUI.UpdateTodoCountText(_currentTodoListID);
		_systemSeService.PlayClick();
		onAddTodo.OnNext(todoData);
	}

	private void OnClickButtonDeleteTodo(TodoData todoData)
	{
		_systemSeService.PlayCancel();
		if (calenderTargetTodo != null && calenderTargetTodo.TodoData == todoData)
		{
			_todoListUI.CloseCalender();
			calenderTargetTodo = null;
		}
		SaveDataManager.Instance.TodoAllData.TodoListDic[_currentTodoListID].RemoveTodo(todoData);
		_todoListUI.DeleteTodoAsync(todoData).Forget();
		_todoListUI.UpdateTodoCountText(_currentTodoListID);
		onRemoveTodo.OnNext(todoData);
	}

	private void OnChangeTodoText(TodoData todoData, string inputText)
	{
		SaveDataManager.Instance.TodoAllData.TodoListDic[_currentTodoListID].SetInputTextTodo(todoData, inputText);
	}

	private void CompleteTodo(TodoUI todoUI)
	{
		_systemSeService.PlayTaskComplete();
		TodoData todoData = todoUI.TodoData;
		todoUI.ChangeUIForCompleteAsync().ContinueWith(async delegate
		{
			todoUI.ToImpossibleSwap();
			if (todoData.CurrentState == TodoState.Complete)
			{
				await _todoListUI.MoveTodoUIToCompleted(todoUI);
			}
			todoUI.ToPossibleSwap();
			todoUI.OnFinishMoveAnim();
		});
		todoData.SetCompleteTodoDatetime(DateTime.Now);
		todoUI.SetComplete(DateTime.Now);
		SaveDataManager.Instance.SaveTodoList(CurrentTodoListData);
		_todoListUI.UpdateTodoCountText(_currentTodoListID);
		TodoData value = todoData.DeepCopy();
		SaveDataManager.Instance.CalendarData.GetDailyData(DateTime.Now).CompleteTodoListDic.TryAdd(todoData.UniqueID, value);
		SaveDataManager.Instance.CalendarData.SaveDateData(DateTime.Now);
		onCompleteTodo.OnNext(value);
	}

	private void UncompleteTodo(TodoUI todoUI)
	{
		_systemSeService.PlayCancel();
		TodoData todoData = todoUI.TodoData;
		DateTime? completed = todoData.Completed;
		todoUI.ChangeUIForUncompleteAsync().ContinueWith(async delegate
		{
			todoUI.ToImpossibleSwap();
			if (todoData.CurrentState == TodoState.Working)
			{
				await _todoListUI.MoveTodoUIToUncomplete(todoUI);
			}
			todoUI.ToPossibleSwap();
			todoUI.OnFinishMoveAnim();
		});
		todoData.Uncomplete();
		todoUI.SetExpire(todoData.Expire);
		SaveDataManager.Instance.SaveTodoList(CurrentTodoListData);
		_todoListUI.UpdateTodoCountText(_currentTodoListID);
		if (completed.HasValue)
		{
			SaveDataManager.Instance.CalendarData.GetDailyData(completed.Value).CompleteTodoListDic.Remove(todoData.UniqueID);
			SaveDataManager.Instance.CalendarData.SaveDateData(completed.Value);
		}
		onUncompleteTodo.OnNext(todoData);
	}

	private bool IsTodoCompletable(TodoData todoData)
	{
		return !string.IsNullOrWhiteSpace(todoData.TodoText);
	}

	public void OnClickButtonDeactivateTodoList()
	{
		Deactivate();
		_systemSeService.PlayCancel();
	}

	public void SwapAfter(ulong listID, ulong target, ulong origin)
	{
		SaveDataManager.Instance.TodoAllData.TodoListDic[listID].SwapTodo(target, origin);
	}
}
