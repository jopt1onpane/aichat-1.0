using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bulbul;

public interface IPomodoroCompletedScheduler
{
	UniTask Schedule<T>(T @ref, Func<bool> condition, UniTask defer, CancellationToken ct) where T : class;
}
