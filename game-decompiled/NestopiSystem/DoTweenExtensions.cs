using DG.Tweening;

namespace NestopiSystem;

public static class DoTweenExtensions
{
	private static readonly DOTweenSynchronizer synchronizer = new DOTweenSynchronizer();

	public static Tween RegisterForSync(this Tween tween, string syncKey = null, bool autoUnregister = true)
	{
		return synchronizer.Register(tween, syncKey);
	}

	public static void UnregisterForSync(this Tween tween, string syncKey = null)
	{
		synchronizer.Unregister(tween, syncKey);
	}

	public static Tween Sync(this Tween tween, string syncKey = null)
	{
		return synchronizer.Sync(tween, syncKey);
	}
}
