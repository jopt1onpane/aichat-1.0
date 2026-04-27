using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class TodoAllData
{
	public Dictionary<ulong, TodoListData> TodoListDic = new Dictionary<ulong, TodoListData>();

	public bool AddTodoList(TodoListData todoListData)
	{
		if (TodoListDic.ContainsKey(todoListData.UniqueID))
		{
			Debug.LogError("すでに同じIDのTodoListが登録されています！");
			return false;
		}
		TodoListDic.Add(todoListData.UniqueID, todoListData);
		SaveDataManager.Instance.SaveTodoList(todoListData);
		return true;
	}

	public void RemoveTodoList(ulong todoListID)
	{
		TodoListDic.Remove(todoListID);
		SaveDataManager.Instance.DeleteTodoList(todoListID);
	}

	public void SetTitleText(ulong todoListID, string titleText)
	{
		if (TodoListDic.TryGetValue(todoListID, out var value))
		{
			value.TitleText = titleText;
			SaveDataManager.Instance.SaveTodoList(value);
		}
	}

	public TodoData GetTodoData(ulong uniqueID, out TodoListData todoListData)
	{
		todoListData = null;
		foreach (TodoListData value2 in TodoListDic.Values)
		{
			if (value2.TodoDic.TryGetValue(uniqueID, out var value) && value != null)
			{
				todoListData = value2;
				return value;
			}
		}
		return null;
	}
}
