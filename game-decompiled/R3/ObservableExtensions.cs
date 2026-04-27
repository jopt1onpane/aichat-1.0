using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace R3;

public static class ObservableExtensions
{
	public static IDisposable Subscribe<T>(this Observable<T> source, ISubject<T> subject)
	{
		return source.Subscribe(subject.AsObserver());
	}

	public static IDisposable Subscribe<T>(this Observable<T> source, Action onNext)
	{
		return source.Subscribe(onNext, delegate(T _, Action o)
		{
			o();
		});
	}

	public static Observable<T> ToObservable<T>(this UniTaskCompletionSource<T> source)
	{
		return source.Task.AsValueTask<T>().ToObservable();
	}

	public static Observable<Unit> ToObservable(this UniTaskCompletionSource source)
	{
		return source.Task.AsValueTask().ToObservable();
	}

	public static Observable<Unit> ToR3Observable(this UniTask task)
	{
		return task.AsValueTask().ToObservable();
	}

	public static Observable<T> ToR3Observable<T>(this UniTask<T> task)
	{
		return UniTaskValueTaskExtensions.AsValueTask(in task).ToObservable();
	}

	public static UniTask<T> ToUniTask<T>(this Observable<T> source, bool useFirstValue = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AsSystemObservable().ToUniTask(useFirstValue, cancellationToken);
	}

	public static IDisposable SubscribeAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync, ReactiveProperty<bool> gate)
	{
		return (from args in source.Select((gate, onNextAsync), (T t, (ReactiveProperty<bool> gate, Func<T, CancellationToken, ValueTask> onNextAsync) x) => (t: t, gate: x.gate, onNextAsync: x.onNextAsync))
			where args.gate.CurrentValue
			select args).SubscribeAwait(async delegate((T t, ReactiveProperty<bool> gate, Func<T, CancellationToken, ValueTask> onNextAsync) args, CancellationToken ct)
		{
			args.gate.Value = false;
			try
			{
				await args.onNextAsync(args.t, ct);
			}
			finally
			{
				args.gate.Value = true;
			}
		}, AwaitOperation.Drop);
	}

	public static IDisposable SubscribeAwait<T, TState>(this Observable<T> source, TState state, Func<T, TState, CancellationToken, ValueTask> onNextAsync, ReactiveProperty<bool> gate)
	{
		return (from args in source.Select((gate, onNextAsync, state), (T t, (ReactiveProperty<bool> gate, Func<T, TState, CancellationToken, ValueTask> onNextAsync, TState state) x) => (t: t, gate: x.gate, onNextAsync: x.onNextAsync, state: x.state))
			where args.gate.CurrentValue
			select args).SubscribeAwait(async delegate((T t, ReactiveProperty<bool> gate, Func<T, TState, CancellationToken, ValueTask> onNextAsync, TState state) args, CancellationToken ct)
		{
			args.gate.Value = false;
			try
			{
				await args.onNextAsync(args.t, args.state, ct);
			}
			finally
			{
				args.gate.Value = true;
			}
		}, AwaitOperation.Drop);
	}
}
