using R3;
using UnityEngine;

public class BlackOutDirection : MonoBehaviour
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
			_directionService.ChangeActiveRenderFeature(DirectionService.RenderFeatureType.Blackout, ratio != 0f);
		}).AddTo(this);
	}

	public void Play(float ratio, int isUseBlind)
	{
		_powerRatio.Value = ratio;
		isUseBlind = ((isUseBlind == 1) ? 1 : 0);
		_material.SetInt("_UseBlind", isUseBlind);
	}

	public void Stop()
	{
		_powerRatio.Value = 0f;
		_material.SetInt("_UseBlind", 0);
	}

	public void EndTidying()
	{
		Stop();
	}

	public void ImmediateChange(float ratio, int isUseBlind)
	{
		_powerRatio.Value = Mathf.Clamp01(ratio);
		isUseBlind = ((isUseBlind == 1) ? 1 : 0);
		_material.SetInt("_UseBlind", isUseBlind);
	}
}
