using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class TodoListUIViewMobile : MonoBehaviour, ITabChangedSetuper
{
	[SerializeField]
	[Header("Todoリスト上部のView")]
	private TodoListContentView _todoListContentView;

	[SerializeField]
	[Header("TodoリストのタスクリストView")]
	private TodoTaskListView _todoTaskListView;

	[SerializeField]
	[Header("カレンダーUI")]
	private CalendarDateTimeSettingView _calendarView;

	private Subject<bool> onPrepareContent = new Subject<bool>();

	public Observable<bool> OnPrepareContent => onPrepareContent;

	public TodoListContentView TodoListContentView => _todoListContentView;

	public TodoTaskListView TodoTaskListView => _todoTaskListView;

	public CalendarDateTimeSettingView CalendarView => _calendarView;

	void ITabChangedSetuper.SetupBeforeTabChanged(bool isChangedFromTab)
	{
		onPrepareContent?.OnNext(isChangedFromTab);
	}

	private void OnDestroy()
	{
		onPrepareContent?.Dispose();
	}

	public void Setup(TodoListUIModel model)
	{
		_todoListContentView.Setup(model);
		_calendarView.Setup();
	}
}
