using R3;
using R3.Triggers;

namespace NestopiSystem;

public class OnApplicationPauseTrigger : ObservableTriggerBase
{
	private Subject<bool> onApplicationPause;

	protected override void RaiseOnCompletedOnDestroy()
	{
		onApplicationPause?.OnCompleted();
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		onApplicationPause?.OnNext(pauseStatus);
	}

	public Observable<bool> OnApplicationPauseAsObservable()
	{
		return onApplicationPause ?? (onApplicationPause = new Subject<bool>());
	}
}
