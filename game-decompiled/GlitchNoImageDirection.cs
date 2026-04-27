using UnityEngine;

public class GlitchNoImageDirection : MonoBehaviour
{
	private readonly int ratioPropertyHash = Shader.PropertyToID("_Ratio");

	[SerializeField]
	private Material _glitchScreenNoImageMaterial;

	[SerializeField]
	private float stopModeRatio;

	[SerializeField]
	private float playModeRatio = 0.1f;

	private DirectionService _directionService;

	public void Setup(DirectionService directionService)
	{
		_directionService = directionService;
		StopGlitch();
	}

	private void SetRatio(float ratio)
	{
		_glitchScreenNoImageMaterial.SetFloat(ratioPropertyHash, ratio);
		_directionService.ChangeActiveRenderFeature(DirectionService.RenderFeatureType.BlackOutAdditive, ratio != 0f);
	}

	public void PlayGlitch()
	{
		SetRatio(playModeRatio);
	}

	public void StopGlitch()
	{
		SetRatio(stopModeRatio);
	}

	public void EndTidying()
	{
		StopGlitch();
	}
}
