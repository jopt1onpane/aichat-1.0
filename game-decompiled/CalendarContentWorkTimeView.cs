using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalendarContentWorkTimeView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _timeText;

	[SerializeField]
	private Button _editButton;

	public Observable<Unit> OnClickEditButton => _editButton.OnClickAsObservable();

	public void SetWorkTimeText(TimeSpan timespan)
	{
		if (timespan.TotalHours >= 24.0)
		{
			_timeText.SetText("{0:00}:{1:00}:{2:00}", 24f, 0f, 0f);
		}
		else
		{
			_timeText.SetText("{0:00}:{1:00}:{2:00}", timespan.Hours, timespan.Minutes, timespan.Seconds);
		}
	}
}
