using System;
using DG.Tweening;
using R3;
using UnityEngine;

public class AdjustAllUnstableDirection : MonoBehaviour
{
	[SerializeField]
	[Header("グリッチ")]
	private Material _glitchMaterial;

	[SerializeField]
	[Header("画面ズレ")]
	private Material _screenMoveMaterial;

	[SerializeField]
	[Header("ブラックアウト")]
	private Material _blackOutMaterial;

	[SerializeField]
	[Header("モノクロノイズ")]
	private Material _monochromeMaterial;

	[SerializeField]
	[Header("色収差")]
	private Material _chromaticAberrationMaterial;

	private Tween _tween;

	private ReactiveProperty<float> _powerRatio = new ReactiveProperty<float>();

	public ReadOnlyReactiveProperty<float> PowerRatio => _powerRatio;

	public void Setup()
	{
		EndTidying();
		_powerRatio.Subscribe(delegate(float ratio)
		{
			_glitchMaterial.SetFloat("_Unstable", ratio);
			_screenMoveMaterial.SetFloat("_Unstable", ratio);
			_blackOutMaterial.SetFloat("_Unstable", ratio);
			_monochromeMaterial.SetFloat("_Unstable", ratio);
			_chromaticAberrationMaterial.SetFloat("_Unstable", ratio);
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
