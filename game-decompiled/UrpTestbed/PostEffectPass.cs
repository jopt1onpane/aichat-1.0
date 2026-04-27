using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UrpTestbed;

internal sealed class PostEffectPass : ScriptableRenderPass
{
	public Material material;

	public override void Execute(ScriptableRenderContext context, ref RenderingData data)
	{
		if (!(material == null))
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get("PostEffect");
			Blit(commandBuffer, ref data, material);
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
	}
}
