using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyUtil;

public class AsyncUtil
{
	public static async UniTask<bool> WaitUntilWithTimeout(Func<bool> condition, float timeoutSeconds, CancellationToken token)
	{
		UniTask uniTask = UniTask.WaitUntil(condition, PlayerLoopTiming.Update, token);
		UniTask uniTask2 = UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds));
		return (await UniTask.WhenAny(uniTask, uniTask2).SuppressCancellationThrow()).Item1;
	}
}
