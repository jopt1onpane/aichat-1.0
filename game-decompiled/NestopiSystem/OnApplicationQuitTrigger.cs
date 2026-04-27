using R3;
using R3.Triggers;

namespace NestopiSystem;

public class OnApplicationQuitTrigger : ObservableTriggerBase
{
	private Subject<Unit> onApplicationQuit;

	protected override void RaiseOnCompletedOnDestroy()
	{
		onApplicationQuit?.OnCompleted();
	}

	public void OnApplicationQuit()
	{
		onApplicationQuit?.OnNext(Unit.Default);
	}

	public Observable<Unit> OnApplicationQuitAsObservable()
	{
		return onApplicationQuit ?? (onApplicationQuit = new Subject<Unit>());
	}
}
