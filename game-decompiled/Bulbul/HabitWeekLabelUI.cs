using System;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class HabitWeekLabelUI : MonoBehaviour
{
	[SerializeField]
	private HabitDateLabelUI[] _dateLabelUIs;

	[SerializeField]
	private Button _prevWeekButton;

	[SerializeField]
	private Button _nextWeekButton;

	public Observable<Unit> OnClickPrevWeek => _prevWeekButton.OnClickAsObservable();

	public Observable<Unit> OnClickNextWeek => _nextWeekButton.OnClickAsObservable();

	public void Initialize()
	{
		HabitDateLabelUI[] dateLabelUIs = _dateLabelUIs;
		for (int i = 0; i < dateLabelUIs.Length; i++)
		{
			dateLabelUIs[i].Initialize();
		}
	}

	public void Setup(DateTime startDate, DateTime today)
	{
		for (int i = 0; i < _dateLabelUIs.Length; i++)
		{
			DateTime dateTime = startDate.AddDays(i);
			_dateLabelUIs[i].Setup(dateTime, dateTime == today, i == 0 || dateTime.Day == 1);
		}
	}

	public HabitDateLabelUI GetDateLabelUIByDayOfWeek(DayOfWeek dayOfWeek)
	{
		int num = (int)(dayOfWeek - 0);
		if (num < 0)
		{
			num += 7;
		}
		return _dateLabelUIs[num];
	}

	public HabitDateLabelUI GetDateLabelUIByDate(DateTime date)
	{
		HabitDateLabelUI[] dateLabelUIs = _dateLabelUIs;
		foreach (HabitDateLabelUI habitDateLabelUI in dateLabelUIs)
		{
			if (habitDateLabelUI.Date.IsSameDay(date))
			{
				return habitDateLabelUI;
			}
		}
		return null;
	}

	public void SetEditMode(bool isEditMode)
	{
		_prevWeekButton.gameObject.SetActive(!isEditMode);
		_nextWeekButton.gameObject.SetActive(!isEditMode);
		HabitDateLabelUI[] dateLabelUIs = _dateLabelUIs;
		for (int i = 0; i < dateLabelUIs.Length; i++)
		{
			dateLabelUIs[i].SetEditMode(isEditMode);
		}
	}
}
