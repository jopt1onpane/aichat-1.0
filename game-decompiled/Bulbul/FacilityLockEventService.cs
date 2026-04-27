using R3;

namespace Bulbul;

public class FacilityLockEventService
{
	private Subject<Unit> _onLock = new Subject<Unit>();

	private Subject<Unit> _onUnlock = new Subject<Unit>();

	public Observable<Unit> OnLock => _onLock;

	public Observable<Unit> OnUnlock => _onUnlock;

	public void SendLockEvent()
	{
		_onLock.OnNext(Unit.Default);
	}

	public void SendUnlockEvent()
	{
		_onUnlock.OnNext(Unit.Default);
	}
}
