namespace Bulbul.Mobile;

public class TodoListSelectorListItemModel
{
	public ulong UniqueId;

	public string Title;

	public bool IsCurrentSelect;

	public float LastClickTime;

	public bool IsCreateNew;

	public TodoListSelectorListItemModel(ulong currentUuid, TodoListData data, bool isCreateNew = false)
	{
		UniqueId = data.UniqueID;
		Title = data.TitleText;
		IsCurrentSelect = currentUuid == data.UniqueID;
		LastClickTime = -1f;
		IsCreateNew = isCreateNew;
	}
}
