using System;
using System.Collections.Generic;
using System.Linq;
using R3;

namespace Bulbul.Mobile;

public class TodoListUIModel : IDisposable
{
	public ulong CurrentTodoListUuid;

	public bool isExpireEditing;

	public bool isCompleteEditing;

	public ulong DateTimeEditingTodoUuid;

	private static bool isTaskRemoveMode;

	private Subject<bool> onChangeRemoveMode = new Subject<bool>();

	private Subject<TodoListData> onSelectTodoList = new Subject<TodoListData>();

	private Subject<TodoListData> onAddTodoList = new Subject<TodoListData>();

	private Subject<ulong> onRemoveTodoList = new Subject<ulong>();

	private Subject<(ulong, string)> onChangeTodoListTitle = new Subject<(ulong, string)>();

	private Subject<(TodoData todoData, int compCount, int taskCount)> onAddTodoData = new Subject<(TodoData, int, int)>();

	private Subject<(ulong uuid, int compCount, int taskCount)> onRemoveTodoData = new Subject<(ulong, int, int)>();

	private Subject<(ulong, string)> onChangeTodoTitle = new Subject<(ulong, string)>();

	private Subject<(ulong, DateTime?)> onSetTodoExpireDate = new Subject<(ulong, DateTime?)>();

	private Subject<(ulong, DateTime?)> onSetTodoCompleteDate = new Subject<(ulong, DateTime?)>();

	private Subject<(TodoData todoData, int compCount, int taskCount)> onSetTodoComplete = new Subject<(TodoData, int, int)>();

	private Subject<(TodoData todoData, int compCount, int taskCount)> onSetTodoUncomplete = new Subject<(TodoData, int, int)>();

	public static bool IsTaskRemoveMode => isTaskRemoveMode;

	public static bool IsListRemoveMode
	{
		get
		{
			if (SaveDataManager.Instance.TodoAllData == null)
			{
				return false;
			}
			if (SaveDataManager.Instance.TodoAllData.TodoListDic == null)
			{
				return false;
			}
			return SaveDataManager.Instance.TodoAllData.TodoListDic.Count > 1;
		}
	}

	public TodoListData CurrentTodoList
	{
		get
		{
			if (!SaveDataManager.Instance.TodoAllData.TodoListDic.ContainsKey(CurrentTodoListUuid))
			{
				return null;
			}
			return SaveDataManager.Instance.TodoAllData.TodoListDic[CurrentTodoListUuid];
		}
	}

	public List<TodoData> SortedWorkingTodoList
	{
		get
		{
			List<TodoData> list = new List<TodoData>();
			foreach (ulong todoOrder in CurrentTodoList.TodoOrderList)
			{
				if (CurrentTodoList.TodoDic.TryGetValue(todoOrder, out var value) && value.CurrentState != TodoState.Complete)
				{
					list.Add(value);
				}
			}
			return list;
		}
	}

	public List<TodoData> SortedCompleteTodoList
	{
		get
		{
			List<TodoData> list = new List<TodoData>();
			foreach (TodoData value in CurrentTodoList.TodoDic.Values)
			{
				if (value != null && value.CurrentState == TodoState.Complete)
				{
					list.Add(value);
				}
			}
			list.Sort(SortCompletedTodoTask);
			return list;
		}
	}

	public IReadOnlyDictionary<ulong, TodoListData> TodoListDic
	{
		get
		{
			if (SaveDataManager.Instance.TodoAllData.TodoListDic == null)
			{
				return null;
			}
			return SaveDataManager.Instance.TodoAllData.TodoListDic;
		}
	}

	public Observable<bool> OnChangeRemoveMode => onChangeRemoveMode;

	public Observable<TodoListData> OnSelectTodoList => onSelectTodoList;

	public Observable<TodoListData> OnAddTodoList => onAddTodoList;

	public Observable<ulong> OnRemoveTodoList => onRemoveTodoList;

	public Observable<(ulong uuid, string title)> OnChangeTodoListTitle => onChangeTodoListTitle;

	public Observable<(TodoData todoData, int compCount, int taskCount)> OnAddTodoData => onAddTodoData;

	public Observable<(ulong uuid, int compCount, int taskCount)> OnRemoveTodoData => onRemoveTodoData;

	public Observable<(ulong uuid, string title)> OnChangeTodoTitle => onChangeTodoTitle;

	public Observable<(ulong uuid, DateTime? expireDate)> OnSetTodoExpireDate => onSetTodoExpireDate;

	public Observable<(ulong uuid, DateTime? completeDate)> OnSetTodoCompleteDate => onSetTodoCompleteDate;

