using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class EndCreditsService : MonoBehaviour
{
	[SerializeField]
	private float FadeInSeconds = 2f;

	[SerializeField]
	private float ShowSeconds = 2f;

	[SerializeField]
	private float FadeOutSeconds = 2f;

	[SerializeField]
	private float DelayNextCreditSeconds = 0.5f;

	[SerializeField]
	private GameObject _creditsParent;

	[SerializeField]
	private CanvasGroup[] _creditsCanvasGroups;

	private bool _isPlaying;

	private CancellationTokenSource _cancellationTokenSoruce;

	public bool IsPlaying => _isPlaying;

	public void Setup()
	{
		Prepare();
	}

	private void Prepare()
	{
		_creditsParent.SetActive(value: false);
		CanvasGroup[] creditsCanvasGroups = _creditsCanvasGroups;
		for (int i = 0; i < creditsCanvasGroups.Length; i++)
		{
			creditsCanvasGroups[i].alpha = 0f;
		}
	}

	public async UniTask Play()
	{
		CancelPlay();
		Prepare();
		_cancellationTokenSoruce = new CancellationTokenSource();
		_creditsParent.SetActive(value: true);
		_isPlaying = true;
		_creditsParent.SetActive(value: true);
		CanvasGroup[] creditsCanvasGroups = _creditsCanvasGroups;
		foreach (CanvasGroup canvasGroup in creditsCanvasGroups)
		{
			canvasGroup.gameObject.SetActive(value: true);
			await canvasGroup.DOFade(1f, FadeInSeconds).WithCancellation(_cancellationTokenSoruce.Token);
			await UniTask.Delay(TimeSpan.FromSeconds(ShowSeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationTokenSoruce.Token);
			await canvasGroup.DOFade(0f, FadeOutSeconds).WithCancellation(_cancellationTokenSoruce.Token);
			await UniTask.Delay(TimeSpan.FromSeconds(DelayNextCreditSeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationTokenSoruce.Token);
		}
		_cancellationTokenSoruce?.Dispose();
		_cancellationTokenSoruce = null;
		_creditsParent.SetActive(value: false);
		_isPlaying = false;
	}

	public void CancelPlay()
	{
		if (_cancellationTokenSoruce != null)
		{
			_cancellationTokenSoruce?.Cancel();
			_cancellationTokenSoruce?.Dispose();
			_cancellationTokenSoruce = null;
		}
		_creditsParent.SetActive(value: false);
		_isPlaying = false;
	}
}
