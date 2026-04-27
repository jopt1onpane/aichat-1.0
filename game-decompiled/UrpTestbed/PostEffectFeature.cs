using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UrpTestbed;

public sealed class PostEffectFeature : ScriptableRendererFeature
{
	public Material material;

	private PostEffectPass _pass;

	public override void Create()
	{
		_pass = new PostEffectPass
		{
			material = material,
			renderPassEvent = RenderPassEvent.AfterRendering
		};
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
	{
		renderer.EnqueuePass(_pass);
	}
}
