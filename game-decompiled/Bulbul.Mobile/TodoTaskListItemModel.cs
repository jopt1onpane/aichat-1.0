using System;

namespace Bulbul.Mobile;

public class TodoTaskListItemModel : TodoTaskListItemBaseModel
{
	public ulong UniqueID;

	public string Title;

	public DateTime? Expire;

	public bool IsHabit;

	public bool IsCreateNew;
}
