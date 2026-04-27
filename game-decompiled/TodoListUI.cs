using System;
using System.Collections.Generic;
using System.Linq;
using Bulbul;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

public class TodoListUI : MonoBehaviour
{
	private class ReorderInfo
	{
		public readonly TodoUI Button;

		public readonly int OriginIndex;

		public ReorderInfo(TodoUI button, int originIndex)
		{
			Button = button;
			OriginIndex = originIndex;
		}
	}

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private ChangeOrderService _changeOrderService;

	private float CompleteListCloseWidth = -3.2f;

	private float CompleteListOpenWidth = 364.2f;

	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[SerializeField]
	private PulldownListUI _pulldown;

	[SerializeField]
	[Header("TodoListUI親オブジェクト")]
	private GameObject _todoListUIParent;

	[SerializeField]
	[Header("TodoListUI用プレハブ")]
	private SelectTodoListUI _todoListUIPrefab;

	[SerializeField]
	private Button addTodoButton;

	[SerializeField]
	[Header("Todoタイトルテキスト")]
	private TextLocalizationBehaviour _titleTextLocalizationBehavior;

	[SerializeField]
	[Header("未完了Todo親オブジェクト")]
	private GameObject _uncompleteTodoUIParent;

	[SerializeField]
	[Header("TodoUI用プレハブ TodoUIを持つオブジェクトを設定")]
	private GameObject _todoUIPrefab;

	[SerializeField]
	[Header("完了済みTodoセクション全体")]
	private GameObject _completedSection;

	[SerializeField]
	[Header("完了済みTodo親オブジェクト")]
	private GameObject _completeTodoUIParent;

	[SerializeField]
	[Header("完了済みセクション折りたたみボタン")]
	private Button _toggleCompletedSectionButton;

	[SerializeField]
	[Header("完了済みセクションコンテンツ（折りたたみ対象）")]
	private RectTransform _completedSectionContent;

	[SerializeField]
	[Header("完了リスト開閉ボタンアニメーション")]
	private Animator _completeListButtonAnimator;

	[SerializeField]
	[Header("Todoの残数テキスト")]
	private TextMeshProUGUI _remainTodoCountText;

	[SerializeField]
	[Header("UIドラッグ用")]
	private DraggableUI _draggableUI;

	[SerializeField]
	private CalenderCoreUI calenderUI;

	[SerializeField]
	private TodoUI dummyButton;

	[SerializeField]
	[Header("TodoリストScrollRect")]
	private ScrollRect _todoListScrollRect;

	[SerializeField]
	[Header("TodoScrollRect")]
	private ScrollRect _todoScrollRect;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	[Header("TodoList追加ボタン")]
	private Button _addTodoListButton;

	private ReorderInfo reorderInfo;

	private readonly Subject<(ulong listID, ulong target, ulong origin)> onSwapAfter = new Subject<(ulong, ulong, ulong)>();

	private List<SelectTodoListUI> _selectTodoListUIList = new List<SelectTodoListUI>();

	private InteractableUI _currentInteractableTodoListUI;

	private List<TodoUI> _uncompleteTodoUIList = new List<TodoUI>();

	private List<TodoUI> _completeTodoUIList = new List<TodoUI>();

	private Stack<TodoUI> _completeTodoUIHideStack = new Stack<TodoUI>();

	private bool _isOpenCompleteList;

	private Tween _openOrCloseCompleteListTween;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private bool _isActive;

	private Tween openTween;

	private readonly Vector3[] worldCorners = new Vector3[4];

	public Observable<(ulong listID, ulong target, ulong origin)> OnSwapAfter => onSwapAfter;

	public List<SelectTodoListUI> SelectTodoListUIList => _selectTodoListUIList;

	public ulong CurrentTodoListID { get; private set; }

	private RectTransform rectTransform
	{
		get
		{
			if (!_rectTransform)
			{
				_rectTransform = base.transform as RectTransform;
			}
			return _rectTransform;
		}
	}

	public CalenderCoreUI CalenderUI => calenderUI;

	public int SelectedTodoListUIListCount => SelectTodoListUIList.Count;

