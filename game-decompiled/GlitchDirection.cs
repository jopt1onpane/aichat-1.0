using R3;
using UnityEngine;

public class GlitchDirection : MonoBehaviour
{
	[SerializeField]
	private Material _glitch1Material;

	[SerializeField]
	private GameObject _glitchCameraObj;

	[SerializeField]
	[Header("カメラをオンオフする間隔")]
	private float _cameraChangeActiveSecondsMin = 0.3f;

	[SerializeField]
	private float _cameraChangeActiveSecondsMax = 1f;

	private MyStopWatch _stopWatch = new MyStopWatch();

	private ReactiveProperty<float> _powerRatio = new ReactiveProperty<float>();

	private DirectionService _directionService;

	public ReadOnlyReactiveProperty<float> PowerRatio => _powerRatio;

	public void Setup(DirectionService directionService)
	{
		_directionService = directionService;
		StopGlitch1();
		_powerRatio.Subscribe(delegate(float ratio)
		{
			_glitch1Material.SetFloat("_Ratio", ratio);
			_directionService.ChangeActiveRenderFeature(DirectionService.RenderFeatureType.Glitch, ratio != 0f);
		}).AddTo(this);
	}

	public void PlayGlitch1(float ratio)
	{
		_glitchCameraObj.SetActive(value: true);
		_powerRatio.Value = ratio;
		_stopWatch.ChangeTargetSecondsForRandom(_cameraChangeActiveSecondsMin, _cameraChangeActiveSecondsMax);
		_stopWatch.Watch.Restart();
	}

	public void StopGlitch1()
	{
		_glitchCameraObj.SetActive(value: false);
		_powerRatio.Value = 0f;
		_stopWatch.Watch.Stop();
	}

	public void EndTidying()
	{
		StopGlitch1();
	}

	private void Update()
	{
		if (_stopWatch.Watch.IsRunning && _stopWatch.IsElapsedTargetTime())
		{
			_glitchCameraObj.SetActive(!_glitchCameraObj.activeSelf);
			_stopWatch.ChangeTargetSecondsForRandom(_cameraChangeActiveSecondsMin, _cameraChangeActiveSecondsMax);
			_stopWatch.Watch.Restart();
		}
	}

	public void ImmediateChange(float ratio)
	{
		_powerRatio.Value = Mathf.Clamp01(ratio);
	}
}
