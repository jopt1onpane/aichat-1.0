using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NestopiSystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class HabitTrackerListItemView : MonoBehaviour, IAnimationView, IRemovableItemView
{
	[SerializeField]
	[Header("View内要素のルート")]
	private RectTransform contentRoot;

	[SerializeField]
	private CanvasGroup contentCanvasGroup;

	[SerializeField]
	[Header("アニメーション中に入力をブロックするオブジェクト")]
	private GameObject raycastBlocker;

	[SerializeField]
	private Image baseImage;

	[SerializeField]
	private CanvasGroup pauseGroup;

	[SerializeField]
	private ToggleStyleButton habitPeriodButton;

	[SerializeField]
	private TMP_InputField titleInputField;

	[SerializeField]
	private ItemDragReorderHandle dragHandle;

	[SerializeField]
	private RemovingModeUI removingModeUI;

	[SerializeField]
	private Button removeButton;

	[SerializeField]
	private HabitTrackerListItemViewDayUI[] dayUIs;

	[SerializeField]
	[Header("ドラッグ操作による並び替えの間有効にするオブジェクト")]
	private GameObject draggingUseBackImage;

	[SerializeField]
	private GameObject draggingUseHandleImage;

	private HabitTrackerListItemModel itemModel;

	private CancellationTokenSource removeAnimationCancelToken;

	private CancellationTokenSource periodChangeCancelToken;

	private string prevText = string.Empty;

	private RectTransform _rectTransform;

	private Subject<string> onRemoveHabit = new Subject<string>();

	private Subject<(string uuid, string title)> onChangeHabitTitle = new Subject<(string, string)>();

	private Subject<(string uuid, bool enable)> onChangeHabitPeriod = new Subject<(string, bool)>();

	private Subject<(string uuid, DateTime date, bool isCompleted)> onChangeHabitCompleted = new Subject<(string, DateTime, bool)>();

	private Subject<(string uuid, DayOfWeek date, bool enable)> onChangeHabitEnable = new Subject<(string, DayOfWeek, bool)>();

	public ItemDragReorderHandle DragHandle => dragHandle;

	public RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = base.transform as RectTransform;
			}
			return _rectTransform;
		}
	}

	CanvasGroup IAnimationView.AnimationCanvasGroup => contentCanvasGroup;

	RectTransform IAnimationView.AnimationRectTransform => contentRoot;

	CancellationToken IRemovableItemView.CancellationToken => removeAnimationCancelToken.Token;

	public Observable<string> OnRemoveHabit => onRemoveHabit;

	public Observable<(string uuid, string title)> OnChangeHabitTitle => onChangeHabitTitle;

	public Observable<(string uuid, bool enable)> OnChangeHabitPeriod => onChangeHabitPeriod;

	public Observable<(string uuid, DateTime date, bool isCompleted)> OnChangeHabitCompleted => onChangeHabitCompleted;

	public Observable<(string uuid, DayOfWeek date, bool enable)> OnChangeHabitEnable => onChangeHabitEnable;

	public void SetActive(bool isActive)
	{
		if (contentRoot.gameObject.activeSelf != isActive)
		{
			contentRoot.gameObject.SetActive(isActive);
		}
	}

	public void ActivateDraggingImages()
	{
		draggingUseBackImage.SetActive(value: true);
		draggingUseHandleImage.SetActive(value: true);
	}

	public void DeactivateDraggingImages()
	{
		draggingUseBackImage.SetActive(value: false);
		draggingUseHandleImage.SetActive(value: false);
	}

	void IAnimationView.SetActiveRaycastBlocker(bool isActive)
	{
		if (!(raycastBlocker == null) && raycastBlocker.activeSelf != isActive)
		{
			raycastBlocker.SetActive(isActive);
		}
	}

	async UniTask IRemovableItemView.Play(ListItemViewAnimations.RemoveAnimationDirection direction, CancellationToken token)
	{
		try
		{
			await ListItemViewAnimations.PlayRemovingAnimation(this, token, TweenCancelBehaviour.Kill, direction);
		}
		catch (OperationCanceledException ex)
		{
			throw ex;
		}
	}

	public void Initialize()
	{
		ObservableSubscribeExtensions.Subscribe(titleInputField.OnEndEditAsObservable(), delegate
		{
			if (string.IsNullOrEmpty(titleInputField.text))
			{
				titleInputField.SetTextWithoutNotify(prevText);
			}
			prevText = titleInputField.text;
			onChangeHabitTitle?.OnNext((itemModel.UniqueId, titleInputField.text));
			titleInputField.enabled = false;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(removeButton.OnClickAsObservable(), delegate
		{
			onRemoveHabit?.OnNext(itemModel.UniqueId);
		}).AddTo(this);
		habitPeriodButton.OnValueChanged.Subscribe(delegate(bool period)
		{
			onChangeHabitPeriod?.OnNext((itemModel.UniqueId, period));
		}).AddTo(this);
		HabitTrackerListItemViewDayUI[] array = dayUIs;
		foreach (HabitTrackerListItemViewDayUI habitTrackerListItemViewDayUI in array)
		{
			if (!(habitTrackerListItemViewDayUI == null))
			{
				habitTrackerListItemViewDayUI.Initialize();
				habitTrackerListItemViewDayUI.OnChangeHabitCompleted.Subscribe(delegate((DateTime date, bool isCompleted) info)
				{
					onChangeHabitCompleted?.OnNext((itemModel.UniqueId, info.date, info.isCompleted));
				}).AddTo(this);
				habitTrackerListItemViewDayUI.OnChangeHabitEnabled.Subscribe(delegate((DayOfWeek date, bool enable) info)
				{
					onChangeHabitEnable?.OnNext((itemModel.UniqueId, info.date, info.enable));
				}).AddTo(this);
			}
		}
	}

	public void UpdateModel(HabitTrackerListItemModel model, bool isRemovingMode, bool isSettingMode)
	{
		itemModel = model;
		CreateRemoveAnimationCancellationTokenSource();
		raycastBlocker.SetActive(value: false);
		if ((object)this != null)
		{
			((IAnimationView)this).AnimationRectTransform.anchoredPosition = new Vector2(0f, 0f);
			((IAnimationView)this).AnimationCanvasGroup.alpha = 1f;
		}
		prevText = model.Title;
		titleInputField.SetTextWithoutNotify(model.Title);
		pauseGroup.alpha = ((!model.IsAlivePeriod) ? 1f : 0f);
		habitPeriodButton.SetToggleWithoutTransition(model.IsAlivePeriod, isNotify: false);
		ChangeRemoveMode(isRemovingMode, isImmediate: true);
		UpdateDayUIs(isSettingMode);
		if (model.IsCreateNew)
		{
			titleInputField.enabled = true;
			titleInputField.ActivateInputField();
			model.IsCreateNew = false;
		}
	}

	private void RemoveAnimationCancel()
	{
		if (removeAnimationCancelToken != null)
		{
			removeAnimationCancelToken.Cancel();
			removeAnimationCancelToken.Dispose();
			removeAnimationCancelToken = null;
		}
	}

	private void PeriodAnimationCancel()
	{
		if (periodChangeCancelToken != null)
		{
			periodChangeCancelToken.Cancel();
			periodChangeCancelToken.Dispose();
			periodChangeCancelToken = null;
		}
	}

	private void CreateRemoveAnimationCancellationTokenSource()
	{
		RemoveAnimationCancel();
		if (removeAnimationCancelToken == null)
		{
			removeAnimationCancelToken = new CancellationTokenSource();
		}
	}

	private void CreatePeriodAnimaitonCancellationTokenSource()
	{
		PeriodAnimationCancel();
		if (periodChangeCancelToken == null)
		{
			periodChangeCancelToken = new CancellationTokenSource();
		}
	}

	public void UpdateDayUIs(bool isSettingMode)
	{
		for (int i = 0; i < dayUIs.Length; i++)
		{
			HabitTrackerListItemViewDayUI habitTrackerListItemViewDayUI = dayUIs[i];
			HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = itemModel.dayInfos[i];
			if (!(habitTrackerListItemViewDayUI == null) && habitTrackerListItemDayInfoModel != null)
			{
				habitTrackerListItemViewDayUI.Setup(habitTrackerListItemDayInfoModel, isSettingMode);
			}
		}
	}

	public void ChangeRemoveMode(bool isRemoveMode, bool isImmediate = false)
	{
		if (isImmediate)
		{
			removingModeUI.TransitionImmediate(isRemoveMode);
		}
		else
		{
			removingModeUI.Transition(isRemoveMode);
		}
	}

	public void ChangeSettingMode(bool isSettingMode)
	{
		UpdateDayUIs(isSettingMode);
	}

	public void ChangeTitle(string title)
	{
		titleInputField.SetTextWithoutNotify(title);
	}

	public void ChangeHabitComplete(DateTime date, bool isCompleted, HabitDateEnableState prevState)
	{
		for (int i = 0; i < itemModel.dayInfos.Length; i++)
		{
			HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = itemModel.dayInfos[i];
			if (habitTrackerListItemDayInfoModel != null && date.IsSameDay(habitTrackerListItemDayInfoModel.date))
			{
				HabitTrackerListItemViewDayUI habitTrackerListItemViewDayUI = dayUIs[i];
				if (!(habitTrackerListItemViewDayUI == null))
				{
					habitTrackerListItemViewDayUI.SetComplete(isCompleted, prevState);
					break;
				}
			}
		}
	}

	public void ChangeHabitEnable(DayOfWeek date, bool enable)
	{
		for (int i = 0; i < itemModel.dayInfos.Length; i++)
		{
			HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = itemModel.dayInfos[i];
			if (habitTrackerListItemDayInfoModel != null && date == habitTrackerListItemDayInfoModel.date.DayOfWeek)
			{
				HabitTrackerListItemViewDayUI habitTrackerListItemViewDayUI = dayUIs[i];
				if (!(habitTrackerListItemViewDayUI == null))
				{
					habitTrackerListItemViewDayUI.SetEnable(enable);
					break;
				}
			}
		}
	}

	public void ChangeHabitPeriod()
	{
		PeriodAnimationCancel();
		CreatePeriodAnimaitonCancellationTokenSource();
		float endValue = ((!itemModel.IsAlivePeriod) ? 1f : 0f);
		pauseGroup.DOFade(endValue, 0.25f).ToUniTask(TweenCancelBehaviour.Kill, periodChangeCancelToken.Token);
		habitPeriodButton.SetToggleWithoutTransition(itemModel.IsAlivePeriod, isNotify: false);
		for (int i = 0; i < itemModel.dayInfos.Length; i++)
		{
			HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel habitTrackerListItemDayInfoModel = itemModel.dayInfos[i];
			if (habitTrackerListItemDayInfoModel != null)
			{
				HabitTrackerListItemViewDayUI habitTrackerListItemViewDayUI = dayUIs[i];
				if (!(habitTrackerListItemViewDayUI == null))
				{
					habitTrackerListItemViewDayUI.ChangeHabitPeriod(habitTrackerListItemDayInfoModel);
				}
			}
		}
	}
}
