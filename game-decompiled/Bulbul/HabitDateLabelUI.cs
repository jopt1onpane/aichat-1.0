using System;
using NestopiSystem.DIContainers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class HabitDateLabelUI : MonoBehaviour
{
	[SerializeField]
	private Image _completedImage;

	[SerializeField]
	private GameObject _todayObj;

	[SerializeField]
	private TMP_Text _dayNumText;

	[SerializeField]
	private TextLocalizationBehaviour _dayOfWeekText;

	[SerializeField]
	private TextLocalizationBehaviour _dayOfWeekTextForEditMode;

	[SerializeField]
	private GameObject _monthObj;

	[SerializeField]
	private TMP_Text _monthText;

	private LanguageSupplier _languageSupplier;

	private DateTime _dateTime;

	private bool _isToday;

	private bool _isEditMode;

	private bool _isCompleted;

	private bool _showMonth;

	public DateTime Date => _dateTime;

	public void Initialize()
	{
		_languageSupplier = ProjectLifetimeScope.Resolve<LanguageSupplier>();
		_languageSupplier.Language.Skip(1).Subscribe(this, delegate(GameLanguageType _, HabitDateLabelUI @this)
		{
			@this.UpdateDayText();
		}).AddTo(this);
	}

	public void Setup(DateTime dateTime, bool isToday, bool showMonth)
	{
		_dateTime = dateTime;
		_isToday = isToday;
		_monthText.text = dateTime.Month.ToString();
		_showMonth = showMonth;
		UpdateDayText();
		UpdateDisplay();
	}

	private void UpdateDayText()
	{
		_dayNumText.text = $"{_dateTime.Day}";
		_dayOfWeekText.Set(MyDefine.GetDayOfWeekLocalizeID(_dateTime.DayOfWeek));
		_dayOfWeekTextForEditMode.Set(MyDefine.GetDayOfWeekLocalizeID(_dateTime.DayOfWeek));
	}

	public void SetCompleted(bool isComplete)
	{
		_isCompleted = isComplete;
		UpdateDisplay();
	}

	public void SetEditMode(bool isEditMode)
	{
		_isEditMode = isEditMode;
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		_todayObj.SetActive(_isToday && !_isEditMode);
		_dayNumText.gameObject.SetActive(!_isEditMode);
		_completedImage.gameObject.SetActive(_isCompleted && !_isEditMode);
		_dayNumText.gameObject.SetActive(!_isEditMode);
		_dayOfWeekText.gameObject.SetActive(!_isEditMode);
		_dayOfWeekTextForEditMode.gameObject.SetActive(_isEditMode);
		_monthObj.SetActive(_showMonth && !_isEditMode);
	}
}