	public Observable<(TodoData todoData, int compCount, int taskCount)> OnSetTodoComplete => onSetTodoComplete;

	public Observable<(TodoData todoData, int compCount, int taskCount)> OnSetTodoUncomplete => onSetTodoUncomplete;

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

	public int GetCurrentWorkingTaskCount()
	{
		return CurrentTodoList.TodoDic.Where((KeyValuePair<ulong, TodoData> x) => x.Value.CurrentState == TodoState.Working).Count();
	}

	public int GetCurrentCompleteTaskCount()
	{
		return CurrentTodoList.TodoDic.Where((KeyValuePair<ulong, TodoData> x) => x.Value.CurrentState == TodoState.Complete).Count();
	}

	void IDisposable.Dispose()
	{
		onChangeRemoveMode?.Dispose();
		onSelectTodoList?.Dispose();
		onAddTodoList?.Dispose();
		onRemoveTodoList?.Dispose();
		onChangeTodoListTitle?.Dispose();
		onAddTodoData?.Dispose();
		onRemoveTodoData?.Dispose();
		onChangeTodoTitle?.Dispose();
		onSetTodoExpireDate?.Dispose();
		onSetTodoComplete?.Dispose();
		onSetTodoUncomplete?.Dispose();
	}

	public void Initialize()
	{
		if (SaveDataManager.Instance.TodoAllData.TodoListDic.Count > 0)
		{
			CurrentTodoListUuid = SaveDataManager.Instance.TodoAllData.TodoListDic.FirstOrDefault().Value.UniqueID;
			return;
		}
		TodoListData todoListData = new TodoListData();
		SaveDataManager.Instance.TodoAllData.AddTodoList(todoListData);
		CurrentTodoListUuid = todoListData.UniqueID;
	}

	public void EnterSettings()
	{
		isTaskRemoveMode = false;
	}

	public void ChangeRemoveMode(bool isRemoveMode)
	{
		isTaskRemoveMode = isRemoveMode;
		onChangeRemoveMode?.OnNext(isTaskRemoveMode);
	}

	public void SelectTodoList(ulong uuid)
	{
		if (SaveDataManager.Instance.TodoAllData.TodoListDic.ContainsKey(uuid))
		{
			TodoListData todoListData = SaveDataManager.Instance.TodoAllData.TodoListDic[uuid];
			CurrentTodoListUuid = todoListData.UniqueID;
			onSelectTodoList?.OnNext(todoListData);
		}
	}

	public void AddTodoList()
	{
		TodoListData todoListData = new TodoListData();
		if (SaveDataManager.Instance.TodoAllData.AddTodoList(todoListData))
		{
			onAddTodoList?.OnNext(todoListData);
			SelectTodoList(todoListData.UniqueID);
		}
	}

	public void RemoveTodoList(ulong uuid)
	{
		if (SaveDataManager.Instance.TodoAllData.TodoListDic.ContainsKey(uuid))
		{
			SaveDataManager.Instance.TodoAllData.RemoveTodoList(uuid);
			if (CurrentTodoListUuid == uuid)
			{
				SelectTodoList(SaveDataManager.Instance.TodoAllData.TodoListDic.First().Key);
			}
			onRemoveTodoList.OnNext(uuid);
		}
	}

	public void ChangeTodoListTitle(ulong uuid, string title)
	{
		if (SaveDataManager.Instance.TodoAllData.TodoListDic.ContainsKey(uuid))
		{
			SaveDataManager.Instance.TodoAllData.SetTitleText(uuid, title);
			onChangeTodoListTitle?.OnNext((uuid, title));
		}
	}

	public void AddTodo()
	{
		TodoData todoData = new TodoData();
		if (SaveDataManager.Instance.TodoAllData.TodoListDic[CurrentTodoListUuid].AddTodo(todoData))
		{
			onAddTodoData?.OnNext((todoData, GetCurrentCompleteTaskCount(), CurrentTodoList.TodoDic.Count()));
		}
	}

	public void RemoveTodo(ulong uuid)
	{
		TodoListData todoListData;
		TodoData todoData = SaveDataManager.Instance.TodoAllData.GetTodoData(uuid, out todoListData);
		if (todoData != null)
		{
			SaveDataManager.Instance.TodoAllData.TodoListDic[CurrentTodoListUuid].RemoveTodo(todoData);
			onRemoveTodoData?.OnNext((uuid, GetCurrentCompleteTaskCount(), CurrentTodoList.TodoDic.Count()));
		}
	}

