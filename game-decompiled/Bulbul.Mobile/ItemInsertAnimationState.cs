using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class ItemInsertAnimationState<T>
{
	private IInfomationProviderForOSAMyAnimation _animationProvider;

	private IOSAModelIndexGetter<T> _indexGetter;

	private CancellationTokenSource _cancellationTokenSource;

	private Subject<int> onStart = new Subject<int>();

	private Subject<T> onComplete = new Subject<T>();

	private int _animationCount;

	public Observable<int> OnStart => onStart;

	public Observable<T> OnComplete => onComplete;

	public bool IsPlayingAnimation => _animationCount > 0;

	public ItemInsertAnimationState(IInfomationProviderForOSAMyAnimation animationProvider, IOSAModelIndexGetter<T> indexGetter)
	{
		_animationProvider = animationProvider;
		_indexGetter = indexGetter;
		_cancellationTokenSource = new CancellationTokenSource();
	}

	public void Cancel()
	{
		if (_cancellationTokenSource != null)
		{
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;
			_cancellationTokenSource = new CancellationTokenSource();
		}
	}

	public async UniTask Play(T uuid, float baseSize, float duration, ListItemViewAnimations.RemoveAnimationDirection direction = ListItemViewAnimations.RemoveAnimationDirection.Left)
	{
		int modelIndex = _indexGetter.GetModelIndex(uuid);
		onStart?.OnNext(modelIndex);
		_animationCount++;
		try
		{
			await PlayReSizeAnimation(uuid, baseSize, duration, _cancellationTokenSource);
		}
		catch (OperationCanceledException ex)
		{
			throw ex;
		}
		finally
		{
			modelIndex = _indexGetter.GetModelIndex(uuid);
			_animationProvider.RequestChangeItemSize(modelIndex, baseSize);
			_animationCount--;
			onComplete?.OnNext(uuid);
		}
	}

	private async UniTask PlayReSizeAnimation(T uuid, float baseSize, float duration, CancellationTokenSource linkedSource)
	{
		float time = 0f;
		float num;
		do
		{
			await UniTask.Yield(PlayerLoopTiming.Update, linkedSource.Token);
			time += Time.unscaledDeltaTime;
			float t = time / duration;
			num = Mathf.Lerp(0f, baseSize, t);
			int modelIndex = _indexGetter.GetModelIndex(uuid);
			_animationProvider.RequestChangeItemSizeAndUpdateLayout(modelIndex, num, endEdgeStationary: true);
		}
		while (num < baseSize);
	}

	public void Dispose()
	{
		onStart?.Dispose();
		onComplete?.Dispose();
	}
}
