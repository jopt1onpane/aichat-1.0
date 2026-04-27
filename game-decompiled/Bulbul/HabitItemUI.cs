using NestopiSystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul;

public class HabitItemUI : MonoBehaviour
{
	public record ViewModel(string Id, ReadOnlyReactiveProperty<string> Title);

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private HoldButtonAnimation _baseHoldButtonAnimUI;

	[SerializeField]
	private InteractableUI _dragInteractableUI;

	[SerializeField]
	private HoldButtonAnimation _dragHoldButtonAnimUI;

	[SerializeField]
	private EventTrigger _reorderTrigger;

	[SerializeField]
	private InteractableUI _removeButton;

	[SerializeField]
	private TMP_InputField _titleInputField;

	[SerializeField]
	private HabitPlayPauseButton _playPauseButton;

	[SerializeField]
	private GameObject _pausedObj;

	[SerializeField]
	private HabitItemDateUI[] _dateUIs;

	public const int DisplayDays = 7;

	private ViewModel _viewModel;

	private readonly Subject<(HabitItemUI button, PointerEventData eventData)> _onStartReorder = new Subject<(HabitItemUI, PointerEventData)>();

	private readonly Subject<(HabitItemUI button, PointerEventData eventData)> _onReorderDrag = new Subject<(HabitItemUI, PointerEventData)>();

	private readonly Subject<(HabitItemUI button, PointerEventData eventData)> _onEndReorder = new Subject<(HabitItemUI, PointerEventData)>();

	private DisposableBag _disposableBag;

	public RectTransform DeletePopoverPivot => _removeButton.transform as RectTransform;

	public string HabitId => _viewModel.Id;

	public Observable<(HabitItemUI button, PointerEventData eventData)> OnStartReorder => _onStartReorder;

	public Observable<(HabitItemUI button, PointerEventData eventData)> OnReorderDrag => _onReorderDrag;

	public Observable<(HabitItemUI button, PointerEventData eventData)> OnEndReorder => _onEndReorder;

	public Observable<string> OnTitleEndEdit => _titleInputField.OnEndEditAsObservable();

	public Observable<Unit> OnClickRemove => _removeButton.GetComponent<Button>().OnClickAsObservable();

	public Observable<Unit> OnClickPlayPause => _playPauseButton.OnClick;

	public bool IsHabitEnabled => _playPauseButton.IsOn;

	public void Initialize()
	{
		HabitItemDateUI[] dateUIs = _dateUIs;
		for (int i = 0; i < dateUIs.Length; i++)
		{
			dateUIs[i].Initialize();
		}
	}

	public void Setup(ViewModel viewModel)
	{
		_disposableBag.Clear();
		_viewModel = viewModel;
		viewModel.Title.Subscribe(delegate(string title)
		{
			_titleInputField.text = title;
		}).AddTo(ref _disposableBag);
		_titleInputField.SetupMultiLineSubmit();
		_removeButton.Setup();
		_baseHoldButtonAnimUI.Setup();
	}

	public void SetupDays(HabitItemDateUI.ViewModel[] dateViewModels)
	{
		for (int i = 0; i < _dateUIs.Length; i++)
		{
			_dateUIs[i].Bind(dateViewModels[i]);
		}
	}

	private void Start()
	{
		_reorderTrigger.OnBeginDragAsObservable().Select(this, (PointerEventData e, HabitItemUI @this) => (@this: @this, e: e)).Subscribe<(HabitItemUI, PointerEventData)>(_onStartReorder)
			.AddTo(this);
		_reorderTrigger.OnDragAsObservable().Select(this, (PointerEventData e, HabitItemUI @this) => (@this: @this, e: e)).Subscribe<(HabitItemUI, PointerEventData)>(_onReorderDrag)
			.AddTo(this);
		_reorderTrigger.OnEndDragAsObservable().Select(this, (PointerEventData e, HabitItemUI @this) => (@this: @this, e: e)).Subscribe<(HabitItemUI, PointerEventData)>(_onEndReorder)
			.AddTo(this);
		_dragInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_reorderTrigger.OnBeginDragAsObservable(), delegate
		{
			ActivateDragAnimation();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_reorderTrigger.OnEndDragAsObservable(), delegate
		{
			DeactivateDragAnimation();
		}).AddTo(this);
	}

	public void SetPlayPause(bool isPlay)
	{
		_playPauseButton.SetIsOn(isPlay);
		_pausedObj.SetActive(!isPlay);
	}

	public void Hide()
	{
		_canvasGroup.alpha = 0f;
	}

	public void Show()
	{
		_canvasGroup.alpha = 1f;
	}

	public void ActivateDragAnimation()
	{
		_baseHoldButtonAnimUI.ActivateUseUI();
		_dragInteractableUI.ActivateUseUI(isUseDoComplete: true);
		_dragHoldButtonAnimUI.ActivateUseUI();
	}

	public void DeactivateDragAnimation()
	{
		_baseHoldButtonAnimUI.DeactivateUseUI();
		_dragInteractableUI.DeactivateUseUI(isUseDoComplete: true);
		_dragHoldButtonAnimUI.DeactivateUseUI();
	}

	private void OnDestroy()
	{
		_disposableBag.Dispose();
	}
}
