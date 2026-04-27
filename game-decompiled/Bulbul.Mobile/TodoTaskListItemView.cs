using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyUtil;
using NestopiSystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class TodoTaskListItemView : MonoBehaviour, IAnimationView, IRemovableItemView
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

	private const string CellAnimInitialize = "Idle";

	private const string CellAnimCompleteTask = "CompleteTask";

	private const string CellAnimUnCompleteTask = "UnCompleteTask";

	private const float AnimDuration = 0.37f;

	private MainState _mainState;

	[SerializeField]
	[Header("自身を構成する一番上層のオブジェクトを指定")]
	private GameObject _rootObject;

	[SerializeField]
	private RemovingModeUI _removingModeUI;

	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private Image _completeBack;

	[SerializeField]
	private Sprite _completeBackDefaultSprite;

	[SerializeField]
	private Animator _checkMarkAnimator;

	[SerializeField]
	private Image _checkMarkBackImage;

	[SerializeField]
	private Image _completeCheckMark;

	[SerializeField]
	[Header("完了日\u3000期日の設定用")]
	private Button _settingDateTimeButton;

	[SerializeField]
	private Image _settingDateTimeIcon;

	[SerializeField]
	private Animator _cellAnimator;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("削除アニメーション等で動かしても問題のないRectTrasform")]
	private RectTransform _animationRectTransform;

	[SerializeField]
	[Header("背景")]
	private Image _backImage;

	[SerializeField]
	[Header("削除ボタン")]
	private Button _todoRemoveButton;

	[SerializeField]
	private Button[] _completedButtons;

	[SerializeField]
	private GameObject _raycastBlocker;

	[SerializeField]
	private GameObject _dragHandleObj;

	[SerializeField]
	private ItemDragReorderHandle _dragReorderHandle;

	[SerializeField]
	private GameObject _habitTrackerTaskBackObj;

	[SerializeField]
	private GameObject _draggingUseBackImage;

	[SerializeField]
	private GameObject _draggingUseHandleImage;

	private CancellationTokenSource _cts = new CancellationTokenSource();

	private TodoState _currentState;

	private bool _isDirty;

	private bool _wasTextEmpty = true;

	private string _prevStr;

	private ulong _uniqueID;

	private DateTime? _settingDateTime;

	private Subject<(ulong, string)> _onChangedEndTodoText = new Subject<(ulong, string)>();

	private Subject<ulong> _onRemoved = new Subject<ulong>();

	private Subject<ulong> _onSwitchCompleted = new Subject<ulong>();

	private Subject<(ulong uniqueId, DateTime? datetime)> _onSettingDateTime = new Subject<(ulong, DateTime?)>();

	private Subject<(string, string)> _onChangedEndTodoTextHabitTrackerTask = new Subject<(string, string)>();

	private Subject<string> _onRemovedHabitTrackerTask = new Subject<string>();

	private CancellationTokenSource _removingAnimationCancellationTokenSoruce = new CancellationTokenSource();

	private bool _isHabitTrackerTask;

	private string _habitID;

	private GameObject _checkMarkAnimatorGameObj;

	private bool _prevCheckMarkAnimatorObjActiveInHierarchy;

	private string initReservationAnimationState;

	private string initReservationCellAnimationState;

	public TodoState CurrentState => _currentState;

	public Observable<(ulong, string)> OnChangedEndTodoText => _onChangedEndTodoText;

	public Observable<ulong> OnRemoved => _onRemoved;

	public Observable<ulong> OnSwitchCompleted => _onSwitchCompleted;

	public Observable<(string, string)> _OnChangedEndTodoTextHabitTrackerTask => _onChangedEndTodoTextHabitTrackerTask;

	public Observable<string> OnRemovedHabitTrackerTask => _onRemovedHabitTrackerTask;

	public Observable<(ulong uniqueId, DateTime? datetime)> OnSettingDateTime => _onSettingDateTime;

	CancellationToken IRemovableItemView.CancellationToken => _removingAnimationCancellationTokenSoruce.Token;

	public CancellationToken DestroyCancellationToken => base.destroyCancellationToken;

	public RectTransform AnimationRectTransform => _animationRectTransform;

	public CanvasGroup AnimationCanvasGroup => _canvasGroup;

	public ItemDragReorderHandle DragReorderHandle => _dragReorderHandle;

	private GameObject CheckMarkAnimatorGameObj
	{
		get
		{
			if (_checkMarkAnimatorGameObj == null)
			{
				_checkMarkAnimatorGameObj = _checkMarkAnimator.gameObject;
			}
			return _checkMarkAnimatorGameObj;
		}
	}

	public void CreateRemovingAnimationCancellatonToken()
	{
		_removingAnimationCancellationTokenSoruce = new CancellationTokenSource();
	}

	public bool CheckSameDate(int year, int month, int day)
	{
		if (!_settingDateTime.HasValue)
		{
			return false;
		}
		if (_settingDateTime.Value.Year == year && _settingDateTime.Value.Month == month)
		{
			return _settingDateTime.Value.Day == day;
		}
		return false;
	}

	public bool CheckSameDate(DateTime dateTime)
	{
		return CheckSameDate(dateTime.Year, dateTime.Month, dateTime.Day);
	}

	public void ActivateDraggingImages()
	{
		_draggingUseBackImage.SetActive(value: true);
		_draggingUseHandleImage.SetActive(value: true);
	}

	public void DeactivateDraggingImages()
	{
		_draggingUseBackImage.SetActive(value: false);
		_draggingUseHandleImage.SetActive(value: false);
	}

	public void SetupDragHandleEnableControl(ReactiveProperty<bool> enableChecker)
	{
		if (enableChecker == null)
		{
			return;
		}
		DragReorderHandle.enabled = enableChecker.Value;
		enableChecker.Subscribe(delegate(bool _)
		{
			if (DragReorderHandle != null)
			{
				DragReorderHandle.enabled = _;
			}
		}).AddTo(this);
	}

	private void Init()
	{
		_todoRemoveButton.onClick.RemoveAllListeners();
		_todoRemoveButton.onClick.AddListener(OnClickButtonRemoveTodo);
		if (_completedButtons != null)
		{
			Button[] completedButtons = _completedButtons;
			foreach (Button obj in completedButtons)
			{
				obj.onClick.RemoveAllListeners();
				obj.onClick.AddListener(OnClickSwitchCompleted);
			}
		}
		_settingDateTimeButton.onClick.RemoveAllListeners();
		_settingDateTimeButton.onClick.AddListener(OnClickSettingDateTimeButton);
	}

	public void SetActive(bool active)
	{
		if (_rootObject.activeSelf != active)
		{
			_rootObject.SetActive(active);
		}
	}

	public void SetParent(Transform parent)
	{
		_rootObject.transform.SetParent(parent);
	}

	public void SetAsLastSibling()
	{
		_rootObject.transform.SetAsLastSibling();
	}

	private void OnDisable()
	{
		CancelRemovingAnimation();
		CancelCompleteAndUnCompleteAnim();
	}

	private void OnDestroy()
	{
		_onChangedEndTodoText.Dispose();
		_onSettingDateTime.Dispose();
		_onRemoved.Dispose();
		_onSwitchCompleted.Dispose();
		CancelCompleteAndUnCompleteAnim();
		CancelRemovingAnimation();
	}

	public void Setup(ulong uniqueID, string text, TodoState state, DateTime? expire, DateTime? completed, bool isRemovingMode, bool isHabitTrackerTask = false, string habitID = null)
	{
		Init();
		_prevCheckMarkAnimatorObjActiveInHierarchy = CheckMarkAnimatorGameObj.activeInHierarchy;
		CancelCompleteAndUnCompleteAnim();
		_completeCheckMark.transform.localScale = new Vector3(0f, 0f, 0f);
		_completeCheckMark.SetAlpha(0f);
		SetActiveRaycastBlocker(active: false);
		_animationRectTransform.anchoredPosition = new Vector2(0f, 0f);
		_canvasGroup.alpha = 1f;
		_uniqueID = uniqueID;
		_habitID = habitID;
		_prevStr = text;
		_inputField.SetTextWithoutNotify(text);
		_currentState = state;
		_wasTextEmpty = string.IsNullOrEmpty(_prevStr);
		_isHabitTrackerTask = isHabitTrackerTask;
		ChangeRemovingMode(isRemovingMode, isImmediate: true);
		_dragHandleObj.SetActive(_currentState == TodoState.Working);
		_inputField.onValueChanged.RemoveAllListeners();
		_inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
		_inputField.onEndEdit.RemoveAllListeners();
		_inputField.onEndEdit.AddListener(delegate
		{
			if (string.IsNullOrEmpty(_inputField.text))
			{
				_inputField.text = _prevStr;
			}
			_prevStr = _inputField.text;
			if (isHabitTrackerTask)
			{
				_onChangedEndTodoTextHabitTrackerTask.OnNext((_habitID, _inputField.text));
			}
			else
			{
				_onChangedEndTodoText.OnNext((_uniqueID, _inputField.text));
			}
			_inputField.enabled = false;
		});
		if (_currentState == TodoState.Working)
		{
			SetExpire(expire);
		}
		else
		{
			SetComplete(completed.Value);
		}
		_cellAnimator.enabled = false;
		_completeBack.sprite = _completeBackDefaultSprite;
		SetActiveHabitTrackerTaskBackImage(isHabitTrackerTask);
		if (_currentState == TodoState.Complete)
		{
			_checkMarkAnimator.enabled = false;
			ImmediateChangeUIForComplete();
		}
		else
		{
			_checkMarkAnimator.enabled = true;
			ImmediateChangeUIForUncomplete();
		}
		_inputField.SetupMultiLineSubmit();
		_inputField.enabled = false;
		ReserveInitialAnimation();
	}

	private void SetActiveHabitTrackerTaskBackImage(bool active)
	{
		if (_habitTrackerTaskBackObj.activeSelf != active)
		{
			_habitTrackerTaskBackObj.SetActive(active);
		}
	}

	private void LateUpdate()
	{
		if (CheckMarkAnimatorGameObj.activeInHierarchy && _prevCheckMarkAnimatorObjActiveInHierarchy != CheckMarkAnimatorGameObj.activeInHierarchy)
		{
			ReserveInitialAnimation();
		}
		_prevCheckMarkAnimatorObjActiveInHierarchy = CheckMarkAnimatorGameObj.activeInHierarchy;
		if (!string.IsNullOrEmpty(initReservationAnimationState))
		{
			_checkMarkAnimator.Play(initReservationAnimationState, 0, 1f);
			initReservationAnimationState = null;
		}
		if (!string.IsNullOrEmpty(initReservationCellAnimationState))
		{
			_cellAnimator.enabled = true;
			_cellAnimator.Play(initReservationCellAnimationState, 0, 1f);
			initReservationCellAnimationState = null;
			_cellAnimator.enabled = false;
		}
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void CancelCompleteAndUnCompleteAnim()
	{
		if (_cts != null)
		{
			_cts.Cancel();
			_cts.Dispose();
			_cts = null;
		}
	}

	public async UniTask ChangeUIForCompleteAsync(CancellationToken cancellationToken)
	{
		CancelCompleteAndUnCompleteAnim();
		_cts = new CancellationTokenSource();
		using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
		try
		{
			SetActiveRaycastBlocker(active: true);
			_cellAnimator.enabled = true;
			_cellAnimator.Play("CompleteTask", 0, 0f);
			_completeCheckMark.transform.DOScale(1f, 0.37f).WithCancellation(linkedSource.Token);
			_completeCheckMark.DOFade(1f, 0.37f).WithCancellation(linkedSource.Token);
			await AsyncUtil.WaitUntilWithTimeout(() => _cellAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, 10f, linkedSource.Token);
			SetActiveRaycastBlocker(active: false);
			_cellAnimator.enabled = false;
		}
		catch (OperationCanceledException ex)
		{
			throw ex;
		}
	}

	public void ImmediateChangeUIForComplete()
	{
		_completeBack.fillAmount = 1f;
		_completeCheckMark.transform.localScale = Vector3.one;
		_completeCheckMark.SetAlpha(1f);
	}

	public async UniTask ChangeUIForUncompleteAsync(CancellationToken cancellationToken)
	{
		CancelCompleteAndUnCompleteAnim();
		_cts = new CancellationTokenSource();
		using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
		try
		{
			_cellAnimator.enabled = true;
			_cellAnimator.Play("UnCompleteTask", 0, 0f);
			SetActiveRaycastBlocker(active: true);
			_completeCheckMark.transform.DOScale(0f, 0.37f).WithCancellation(linkedSource.Token);
			_completeCheckMark.DOFade(0f, 0.37f).WithCancellation(linkedSource.Token);
			await AsyncUtil.WaitUntilWithTimeout(() => _cellAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, 10f, linkedSource.Token);
			SetActiveRaycastBlocker(active: false);
			_cellAnimator.enabled = false;
		}
		catch (OperationCanceledException ex)
		{
			throw ex;
		}
	}

	public void ImmediateChangeUIForUncomplete()
	{
		_completeBack.fillAmount = 0f;
		_completeCheckMark.transform.localScale = Vector3.zero;
		_completeCheckMark.SetAlpha(0f);
	}

	public void CellAnimatorFinalizer()
	{
		_cellAnimator.Play("Idle", 0, 1f);
	}

	public void OnClickButtonRemoveTodo()
	{
		if (!_removingModeUI.IsTransitioning)
		{
			if (_isHabitTrackerTask)
			{
				_onRemovedHabitTrackerTask.OnNext(_habitID);
			}
			else
			{
				_onRemoved.OnNext(_uniqueID);
			}
		}
	}

	public void OnClickSwitchCompleted()
	{
		if (!_removingModeUI.IsTransitioning && !_wasTextEmpty)
		{
			_onSwitchCompleted.OnNext(_uniqueID);
		}
	}

	public void OnClickSettingDateTimeButton()
	{
		if (!_removingModeUI.IsTransitioning && !_isHabitTrackerTask)
		{
			_onSettingDateTime.OnNext((_uniqueID, _settingDateTime));
		}
	}

	public void SetExpire(DateTime? expire)
	{
		TMP_Text componentInChildren = _settingDateTimeButton.GetComponentInChildren<TMP_Text>();
		if (expire.HasValue)
		{
			DateTime valueOrDefault = expire.GetValueOrDefault();
			bool flag = valueOrDefault.Date < DateTime.Now.Date;
			componentInChildren.color = (flag ? Color.red : Color.white);
			componentInChildren.text = $"{valueOrDefault:MM/dd}";
			_settingDateTimeIcon.gameObject.SetActive(value: false);
			_settingDateTime = valueOrDefault;
		}
		else
		{
			componentInChildren.SetAlpha(0f);
			_settingDateTimeIcon.gameObject.SetActive(value: true);
			_settingDateTime = null;
		}
	}

	public void SetComplete(DateTime completed)
	{
		_settingDateTime = completed;
		SetCompleteCore(completed);
	}

	private void SetCompleteCore(DateTime completed)
	{
		TMP_Text componentInChildren = _settingDateTimeButton.GetComponentInChildren<TMP_Text>();
		componentInChildren.color = Color.white;
		componentInChildren.text = $"{completed:MM/dd}";
		_settingDateTimeIcon.gameObject.SetActive(value: false);
	}

	public void Hide()
	{
		_canvasGroup.alpha = 0f;
	}

	public void Show()
	{
		_canvasGroup.alpha = 1f;
	}

	public void EditMainText()
	{
		_inputField.enabled = true;
		_inputField.ActivateInputField();
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
		initReservationCellAnimationState = "Idle";
	}

	private void OnInputFieldValueChanged(string currentText)
	{
		if (_checkMarkAnimator == null)
		{
			return;
		}
		bool flag = string.IsNullOrEmpty(currentText);
		if (CurrentState != TodoState.Complete)
		{
			if (_wasTextEmpty && !flag)
			{
				_checkMarkAnimator.Play("InputTodoTask", 0, 0f);
			}
			else if (!_wasTextEmpty && flag)
			{
				_checkMarkAnimator.Play("DeleteTodoTaskText", 0, 0f);
			}
		}
		_wasTextEmpty = flag;
	}

	public async UniTask PlayRemoveFadeOutAnimationAsync()
	{
		SetActiveRaycastBlocker(active: true);
		await _canvasGroup.DOFade(0f, 0.5f);
	}

	public void SetActiveRaycastBlocker(bool active)
	{
		if (_raycastBlocker.activeSelf != active)
		{
			_raycastBlocker.SetActive(active);
		}
	}

	public void ChangeRemovingMode(bool isRemoving, bool isImmediate)
	{
		_inputField.enabled = !isRemoving;
		_inputField.interactable = !isRemoving;
		_settingDateTimeButton.interactable = !isRemoving && !_isHabitTrackerTask;
		if (isImmediate)
		{
			_removingModeUI.TransitionImmediate(isRemoving);
		}
		else
		{
			_removingModeUI.Transition(isRemoving);
		}
	}

	public void CancelRemovingAnimation()
	{
		if (_removingAnimationCancellationTokenSoruce != null)
		{
			_removingAnimationCancellationTokenSoruce.Cancel();
			_removingAnimationCancellationTokenSoruce.Dispose();
			_removingAnimationCancellationTokenSoruce = null;
		}
	}

	async UniTask IRemovableItemView.Play(ListItemViewAnimations.RemoveAnimationDirection direction, CancellationToken token)
	{
		await ListItemViewAnimations.PlayRemovingAnimation(this, token, TweenCancelBehaviour.KillAndCancelAwait, direction);
	}
}
