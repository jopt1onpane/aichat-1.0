using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class HabitTrackerListItemViewDayUI : MonoBehaviour
{
	[Header("通常オブジェクト")]
	[SerializeField]
	private GameObject normalObjectRoot;

	[SerializeField]
	private CanvasGroup normalObjectGroup;

	[SerializeField]
	private Button checkButton;

	[SerializeField]
	private Image checkMarkDeadPeriod;

	[SerializeField]
	private Image checkMarkDeactive;

	[SerializeField]
	private Image checkMarkActive;

	[SerializeField]
	private GameObject cellRightLineObj;

	[SerializeField]
	private Image effectImage;

	[Header("設定中オブジェクト")]
	[SerializeField]
	private GameObject settingObjectRoot;

	[SerializeField]
	private CanvasGroup settingObjectGroup;

	[SerializeField]
	private Button settingButton;

	[SerializeField]
	private RectTransform minusButton;

	[SerializeField]
	private RectTransform plusButton;

	private HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel uiModel;

	private CancellationTokenSource settingAnimationTokenSource;

	private Tween dayAnimationTween;

	private Subject<(DateTime date, bool isCompleted)> onChangeHabitCompleted = new Subject<(DateTime, bool)>();

	private Subject<(DayOfWeek date, bool enable)> onChangeHabitEnabled = new Subject<(DayOfWeek, bool)>();

	public Observable<(DateTime date, bool isCompleted)> OnChangeHabitCompleted => onChangeHabitCompleted;

	public Observable<(DayOfWeek date, bool enable)> OnChangeHabitEnabled => onChangeHabitEnabled;

	private void OnDestroy()
	{
		onChangeHabitCompleted?.Dispose();
		onChangeHabitEnabled?.Dispose();
		CheckAnimationCancel();
		SettingAnimationCancel();
	}

	public void Initialize()
	{
		if (settingAnimationTokenSource == null)
		{
			settingAnimationTokenSource = new CancellationTokenSource();
		}
		ObservableSubscribeExtensions.Subscribe(checkButton.OnClickAsObservable(), delegate
		{
			onChangeHabitCompleted?.OnNext((uiModel.date, !uiModel.isChecked));
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingButton.OnClickAsObservable(), delegate
		{
			onChangeHabitEnabled?.OnNext((uiModel.date.DayOfWeek, (uiModel.enableState != HabitDateEnableState.Enabled) ? true : false));
		}).AddTo(this);
	}

	public void Setup(HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel model, bool isSettingMode)
	{
		dayAnimationTween?.Complete();
		dayAnimationTween = null;
		uiModel = model;
		if (!isSettingMode)
		{
			normalObjectRoot.SetActive(value: true);
			normalObjectGroup.alpha = 1f;
			settingObjectRoot.SetActive(value: false);
			settingObjectGroup.alpha = 0f;
			if (model.isChecked)
			{
				checkButton.enabled = true;
				checkButton.interactable = true;
				checkMarkActive.gameObject.SetActive(model.isChecked);
				effectImage.gameObject.SetActive(model.isChecked);
				checkMarkDeadPeriod.gameObject.SetActive(value: false);
				checkMarkDeactive.gameObject.SetActive(value: false);
				return;
			}
			effectImage.gameObject.SetActive(value: false);
			SetActiveCellRightLineObj(model.IsCellDisplay());
			switch (model.enableState)
			{
			case HabitDateEnableState.Enabled:
				checkButton.enabled = true;
				checkButton.interactable = true;
				checkMarkActive.gameObject.SetActive(model.isChecked);
				checkMarkDeactive.gameObject.SetActive(!model.isChecked);
				checkMarkDeadPeriod.gameObject.SetActive(value: false);
				checkMarkDeactive.color = Color.white;
				break;
			case HabitDateEnableState.Disabled:
			case HabitDateEnableState.DeadPeriod:
				ChangeCheckDisableAndDeadPeriodMode();
				break;
			}
		}
		else
		{
			normalObjectRoot.SetActive(value: false);
			normalObjectGroup.alpha = 0f;
			checkButton.enabled = false;
			effectImage.gameObject.SetActive(value: false);
			SetActiveCellRightLineObj(model.IsCellDisplay());
			settingObjectRoot.SetActive(value: true);
			settingObjectGroup.alpha = 1f;
			switch (model.enableState)
			{
			case HabitDateEnableState.Enabled:
				minusButton.gameObject.SetActive(value: true);
				plusButton.gameObject.SetActive(value: false);
				break;
			case HabitDateEnableState.Disabled:
			case HabitDateEnableState.DeadPeriod:
				minusButton.gameObject.SetActive(value: false);
				plusButton.gameObject.SetActive(value: true);
				break;
			}
		}
	}

	private void ChangeCheckDisableAndDeadPeriodMode()
	{
		checkButton.enabled = false;
		checkButton.interactable = false;
		checkMarkDeadPeriod.gameObject.SetActive(value: true);
		checkMarkDeactive.gameObject.SetActive(value: false);
		checkMarkActive.gameObject.SetActive(value: false);
	}

	private void SetActiveCellRightLineObj(bool active)
	{
		if (cellRightLineObj.activeSelf != active)
		{
			cellRightLineObj.SetActive(active);
		}
	}

	public void SetComplete(bool isComplete, HabitDateEnableState prevState)
	{
		if (isComplete)
		{
			CircleToCheckMarkAnimation();
			return;
		}
		effectImage.gameObject.SetActive(value: false);
		switch (uiModel.enableState)
		{
		case HabitDateEnableState.Enabled:
			checkMarkDeactive.color = Color.white;
			CheckMarkToCircleAnimation(isButtonActivate: true);
			break;
		case HabitDateEnableState.Disabled:
			checkMarkDeactive.color = Color.white.WithA(0.3125f);
			CheckMarkToCircleAnimation(isButtonActivate: false);
			break;
		case HabitDateEnableState.DeadPeriod:
			checkMarkDeadPeriod.color = Color.white.WithA(0.3125f);
			CheckMarkToPeriodAnimation();
			break;
		}
	}

	public void SetEnable(bool enable)
	{
		switch (uiModel.enableState)
		{
		case HabitDateEnableState.Enabled:
			PlusToMinusAnimation();
			break;
		case HabitDateEnableState.Disabled:
		case HabitDateEnableState.DeadPeriod:
			MinusToPlusAnimation();
			break;
		}
	}

	private void MinusToPlusAnimation()
	{
		SettingAnimationCancel();
		settingAnimationTokenSource = new CancellationTokenSource();
		Sequence s = DOTween.Sequence();
		RectTransform minusRect = minusButton.transform as RectTransform;
		RectTransform plusRect = plusButton.transform as RectTransform;
		minusButton.gameObject.SetActive(value: true);
		plusButton.gameObject.SetActive(value: true);
		plusRect.localScale = Vector3.zero;
		s.Join(minusRect.DOScale(0f, 0.2f).SetEase(Ease.OutCubic)).Join(plusRect.DOScale(1f, 0.2f).SetEase(Ease.InCubic)).OnComplete(delegate
		{
			minusButton.gameObject.SetActive(value: false);
			minusRect.localScale = Vector3.one;
			plusRect.localScale = Vector3.one;
		})
			.ToUniTask(TweenCancelBehaviour.Kill, settingAnimationTokenSource.Token);
	}

	private void PlusToMinusAnimation()
	{
		SettingAnimationCancel();
		settingAnimationTokenSource = new CancellationTokenSource();
		Sequence s = DOTween.Sequence();
		RectTransform minusRect = minusButton.transform as RectTransform;
		RectTransform plusRect = plusButton.transform as RectTransform;
		minusButton.gameObject.SetActive(value: true);
		plusButton.gameObject.SetActive(value: true);
		minusRect.localScale = Vector3.zero;
		s.Join(minusRect.DOScale(1f, 0.2f).SetEase(Ease.InCubic)).Join(plusRect.DOScale(0f, 0.2f).SetEase(Ease.OutCubic)).OnComplete(delegate
		{
			plusButton.gameObject.SetActive(value: false);
			minusRect.localScale = Vector3.one;
			plusRect.localScale = Vector3.one;
		})
			.ToUniTask(TweenCancelBehaviour.Kill, settingAnimationTokenSource.Token);
	}

	private void SettingAnimationCancel()
	{
		if (settingAnimationTokenSource != null)
		{
			settingAnimationTokenSource.Cancel();
			settingAnimationTokenSource.Dispose();
			settingAnimationTokenSource = null;
		}
	}

	public void ChangeHabitPeriod(HabitTrackerListItemModel.HabitTrackerListItemDayInfoModel model)
	{
		HabitDateEnableState enableState = uiModel.enableState;
		uiModel = model;
		if (model.isChecked)
		{
			return;
		}
		HabitDateEnableState enableState2 = uiModel.enableState;
		_ = checkMarkDeactive.gameObject.activeSelf;
		_ = checkMarkDeadPeriod.gameObject.activeSelf;
		if (enableState2 == HabitDateEnableState.DeadPeriod || enableState2 == HabitDateEnableState.Disabled)
		{
			checkMarkDeadPeriod.color = Color.white.WithA(0.3125f);
			if (enableState == HabitDateEnableState.Enabled)
			{
				CircleToPeriodAnimation();
			}
		}
		else
		{
			checkMarkDeactive.color = ((enableState2 == HabitDateEnableState.Enabled) ? Color.white : Color.white.WithA(0.3125f));
			if (uiModel.enableState == HabitDateEnableState.Enabled)
			{
				PeriodToCircleAnimation(enableState2 == HabitDateEnableState.Enabled);
			}
		}
	}

	private void CircleToCheckMarkAnimation()
	{
		CheckAnimationCancel();
		RectTransform rectTransform = checkMarkActive.rectTransform;
		RectTransform rectTransform2 = effectImage.rectTransform;
		checkButton.enabled = false;
		checkButton.interactable = false;
		rectTransform.localScale = Vector3.zero;
		rectTransform2.localScale = Vector3.one + Vector3.down;
		effectImage.gameObject.SetActive(value: true);
		checkMarkActive.gameObject.SetActive(value: true);
		checkMarkDeadPeriod.gameObject.SetActive(value: false);
		dayAnimationTween = DOTween.Sequence().Join(rectTransform.DOScale(1f, 0.25f).SetEase(Ease.OutCubic)).Join(rectTransform2.DOScaleY(1f, 0.25f).SetEase(Ease.InOutExpo))
			.OnComplete(delegate
			{
				checkButton.enabled = true;
				checkButton.interactable = true;
				checkMarkDeactive.gameObject.SetActive(value: false);
			});
	}

	private void CheckMarkToCircleAnimation(bool isButtonActivate)
	{
		CheckAnimationCancel();
		Sequence s = DOTween.Sequence();
		RectTransform checkRect = checkMarkActive.rectTransform;
		RectTransform rectTransform = checkMarkDeadPeriod.rectTransform;
		RectTransform effectRect = effectImage.rectTransform;
		checkButton.enabled = false;
		checkButton.interactable = false;
		checkMarkDeactive.gameObject.SetActive(value: true);
		checkMarkDeadPeriod.gameObject.SetActive(value: false);
		if (uiModel.enableState == HabitDateEnableState.Disabled)
		{
			checkMarkDeactive.gameObject.SetActive(value: false);
			checkMarkDeadPeriod.gameObject.SetActive(value: true);
			SetActiveCellRightLineObj(active: true);
			rectTransform.localScale = Vector3.zero;
			s.Join(rectTransform.DOScale(1f, 0.25f).SetEase(Ease.OutCubic));
		}
		dayAnimationTween = s.Join(checkRect.DOScale(0f, 0.25f).SetEase(Ease.OutCubic)).Join(effectRect.DOScaleY(0f, 0.25f).SetEase(Ease.InOutCubic)).OnComplete(delegate
		{
			checkButton.enabled = isButtonActivate;
			checkButton.interactable = isButtonActivate;
			checkMarkActive.gameObject.SetActive(value: false);
			effectImage.gameObject.SetActive(value: false);
			checkRect.localScale = Vector3.one;
			effectRect.localScale = Vector3.one;
			if (uiModel.enableState == HabitDateEnableState.Disabled)
			{
				ChangeCheckDisableAndDeadPeriodMode();
			}
		});
	}

	private void CheckMarkToPeriodAnimation()
	{
		CheckAnimationCancel();
		RectTransform checkRect = checkMarkActive.rectTransform;
		RectTransform effectRect = effectImage.rectTransform;
		RectTransform rectTransform = checkMarkDeadPeriod.rectTransform;
		checkButton.enabled = false;
		checkButton.interactable = false;
		checkMarkDeadPeriod.gameObject.SetActive(value: true);
		rectTransform.localScale = Vector3.zero;
		dayAnimationTween = DOTween.Sequence().Join(checkRect.DOScale(0f, 0.25f).SetEase(Ease.OutCubic)).Join(effectRect.DOScaleY(0f, 0.25f).SetEase(Ease.OutCubic))
			.Join(rectTransform.DOScale(1f, 0.25f).SetEase(Ease.InQuart))
			.OnComplete(delegate
			{
				checkMarkActive.gameObject.SetActive(value: false);
				effectImage.gameObject.SetActive(value: false);
				checkRect.localScale = Vector3.one;
				effectRect.localScale = Vector3.one;
			});
	}

	private void PeriodToCircleAnimation(bool isButtonActivate)
	{
		CheckAnimationCancel();
		RectTransform rectTransform = checkMarkDeactive.rectTransform;
		RectTransform periodRect = checkMarkDeadPeriod.rectTransform;
		checkButton.enabled = false;
		checkButton.interactable = false;
		rectTransform.localScale = Vector3.zero;
		periodRect.localScale = Vector3.one;
		checkMarkDeactive.gameObject.SetActive(value: true);
		checkMarkDeadPeriod.gameObject.SetActive(value: true);
		dayAnimationTween = DOTween.Sequence().Join(rectTransform.DOScale(1f, 0.25f).SetEase(Ease.InQuart)).Join(periodRect.DOScale(0f, 0.25f).SetEase(Ease.OutQuart))
			.OnComplete(delegate
			{
				checkButton.enabled = isButtonActivate;
				checkButton.interactable = isButtonActivate;
				checkMarkDeadPeriod.gameObject.SetActive(value: false);
				periodRect.localScale = Vector3.one;
			});
	}

	private void CircleToPeriodAnimation()
	{
		CheckAnimationCancel();
		RectTransform circleRect = checkMarkDeactive.rectTransform;
		RectTransform rectTransform = checkMarkDeadPeriod.rectTransform;
		checkButton.enabled = false;
		checkButton.interactable = false;
		rectTransform.localScale = Vector3.zero;
		checkMarkDeactive.gameObject.SetActive(value: true);
		checkMarkDeadPeriod.gameObject.SetActive(value: true);
		dayAnimationTween = DOTween.Sequence().Join(circleRect.DOScale(0f, 0.25f).SetEase(Ease.OutQuart)).Join(rectTransform.DOScale(1f, 0.25f).SetEase(Ease.InQuart))
			.OnComplete(delegate
			{
				checkMarkDeactive.gameObject.SetActive(value: false);
				circleRect.localScale = Vector3.one;
			});
	}

	public void CheckAnimationCancel()
	{
		if (dayAnimationTween != null)
		{
			dayAnimationTween.Kill();
			dayAnimationTween = null;
		}
	}
}
