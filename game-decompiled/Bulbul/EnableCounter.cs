using System;
using R3;

namespace Bulbul;

public class EnableCounter
{
	public struct EnableScope : IDisposable
	{
		private readonly EnableCounter _enableCounter;

		private bool _isDisposed;

		public EnableScope(EnableCounter enableCounter)
		{
			_enableCounter = enableCounter;
			_enableCounter.Enable();
			_isDisposed = false;
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
				_enableCounter.Disable();
			}
		}
	}

	private int _count;

	private readonly ReactiveProperty<bool> _enabled = new ReactiveProperty<bool>(value: false);

	public ReadOnlyReactiveProperty<bool> Enabled => _enabled;

	public EnableScope CreateEnableScope()
	{
		return new EnableScope(this);
	}

	private void Enable()
	{
		_count++;
		UpdateState();
	}

	private void Disable()
	{
		_count--;
		if (_count < 0)
		{
			_count = 0;
		}
		UpdateState();
	}

	private void UpdateState()
	{
		_enabled.Value = _count > 0;
	}
}
