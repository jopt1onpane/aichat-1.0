using System;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class HabitTrackerWeekView : MonoBehaviour
{
	[SerializeField]
	private HabitWeekLabelUI habitWeekLabel;

	private Subject<bool> onChangeWeek = new Subject<bool>();

	private HabitTrackerUIModel model;

	public Observable<bool> OnChangeWeek => onChangeWeek;

	private void Start()
	{
		ObservableSubscribeExtensions.Subscribe(habitWeekLabel.OnClickPrevWeek, delegate
		{
			onChangeWeek?.OnNext(value: false);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(habitWeekLabel.OnClickNextWeek, delegate
		{
			onChangeWeek?.OnNext(value: true);
		}).AddTo(this);
	}

	public void Setup()
	{
		habitWeekLabel.Initialize();
	}

	public void EnterSetting(HabitTrackerUIModel uiModel)
	{
		model = uiModel;
		habitWeekLabel.Setup(uiModel.CurrentStartDate, DateTime.Today);
		UpdateMarkCompleteAll();
	}

	public void ChangeWeek(bool isNext)
	{
		habitWeekLabel.Setup(model.CurrentStartDate, DateTime.Today);
		UpdateMarkCompleteAll();
	}

	public void SetEditMode(bool isSetting)
	{
		habitWeekLabel.SetEditMode(isSetting);
	}

	public void UpdateMarkCompleteForDate(DateTime date)
	{
		HabitDateLabelUI dateLabelUIByDate = habitWeekLabel.GetDateLabelUIByDate(date);
		if (!(dateLabelUIByDate == null))
		{
			dateLabelUIByDate.SetCompleted(model.IsAllCompletedForDate(date));
		}
	}

	public void UpdateMarkCompleteAll()
	{
		DateTime currentStartDate = model.CurrentStartDate;
		for (int i = 0; i <= 6; i++)
		{
			DateTime date = currentStartDate.AddDays(i);
			HabitDateLabelUI dateLabelUIByDate = habitWeekLabel.GetDateLabelUIByDate(date);
			if (!(dateLabelUIByDate == null))
			{
				dateLabelUIByDate.SetCompleted(model.IsAllCompletedForDate(date));
			}
		}
	}
}
