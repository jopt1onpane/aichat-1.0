using System;

namespace Bulbul.Mobile;

public class TodoTaskListCompleteItemModel : TodoTaskListItemBaseModel
{
	public ulong UniqueID;

	public string Title;

	public DateTime? Expire;

	public DateTime? CompletedDay;

	public bool IsHabit;
}
