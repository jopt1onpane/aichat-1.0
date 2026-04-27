using System;
using System.Collections.Generic;
using System.Linq;

namespace Bulbul;

public class RoomCalendarController
{
	private const string TASK_FILENAME = "task.es3";

	public void SaveMemo(DateTime date, string memo)
	{
		string calendarDateKey = GetCalendarDateKey(date);
		if (string.IsNullOrWhiteSpace(memo))
		{
			ES3.DeleteKey(calendarDateKey, "task.es3");
		}
		else
		{
			ES3.Save(calendarDateKey, memo, "task.es3");
		}
	}

	public string LoadMemo(DateTime date)
	{
		return ES3.Load(GetCalendarDateKey(date), "task.es3", "");
	}

	public List<TaskES3> LoadTask(DateTime date)
	{
		return ES3.Load(GetTaskDateKey(date), "task.es3", new List<TaskES3>());
	}

	public List<TaskES3> AddTask(DateTime date, TaskES3 task)
	{
		string taskDateKey = GetTaskDateKey(date);
		List<TaskES3> list = ES3.Load(taskDateKey, "task.es3", new List<TaskES3>());
		list.Add(task);
		ES3.Save(taskDateKey, list, "task.es3");
		return ES3.Load(taskDateKey, "task.es3", (List<TaskES3>)null);
	}

	public void DeleteTask(DateTime date, string id)
	{
		string taskDateKey = GetTaskDateKey(date);
		List<TaskES3> list = ES3.Load(taskDateKey, "task.es3", (List<TaskES3>)null);
		if (list != null)
		{
			list.RemoveAll((TaskES3 t) => t.UniqueKey == id);
			if (list.Any())
			{
				ES3.Save(taskDateKey, list, "task.es3");
			}
			else
			{
				ES3.DeleteKey(taskDateKey, "task.es3");
			}
		}
	}

	private string GetCalendarDateKey(DateTime date)
	{
		return $"CALENDAR{date:yyyyMMdd}";
	}

	private string GetTaskDateKey(DateTime date)
	{
		return $"TASK{date:yyyyMMdd}";
	}
}
