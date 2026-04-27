using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class AprilFoolSunglasses : MonoBehaviour
{
	[SerializeField]
	[Header("オブジェクト")]
	private GameObject _object;

	[SerializeField]
	[Header("マテリアル")]
	private Material _material;

	private Color _emissionColor;

	private Tween _shineTween;

	private CancellationTokenSource _cts;

	public void Setup()
	{
		Color color = _material.GetColor("_EmissionColor");
		_emissionColor = color;
		_emissionColor.a = 0f;
		_material.SetColor("_EmissionColor", _emissionColor);
	}

	public void Activate()
	{
		_object.SetActive(value: true);
	}

	public void Deactivate()
	{
		_object.SetActive(value: false);
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
		_emissionColor.a = 0f;
		_material.SetColor("_EmissionColor", _emissionColor);
	}

	public async UniTask Shine(float startDuration, float waitDuration, float endDuration, CancellationToken cancellationToken = default(CancellationToken))
	{
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = (cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new CancellationTokenSource());
		CancellationToken token = _cts.Token;
		try
		{
			_shineTween = DOTween.To(() => _emissionColor.a, delegate(float x)
			{
				_emissionColor.a = x;
			}, 1f, startDuration).OnUpdate(delegate
			{
				_material.SetColor("_EmissionColor", _emissionColor);
			});
			await _shineTween.ToUniTask(TweenCancelBehaviour.Kill, token);
			await UniTask.Delay(TimeSpan.FromSeconds(waitDuration), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			_shineTween = DOTween.To(() => _emissionColor.a, delegate(float x)
			{
				_emissionColor.a = x;
			}, 0f, endDuration).OnUpdate(delegate
			{
				_material.SetColor("_EmissionColor", _emissionColor);
			});
			await _shineTween.ToUniTask(TweenCancelBehaviour.Kill, token);
		}
		catch
		{
			_emissionColor.a = 0f;
			_material.SetColor("_EmissionColor", _emissionColor);
		}
	}
}
