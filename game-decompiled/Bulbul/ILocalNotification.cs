using System;

namespace Bulbul;

public interface ILocalNotification
{
	string SmallIcon { get; set; }

	string LargeIcon { get; set; }

	void CreateChannel(string channelId, string channelName, string description);

	void CancelAll();

	void CancelType(NotificationCtrl.PushChannel channel);

	string Schedule(NotificationCtrl.PushChannel channel, int iconType, string title, string text, DateTime fireTime);

	string ScheduleInSeconds(NotificationCtrl.PushChannel channel, int iconType, string title, string text, int seconds);

	string ScheduleInMinutes(NotificationCtrl.PushChannel channel, int iconType, string title, string text, int minutes);

	string ScheduleInHours(NotificationCtrl.PushChannel channel, int iconType, string title, string text, int hours);
}
