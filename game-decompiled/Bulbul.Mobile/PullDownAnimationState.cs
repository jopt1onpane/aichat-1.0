using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class PullDownAnimationState : IDisposable
{
	public enum FinishReasonType
	{
		Finished,
		Canceled
	}

	private IInfomationProviderForOSAMyAnimation _animationInfoProvider;

	private CancellationTokenSource _cancelToken;

	private Subject<Unit> _onStartAnimation = new Subject<Unit>();

	private Subject<FinishReasonType> _onFinishAnimation = new Subject<FinishReasonType>();

	private Subject<Unit> _onRequestedRemovalModelFromListTop = new Subject<Unit>();

	private Subject<Unit> _onRequestedInsertionModelFromListBottom = new Subject<Unit>();

	private Subject<bool> _onSkipAnimation = new Subject<bool>();

	public bool IsPlaying { get; private set; }

	public Observable<Unit> OnStartAnimation => _onStartAnimation;

	public Observable<FinishReasonType> OnFinishAnimation => _onFinishAnimation;

	public Observable<Unit> OnRequestedRemovalModelFromListTop => _onRequestedRemovalModelFromListTop;

	public Observable<Unit> OnRequestedInsertionModelFromListBottom => _onRequestedInsertionModelFromListBottom;

	public Observable<bool> OnSkipAnimation => _onSkipAnimation;

	public PullDownAnimationState(IInfomationProviderForOSAMyAnimation animationInfoProvider)
	{
		_animationInfoProvider = animationInfoProvider;
	}

	public void Cancel()
	{
		if (_cancelToken != null)
		{
			_cancelToken.Cancel();
			_cancelToken.Dispose();
			_cancelToken = null;
			IsPlaying = false;
			_onFinishAnimation?.OnNext(FinishReasonType.Canceled);
		}
	}

	public void Dispose()
	{
		_onFinishAnimation?.Dispose();
		_onFinishAnimation = null;
		_onStartAnimation?.Dispose();
		_onStartAnimation = null;
		_onRequestedInsertionModelFromListBottom?.Dispose();
		_onRequestedInsertionModelFromListBottom = null;
		_onRequestedRemovalModelFromListTop?.Dispose();
		_onRequestedRemovalModelFromListTop = null;
		Cancel();
	}

	public void PlayAnimation(bool isOpen, int startIdx, int length, int animationSkipBorderLengh, float baseItemHeight, float durationSec)
	{
		if (isOpen)
		{
			PlayOpen(startIdx, length, baseItemHeight, animationSkipBorderLengh, durationSec).Forget();
		}
		else
		{
			PlayClose(startIdx, length, baseItemHeight, animationSkipBorderLengh, durationSec).Forget();
		}
	}

	private async UniTask PlayClose(int controllingIdx, int length, float baseItemHeight, int animationSkipBorderLength, float durationSec)
	{
		Cancel();
		if (length <= 0)
		{
			return;
		}
		IsPlaying = true;
		_onStartAnimation.OnNext(Unit.Default);
		_cancelToken = new CancellationTokenSource();
		int remain = length;
		int count = 0;
		int num = ((length < animationSkipBorderLength) ? length : animationSkipBorderLength);
		float oneSec = durationSec / (float)num;
		float time = 0f;
		await UniTask.Yield(PlayerLoopTiming.Update, _cancelToken.Token);
		while (remain > 0)
		{
			float num2 = (time - oneSec * (float)(length - remain)) / oneSec;
			float size = Mathf.Lerp(baseItemHeight, 0f, Mathf.Min(1f, num2));
			if (num2 < 1f)
			{
				_animationInfoProvider.RequestChangeItemSizeAndUpdateLayout(controllingIdx, size);
			}
			else
			{
				_onRequestedRemovalModelFromListTop.OnNext(Unit.Default);
			}
			if (num2 < 1f)
			{
				time += Time.deltaTime;
				if (time > durationSec)
				{
					_animationInfoProvider.RequestChangeItemSize(controllingIdx, 0f);
					_onRequestedRemovalModelFromListTop.OnNext(Unit.Default);
					remain--;
					break;
				}
				await UniTask.Yield(PlayerLoopTiming.Update, _cancelToken.Token);
			}
			else
			{
				remain--;
				count++;
				if (count >= animationSkipBorderLength)
				{
					break;
				}
			}
		}
		if (remain > 0)
		{
			_onSkipAnimation.OnNext(value: false);
		}
		IsPlaying = false;
		_cancelToken.Dispose();
		_cancelToken = null;
		_onFinishAnimation.OnNext(FinishReasonType.Finished);
	}

	private async UniTask PlayOpen(int controllingIdx, int length, float baseItemHeight, int animationSkipBorderLength, float durationSec)
	{
		Cancel();
		if (length <= 0)
		{
			return;
		}
		IsPlaying = true;
		_onStartAnimation.OnNext(Unit.Default);
		_cancelToken = new CancellationTokenSource();
		int remain = length;
		int count = 0;
		int num = ((length < animationSkipBorderLength) ? length : animationSkipBorderLength);
		float oneSec = durationSec / (float)num;
		float time = 0f;
		_onRequestedInsertionModelFromListBottom.OnNext(Unit.Default);
		while (remain > 0)
		{
			float num2 = (time - oneSec * (float)(length - remain)) / oneSec;
			float size = Mathf.Lerp(0f, baseItemHeight, Mathf.Min(1f, num2));
			_animationInfoProvider.RequestChangeItemSizeAndUpdateLayout(controllingIdx, size);
			if (num2 < 1f)
			{
				time += Time.deltaTime;
				if (time > durationSec)
				{
					_animationInfoProvider.RequestChangeItemSizeAndUpdateLayout(controllingIdx, baseItemHeight);
					remain--;
					break;
				}
				await UniTask.Yield(PlayerLoopTiming.Update, _cancelToken.Token);
				continue;
			}
			remain--;
			count++;
			if (count >= animationSkipBorderLength)
			{
				break;
			}
			if (remain > 0)
			{
				_onRequestedInsertionModelFromListBottom.OnNext(Unit.Default);
			}
		}
		if (remain > 0)
		{
			_onSkipAnimation.OnNext(value: true);
		}
		IsPlaying = false;
		_cancelToken.Dispose();
		_cancelToken = null;
		_onFinishAnimation.OnNext(FinishReasonType.Finished);
	}
}
