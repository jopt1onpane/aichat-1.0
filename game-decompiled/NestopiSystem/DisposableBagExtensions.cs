using R3;

namespace NestopiSystem;

public static class DisposableBagExtensions
{
	public static void DisposeAndRecreate(this ref DisposableBag self)
	{
		self.Dispose();
		self = default(DisposableBag);
	}

	public static void DisposeAndRecreate(this ref DisposableBag self, int capacity)
	{
		self.Dispose();
		self = new DisposableBag(capacity);
	}

	public static void ClearAndRecreate(this ref DisposableBag self)
	{
		self.Clear();
		self = default(DisposableBag);
	}

	public static void ClearAndRecreate(this ref DisposableBag self, int capacity)
	{
		self.Clear();
		self = new DisposableBag(capacity);
	}
}
