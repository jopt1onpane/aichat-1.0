using NestopiSystem;
using NestopiSystem.DIContainers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class SelectPageUI : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	[Inject]
	private LocalizationMasterWrapper _localizationMaster;

	[Inject]
	private NoteService _noteService;

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private TMP_InputField _titleInputField;

	[SerializeField]
	private Button _selectPageButton;

	[SerializeField]
	private InteractableUI _interactableUI;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private EventTrigger reorderTrigger;

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
	private Button _removePageButton;

	private readonly Subject<(SelectPageUI button, PointerEventData eventData)> onStartReorder = new Subject<(SelectPageUI, PointerEventData)>();

	private readonly Subject<(SelectPageUI button, PointerEventData eventData)> onReorderDrag = new Subject<(SelectPageUI, PointerEventData)>();

	private readonly Subject<(SelectPageUI button, PointerEventData eventData)> onEndReorder = new Subject<(SelectPageUI, PointerEventData)>();

	private float _lastClickTime = -1f;

	private bool _isMouseOver;

	private bool _isDirty;

	private bool _isDrag;

	public ulong PageID { get; private set; }

	public Observable<(SelectPageUI button, PointerEventData eventData)> OnStartReorder => onStartReorder;

	public Observable<(SelectPageUI button, PointerEventData eventData)> OnReorderDrag => onReorderDrag;

	public Observable<(SelectPageUI button, PointerEventData eventData)> OnEndReorder => onEndReorder;

	public void Setup(PageDataV2 pageData)
	{
		Init();
		if (_noteService == null)
		{
			_noteService = RoomLifetimeScope.Resolve<NoteService>();
		}
		if (_systemSeService == null)
		{
			_systemSeService = RoomLifetimeScope.Resolve<SystemSeService>();
		}
		PageID = pageData.UniqueID;
		string pageTitle = _noteService.GetPageTitle(PageID);
		ObservableSubscribeExtensions.Subscribe(_selectPageButton.OnClickAsObservable(), delegate
		{
			OnClickHeader();
		}).AddTo(this);
		_noteService.CurrentPageID.Subscribe(delegate(ulong pageID)
		{
			if (pageID == PageID)
			{
				_interactableUI.ActivateUseUI();
			}
			else
			{
				_interactableUI.DeactivateUseUI();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_removePageButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayCancel();
			_noteService.RemovePage(PageID);
			_noteService.SelectFirstPage();
		}).AddTo(this);
		_noteService.OnRemovePage.Subscribe(delegate(ulong pageID)
		{
			if (pageID == PageID)
			{
				Destroy();
			}
			else if (SaveDataManager.Instance.NoteData.NoteList.Titles.Count <= 1)
			{
				_removePageButton.gameObject.SetActive(value: false);
			}
		}).AddTo(this);
		_titleInputField.OnValueChangedAsObservable().Subscribe(delegate(string titleText)
		{
			_noteService.ChangeTitle(PageID, titleText);
		}).AddTo(this);
		_titleInputField.OnEndEditAsObservable().Subscribe(delegate(string titleText)
		{
			_noteService.ChangeEndTitle(PageID, titleText);
		}).AddTo(this);
		_noteService.OnChangeTitle.Subscribe(delegate((ulong pageID, string title) info)
		{
			if (info.pageID == PageID)
			{
				_titleInputField.SetTextWithoutNotify(info.title);
			}
		}).AddTo(this);
		reorderTrigger.OnBeginDragAsObservable().Select(this, (PointerEventData e, SelectPageUI @this) => (@this: @this, e: e)).Subscribe(delegate((SelectPageUI @this, PointerEventData e) result)
		{
			onStartReorder.OnNext(result);
			StartReorder(isUseDoComplete: true);
		})
			.AddTo(this);
		reorderTrigger.OnDragAsObservable().Select(this, (PointerEventData e, SelectPageUI @this) => (@this: @this, e: e)).Subscribe<(SelectPageUI, PointerEventData)>(onReorderDrag)
			.AddTo(this);
		reorderTrigger.OnEndDragAsObservable().Select(this, (PointerEventData e, SelectPageUI @this) => (@this: @this, e: e)).Subscribe(delegate((SelectPageUI @this, PointerEventData e) result)
		{
			onEndReorder.OnNext(result);
			TidyingEndDrag();
		})
			.AddTo(this);
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerClick;
		entry.callback.AddListener(delegate
		{
			OnClickHeader();
		});
		reorderTrigger.triggers.Add(entry);
		if (pageTitle == null || pageTitle == string.Empty)
		{
			if (_localizationMaster == null)
			{
				_localizationMaster = ProjectLifetimeScope.Resolve<LocalizationMasterWrapper>();
			}
			_localizationMaster.Bind(_titleInputField, "ui_note_init_title");
			_noteService.ChangeEndTitle(PageID, _titleInputField.text);
		}
		else
		{
			_titleInputField.text = pageTitle;
		}
		DeactivateMouseOverImage();
	}

	public void Init()
	{
		_interactableUI.Setup();
		_baseHoldButtonAnim.Setup();
		_dragInteractableUI.Setup();
	}

	public void SetupForDummy(PageDataV2 pageData)
	{
		PageID = pageData.UniqueID;
		_titleInputField.text = _noteService.GetPageTitle(PageID);
		_interactableUI.ActivateUseUI();
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
		if (SaveDataManager.Instance.NoteData.NoteList.Titles.Count > 1)
		{
			_removePageButton.gameObject.SetActive(value: true);
		}
	}

	private void DeactivateMouseOverImage()
	{
		_removePageButton.gameObject.SetActive(value: false);
	}

	private void OnClickHeader()
	{
		if (Time.time - _lastClickTime <= 0.5f)
		{
			EditTitle();
			_lastClickTime = -1f;
		}
		else
		{
			_systemSeService.PlayClick();
			SelectPage();
			_lastClickTime = Time.time;
		}
	}

	public void SelectPage()
	{
		_noteService.ChangePage(PageID);
	}

	public void EditTitle()
	{
		_titleInputField.ActivateInputField();
	}

	public void Destroy()
	{
		Object.Destroy(base.gameObject);
	}

	public void Hide()
	{
		canvasGroup.alpha = 0f;
	}

	public void Show()
	{
		canvasGroup.alpha = 1f;
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

	public void OnScroll(PointerEventData eventData)
	{
		ScrollRect componentInParent = GetComponentInParent<ScrollRect>();
		if (componentInParent != null)
		{
			componentInParent.OnScroll(eventData);
		}
	}
}
