using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class ItemRemoveAnimationState<T> : IDisposable
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

	public ItemRemoveAnimationState(IInfomationProviderForOSAMyAnimation animationProvider, IOSAModelIndexGetter<T> indexGetter)
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

	public async UniTask Play(IRemovableItemView removeItemView, T uuid, float baseSize, float duration, ListItemViewAnimations.RemoveAnimationDirection direction = ListItemViewAnimations.RemoveAnimationDirection.Left)
	{
		int modelIndex = _indexGetter.GetModelIndex(uuid);
		onStart?.OnNext(modelIndex);
		_animationCount++;
		try
		{
			CancellationToken cancellationToken = removeItemView.CancellationToken;
			using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
			UniTask uniTask = removeItemView.Play(direction, linkedSource.Token);
			UniTask uniTask2 = UniTask.Delay(100);
			await UniTask.WhenAny(uniTask, uniTask2);
			await PlayReSizeAnimation(uuid, baseSize, duration, linkedSource);
			linkedSource.Token.ThrowIfCancellationRequested();
		}
		finally
		{
			_animationCount--;
			onComplete?.OnNext(uuid);
		}
	}

	private async UniTask PlayReSizeAnimation(T uuid, float baseSize, float duration, CancellationTokenSource linkedSource)
	{
		_ = baseSize;
		float time = 0f;
		float num;
		do
		{
			await UniTask.Yield(PlayerLoopTiming.Update, linkedSource.Token);
			time += Time.unscaledDeltaTime;
			float t = time / duration;
			num = Mathf.Lerp(baseSize, 0f, t);
			int modelIndex = _indexGetter.GetModelIndex(uuid);
			if (modelIndex == -1)
			{
				throw new OperationCanceledException();
			}
			_animationProvider.RequestChangeItemSizeAndUpdateLayout(modelIndex, num);
		}
		while (num > 0f);
	}

	void IDisposable.Dispose()
	{
		onStart?.Dispose();
		onComplete?.Dispose();
	}
}
