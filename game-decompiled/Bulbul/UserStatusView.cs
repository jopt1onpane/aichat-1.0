using System;
using System.Globalization;
using Bulbul.Web;
using UnityEngine;

namespace Bulbul;

public class UserStatusView : MonoBehaviour
{
	[SerializeField]
	private TextLocalizationBehaviour levelText;

	[SerializeField]
	private TextLocalizationBehaviour pomodoroSecondsText;

	[SerializeField]
	private TextLocalizationBehaviour lastSaveDateText;

	[field: SerializeField]
	public ButtonEventObservable SubmitButton { get; private set; }

	public void Setup(UserStatus userStatus)
	{
		if (userStatus == null)
		{
			levelText.Text.SetText("Lv. ----");
			pomodoroSecondsText.Text.SetText("--:--:--");
			lastSaveDateText.Text.SetText("----/--/-- --:--:--");
			return;
		}
		levelText.Text.SetText("Lv. {0}", userStatus.Level);
		TimeSpan timeSpan = TimeSpan.FromSeconds(userStatus.PomodoroSeconds);
		int num = (int)timeSpan.TotalHours;
		pomodoroSecondsText.Text.SetText($"{num}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}");
		string lastSaveDate = userStatus.LastSaveDate;
		if (!string.IsNullOrEmpty(lastSaveDate))
		{
			if (DateTime.TryParseExact(lastSaveDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
			{
				TimeSpan offset = TimeSpan.FromHours(9.0);
				DateTimeOffset dateTimeOffset = new DateTimeOffset(result, offset);
				lastSaveDateText.Text.SetText(dateTimeOffset.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss"));
			}
			else
			{
				lastSaveDateText.Text.SetText(lastSaveDate + "(JST)");
			}
		}
		else
		{
			lastSaveDateText.Text.SetText("----/--/-- --:--:--");
		}
	}
}
