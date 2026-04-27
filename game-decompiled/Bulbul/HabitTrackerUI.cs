using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class HabitTrackerUI : MonoBehaviour
{
	[Inject]
	private HabitDataService _habitDataService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private ChangeOrderService _changeOrderService;

	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[SerializeField]
	[Header("UIドラッグ用")]
	private DraggableUI _draggableUI;

	[SerializeField]
	[Header("閉じるボタン")]
	private Button _closeButton;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private HabitListUI _habitListUI;

	private InteractableUI _currentInteractableUI;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private bool _isActive;

	public void Setup()
	{
		_rectTransform = base.transform as RectTransform;
		_draggableUI.Setup();
		_habitListUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_closeButton.OnClickAsObservable(), delegate
		{
			Deactivate();
			_systemSeService.PlayCancel();
		}).AddTo(this);
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
		_changeOrderService.BringToFront(ChangeOrderService.OrderItemType.Habit);
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
		_rectTransform.anchoredPosition = new Vector3(-194f, -123f, 0f);
	}
}
