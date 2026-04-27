using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace NestopiSystem;

public class ThrottledExecutor : IDisposable
{
	private readonly Action _action;

	private readonly float _delaySeconds;

	private CancellationTokenSource _cts;

	public ThrottledExecutor(Action action, float delaySeconds)
	{
		_action = action;
		_delaySeconds = delaySeconds;
	}

	public void Reserve()
	{
		if (_cts == null)
		{
			_cts = new CancellationTokenSource();
			ExecuteAsync(_cts.Token).Forget();
		}
	}

	private async UniTaskVoid ExecuteAsync(CancellationToken token)
	{
		await UniTask.Delay(TimeSpan.FromSeconds(_delaySeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		_action?.Invoke();
		_cts?.Dispose();
		_cts = null;
	}

	public void Flush()
	{
		if (_cts != null)
		{
			Cancel();
			_action?.Invoke();
		}
	}

	public void Cancel()
	{
		if (_cts != null)
		{
			_cts.Cancel();
			_cts.Dispose();
			_cts = null;
		}
	}

	public void Dispose()
	{
		Cancel();
	}
}
