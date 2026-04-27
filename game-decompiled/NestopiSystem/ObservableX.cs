using System;
using System.Threading;
using R3;
using UnityEngine;

namespace NestopiSystem;

public static class ObservableX
{
	public static Observable<Unit> OnApplicationQuiting(CancellationToken cancellationToken = default(CancellationToken))
	{
		return Observable.FromEvent(delegate(Action h)
		{
			Application.quitting += h;
		}, delegate(Action h)
		{
			Application.quitting -= h;
		}, cancellationToken);
	}

	public static Observable<Unit> OnApplicationQuitAsObservable(this Component component)
	{
		if (component == null)
		{
			return Observable.Empty<Unit>();
		}
		return component.GetOrAddComponent<OnApplicationQuitTrigger>().OnApplicationQuitAsObservable();
	}

	public static Observable<bool> OnApplicationPauseAsObservable(this Component component)
	{
		if (component == null)
		{
			return Observable.Empty<bool>();
		}
		return component.GetOrAddComponent<OnApplicationPauseTrigger>().OnApplicationPauseAsObservable();
	}

	public static Observable<bool> OnApplicationFocusAsObservable(this Component component)
	{
		if (component == null)
		{
			return Observable.Empty<bool>();
		}
		return component.GetOrAddComponent<OnApplicationFocusTrigger>().OnApplicationFocusAsObservable();
	}
}
