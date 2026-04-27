using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class TodoListContentView : MonoBehaviour
{
	private const float SelectorMinSize = 450f;

	private const float SelectorMaxSize = 1090f;

	private const float SelectorPaddingSize = 50f;

	[Header("表示タスクリスト選択")]
	[SerializeField]
	private PulldownListUI _todoListSelector;

	[SerializeField]
	private Canvas _todoListSelectorCanvas;

	[SerializeField]
	private ClickOutsideDetector _selectorOutsideDetector;

	[SerializeField]
	private CanvasGroup _todoListSelectorDeactivateObj;

	[SerializeField]
	private TodoListSelectorListView _todoListSelectorListView;

	[SerializeField]
	private Button _addListButton;

	[Header("タスクの追加 / 削除")]
	[SerializeField]
	private Button _addTaskButton;

	[SerializeField]
	private CanvasGroup _addTaskDeactivate;

	[SerializeField]
	private TextMeshProUGUI _taskCountText;

	[SerializeField]
	private ToggleStyleButton _removeModeToggle;

	[SerializeField]
	private Button _switchRemoveModeButton;

	[SerializeField]
	private Button _switchNormalModeButton;

	[SerializeField]
	private GameObject _raycastBlocker;

	private TodoListUIModel todoListUIModel;

	private Subject<bool> onChangeRemoveMode = new Subject<bool>();

	private Subject<Unit> onAddTask = new Subject<Unit>();

	private Subject<ulong> onSelectTodoList = new Subject<ulong>();

	private Subject<Unit> onAddTodoList = new Subject<Unit>();

	private Subject<ulong> onRemoveTodoList = new Subject<ulong>();

	private Subject<(ulong uuid, string title)> onChangeTodoListTitle = new Subject<(ulong, string)>();

	public Observable<bool> OnChangeRemoveMode => onChangeRemoveMode;

	public Observable<Unit> OnAddTask => onAddTask;

	public Observable<ulong> OnSelectTodoList => onSelectTodoList;

	public Observable<Unit> OnAddTodoList => onAddTodoList;

	public Observable<ulong> OnRemoveTodoList => onRemoveTodoList;

	public Observable<(ulong uuid, string title)> OnChangeTodoListTitle => onChangeTodoListTitle;

	private void OnDestroy()
	{
		onChangeRemoveMode?.Dispose();
		onAddTask?.Dispose();
		onSelectTodoList?.Dispose();
		onAddTodoList?.Dispose();
		onRemoveTodoList?.Dispose();
		onChangeTodoListTitle?.Dispose();
	}

	private void OnDisable()
	{
		_todoListSelector.ClosePullDown(immediate: true);
	}

	public void Setup(TodoListUIModel model)
	{
		todoListUIModel = model;
		_raycastBlocker.SetActive(value: false);
		_todoListSelector.Setup();
		ObservableSubscribeExtensions.Subscribe(_todoListSelector.OnClickPullDownButton, delegate
		{
			_todoListSelectorCanvas.overrideSorting = true;
			_todoListSelectorCanvas.sortingOrder = 2;
			_raycastBlocker.SetActive(value: true);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_todoListSelector.OnClosePulldownComplete, delegate
		{
			_todoListSelectorCanvas.overrideSorting = false;
			_todoListSelectorCanvas.sortingOrder = 0;
			_raycastBlocker.SetActive(value: false);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_selectorOutsideDetector.OnClickOutside, delegate
		{
			if (_todoListSelectorListView.Gate.CurrentValue && _todoListSelector.IsOpen)
			{
				_todoListSelector.ClosePullDown();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_addListButton.OnClickAsObservable(), delegate
		{
			AddTodoListNotify();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_addTaskButton.OnClickAsObservable(), delegate
		{
			AddTodoTaskNotify();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_switchRemoveModeButton.OnClickAsObservable(), delegate
		{
			ChangeRemoveModeNotify(isRemoveMode: true);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_switchNormalModeButton.OnClickAsObservable(), delegate
		{
			ChangeRemoveModeNotify(isRemoveMode: false);
		}).AddTo(this);
		_todoListSelectorListView.OnClickSelect.Subscribe(delegate(ulong uuid)
		{
			_todoListSelector.ClosePullDown();
			SelectTodoListNotify(uuid);
		}).AddTo(this);
		_todoListSelectorListView.OnClickRemove.Subscribe(delegate(ulong uuid)
		{
			RemoveTodoListNotify(uuid);
		}).AddTo(this);
		_todoListSelectorListView.OnValueChangeTitle.Subscribe(delegate((ulong uuid, string title) info)
		{
			OnValueChangedTodoListTitle(info.uuid, info.title);
		}).AddTo(this);
		_todoListSelectorListView.OnEndEditTitle.Subscribe(delegate((ulong uuid, string title) info)
		{
			EndEditTodoListTitleNotify(info.uuid, info.title);
		}).AddTo(this);
	}

	public void EnterSettings(TodoListUIModel model)
	{
		todoListUIModel = model;
		_todoListSelector.ChangeSelectContentText(model.CurrentTodoList.TitleText);
		_todoListSelectorListView.EnterSettings(model);
		TodoSelectorPullDownResize();
		ChangeTodoRemoveMode(TodoListUIModel.IsTaskRemoveMode);
		OnUpdateTaskCount(model.GetCurrentCompleteTaskCount(), model.CurrentTodoList.TodoDic.Count);
	}

	public void TodoSelectorPullDownResize()
	{
		float num = 0f;
		float y = (_todoListSelector.transform as RectTransform).sizeDelta.y;
		float y2 = (_addListButton.transform as RectTransform).sizeDelta.y;
		float num2 = _todoListSelectorListView.Parameters.DefaultItemSize * (float)_todoListSelectorListView.DataCount;
		num = y + y2 + num2 + 50f;
		if (num < 450f)
		{
			num = 450f;
		}
		else if (num > 1090f)
		{
			num = 1090f;
		}
		_todoListSelector.SetTargetPullDownSizeDelta(num);
		if (_todoListSelector.IsOpen)
		{
			_todoListSelector.OpenPullDown();
		}
	}

	public void OnUpdateTaskCount(int completeTaskCount, int allTaskCount)
	{
		_taskCountText.SetText($"{completeTaskCount}/{allTaskCount}");
		bool flag = allTaskCount > 0;
		_removeModeToggle.gameObject.SetActive(flag);
		if (!flag && TodoListUIModel.IsTaskRemoveMode)
		{
			ChangeRemoveModeNotify(isRemoveMode: false);
		}
	}

	public void SelectTodoList(ulong uuid)
	{
		if (todoListUIModel.TodoListDic.ContainsKey(uuid))
		{
			_todoListSelector.ChangeSelectContentText(todoListUIModel.TodoListDic[uuid].TitleText);
			OnUpdateTaskCount(todoListUIModel.GetCurrentCompleteTaskCount(), todoListUIModel.CurrentTodoList.TodoDic.Count);
		}
		_todoListSelectorListView.SelectedTodoListItem(uuid);
	}

	public void AddTodoList(TodoListData data)
	{
		_todoListSelectorListView.AddTodoListItem(data);
		TodoSelectorPullDownResize();
	}

	public void RemoveTodoList(ulong uuid)
	{
		_todoListSelectorListView.RemoveTodoListItem(uuid);
		TodoSelectorPullDownResize();
	}

	public void ChangeTodoListTitle(ulong uuid, string title)
	{
		_todoListSelectorListView.ChangeTodoListTitle(uuid, title);
	}

	public void ChangeTodoRemoveMode(bool isRemoveMode)
	{
		_removeModeToggle.SetToggleWithoutTransition(isRemoveMode, isNotify: false);
		_todoListSelector.SetPullDownButtonInteractable(!isRemoveMode);
		_todoListSelectorDeactivateObj.gameObject.SetActive(isRemoveMode);
		_todoListSelectorDeactivateObj.alpha = (isRemoveMode ? 1 : 0);
		_addTaskButton.enabled = !isRemoveMode;
		_addTaskButton.interactable = !isRemoveMode;
		_addTaskDeactivate.gameObject.SetActive(isRemoveMode);
		_addTaskDeactivate.alpha = (isRemoveMode ? 1 : 0);
	}

	private void AddTodoListNotify()
	{
		onAddTodoList?.OnNext(Unit.Default);
	}

	private void AddTodoTaskNotify()
	{
		onAddTask?.OnNext(Unit.Default);
	}

	private void ChangeRemoveModeNotify(bool isRemoveMode)
	{
		onChangeRemoveMode?.OnNext(isRemoveMode);
	}

	private void SelectTodoListNotify(ulong uuid)
	{
		onSelectTodoList.OnNext(uuid);
	}

	private void RemoveTodoListNotify(ulong uuid)
	{
		onRemoveTodoList.OnNext(uuid);
	}

	private void OnValueChangedTodoListTitle(ulong uuid, string title)
	{
		if (todoListUIModel.CurrentTodoListUuid == uuid)
		{
			_todoListSelector.ChangeSelectContentText(title);
		}
	}

	private void EndEditTodoListTitleNotify(ulong uuid, string title)
	{
		onChangeTodoListTitle?.OnNext((uuid, title));
	}
}
