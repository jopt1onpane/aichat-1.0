using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class HabitTrackerContentView : MonoBehaviour
{
	[SerializeField]
	private Button addHabitButton;

	[SerializeField]
	private CanvasGroup addHabitDeactivate;

	[SerializeField]
	private Button habitSettingButton;

	[SerializeField]
	private CanvasGroup habitSettingDeactivate;

	[SerializeField]
	private CanvasGroup habitSettingChildGroup;

	[SerializeField]
	private Button habitSettingRevertButton;

	[SerializeField]
	private Button habitSettingApplyButton;

	[SerializeField]
	private ToggleStyleButton habitRemoveModeButton;

	[SerializeField]
	private HabitTrackerWeekView weekView;

	private Subject<Unit> onClickAddHabit = new Subject<Unit>();

	private Subject<Unit> onClickHabitSetting = new Subject<Unit>();

	private Subject<bool> onClickHabitSettingComplete = new Subject<bool>();

	private Subject<bool> onClickHabitRemoveMode = new Subject<bool>();

	private HabitTrackerUIModel model;

	private CancellationTokenSource animCancellationToken;

	private const float HabitSettingAnimationBasePosX = -86f;

	private const float HabitSettingDeactiveWidth = 372f;

	private const float HabitSettingDeactivePosX = 100f;

	private const float HabitSettingActiveWidth = 576f;

	private const float HabitSettingActivePosX = 202f;

	public Observable<Unit> OnClickAddHabit => onClickAddHabit;

	public Observable<Unit> OnClickHabitSetting => onClickHabitSetting;

	public Observable<bool> OnClickHabitSettingComplete => onClickHabitSettingComplete;

	public Observable<bool> OnClickHabitRemoveMode => onClickHabitRemoveMode;

	public Observable<bool> OnChangeWeek => weekView.OnChangeWeek;

	private void OnDestroy()
	{
		onClickAddHabit?.Dispose();
		onClickHabitSetting?.Dispose();
		onClickHabitSettingComplete?.Dispose();
		Cancel();
	}

	private void Start()
	{
		ObservableSubscribeExtensions.Subscribe(addHabitButton.OnClickAsObservable(), delegate
		{
			onClickAddHabit?.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(habitSettingButton.OnClickAsObservable(), delegate
		{
			if (!model.IsSettingMode)
			{
				habitSettingButton.interactable = false;
				PlaySettingButtonTween(isOpen: true);
				onClickHabitSetting?.OnNext(Unit.Default);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(habitSettingApplyButton.OnClickAsObservable(), delegate
		{
			if (model.IsSettingMode)
			{
				PlaySettingButtonTween(isOpen: false);
				onClickHabitSettingComplete?.OnNext(value: true);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(habitSettingRevertButton.OnClickAsObservable(), delegate
		{
			if (model.IsSettingMode)
			{
				PlaySettingButtonTween(isOpen: false);
				onClickHabitSettingComplete?.OnNext(value: false);
			}
		}).AddTo(this);
		habitRemoveModeButton.OnValueChanged.Subscribe(delegate(bool isRemoveMode)
		{
			onClickHabitRemoveMode?.OnNext(isRemoveMode);
		}).AddTo(this);
	}

	private void PlaySettingButtonTween(bool isOpen)
	{
		Cancel();
		animCancellationToken = new CancellationTokenSource();
		Sequence s = DOTween.Sequence();
		RectTransform rectTransform = habitSettingButton.transform as RectTransform;
		Vector2 sizeDelta = rectTransform.sizeDelta;
		sizeDelta.x = (isOpen ? 576f : 372f);
		float endValue = (isOpen ? 1f : 0f);
		s.Join(rectTransform.DOSizeDelta(sizeDelta, 0.25f)).Join(habitSettingChildGroup.DOFade(endValue, 0.25f)).OnComplete(delegate
		{
			if (isOpen)
			{
				habitSettingChildGroup.interactable = true;
				habitSettingChildGroup.blocksRaycasts = true;
			}
			else
			{
				habitSettingChildGroup.interactable = false;
				habitSettingChildGroup.blocksRaycasts = false;
				habitSettingButton.interactable = true;
			}
		})
			.ToUniTask(TweenCancelBehaviour.Kill, animCancellationToken.Token);
	}

	private void AddHabitButtonSetting(bool isRemoveMode)
	{
		addHabitButton.enabled = !isRemoveMode;
		addHabitButton.interactable = !isRemoveMode;
		addHabitDeactivate.gameObject.SetActive(isRemoveMode);
		addHabitDeactivate.alpha = (isRemoveMode ? 1f : 0f);
	}

	private void SettingButtonTransitionImmediate(bool isSettingMode, bool isRemoveMode)
	{
		Cancel();
		RectTransform obj = habitSettingButton.transform as RectTransform;
		Vector2 sizeDelta = obj.sizeDelta;
		sizeDelta.x = (isSettingMode ? 576f : 372f);
		obj.sizeDelta = sizeDelta;
		habitSettingChildGroup.alpha = (isSettingMode ? 1f : 0f);
		habitSettingChildGroup.interactable = isSettingMode && !isRemoveMode;
		habitSettingChildGroup.blocksRaycasts = isSettingMode && !isRemoveMode;
		habitSettingButton.enabled = !isRemoveMode;
		habitSettingButton.interactable = !isSettingMode && !isRemoveMode;
		habitSettingDeactivate.gameObject.SetActive(isRemoveMode);
		habitSettingDeactivate.alpha = (isRemoveMode ? 1f : 0f);
	}

	public void Setup()
	{
		weekView.Setup();
	}

	public void EnterSetting(HabitTrackerUIModel uiModel)
	{
		model = uiModel;
		AddHabitButtonSetting(model.IsRemoveMode);
		SettingButtonTransitionImmediate(uiModel.IsSettingMode, model.IsRemoveMode);
		habitRemoveModeButton.SetToggleWithoutTransition(uiModel.IsRemoveMode, isNotify: false);
		weekView.EnterSetting(uiModel);
		weekView.SetEditMode(uiModel.IsSettingMode);
	}

	public void ChangeWeek(bool isNext)
	{
		weekView.ChangeWeek(isNext);
	}

	public void UpdateRemoveButtonState(int habitTrackerCount)
	{
		bool flag = habitTrackerCount > 0;
		habitRemoveModeButton.gameObject.SetActive(flag);
		if (!flag && model != null && model.IsRemoveMode)
		{
			habitRemoveModeButton.SetToggleWithoutTransition(isOn: false, isNotify: false);
			onClickHabitRemoveMode?.OnNext(value: false);
		}
	}

	public void ChangeRemoveMode(bool isRemoveMode)
	{
		habitRemoveModeButton.SetToggleWithoutTransition(isRemoveMode, isNotify: false);
		addHabitButton.enabled = !isRemoveMode;
		addHabitButton.interactable = !isRemoveMode;
		addHabitDeactivate.gameObject.SetActive(isRemoveMode);
		addHabitDeactivate.alpha = (isRemoveMode ? 1f : 0f);
		habitSettingButton.enabled = !isRemoveMode;
		habitSettingButton.interactable = !isRemoveMode;
		habitSettingDeactivate.gameObject.SetActive(isRemoveMode);
		habitSettingDeactivate.alpha = (isRemoveMode ? 1f : 0f);
	}

	public void ChangeSettingMode(bool isSettingMode)
	{
		weekView.SetEditMode(isSettingMode);
	}

	public void UpdateMarkCompleteAll()
	{
		weekView.UpdateMarkCompleteAll();
	}

	public void UpdateMarkCompleteForDate(DateTime date)
	{
		weekView.UpdateMarkCompleteForDate(date);
	}

	public void Cancel()
	{
		if (animCancellationToken != null)
		{
			animCancellationToken.Cancel();
			animCancellationToken.Dispose();
			animCancellationToken = null;
		}
	}
}
