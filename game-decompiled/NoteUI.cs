using Bulbul;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class NoteUI : MonoBehaviour, INoteUI
{
	[Inject]
	private NoteService _noteService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private ChangeOrderService _changeOrderService;

	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[SerializeField]
	[Header("現在のページUI")]
	private CurrentPageUI _currentPageUI;

	[SerializeField]
	[Header("ページ選択リスト")]
	private SelectPageListUI _selectPageListUI;

	[SerializeField]
	[Header("UIドラッグ用")]
	private DraggableUI _draggableUI;

	[SerializeField]
	[Header("ノートを閉じるボタン")]
	private Button _noteCloseButton;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	private InteractableUI _currentInteractableUI;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private bool _isActive;

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

	public void Setup()
	{
		ObservableSubscribeExtensions.Subscribe(_noteCloseButton.OnClickAsObservable(), delegate
		{
			_noteService.CloseNote();
			_systemSeService.PlayCancel();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_noteService.OnCloseNote, delegate
		{
			Deactivate();
		}).AddTo(this);
		_currentPageUI.Setup();
		_selectPageListUI.Setup();
		_draggableUI.Setup();
		_rectTransform = base.transform as RectTransform;
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
		_changeOrderService.BringToFront(ChangeOrderService.OrderItemType.Note);
		_isActive = true;
		_facilityOpenButton.ActivateUseUI();
		base.gameObject.SetActive(value: true);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		InitializePosition();
		float endValue = -62f;
		_moveTween = _rectTransform.DOAnchorPosY(endValue, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
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
		rectTransform.anchoredPosition = new Vector3(-150f, -70f, 0f);
	}
}
