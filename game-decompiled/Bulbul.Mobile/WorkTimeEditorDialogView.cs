using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class WorkTimeEditorDialogView : MonoBehaviour
{
	private static readonly string _format = "{0:D2}";

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private TMP_InputField _hourInputField;

	[SerializeField]
	private TMP_InputField _minInputField;

	[SerializeField]
	private TMP_InputField _secInputField;

	[SerializeField]
	private Button _okButton;

	[SerializeField]
	private Button _cancelButton;

	[SerializeField]
	[Header("実態は画面外判定用BGボタン")]
	private Button _closeButton;

	private Subject<double> _onSubmit = new Subject<double>();

	private string _prevInputHour = "";

	private string _prevInputMin = "";

	private string _prevInputSec = "";

	public Observable<double> OnSubmit => _onSubmit;

	public void Setup()
	{
		ObservableSubscribeExtensions.Subscribe(_okButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayClick();
			OnClickOkButton();
			Close();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_cancelButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayCancel();
			OnClickCancelButton();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_closeButton.OnClickAsObservable(), delegate
		{
			OnClickCancelButton();
		}).AddTo(this);
		_hourInputField.OnEndEditAsObservable().Subscribe(delegate(string str)
		{
			if (!int.TryParse(str, out var result) || CheckInvalidValue(result, 99))
			{
				_hourInputField.SetTextWithoutNotify(_prevInputHour);
			}
			else if (result >= 24)
			{
				_hourInputField.SetTextWithoutNotify("24");
				_minInputField.SetTextWithoutNotify("00");
				_secInputField.SetTextWithoutNotify("00");
				_prevInputMin = _minInputField.text;
				_prevInputSec = _secInputField.text;
				_prevInputHour = _hourInputField.text;
			}
			else
			{
				_hourInputField.SetTextWithoutNotify(string.Format(_format, result));
				_prevInputHour = _hourInputField.text;
			}
		}).AddTo(this);
		_minInputField.OnEndEditAsObservable().Subscribe(delegate(string str)
		{
			if (!int.TryParse(str, out var result) || CheckInvalidValue(result, 99))
			{
				_minInputField.SetTextWithoutNotify(_prevInputMin);
			}
			else if (_hourInputField.text == "24")
			{
				_minInputField.SetTextWithoutNotify("00");
				_secInputField.SetTextWithoutNotify("00");
				_prevInputMin = _minInputField.text;
				_prevInputSec = _secInputField.text;
			}
			else
			{
				if (result >= 60)
				{
					result = 59;
				}
				_minInputField.SetTextWithoutNotify(string.Format(_format, result));
				_prevInputMin = _minInputField.text;
			}
		}).AddTo(this);
		_secInputField.OnEndEditAsObservable().Subscribe(delegate(string str)
		{
			if (!int.TryParse(str, out var result) || CheckInvalidValue(result, 99))
			{
				_secInputField.SetTextWithoutNotify(_prevInputSec);
			}
			else if (_hourInputField.text == "24")
			{
				_minInputField.SetTextWithoutNotify("00");
				_secInputField.SetTextWithoutNotify("00");
				_prevInputMin = _minInputField.text;
				_prevInputSec = _secInputField.text;
			}
			else
			{
				if (result >= 60)
				{
					result = 59;
				}
				_secInputField.SetTextWithoutNotify(string.Format(_format, result));
				_prevInputSec = _secInputField.text;
			}
		}).AddTo(this);
	}

	public void OnClickOkButton()
	{
		if (int.TryParse(_hourInputField.text, out var result) && int.TryParse(_minInputField.text, out var result2) && int.TryParse(_secInputField.text, out var result3))
		{
			_onSubmit.OnNext(new TimeSpan(result, result2, result3).TotalSeconds);
		}
	}

	private bool CheckInvalidValue(int num, int max, int min = 0)
	{
		if (num > max)
		{
			return true;
		}
		return num < min;
	}

	public void OnClickCancelButton()
	{
		Close();
	}

	public void Open(int hour, int min, int sec)
	{
		_systemSeService.PlaySelect();
		base.gameObject.SetActive(value: true);
		_hourInputField.SetTextWithoutNotify(string.Format(_format, hour));
		_minInputField.SetTextWithoutNotify(string.Format(_format, min));
		_secInputField.SetTextWithoutNotify(string.Format(_format, sec));
		_prevInputHour = _hourInputField.text;
		_prevInputMin = _minInputField.text;
		_prevInputSec = _secInputField.text;
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
	}
}
