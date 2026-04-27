using UnityEngine;

namespace Bulbul.Mobile;

public class CalendarContentsListItemCompletedTaskView : MonoBehaviour
{
	[SerializeField]
	private TodoTaskListItemView _todoListItemView;

	public TodoTaskListItemView TodoListItemView => _todoListItemView;

	public void UpdateView(CalendarCompletedTaskViewModel model, bool isRemovingMode)
	{
		_todoListItemView.CancelRemovingAnimation();
		_todoListItemView.CreateRemovingAnimationCancellatonToken();
		_todoListItemView.Setup(model.UniqueID, model.Title, TodoState.Complete, null, model.CompletedDateTime, isRemovingMode, model.IsHabitTrackerTask, model.HabitTrackerID);
	}

	public void ChangeRemovingMode(bool active)
	{
		_todoListItemView.ChangeRemovingMode(active, isImmediate: false);
	}
}
