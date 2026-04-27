using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace NestopiSystem;

public static class CancellationTokenExtensions
{
	public static CancellationTokenSource CreateLinkedTokenSource(this CancellationToken token)
	{
		return CancellationTokenSource.CreateLinkedTokenSource(token);
	}

	public static CancellationTokenSource CreateLinkedTokenSource(this CancellationToken token1, CancellationToken token2)
	{
		return CancellationTokenSource.CreateLinkedTokenSource(token1, token2);
	}

	public static void CancelDispose(this CancellationTokenSource self)
	{
		if (!self.IsCancellationRequested)
		{
			self.Cancel();
			self.Dispose();
		}
	}

	public static CancellationTokenRegistration RegisterCompletionSource<T>(this CancellationToken self, UniTaskCompletionSource<T> ucs)
	{
		return self.RegisterWithoutCaptureExecutionContext(delegate(object state)
		{
			((UniTaskCompletionSource<T>)state).TrySetCanceled();
		}, ucs);
	}

	public static CancellationTokenRegistration RegisterCompletionSource(this CancellationToken self, UniTaskCompletionSource ucs)
	{
		return self.RegisterWithoutCaptureExecutionContext(delegate(object state)
		{
			((UniTaskCompletionSource)state).TrySetCanceled();
		}, ucs);
	}

	public static CancellationTokenRegistration RegisterWithState<T>(this CancellationToken self, Action<T> action, T state)
	{
		return self.Register(delegate(object obj)
		{
			var (action2, obj2) = ((Action<T>, T))obj;
			action2(obj2);
		}, (action, state));
	}
}
