using System;
using System.Globalization;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class CurrentDateAndTimeUI : MonoBehaviour, ICurrentDateAndTimeUI
{
	private string AMLocalizeID = "ui_time_am";

	private string PMLocalizeID = "ui_time_pm";

	private string MidTimeLocalizeID = "ui_time_midtime";

	[Inject]
	private DateService _dateService;

	[Inject]
	private LanguageSupplier languageSupplier;

	[SerializeField]
	private TextMeshProUGUI _dateText;

	[SerializeField]
	private TextMeshProUGUI _timeText;

	[SerializeField]
	private TextLocalizationBehaviour _amPmText;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("Enの曜日と月の表記を省略系表記にするか")]
	private bool _isUsingENAbbreviation;

	private int _displayHour;

	private int _displayMinute;

	public void Setup()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.Merge<Unit>(_dateService.OnChangeTime.AsUnitObservable(), languageSupplier.Language.AsUnitObservable()), delegate
		{
			UpdateDateAndTime(DateTime.Now);
		}).AddTo(this);
	}

	private void UpdateDateAndTime(DateTime dateTime)
	{
		GameLanguageType lang = languageSupplier.Get();
		string text = GetDateText();
		_dateText.text = text;
		_timeText.text = GetTimeText();
		AdjustAMPMText();
		void AdjustAMPMText()
		{
			if (SaveDataManager.Instance.SettingData.TimeFormat.Value == TimeFormatType.All)
			{
				_amPmText.Text.text = string.Empty;
			}
			else if (lang == GameLanguageType.ChineseSimplified)
			{
				if (dateTime.Hour >= 0 && dateTime.Hour < 12)
				{
					_amPmText.Set(AMLocalizeID);
				}
				else if (dateTime.Hour >= 12 && dateTime.Hour < 13)
				{
					_amPmText.Set(MidTimeLocalizeID);
				}
				else
				{
					_amPmText.Set(PMLocalizeID);
				}
			}
			else
			{
				string text2 = SaveDataManager.Instance.SettingData.TimeFormat.Value switch
				{
					TimeFormatType.All => string.Empty, 
					TimeFormatType.AMPM => dateTime.ToString("tt", CultureInfo.CreateSpecificCulture("en-US")), 
					_ => string.Empty, 
				};
				if (text2 == "AM")
				{
					_amPmText.Set(AMLocalizeID);
				}
				else if (text2 == "PM")
				{
					_amPmText.Set(PMLocalizeID);
				}
				else
				{
					_amPmText.Text.text = string.Empty;
				}
			}
		}
		string GetDateText()
		{
			return lang switch
			{
				GameLanguageType.Japanese => dateTime.ToString("yyyy/MM/dd(ddd)", new CultureInfo("ja-JP")), 
				GameLanguageType.ChineseSimplified => dateTime.ToString("yyyy/MM/dd(ddd)", new CultureInfo("zh-CN")), 
				GameLanguageType.ChineseTraditional => dateTime.ToString("yyyy/MM/dd(ddd)", new CultureInfo("zh-TW")), 
				GameLanguageType.Portuguese => dateTime.ToString("dd/MM/yyyy(ddd)", new CultureInfo("pt-BR")), 
				GameLanguageType.Korean => dateTime.ToString("yyyy/MM/dd(ddd)", new CultureInfo("ko-KR")), 
				GameLanguageType.Russian => dateTime.ToString("dd/MM/yyyy(ddd)", new CultureInfo("ru-RU")), 
				_ => _isUsingENAbbreviation ? dateTime.ToString("ddd, MMM dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")) : dateTime.ToString("dddd, MMMM dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")), 
			};
		}
		string GetTimeText()
		{
			if (SaveDataManager.Instance.SettingData.TimeFormat.Value == TimeFormatType.All)
			{
				return dateTime.ToString("HH:mm");
			}
			if (lang == GameLanguageType.ChineseSimplified)
			{
				if (dateTime.Hour >= 0 && dateTime.Hour < 13)
				{
					return dateTime.ToString("HH:mm");
				}
				return dateTime.ToString("hh:mm");
			}
			return dateTime.ToString("hh:mm");
		}
	}

	public void Activate()
	{
		_canvasGroup.DOFade(1f, 0.2f);
	}

	public void Deactivate()
	{
		_canvasGroup.DOFade(0f, 0.2f);
	}
}
