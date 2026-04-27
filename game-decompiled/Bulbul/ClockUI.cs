using System;
using R3;
using TMPro;
using UnityEngine;

namespace Bulbul;

public class ClockUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _clockText;

	private int _displayHour;

	private int _displayMinute;

	private void Start()
	{
		_clockText.SetText("{0:00}:{1:00}", DateTime.Now.Hour, DateTime.Now.Minute);
		ObservableSubscribeExtensions.Subscribe(Observable.Interval(TimeSpan.FromSeconds(0.20000000298023224)), delegate
		{
			DateTime now = DateTime.Now;
			if (now.Hour != _displayHour || now.Minute != _displayMinute)
			{
				_clockText.SetText("{0:00}:{1:00}", now.Hour, now.Minute);
				int hour = now.Hour;
				int minute = now.Minute;
				_displayHour = hour;
				_displayMinute = minute;
			}
		}).AddTo(this);
	}

	private void OnDestroy()
	{
	}
}
