using R3;
using UnityEngine;

public class ScreenMoveDirection : MonoBehaviour
{
	[SerializeField]
	private Material _material;

	private ReactiveProperty<float> _powerRatio = new ReactiveProperty<float>();

	private DirectionService _directionService;

	public ReadOnlyReactiveProperty<float> PowerRatio => _powerRatio;

	public void Setup(DirectionService directionService)
	{
		_directionService = directionService;
		Stop();
		_powerRatio.Subscribe(delegate(float ratio)
		{
			_material.SetFloat("_Ratio", ratio);
			_directionService.ChangeActiveRenderFeature(DirectionService.RenderFeatureType.ScreenMove, ratio != 0f);
		}).AddTo(this);
	}

	public void Play(float ratio)
	{
		_powerRatio.Value = ratio;
	}

	public void Stop()
	{
		_powerRatio.Value = 0f;
	}

	public void EndTidying()
	{
		Stop();
	}

	public void ImmediateChange(float ratio)
	{
		_powerRatio.Value = Mathf.Clamp01(ratio);
	}
}
