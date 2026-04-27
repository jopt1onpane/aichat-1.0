using System;
using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyUtil;
using NestopiSystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TodoUI : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	private enum MainState
	{
		ValueChanged,
		OnStayPointer,
		Remove,
		Complete
	}

	private const string CheckMarkAnimNameDelete = "DeleteTodoTaskText";

	private const string CheckMarkAnimNameDeleteEnd = "DeleteTodoTaskTextEnd";

	private const string CheckMarkAnimNameInputTodoTask = "InputTodoTask";

	private const string CheckMarkAnimNameInputTodoTaskEnd = "InputTodoTaskEnd";

	private const string CellAnimCompleteTask = "CompleteTask";

	private const string CellAnimCompleteTaskSlideIn = "CompleteTaskSlideIn";

	private const string CellAnimUnCompleteTaskSlideIn = "UnCompleteTaskSlideIn";

	private const string CellAnimUnCompleteTaskSlideOut = "UnCompleteTaskSlideOut";

	private const string CellAnimCompleteTaskSlideOut = "CompleteTaskSlideOut";

	private const string CellAnimUnCompleteTask = "UnCompleteTask";

	private const float AnimDuration = 0.37f;

	private MainState _mainState;

	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private Image completeBack;

	[SerializeField]
	private Animator _checkMarkAnimator;

	[SerializeField]
	private Image _checkMarkBackImage;

	[SerializeField]
	private Image completeCheckMark;

	[SerializeField]
	private ButtonEventObservable expireButton;

	[SerializeField]
	private TooltipTarget _expireTipTarget;

	[SerializeField]
	private Image expireIcon;

	[SerializeField]
	private Animator _cellAnimator;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private EventTrigger reorderTrigger;

	[SerializeField]
	[Header("背景")]
	private Image _backImage;

	[SerializeField]
	[Header("通常時の背景")]
	private Sprite _backBaseSprite;

	[SerializeField]
	[Header("マウスオーバー時の背景")]
	private Sprite _backMouseOverSprite;

	[SerializeField]
	private HoldButtonAnimation _baseHoldButtonAnim;

	[SerializeField]
	[Header("ドラッグ")]
	private CanvasGroup _dragUICanvasGroup;

	[SerializeField]
	[Header("ドラッグ")]
	private InteractableUI _dragInteractableUI;

	[SerializeField]
	[Header("ドラッグ")]
	private HoldButtonAnimation _dragHoldButtonAnim;

	[SerializeField]
	[Header("削除ボタン")]
	private GameObject _todoRemoveButton;

	[SerializeField]
	[Header("削除ボタン")]
	private CanvasGroup _deleteUICanvasGroup;

	private bool _isAnimationCompleted = true;

	private string _currentCellAnimName = "";

	private CancellationTokenSource _cts = new CancellationTokenSource();

	private TodoData _todoData;

	private bool _isPossibleSwap = true;

	private Action<TodoData> _onDeleteTodoAction;

	private readonly Subject<TodoUI> onSwitchComplete = new Subject<TodoUI>();

	private static Subject<TodoData> onEditEndTodoText = new Subject<TodoData>();

	private static Subject<TodoData> onEditCompleted = new Subject<TodoData>();

	private readonly Subject<(TodoUI button, PointerEventData eventData)> onStartReorder = new Subject<(TodoUI, PointerEventData)>();

	private readonly Subject<(TodoUI button, PointerEventData eventData)> onReorderDrag = new Subject<(TodoUI, PointerEventData)>();

	private readonly Subject<(TodoUI button, PointerEventData eventData)> onEndReorder = new Subject<(TodoUI, PointerEventData)>();

	private bool _isMouseOver;

	private bool _isDirty;

	private bool _isDrag;

	private bool _wasTextEmpty = true;

	private bool _isSubscribedDragEvent;

	private Transform _transform;

	private string initReservationAnimationState;

	public TodoData TodoData => _todoData;

	public bool IsPossibleSwap => _isPossibleSwap;

	public Observable<TodoUI> OnSwitchComplete => onSwitchComplete;

	public Observable<TodoUI> OnExpireClick => expireButton.OnClick.Select((Unit _) => this);

	public Observable<(TodoUI button, PointerEventData eventData)> OnStartReorder => onStartReorder;

	public Observable<(TodoUI button, PointerEventData eventData)> OnReorderDrag => onReorderDrag;

	public Observable<(TodoUI button, PointerEventData eventData)> OnEndReorder => onEndReorder;

	public bool IsSubscribedDragEvent => _isSubscribedDragEvent;

	public Transform Transform
	{
		get
		{
			if (_transform == null)
			{
				_transform = base.transform;
			}
			return _transform;
		}
	}

	public CancellationToken DestroyCancellationToken => base.destroyCancellationToken;

	public void Init()
	{
		_baseHoldButtonAnim.Setup();
		_dragHoldButtonAnim.Setup();
	}

	public void SetActive(bool active)
	{
		if (base.gameObject.activeSelf != active)
		{
			base.gameObject.SetActive(active);
		}
	}

	public void Setup(TodoData todoData, Action<TodoData, string> onChangeEndTodoTextAction = null, Action<TodoData> onDeleteTodoAction = null)
	{
		Init();
		_todoData = todoData;
		_inputField.text = todoData.TodoText;
		_wasTextEmpty = string.IsNullOrEmpty(todoData.TodoText);
		_inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
		_inputField.onEndEdit.AddListener(delegate
		{
			onChangeEndTodoTextAction?.Invoke(_todoData, _inputField.text);
			onEditEndTodoText.OnNext(_todoData);
		});
		_inputField.SetupMultiLineSubmit();
		_onDeleteTodoAction = onDeleteTodoAction;
		onEditEndTodoText.Where((TodoData x) => x.UniqueID == todoData.UniqueID).Subscribe(delegate(TodoData x)
		{
			_inputField.text = x.TodoText;
		}).AddTo(this);
		onEditCompleted.Where((TodoData x) => x.UniqueID == todoData.UniqueID).Subscribe(delegate(TodoData x)
		{
			SetCompleteCore(x.Completed);
		}).AddTo(this);
		if (todoData.CurrentState == TodoState.Working)
		{
			SetExpire(todoData.Expire);
		}
		else
		{
			SetComplete(todoData.Completed);
		}
		if (todoData.CurrentState == TodoState.Complete)
		{
			ImmediateChangeUIForComplete();
		}
		DeactivateMouseOverImage();
		ReserveInitialAnimation();
		reorderTrigger.OnBeginDragAsObservable().Select(this, (PointerEventData e, TodoUI @this) => (@this: @this, e: e)).Subscribe(delegate((TodoUI @this, PointerEventData e) result)
		{
			onStartReorder.OnNext(result);
			StartReorder(isUseDoComplete: true);
		})
			.AddTo(this);
		reorderTrigger.OnDragAsObservable().Select(this, (PointerEventData e, TodoUI @this) => (@this, e: e)).Subscribe<(TodoUI, PointerEventData)>(onReorderDrag)
			.AddTo(this);
		reorderTrigger.OnEndDragAsObservable().Select(this, (PointerEventData e, TodoUI @this) => (@this: @this, e: e)).Subscribe(delegate((TodoUI @this, PointerEventData e) result)
		{
			onEndReorder.OnNext(result);
			TidyingEndDrag();
		})
			.AddTo(this);
		_cellAnimator.keepAnimatorStateOnDisable = true;
	}

	private void OnEnable()
	{
		if (!_isAnimationCompleted)
		{
			PlayCellAnim(_currentCellAnimName, 1f).Forget();
		}
	}

	public void FinishDragSubscribe()
	{
		_isSubscribedDragEvent = true;
	}

	public void SetupForDummy(TodoData todoData)
	{
		_todoData = todoData;
		_inputField.text = todoData.TodoText;
		_wasTextEmpty = string.IsNullOrEmpty(todoData.TodoText);
		if (!_todoData.Completed.HasValue)
		{
			SetExpire(_todoData.Expire);
			ChangeUIForUncomplete();
		}
		else
		{
			ImmediateChangeUIForComplete();
			SetCompleteCore(todoData.Completed);
		}
		ReserveInitialAnimation();
		StartReorder();
	}

	private void StartReorder(bool isUseDoComplete = false)
	{
		_isDrag = true;
		_isMouseOver = false;
		_isDirty = false;
		_baseHoldButtonAnim.ActivateUseUI();
		_dragHoldButtonAnim.ActivateUseUI();
		_dragInteractableUI.ActivateUseUI(isUseDoComplete);
		ActivateMouseOverImage();
	}

	public void TidyingEndDrag(bool isUseDoComplete = false)
	{
		_isDrag = false;
		_isMouseOver = false;
		_isDirty = false;
		_baseHoldButtonAnim.DeactivateUseUI();
		_dragHoldButtonAnim.DeactivateUseUI();
		_dragInteractableUI.DeactivateUseUI(isUseDoComplete);
		DeactivateMouseOverImage();
	}

	private void LateUpdate()
	{
		if (!string.IsNullOrEmpty(initReservationAnimationState))
		{
			_checkMarkAnimator.Play(initReservationAnimationState, 0, 1f);
			initReservationAnimationState = null;
		}
		if (!_isDrag && _isDirty)
		{
			_isDirty = false;
			UpdateBackImage();
		}
	}

	private void UpdateBackImage()
	{
		if (_isMouseOver)
		{
			ActivateMouseOverImage();
		}
		else
		{
			DeactivateMouseOverImage();
		}
	}

	private void ActivateMouseOverImage()
	{
		_todoRemoveButton.SetActive(value: true);
		_backImage.sprite = _backMouseOverSprite;
	}

	private void DeactivateMouseOverImage()
	{
		_todoRemoveButton.SetActive(value: false);
		_backImage.sprite = _backBaseSprite;
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void OnClickButtonComplete()
	{
		if (canvasGroup.interactable && !_wasTextEmpty)
		{
			canvasGroup.interactable = false;
			onSwitchComplete.OnNext(this);
		}
	}

	public async UniTask ChangeUIForCompleteAsync()
	{
		_cellAnimator.Play("CompleteTask", 0, 0f);
		_currentCellAnimName = base.name;
		_isAnimationCompleted = false;
		_dragUICanvasGroup.blocksRaycasts = false;
		_dragUICanvasGroup.DOFade(0f, 0.37f).SetLink(base.gameObject);
		_deleteUICanvasGroup.blocksRaycasts = false;
		_deleteUICanvasGroup.DOFade(0f, 0.37f).SetLink(base.gameObject);
		completeCheckMark.transform.DOScale(1f, 0.37f).SetLink(base.gameObject);
		completeCheckMark.DOFade(1f, 0.37f).SetLink(base.gameObject);
		await AsyncUtil.WaitUntilWithTimeout(() => _cellAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, 10f, _cts.Token);
		if (base.gameObject.activeInHierarchy)
		{
			_isAnimationCompleted = true;
		}
	}

	public async UniTask SlideInToComplete()
	{
		await PlayCellAnim("CompleteTaskSlideIn");
	}

	public async UniTask SlideInToUnComplete()
	{
		_dragUICanvasGroup.gameObject.SetActive(value: true);
		_dragUICanvasGroup.alpha = 1f;
		await PlayCellAnim("UnCompleteTaskSlideIn");
	}

	public async UniTask SlideOutFromComplete()
	{
		await PlayCellAnim("UnCompleteTaskSlideOut");
	}

	public async UniTask SlideOutFromUnComplete()
	{
		await PlayCellAnim("CompleteTaskSlideOut");
	}

	public void ImmediateChangeUIForComplete()
	{
		_dragUICanvasGroup.gameObject.SetActive(value: false);
		_dragUICanvasGroup.alpha = 0f;
		_dragUICanvasGroup.interactable = false;
		_dragUICanvasGroup.blocksRaycasts = false;
		completeBack.fillAmount = 1f;
		completeCheckMark.transform.localScale = Vector3.one;
		completeCheckMark.SetAlpha(1f);
	}

	public async UniTask ChangeUIForUncompleteAsync()
	{
		_cellAnimator.Play("UnCompleteTask", 0, 0f);
		completeBack.fillAmount = 0f;
		_deleteUICanvasGroup.blocksRaycasts = false;
		_deleteUICanvasGroup.DOFade(0f, 0.37f).SetLink(base.gameObject);
		completeCheckMark.transform.DOScale(0f, 0.37f).SetLink(base.gameObject);
		completeCheckMark.DOFade(0f, 0.37f).SetLink(base.gameObject);
		await AsyncUtil.WaitUntilWithTimeout(() => _cellAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, 10f, _cts.Token);
	}

	public void OnFinishMoveAnim()
	{
		_deleteUICanvasGroup.blocksRaycasts = true;
		_deleteUICanvasGroup.DOFade(1f, 0.37f).SetLink(base.gameObject);
		if (_todoData.CurrentState == TodoState.Complete)
		{
			_dragUICanvasGroup.interactable = false;
			_dragUICanvasGroup.blocksRaycasts = false;
		}
		else
		{
			_dragUICanvasGroup.interactable = true;
			_dragUICanvasGroup.blocksRaycasts = true;
		}
		canvasGroup.interactable = true;
	}

	public void ChangeUIForUncomplete()
	{
		completeBack.fillAmount = 0f;
		completeCheckMark.transform.localScale = Vector3.zero;
		completeCheckMark.SetAlpha(0f);
	}

	public void OnClickButtonRemoveTodo()
	{
		_onDeleteTodoAction?.Invoke(_todoData);
	}

	public void OnPointerEnter()
	{
		if (!_isDrag)
		{
			_isMouseOver = true;
			_isDirty = true;
		}
	}

	public void OnPointerExit()
	{
		if (!_isDrag)
		{
			_isMouseOver = false;
			_isDirty = true;
		}
	}

	public void SetExpire(DateTime? expire)
	{
		TodoData.SetExpire(expire);
		TMP_Text componentInChildren = expireButton.GetComponentInChildren<TMP_Text>();
		if (expire.HasValue)
		{
			DateTime valueOrDefault = expire.GetValueOrDefault();
			bool flag = valueOrDefault.Date < DateTime.Now.Date;
			componentInChildren.color = (flag ? Color.red : Color.white);
			componentInChildren.text = $"{valueOrDefault:MM/dd}";
			expireIcon.gameObject.SetActive(value: false);
		}
		else
		{
			componentInChildren.SetAlpha(0f);
			expireIcon.gameObject.SetActive(value: true);
		}
		_expireTipTarget.SetContentLocalizeID("ui_guide_todo_set_due_date");
	}

	public void SetComplete(DateTime? completed)
	{
		TodoData.SetCompleteTodoDatetime(completed);
		onEditCompleted.OnNext(TodoData);
		SetCompleteCore(completed);
		_expireTipTarget.SetContentLocalizeID("ui_guide_todo_change_complete_date");
	}

	private void SetCompleteCore(DateTime? completed)
	{
		if (!completed.HasValue)
		{
			SetExpire(_todoData.Expire);
			return;
		}
		TMP_Text componentInChildren = expireButton.GetComponentInChildren<TMP_Text>();
		componentInChildren.color = Color.white;
		componentInChildren.text = $"{completed:MM/dd}";
		expireIcon.gameObject.SetActive(value: false);
	}

	public void Hide()
	{
		canvasGroup.alpha = 0f;
	}

	public void Show()
	{
		canvasGroup.alpha = 1f;
	}

	public void EditMainText()
	{
		_inputField.ActivateInputField();
	}

	public void ToPossibleSwap()
	{
		_isPossibleSwap = true;
	}

	public void ToImpossibleSwap()
	{
		_isPossibleSwap = false;
	}

	private void ReserveInitialAnimation()
	{
		if (_wasTextEmpty)
		{
			initReservationAnimationState = "DeleteTodoTaskTextEnd";
		}
		else
		{
			initReservationAnimationState = "InputTodoTaskEnd";
		}
	}

	private void OnInputFieldValueChanged(string currentText)
	{
		if (!(_checkMarkAnimator == null))
		{
			bool flag = string.IsNullOrEmpty(currentText);
			if (_wasTextEmpty && !flag)
			{
				_checkMarkAnimator.Play("InputTodoTask", 0, 0f);
			}
			else if (!_wasTextEmpty && flag)
			{
				_checkMarkAnimator.Play("DeleteTodoTaskText", 0, 0f);
			}
			_wasTextEmpty = flag;
		}
	}

	private async UniTask PlayCellAnim(string name, float count = 0f)
	{
		_cellAnimator.Play(name, 0, count);
		_currentCellAnimName = name;
		_isAnimationCompleted = false;
		await AsyncUtil.WaitUntilWithTimeout(() => _cellAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, 10f, _cts.Token);
		if (base.gameObject.activeInHierarchy)
		{
			_isAnimationCompleted = true;
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		ScrollRect componentInParent = GetComponentInParent<ScrollRect>();
		if (componentInParent != null)
		{
			componentInParent.OnScroll(eventData);
		}
	}
}
