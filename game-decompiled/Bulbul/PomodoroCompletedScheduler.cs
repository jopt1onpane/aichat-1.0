using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using VContainer.Unity;

namespace Bulbul;

public class PomodoroCompletedScheduler : IPomodoroCompletedScheduler, IStartable, IDisposable
{
	private readonly struct ScheduleItem(object @ref, Func<bool> condition, UniTask defer, UniTaskCompletionSource completion, CancellationToken cancellation) : IEquatable<ScheduleItem>
	{
		public readonly object Ref = @ref;

		public readonly Func<bool> Condition = condition;

		public readonly UniTask Defer = defer;

		public readonly UniTaskCompletionSource Completion = completion;

		public readonly CancellationToken Cancellation = cancellation;

		public bool Equals(ScheduleItem other)
		{
			return Ref == other.Ref;
		}

		public override bool Equals(object obj)
		{
			if (obj is ScheduleItem other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (Ref == null)
			{
				return 0;
			}
			return Ref.GetHashCode();
		}
	}

	private List<ScheduleItem> items = new List<ScheduleItem>();

	private DisposableBag disposable;

	private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

	void IStartable.Start()
	{
		Observable.EveryUpdate().SubscribeAwait(async delegate(Unit _, CancellationToken ct)
		{
			if (items == null || items.Count == 0)
			{
				return;
			}
			ScheduleItem[] pooledArray = ArrayPool<ScheduleItem>.Shared.Rent(items.Count);
			using (Disposable.Create(pooledArray, delegate(ScheduleItem[] x)
			{
				ArrayPool<ScheduleItem>.Shared.Return(x);
			}))
			{
				int count = items.Count;
				items.CopyTo(pooledArray);
				for (int i = 0; i < count; i++)
				{
					ScheduleItem item = pooledArray[i];
					if (item.Cancellation.IsCancellationRequested)
					{
						items.Remove(item);
					}
					else if (item.Condition())
					{
						try
						{
							await item.Defer.AttachExternalCancellation(ct);
						}
						finally
						{
							items.Remove(item);
							item.Completion.TrySetResult();
						}
					}
				}
			}
		}, AwaitOperation.Drop).AddTo(ref disposable);
	}

	public async UniTask Schedule<T>(T @ref, Func<bool> condition, UniTask defer, CancellationToken ct) where T : class
	{
		if (!items.Any((ScheduleItem x) => x.Ref == @ref))
		{
			UniTaskCompletionSource uniTaskCompletionSource = new UniTaskCompletionSource();
			ct.RegisterCompletionSource(uniTaskCompletionSource);
			items.Add(new ScheduleItem(@ref, condition, defer, uniTaskCompletionSource, ct));
			await uniTaskCompletionSource.Task;
		}
	}

	public void Dispose()
	{
		items = null;
		cancellation.Cancel();
		cancellation.Dispose();
	}
}
