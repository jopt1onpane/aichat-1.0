using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bulbul;

public class DefaultPomodoroCompletedScheduler : IPomodoroCompletedScheduler
{
	public async UniTask Schedule<T>(T @ref, Func<bool> condition, UniTask defer, CancellationToken ct) where T : class
	{
	}
}
