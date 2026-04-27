using R3;
using R3.Triggers;

namespace NestopiSystem;

public class OnApplicationFocusTrigger : ObservableTriggerBase
{
	private Subject<bool> onApplicationFocus;

	protected override void RaiseOnCompletedOnDestroy()
	{
		onApplicationFocus?.OnCompleted();
	}

	public void OnApplicationFocus(bool hasFocus)
	{
		onApplicationFocus?.OnNext(hasFocus);
	}

	public Observable<bool> OnApplicationFocusAsObservable()
	{
		return onApplicationFocus ?? (onApplicationFocus = new Subject<bool>());
	}
}
