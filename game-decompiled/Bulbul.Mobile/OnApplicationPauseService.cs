using System;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class OnApplicationPauseService : MonoBehaviour
{
	private Subject<DateTime> _onSuspend = new Subject<DateTime>();

	private Subject<(DateTime, double)> _onResume = new Subject<(DateTime, double)>();

	private DateTime _suspendDateTime;

	public Observable<DateTime> OnSuspend => _onSuspend;

	public Observable<(DateTime, double)> OnResume => _onResume;

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			_suspendDateTime = DateTime.Now;
			_onSuspend.OnNext(_suspendDateTime);
		}
		else
		{
			DateTime now = DateTime.Now;
			double totalSeconds = (now - _suspendDateTime).TotalSeconds;
			_onResume.OnNext((now, totalSeconds));
		}
	}
}
