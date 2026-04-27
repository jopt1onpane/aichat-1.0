using System;
using DG.Tweening;
using R3;
using UnityEngine;

public class MonochromeNoiseDirection : MonoBehaviour
{
	[SerializeField]
	private Material _material;

	private Tween _tween;

	private ReactiveProperty<float> _powerRatio = new ReactiveProperty<float>();

	private DirectionService _directionService;

	public ReadOnlyReactiveProperty<float> PowerRatio => _powerRatio;

	public void Setup(DirectionService directionService)
	{
		_directionService = directionService;
		EndTidying();
		_powerRatio.Subscribe(delegate(float ratio)
		{
			_material.SetFloat("_Ratio", ratio);
			_directionService.ChangeActiveRenderFeature(DirectionService.RenderFeatureType.SimpleNoise, ratio != 0f);
		}).AddTo(this);
	}

	public void Play(float toRatio, float toSecond, Ease easeType = Ease.Linear, Action onComplete = null)
	{
		Stop();
		_tween = DOTween.To(() => _powerRatio.Value, delegate(float value)
		{
			_powerRatio.Value = value;
		}, toRatio, toSecond).SetEase(easeType).OnComplete(delegate
		{
			_tween = null;
			onComplete?.Invoke();
		});
	}

	public void Stop()
	{
		if (_tween != null && _tween.IsActive())
		{
			_tween.Kill();
			_tween = null;
		}
	}

	public void EndTidying()
	{
		Stop();
		_powerRatio.Value = 0f;
	}

	public void ImmediateChange(float ratio)
	{
		_powerRatio.Value = Mathf.Clamp01(ratio);
	}
}
