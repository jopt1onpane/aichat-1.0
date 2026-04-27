using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace NestopiSystem;

public class AsyncLock
{
	public readonly struct AsyncLockScope(SemaphoreSlim semaphore) : IDisposable
	{
		private readonly SemaphoreSlim semaphore = semaphore;

		public void Dispose()
		{
			semaphore?.Release();
		}
	}

	private readonly SemaphoreSlim semaphore;

	public static readonly UniTask<AsyncLockScope> EmptyLock = new UniTask<AsyncLockScope>(new AsyncLockScope(null));

	public AsyncLock()
	{
		semaphore = new SemaphoreSlim(1, 1);
	}

	public async UniTask<AsyncLockScope> LockAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return new AsyncLockScope(semaphore);
	}

	public async UniTask WaitAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			await semaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			semaphore.Release();
		}
	}
}
