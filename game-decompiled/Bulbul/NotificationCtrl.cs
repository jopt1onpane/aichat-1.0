using System;
using System.Collections.Generic;
using NestopiSystem.DIContainers;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class NotificationCtrl
{
	public enum PushChannel
	{
		None,
		Pomodoro,
		Remind
	}

	public enum PushType
	{
		Rest,
		Concentration
	}

	public struct PushTimer(PushType type, int timer)
	{
		public PushType type = type;

		public int timer = timer;
	}

	[Inject]
	private LocalizationMasterWrapper localizationMasterWrapper;

	private static NotificationCtrl instance;

	public ILocalNotification notification { get; private set; }

	public static NotificationCtrl GetInstance()
	{
		if (instance == null)
		{
			instance = new NotificationCtrl();
		}
		return instance;
	}

	private NotificationCtrl()
	{
		if (localizationMasterWrapper == null)
		{
			localizationMasterWrapper = ProjectLifetimeScope.Resolve<LocalizationMasterWrapper>();
		}
		Initialize();
	}

	public void Initialize()
	{
		notification.CreateChannel("notification", "Chill with You", "Notification Channel");
	}

	public void SetPomodoroPush(bool isPomodoroPush, IReadOnlyList<PushTimer> pushTimers)
	{
		notification.CancelType(PushChannel.Pomodoro);
		if (!isPomodoroPush)
		{
			return;
		}
		DateTime now = DateTime.Now;
		int num = 0;
		for (int i = 0; i < pushTimers.Count; i++)
		{
			string title = "";
			string text = "";
			num += pushTimers[i].timer;
			if (i == pushTimers.Count - 1)
			{
				string localizeID = "push_notifications_pomodoro_complete_title";
				localizationMasterWrapper.TryGet(localizeID, out var result);
				string localizeID2 = "push_notifications_pomodoro_complete_" + (UnityEngine.Random.Range(0, 3) + 1).ToString("d");
				localizationMasterWrapper.TryGet(localizeID2, out var result2);
				title = result;
				text = result2;
			}
			else if (pushTimers[i].type == PushType.Concentration)
			{
				string localizeID3 = "push_notifications_pomodoro_work_title";
				localizationMasterWrapper.TryGet(localizeID3, out var result3);
				string localizeID4 = "push_notifications_pomodoro_start_work_" + (UnityEngine.Random.Range(0, 3) + 1).ToString("d");
				localizationMasterWrapper.TryGet(localizeID4, out var result4);
				title = result3;
				text = result4;
			}
			else if (pushTimers[i].type == PushType.Rest)
			{
				string localizeID5 = "push_notifications_pomodoro_break_title";
				localizationMasterWrapper.TryGet(localizeID5, out var result5);
				string localizeID6 = "push_notifications_pomodoro_start_break_" + (UnityEngine.Random.Range(0, 3) + 1).ToString("d");
				localizationMasterWrapper.TryGet(localizeID6, out var result6);
				title = result5;
				text = result6;
			}
			notification.Schedule(PushChannel.Pomodoro, 1, title, text, now.AddMinutes(num));
		}
	}

	public void SetRemindPush(bool isRemindPush)
	{
		notification.CancelType(PushChannel.Remind);
		if (isRemindPush)
		{
			DateTime now = DateTime.Now;
			string text = "";
			string text2 = "";
			int[] array = new int[6] { 1, 2, 3, 5, 7, 15 };
			int[] array2 = new int[6] { 1, 2, 2, 1, 1, 1 };
			int[] array3 = new int[6] { 1, 2, 2, 1, 1, 1 };
			for (int i = 0; i < array.Length; i++)
			{
				string localizeID = ((array3[i] != 1) ? "push_notifications_inactivity_satone" : "push_notifications_inactivity_title");
				localizationMasterWrapper.TryGet(localizeID, out var result);
				int num = array[i];
				int num2 = UnityEngine.Random.Range(0, array2[i]) + 1;
				string localizeID2 = "push_notifications_inactivity_days_" + num.ToString("d") + "_type_" + num2.ToString("d");
				localizationMasterWrapper.TryGet(localizeID2, out var result2);
				text = result;
				text2 = result2;
				notification.Schedule(PushChannel.Remind, array3[i], text, text2, now.AddDays(num));
			}
		}
	}
}