	public void ChangeTodoTitle(ulong uuid, string title)
	{
		TodoListData todoListData;
		TodoData todoData = SaveDataManager.Instance.TodoAllData.GetTodoData(uuid, out todoListData);
		if (todoData != null && CurrentTodoList.SetInputTextTodo(todoData, title))
		{
			DateTime? completed = todoData.Completed;
			if (completed.HasValue && SaveDataManager.Instance.CalendarData.GetDailyData(completed.Value).CompleteTodoListDic.TryGetValue(uuid, out var value))
			{
				value.TodoText = title;
				SaveDataManager.Instance.CalendarData.SaveDateData(completed.Value);
			}
			onChangeTodoTitle?.OnNext((uuid, title));
		}
	}

	public void SetExpireDate(ulong uuid, DateTime? date)
	{
		TodoListData todoListData;
		TodoData todoData = SaveDataManager.Instance.TodoAllData.GetTodoData(uuid, out todoListData);
		if (todoData != null)
		{
			todoData.SetExpire(date);
			SaveDataManager.Instance.SaveTodoList(todoListData);
			onSetTodoExpireDate?.OnNext((uuid, date));
		}
	}

	public void SetCompleteDate(ulong uuid, DateTime? date)
	{
		TodoListData todoListData;
		TodoData todoData = SaveDataManager.Instance.TodoAllData.GetTodoData(uuid, out todoListData);
		if (todoData == null)
		{
			return;
		}
		DateTime? completed = todoData.Completed;
		todoData.SetCompleteTodoDatetime(date);
		SaveDataManager.Instance.SaveTodoList(todoListData);
		if (completed.HasValue)
		{
			Dictionary<ulong, TodoData> completeTodoListDic = SaveDataManager.Instance.CalendarData.GetDailyData(completed.Value).CompleteTodoListDic;
			if (completeTodoListDic.TryGetValue(uuid, out var value))
			{
				completeTodoListDic.Remove(uuid);
				SaveDataManager.Instance.CalendarData.SaveDateData(completed.Value);
				value.SetCompleteTodoDatetime(date);
				SaveDataManager.Instance.CalendarData.GetDailyData(date.Value).CompleteTodoListDic.TryAdd(todoData.UniqueID, value);
				SaveDataManager.Instance.CalendarData.SaveDateData(date.Value);
			}
			else
			{
				TodoData value2 = todoData.DeepCopy();
				SaveDataManager.Instance.CalendarData.GetDailyData(date.Value).CompleteTodoListDic.TryAdd(todoData.UniqueID, value2);
				SaveDataManager.Instance.CalendarData.SaveDateData(date.Value);
			}
		}
		onSetTodoCompleteDate?.OnNext((uuid, date));
	}

	public void CompleteTodo(ulong uuid)
	{
		TodoListData todoListData;
		TodoData todoData = SaveDataManager.Instance.TodoAllData.GetTodoData(uuid, out todoListData);
		if (todoData != null)
		{
			todoData.SetCompleteTodoDatetime(DateTime.Now);
			SaveDataManager.Instance.SaveTodoList(todoListData);
			TodoData value = todoData.DeepCopy();
			SaveDataManager.Instance.CalendarData.GetDailyData(DateTime.Now).CompleteTodoListDic.TryAdd(todoData.UniqueID, value);
			SaveDataManager.Instance.CalendarData.SaveDateData(DateTime.Now);
			onSetTodoComplete?.OnNext((todoData, GetCurrentCompleteTaskCount(), CurrentTodoList.TodoDic.Count));
		}
	}

	public void UncompleteTodo(ulong uuid)
	{
		TodoListData todoListData;
		TodoData todoData = SaveDataManager.Instance.TodoAllData.GetTodoData(uuid, out todoListData);
		if (todoData != null)
		{
			DateTime? completed = todoData.Completed;
			todoData.Uncomplete();
			SaveDataManager.Instance.SaveTodoList(todoListData);
			if (completed.HasValue)
			{
				SaveDataManager.Instance.CalendarData.GetDailyData(completed.Value).CompleteTodoListDic.Remove(todoData.UniqueID);
				SaveDataManager.Instance.CalendarData.SaveDateData(completed.Value);
			}
			onSetTodoUncomplete?.OnNext((todoData, GetCurrentCompleteTaskCount(), CurrentTodoList.TodoDic.Count));
		}
	}

	public void SwapAfter(ulong target, ulong origin)
	{
		SaveDataManager.Instance.TodoAllData.TodoListDic[CurrentTodoListUuid].SwapTodo(target, origin);
	}
}
