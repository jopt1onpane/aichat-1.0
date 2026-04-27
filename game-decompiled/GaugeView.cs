using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class GaugeView : MonoBehaviour
{
	[SerializeField]
	private Image _gaugeImage;

	[SerializeField]
	private float _gaugeMaxWidth;

	[SerializeField]
	private float _animSeconds;

	[SerializeField]
	private Ease _easeType;

	private float _gaugeMax;

	private ReactiveProperty<float> _currentValue = new ReactiveProperty<float>();

	private float _targetValue;

	private Tween _tween;

	public ReadOnlyReactiveProperty<float> CurrentValue => _currentValue;

	public void Setup(float currentValue, float gaugeMax)
	{
		_gaugeMax = gaugeMax;
		_currentValue.Value = currentValue;
		ObservableSubscribeExtensions.Subscribe(_currentValue, delegate
		{
			ManualUpdate();
		}).AddTo(this);
	}

	private void ManualUpdate()
	{
		Vector2 sizeDelta = _gaugeImage.rectTransform.sizeDelta;
		sizeDelta.x = _currentValue.Value / _gaugeMax * _gaugeMaxWidth;
		_gaugeImage.rectTransform.sizeDelta = sizeDelta;
	}

	public async UniTask OnAddValue(float addedValue)
	{
		_tween?.Kill();
		_tween = DOTween.To(() => _currentValue.Value, delegate(float x)
		{
			_currentValue.Value = x;
		}, addedValue, _animSeconds).SetEase(_easeType);
		await _tween.Play();
	}
}
