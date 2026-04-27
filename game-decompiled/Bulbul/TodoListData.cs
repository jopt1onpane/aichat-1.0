using System;
using System.Collections.Generic;
using GUPS.Obfuscator.Attribute;
using NestopiSystem;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class TodoListData
{
	public ulong UniqueID;

	public string TitleText;

	public Dictionary<ulong, TodoData> TodoDic = new Dictionary<ulong, TodoData>();

	public List<ulong> TodoOrderList = new List<ulong>();

	public TodoListData()
	{
		UniqueID = UniqueIDGenerator.GetNewID();
		TitleText = null;
	}

	public bool AddTodo(TodoData todoData)
	{
		if (TodoDic.ContainsKey(todoData.UniqueID))
		{
			return false;
		}
		TodoDic.Add(todoData.UniqueID, todoData);
		TodoOrderList.Add(todoData.UniqueID);
		SaveDataManager.Instance.SaveTodoList(this);
		return true;
	}

	public void RemoveTodo(TodoData todoData)
	{
		TodoDic.Remove(todoData.UniqueID);
		TodoOrderList.Remove(todoData.UniqueID);
		SaveDataManager.Instance.SaveTodoList(this);
	}

	public void SwapTodo(ulong target, ulong origin)
	{
		Core(TodoOrderList, target, origin);
		SaveDataManager.Instance.SaveTodoList(this);
		static void Core(IList<ulong> list, ulong item, ulong num3)
		{
			int num = list.IndexOf(item);
			if (num >= 0)
			{
				int num2 = 0;
				if (num3 != 0L)
				{
					num2 = list.IndexOf(num3);
				}
				if (num2 >= 0)
				{
					if (num3 != 0L && num > num2)
					{
						num2++;
					}
					list.Move(num, num2);
				}
			}
		}
	}

	public bool SetInputTextTodo(TodoData todoData, string inputText)
	{
		if (!TodoDic.ContainsKey(todoData.UniqueID))
		{
			return false;
		}
		TodoDic[todoData.UniqueID].TodoText = inputText;
		SaveDataManager.Instance.SaveTodoList(this);
		return true;
	}
}