	public Observable<Unit> OnCalendarDisable => calenderUI.OnDisableAsObservable();

	public Observable<SelectCalendarDayUI> OnCalendarSelectDay => calenderUI.OnSelectedDay;

	public Observable<Unit> OnClickCalendarOutside => calenderUI.OnClickOutside;

	public Observable<Unit> OnClickCloseButton => _closeButton.OnClickAsObservable();

	public Observable<Unit> OnClickAddTodoButton => addTodoButton.OnClickAsObservable();

	public Observable<Unit> OnClickAddTodoListButton => _addTodoListButton.OnClickAsObservable();

	public void Setup()
	{
		_draggableUI.Setup();
		calenderUI.Setup(isValidWorkCheck: false, resetOnDisable: true);
		_pulldown.Setup();
		_rectTransform = base.transform as RectTransform;
		_titleTextLocalizationBehavior.Set("ui_todo_list_title");
		_toggleCompletedSectionButton?.onClick.AddListener(OnClickButtonOpenOrCloseCompleteList);
		CloseCompleteList(isImmediate: true);
		dummyButton.Init();
	}

	public void UpdateUI()
	{
		if (_draggableUI.IsDrag())
		{
			Graphic component = base.gameObject.GetComponent<Graphic>();
			Vector3 position = _draggableUI.CalculateDragPos(component, base.transform.position);
			base.transform.position = position;
		}
	}

	public bool IsActive()
	{
		return _isActive;
	}

	public void Activate()
	{
		_changeOrderService.BringToFront(ChangeOrderService.OrderItemType.Todo);
		_isActive = true;
		CloseTodoListPullDown(immediate: true);
		_facilityOpenButton.ActivateUseUI();
		base.gameObject.SetActive(value: true);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		InitializePosition();
		float endValue = 10f;
		_moveTween = _rectTransform.DOAnchorPosY(endValue, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
		CloseCompleteList(isImmediate: true);
	}

	public void Deactivate()
	{
		_isActive = false;
		_facilityOpenButton.DeactivateUseUI();
		_moveTween?.Kill();
		_fadeTween?.Kill();
		float endValue = _rectTransform.anchoredPosition.y + -8f;
		_moveTween = _rectTransform.DOAnchorPosY(endValue, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	public void InitializePosition()
	{
		rectTransform.anchoredPosition = new Vector3(-1213f, 2f, 0f);
	}

	public void OpenTodoListPullDown()
	{
		_pulldown.OpenPullDown();
	}

	public void CloseTodoListPullDown(bool immediate = false)
	{
		_pulldown.ClosePullDown(immediate);
	}

	public void AddTodoListUI(TodoListData todoListData, Action<ulong, InteractableUI> onSelectPage, Action<ulong, string> onValueChangedTitleAction, Action<ulong, string> onChangedTitle, Action<ulong> onDeletePageAction)
	{
		GameObject obj = UnityEngine.Object.Instantiate(_todoListUIPrefab.gameObject, _todoListUIParent.transform, worldPositionStays: false);
		Vector3 localPosition = obj.transform.localPosition;
		localPosition.z = 0f;
		obj.transform.localPosition = localPosition;
		SelectTodoListUI component = obj.GetComponent<SelectTodoListUI>();
		component.Setup(todoListData, onSelectPage, onValueChangedTitleAction, onChangedTitle, onDeletePageAction);
		_selectTodoListUIList.Add(component);
		component.OnClickButtonSelectTodoList();
		component.OnClickButtonEditTitleText();
		float endValue = 0f;
		_todoListScrollRect.DOVerticalNormalizedPos(endValue, 0.5f);
	}

	public void RemoveSelectTodoListUI(ulong todoListID)
	{
		SelectTodoListUI selectTodoListUI = null;
		for (int i = 0; i < _selectTodoListUIList.Count; i++)
		{
			if (_selectTodoListUIList[i].TodoListID == todoListID)
			{
				selectTodoListUI = _selectTodoListUIList[i];
			}
		}
		if (selectTodoListUI != null)
		{
			selectTodoListUI.Destroy();
			_selectTodoListUIList.Remove(selectTodoListUI);
		}
		else
		{
			Debug.LogError($"Todoリストの削除：指定されたID{todoListID}が見つかりませんでした。");
		}
	}

	public void SelectFirstList()
	{
		_selectTodoListUIList[0].OnClickButtonSelectTodoList();
	}

	public void EnableFirstListRemoveButton()
	{
		_selectTodoListUIList[0].EnableRemoveButton();
	}

	public void DisableFirstListRemoveButton()
	{
		_selectTodoListUIList[0].DisableRemoveButton();
	}

	public void OnSelectTodoListUI(TodoListData todoListData, InteractableUI interactableUI)
	{
		CurrentTodoListID = todoListData.UniqueID;
		_pulldown.ChangeSelectContentText(todoListData.TitleText);
		for (int num = _uncompleteTodoUIList.Count - 1; num >= 0; num--)
		{
			_uncompleteTodoUIList[num].Destroy();
			_uncompleteTodoUIList.RemoveAt(num);
		}
		for (int num2 = _completeTodoUIList.Count - 1; num2 >= 0; num2--)
		{
			_completeTodoUIList[num2].Destroy();
			_completeTodoUIList.RemoveAt(num2);
		}
		while (_completeTodoUIHideStack.Count != 0)
		{
			_completeTodoUIHideStack.Pop().Destroy();
		}
		_currentInteractableTodoListUI?.DeactivateUseUI();
		interactableUI?.ActivateUseUI();
		_currentInteractableTodoListUI = interactableUI;
	}

	public void OnValueChangedTitleText(ulong pageID, string inputText)
	{
		if (pageID == CurrentTodoListID)
		{
			_pulldown.ChangeSelectContentText(inputText);
		}
	}

	private void OnClickButtonOpenOrCloseCompleteList()
	{
		if (_isOpenCompleteList)
		{
			CloseCompleteList();
		}
		else
		{
			OpenCompleteList();
		}
	}

	private void OpenCompleteList(bool isImmediate = false)
	{
		_openOrCloseCompleteListTween?.Kill();
		Vector2 sizeDelta = _completedSectionContent.sizeDelta;
		if (isImmediate)
		{
			_completedSectionContent.sizeDelta = new Vector2(CompleteListOpenWidth, _completedSectionContent.sizeDelta.y);
			_isOpenCompleteList = true;
		}
		else
		{
			_openOrCloseCompleteListTween = _completedSectionContent.DOSizeDelta(new Vector2(CompleteListOpenWidth, sizeDelta.y), 0.2f).OnComplete(delegate
			{
				_isOpenCompleteList = true;
			});
		}
	}

	private void CloseCompleteList(bool isImmediate = false)
	{
		_openOrCloseCompleteListTween?.Kill();
		Vector2 sizeDelta = _completedSectionContent.sizeDelta;
		if (isImmediate)
		{
			_completedSectionContent.sizeDelta = new Vector2(CompleteListCloseWidth, _completedSectionContent.sizeDelta.y);
			_isOpenCompleteList = false;
		}
		else
		{
			_openOrCloseCompleteListTween = _completedSectionContent.DOSizeDelta(new Vector2(CompleteListCloseWidth, sizeDelta.y), 0.2f).OnComplete(delegate
			{
				_isOpenCompleteList = false;
			});
		}
	}

	public TodoUI AddUncompleteTodoUI(TodoData todoData, Action<TodoData, string> onChangeTodoText, Action<TodoData> onDeleteTodoAction)
	{
		TodoUI component = UnityEngine.Object.Instantiate(_todoUIPrefab, _uncompleteTodoUIParent.transform, worldPositionStays: false).GetComponent<TodoUI>();
		Vector3 localPosition = component.Transform.localPosition;
		localPosition.z = 0f;
		component.Transform.localPosition = localPosition;
		component.Setup(todoData, onChangeTodoText, onDeleteTodoAction);
		component.OnStartReorder.Subscribe(delegate((TodoUI button, PointerEventData eventData) x)
		{
			OnStartReorder(x.button, x.eventData);
		}).AddTo(this);
		component.OnReorderDrag.Subscribe(delegate((TodoUI button, PointerEventData eventData) x)
		{
			OnDragReorder(x.button, x.eventData);
		}).AddTo(this);
		component.OnEndReorder.Subscribe(delegate((TodoUI button, PointerEventData eventData) x)
		{
			OnEndReorder(x.button, x.eventData);
		}).AddTo(this);
		component.FinishDragSubscribe();
		_uncompleteTodoUIList.Add(component);
		component.EditMainText();
		float endValue = 0f;
		_todoScrollRect.DOVerticalNormalizedPos(endValue, 0.5f);
		return component;
	}

	public TodoUI AddCompleteTodoUI(TodoData todoData, Action<TodoData, string> onChangeTodoText, Action<TodoData> onDeleteTodoAction)
	{
		TodoUI component = UnityEngine.Object.Instantiate(_todoUIPrefab, _completeTodoUIParent.transform, worldPositionStays: false).GetComponent<TodoUI>();
		Vector3 localPosition = component.Transform.localPosition;
		localPosition.z = 0f;
		component.Transform.localPosition = localPosition;
		component.Setup(todoData, onChangeTodoText, onDeleteTodoAction);
		_completeTodoUIList.Insert(0, component);
		UpdateCompletedTaskListHideAndShowUI();
		return component;
	}

	public TodoUI AddTodoUI(TodoData todoData, Action<TodoData, string> onChangeTodoText, Action<TodoData> onDeleteTodoAction)
	{
		if (todoData.CurrentState == TodoState.Working)
		{
			return AddUncompleteTodoUI(todoData, onChangeTodoText, onDeleteTodoAction);
		}
		return AddCompleteTodoUI(todoData, onChangeTodoText, onDeleteTodoAction);
	}

	private void OnStartReorder(TodoUI button, PointerEventData eventData)
	{
		if (button.IsPossibleSwap && reorderInfo == null)
		{
			reorderInfo = new ReorderInfo(button, button.Transform.GetSiblingIndex());
			dummyButton.SetupForDummy(button.TodoData);
			dummyButton.gameObject.SetActive(value: true);
			button.Hide();
		}
	}

	private void OnDragReorder(TodoUI button, PointerEventData eventData)
	{
		if (reorderInfo == null || reorderInfo.Button != button)
		{
			return;
		}
		Vector3 position = dummyButton.transform.position;
		position.y = eventData.position.y;
		dummyButton.transform.position = position;
		int num = -1;
		for (int i = 0; i < _uncompleteTodoUIParent.transform.childCount; i++)
		{
			Transform child = _uncompleteTodoUIParent.transform.GetChild(i);
			if (!(dummyButton.transform.position.y + (dummyButton.transform as RectTransform).rect.height * 0.5f < child.position.y))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			button.Transform.SetAsLastSibling();
		}
		else
		{
			button.Transform.SetSiblingIndex(num);
		}
	}

	private void OnEndReorder(TodoUI button, PointerEventData eventData)
	{
		if (reorderInfo != null && !(reorderInfo.Button != button))
		{
			dummyButton.gameObject.SetActive(value: false);
			dummyButton.TidyingEndDrag(isUseDoComplete: true);
			int originIndex = reorderInfo.OriginIndex;
			int siblingIndex = button.Transform.GetSiblingIndex();
			reorderInfo = null;
			button.Show();
			if (originIndex != siblingIndex)
			{
				ulong item = ((siblingIndex == 0) ? 0 : _uncompleteTodoUIParent.transform.GetChild(siblingIndex - 1).GetComponent<TodoUI>().TodoData.UniqueID);
				onSwapAfter.OnNext((CurrentTodoListID, button.TodoData.UniqueID, item));
			}
		}
	}

	public void DeleteTodo(TodoData todoData)
	{
		TodoUI todoUI = null;
		bool flag = false;
		foreach (TodoUI uncompleteTodoUI in _uncompleteTodoUIList)
		{
			if (uncompleteTodoUI.TodoData == todoData)
			{
				todoUI = uncompleteTodoUI;
				flag = true;
				break;
			}
		}
		if (todoUI == null)
		{
			foreach (TodoUI completeTodoUI in _completeTodoUIList)
			{
				if (completeTodoUI.TodoData == todoData)
				{
					todoUI = completeTodoUI;
					break;
				}
			}
		}
		if (todoUI != null)
		{
			todoUI.Destroy();
			if (flag)
			{
				_uncompleteTodoUIList.Remove(todoUI);
				return;
			}
			_completeTodoUIList.Remove(todoUI);
			UpdateCompletedTaskListHideAndShowUI();
		}
	}

	public async UniTask DeleteTodoAsync(TodoData todoData)
	{
		bool isUncomplete = false;
		TodoUI ui = _uncompleteTodoUIList.FirstOrDefault((TodoUI x) => x.TodoData == todoData);
		if (ui != null)
		{
			isUncomplete = true;
		}
		else
		{
			ui = _completeTodoUIList.FirstOrDefault((TodoUI x) => x.TodoData == todoData);
		}
		if (!(ui == null))
		{
			RectTransform obj = ui.Transform as RectTransform;
			await obj.DOSizeDelta(new Vector2(obj.sizeDelta.x, 0f), 0.1f).ToUniTask();
			ui.Destroy();
			if (isUncomplete)
			{
				_uncompleteTodoUIList.Remove(ui);
				return;
			}
			_completeTodoUIList.Remove(ui);
			UpdateCompletedTaskListHideAndShowUI();
		}
	}

	public void UpdateTodoCountText(ulong currentTodoListID)
	{
		int num = 0;
		Dictionary<ulong, TodoData> todoDic = SaveDataManager.Instance.TodoAllData.TodoListDic[currentTodoListID].TodoDic;
		foreach (KeyValuePair<ulong, TodoData> item in todoDic)
		{
			if (item.Value.CurrentState == TodoState.Complete)
			{
				num++;
			}
		}
		_remainTodoCountText.text = num + "/" + todoDic.Count;
	}

	public void OpenCalender(DateTime? dateTime)
	{
		DateTime dateTime2 = dateTime ?? DateTime.Now;
		calenderUI.View(dateTime2.Year, dateTime2.Month);
		calenderUI.gameObject.SetActive(value: true);
		openTween?.Kill(complete: true);
		RectTransform rectTransform = calenderUI.transform.parent.transform as RectTransform;
		calenderUI.transform.localPosition = Vector3.zero;
		if (rectTransform.anchoredPosition.x < 0f)
		{
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x * -1f, rectTransform.anchoredPosition.y);
		}
		CanvasScaler componentInParent = calenderUI.GetComponentInParent<CanvasScaler>();
		Vector3 lossyScale = componentInParent.transform.lossyScale;
		RectTransform component = componentInParent.GetComponent<RectTransform>();
		calenderUI.RectTransform.GetWorldCorners(worldCorners);
		float x = worldCorners[0].x;
		float x2 = worldCorners[2].x;
		float y = worldCorners[0].y;
		float y2 = worldCorners[2].y;
		component.GetWorldCorners(worldCorners);
		float x3 = worldCorners[0].x;
		float x4 = worldCorners[2].x;
		float y3 = worldCorners[0].y;
		float y4 = worldCorners[2].y;
		Vector2 anchoredPosition = calenderUI.RectTransform.anchoredPosition;
		float num = y2 - y4;
		float num2 = y3 - y;
		Vector3 localScale = rectTransform.localScale;
		if (num > 0f)
		{
			anchoredPosition.y -= num / (lossyScale.y * localScale.y);
		}
		else if (num2 > 0f)
		{
			anchoredPosition.y += num2 / (lossyScale.y * localScale.y);
		}
		calenderUI.RectTransform.anchoredPosition = anchoredPosition;
		Vector2 anchoredPosition2 = rectTransform.anchoredPosition;
		bool num3 = x < x3;
		bool flag = x2 > x4;
		if (num3 && anchoredPosition2.x < 0f)
		{
			anchoredPosition2.x *= -1f;
			rectTransform.anchoredPosition = anchoredPosition2;
		}
		else if (flag && anchoredPosition2.x > 0f)
		{
			anchoredPosition2.x *= -1f;
			rectTransform.anchoredPosition = anchoredPosition2;
		}
		calenderUI.transform.localScale = Vector3.one * 0.7f;
		openTween = calenderUI.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
	}

	public void CloseCalender()
	{
		calenderUI.gameObject.SetActive(value: false);
	}

	public TodoUI GetTodoUIFromData(TodoData todoData)
	{
		foreach (TodoUI uncompleteTodoUI in _uncompleteTodoUIList)
		{
			if (uncompleteTodoUI.TodoData == todoData)
			{
				return uncompleteTodoUI;
			}
		}
		foreach (TodoUI completeTodoUI in _completeTodoUIList)
		{
			if (completeTodoUI.TodoData == todoData)
			{
				return completeTodoUI;
			}
		}
		return null;
	}

	public async UniTask PlayAddCompleteTodoAnim()
	{
		await UniTask.Delay(TimeSpan.FromSeconds(0.23899999260902405));
		_completeListButtonAnimator.Play("CompleteListAddCell", 0, 0f);
	}

	public async UniTask MoveTodoUIToCompleted(TodoUI todoUI)
	{
		if (_uncompleteTodoUIList.Contains(todoUI))
		{
			PlayAddCompleteTodoAnim().Forget();
			await todoUI.SlideOutFromUnComplete();
			_uncompleteTodoUIList.Remove(todoUI);
			_completeTodoUIList.Add(todoUI);
			todoUI.Transform.SetParent(_completeTodoUIParent.transform, worldPositionStays: false);
			todoUI.Transform.SetAsFirstSibling();
			await todoUI.SlideInToComplete();
			UpdateCompletedTaskListHideAndShowUI();
		}
	}

	public async UniTask MoveTodoUIToUncomplete(TodoUI todoUI)
	{
		if (!_completeTodoUIList.Contains(todoUI))
		{
			return;
		}
		await todoUI.SlideOutFromComplete();
		_completeTodoUIList.Remove(todoUI);
		_uncompleteTodoUIList.Add(todoUI);
		todoUI.Transform.SetParent(_uncompleteTodoUIParent.transform, worldPositionStays: false);
		todoUI.Transform.SetAsFirstSibling();
		if (!todoUI.IsSubscribedDragEvent)
		{
			todoUI.OnStartReorder.Subscribe(delegate((TodoUI button, PointerEventData eventData) x)
			{
				OnStartReorder(x.button, x.eventData);
			}).AddTo(this);
			todoUI.OnReorderDrag.Subscribe(delegate((TodoUI button, PointerEventData eventData) x)
			{
				OnDragReorder(x.button, x.eventData);
			}).AddTo(this);
			todoUI.OnEndReorder.Subscribe(delegate((TodoUI button, PointerEventData eventData) x)
			{
				OnEndReorder(x.button, x.eventData);
			}).AddTo(this);
			todoUI.FinishDragSubscribe();
		}
		await todoUI.SlideInToUnComplete();
		UpdateCompletedTaskListHideAndShowUI();
	}

	private void UpdateCompletedTaskListHideAndShowUI()
	{
		int count = _completeTodoUIList.Count;
		if (count > 50)
		{
			TodoUI todoUI = _completeTodoUIList[0];
			_completeTodoUIList.RemoveAt(0);
			todoUI.SetActive(active: false);
			_completeTodoUIHideStack.Push(todoUI);
		}
		else if (_completeTodoUIHideStack.Count > 0 && count < 50)
		{
			TodoUI todoUI2 = _completeTodoUIHideStack.Pop();
			todoUI2.SetActive(active: true);
			_completeTodoUIList.Insert(0, todoUI2);
		}
	}

	public void SelectFirstTodoList()
	{
		SelectTodoListUIList[0].OnClickButtonSelectTodoList();
	}
}
